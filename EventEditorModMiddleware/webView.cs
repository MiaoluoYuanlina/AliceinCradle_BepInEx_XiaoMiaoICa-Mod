using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventEditorUI
{
    public partial class webView : Form
    {
        // Configuration
        private readonly string _editorUrl;
        private readonly string _gameDirectory;
        private readonly int _gamePid;

        // WebView2 control (created programmatically)
        private Microsoft.Web.WebView2.WinForms.WebView2 _webViewControl;

        // Language dialog state - TaskCompletionSource to await JS results
        private TaskCompletionSource<string[]> _currentLanguageTcs;
        private bool _languageDialogOpen = false;

        // Message DTO for deserializing JS messages
        private class WebMessage
        {
            public string Type { get; set; }
            public string Tag { get; set; }
            public string[] Values { get; set; }
        }

        public webView(string editorUrl, string gameDirectory, int gamePid)
        {
            InitializeComponent();
            _editorUrl = editorUrl;
            _gameDirectory = gameDirectory;
            _gamePid = gamePid;

            this.Text = "Event Editor - XiaoMiaoICa";
            this.WindowState = FormWindowState.Maximized;

            // Create WebView2 control programmatically (fills entire form)
            _webViewControl = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill
            };
            _webViewControl.KeyDown += OnBrowserKeyDown;
            this.Controls.Add(_webViewControl);

            var menuStrip = new MenuStrip();
            var windowMenu = new ToolStripMenuItem("窗口(&W)");
            var newWindowItem = new ToolStripMenuItem("新建编辑器窗口(&N)")
            {
                ShortcutKeys = Keys.Control | Keys.N
            };
            newWindowItem.Click += (sender, e) =>
                EventEditorModMiddleware.Program.StartForm(_editorUrl, _gameDirectory, _gamePid);
            windowMenu.DropDownItems.Add(newWindowItem);
            menuStrip.Items.Add(windowMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            this.Load += Form_Load;
            this.FormClosing += Form_FormClosing;
        }

        // ==================== Form Lifecycle ====================

        private void OnBrowserKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData != (Keys.Control | Keys.N)) return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            // Leave the browser's keyboard callback before initializing another WebView.
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing)
                    EventEditorModMiddleware.Program.StartForm(_editorUrl, _gameDirectory, _gamePid);
            }));
        }

        private async void Form_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize WebView2 (uses Evergreen runtime, pre-installed on Windows 11)
                await _webViewControl.EnsureCoreWebView2Async(null);
                if (IsDisposed || Disposing || _webViewControl == null) return;

                // Wire up events
                _webViewControl.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                _webViewControl.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

                // Navigate to editor URL
                _webViewControl.CoreWebView2.Navigate(_editorUrl);
                Console.WriteLine("WebView2 初始化完成，正在导航到: " + _editorUrl);
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                MessageBox.Show(
                    "WebView2 Runtime 未安装。\n\n" +
                    "请从以下链接安装:\nhttps://go.microsoft.com/fwlink/p/?LinkId=2124703\n\n" +
                    $"错误详情: {ex.Message}",
                    "WebView2 不可用",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up WebView2
            if (_webViewControl != null)
            {
                if (_webViewControl.CoreWebView2 != null)
                {
                    _webViewControl.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
                    _webViewControl.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
                }
                _webViewControl.Dispose();
                _webViewControl = null;
            }
            Console.WriteLine("WebView 表单已关闭");
        }

        // ==================== WebView2 Events ====================

        private async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Console.WriteLine($"导航失败: {e.WebErrorStatus}");
                return;
            }

            Console.WriteLine("页面加载完成，正在注入脚本...");

            // STEP 0: Inject the JS-to-C# bridge (replaces Playwright's ExposeFunctionAsync)
            await ExecuteScriptAsync(@"
(function() {
    // Bridge: JS can call window.callCSharp(tag) to send messages to C#
    window.callCSharp = function(tag) {
        window.chrome.webview.postMessage(
            JSON.stringify({ type: 'callCSharp', tag: tag }));
    };
    // Bridge: JS can call window.onLanguageSubmit(values) to send language data to C#
    window.onLanguageSubmit = function(values) {
        window.chrome.webview.postMessage(
            JSON.stringify({ type: 'onLanguageSubmit', values: values }));
    };
})();
            ");

            // STEP 1: Rewrite original button text
            await ExecuteScriptAsync(@"
(() => {
    const btn = document.querySelector('button[onclick*=""saveRaw(true)""]');
    if (btn) btn.textContent = '复制工程到剪切板';
})();
            ");

            // STEP 2: Inject custom C# buttons (A, B, C)
            await ExecuteScriptAsync(@"
(() => {
    const targetP = Array.from(document.querySelectorAll('p'))
        .find(p => p.innerText.includes('导出对话'));

    if (!targetP) return;

    if (document.getElementById('__csharp_btn_A__')) return;

    const newP = document.createElement('p');
    newP.id = '__csharp_toolbar__';

    const makeBtn = (id, text, tag) => {
        const btn = document.createElement('button');
        btn.id = id;
        btn.textContent = text;
        btn.onclick = () => window.callCSharp(tag);
        return btn;
    };

    // 添加按钮
    newP.appendChild(makeBtn('__csharp_btn_A__', '执行', 'A'));
    newP.appendChild(makeBtn('__csharp_btn_B__', '复制项目并保存到游戏', 'B'));
    newP.appendChild(makeBtn('__csharp_btn_C__', '保存工程到指定文件夹', 'C'));
    //newP.appendChild(makeBtn('__csharp_btn_D__', 'TestPing', 'D'));

    targetP.insertAdjacentElement('afterend', newP);
})();
            ");

            // STEP 3: Inject custom CSS
            await ExecuteScriptAsync(@"
(() => {
    if (document.getElementById('__editor_toolbar_style__')) return;

    const style = document.createElement('style');
    style.id = '__editor_toolbar_style__';
    style.innerHTML = `
        /* ===== 工具栏整体（p） ===== */
        p {
            display: flex !important;
            flex-wrap: wrap;
            align-items: center;
            gap: 8px;
            padding: 12px 14px;
            margin: 12px 0;
            background: #ffffff;
            border-radius: 14px;
            border: 1px solid #f1ecff;
            box-shadow:
                0 6px 18px rgba(180, 150, 255, 0.18),
                0 1px 2px rgba(0, 0, 0, 0.04);
        }

        /* ===== 所有按钮（含原生 + 注入） ===== */
        p button {
            appearance: none;
            border: 1px solid #e6dcff;
            background: linear-gradient(135deg, #f4ebff, #ffe9f3);
            color: #6b4bbd;
            border-radius: 10px;
            padding: 7px 14px;
            font-size: 13px;
            font-weight: 500;
            cursor: pointer;
            transition: all 0.18s ease;
            box-shadow: 0 2px 6px rgba(170, 150, 255, 0.18);
            white-space: nowrap;
        }

        p button:hover {
            background: linear-gradient(135deg, #eadbff, #ffd6ea);
            transform: translateY(-1px);
            box-shadow: 0 6px 14px rgba(180, 150, 255, 0.25);
        }

        p button:active {
            transform: scale(0.96);
            box-shadow: 0 2px 6px rgba(180, 150, 255, 0.2);
        }

        /* ===== 特殊按钮 ===== */
        p button[id='__csharp_btn_A__'] ,
        p button[id='__csharp_btn_C__'] ,
        p button[id='__csharp_btn_B__'] {
            background: linear-gradient(135deg, #ffd6eb, #f3d1ff);
            color: #8a3fa9;
            border-color: #f1c4ff;
        }

        /* ===== checkbox + 文本 ===== */
        p input[type='checkbox'] {
            accent-color: #b48cff;
            transform: scale(1.15);
            cursor: pointer;
            margin-left: 6px;
            margin-right: 4px;
        }

        p input[type='checkbox'] + text,
        p {
            color: #7a6ca8;
            font-size: 12.5px;
        }

        /* 工程模式文字优化 */
        p input#project {
            margin-left: 10px;
        }

        /* ===== 让 checkbox 和文字像一个整体 ===== */
        p input#project {
            margin-right: 4px;
        }
    `;
    document.head.appendChild(style);
})();
            ");

            // STEP 4: Add file import drop zone
            await ExecuteScriptAsync(@"
(() => {
    // 1. 定位工具栏 (容器)
    const toolbar = document.getElementById('__csharp_toolbar__');
    if (!toolbar) return;

    // 2. 定位要赋值的目标编辑框 (沿用上一个需求的 ID)
    const textArea = document.getElementById('codeArea');

    // 3. 防止重复注入
    if (document.getElementById('__small_dropzone__')) return;

    // 4. 创建紧凑型拖拽区 (使用 label 标签以便利用行内属性)
    const dropZone = document.createElement('label');
    dropZone.id = '__small_dropzone__';
    dropZone.innerText = '📂 拖入或点击读取文件';

    // 5. 设置样式：小巧、行内、虚线框
    Object.assign(dropZone.style, {
        display: 'inline-block',       // 和按钮排在同一行
        marginLeft: '10px',            // 与左边按钮的间距
        padding: '3px 8px',            // 内部填充尽可能小
        border: '1px dashed #666',     // 虚线框表示这是拖拽区
        borderRadius: '3px',
        fontSize: '13px',              // 字体稍小
        cursor: 'pointer',
        backgroundColor: '#fff',
        color: '#333',
        verticalAlign: 'middle',       // 垂直对齐
        transition: 'all 0.2s'
    });

    // 6. 创建隐藏的文件输入框
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.style.display = 'none';
    dropZone.appendChild(fileInput);

    // --- 核心逻辑 ---
    const handleFile = (file) => {
        if (!file || !textArea) {
             if(!textArea) alert('未找到 id 为 codeArea 的编辑框！');
             return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            textArea.value = e.target.result;
            // 触发 React/Vue/Angular 可能需要的 input 事件
            textArea.dispatchEvent(new Event('input', { bubbles: true }));

            // 成功提示特效
            const oldText = dropZone.firstChild.textContent; // 保存旧文本
            dropZone.firstChild.textContent = '✅ 读取成功 点击读取工程加载拼图';
            dropZone.style.borderColor = 'green';
            dropZone.style.color = 'green';

            setTimeout(() => {
                dropZone.firstChild.textContent = oldText;
                dropZone.style.borderColor = '#666';
                dropZone.style.color = '#333';
            }, 1500);
        };
        reader.readAsText(file);
    };

    // --- 事件监听 ---

    // 点击选择
    fileInput.addEventListener('change', (e) => {
        handleFile(e.target.files[0]);
        fileInput.value = '';
    });

    // 拖拽进入
    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.style.backgroundColor = '#e3f2fd'; // 变蓝
        dropZone.style.borderColor = '#2196F3';
    });

    // 拖拽离开
    dropZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        dropZone.style.backgroundColor = '#fff';
        dropZone.style.borderColor = '#666';
    });

    // 放置文件
    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.style.backgroundColor = '#fff';
        dropZone.style.borderColor = '#666';

        if (e.dataTransfer.files.length > 0) {
            handleFile(e.dataTransfer.files[0]);
        }
    });

    // 7. 插入到工具栏最后
    toolbar.appendChild(dropZone);

})();
            ");

            // STEP 5: Add resize modal
            await ExecuteScriptAsync(@"
(async () => {
    // 1. 检查是否已经注入过样式，没有则注入
    if (!document.getElementById('__custom_resize_style__')) {
        const style = document.createElement('style');
        style.id = '__custom_resize_style__';
        style.innerHTML = `
            #custom_modal_mask {
                position: fixed; top: 0; left: 0; width: 100%; height: 100%;
                background: rgba(0,0,0,0.5); display: flex; align-items: center;
                justify-content: center; z-index: 9999;
            }
            #custom_modal_box {
                background: white; padding: 20px; border-radius: 8px;
                box-shadow: 0 4px 15px rgba(0,0,0,0.3); width: 300px; font-family: sans-serif;
            }
            #custom_modal_box h3 { margin-top: 0; font-size: 16px; color: #333; }
            #custom_modal_box input {
                width: 100%; box-sizing: border-box; padding: 8px;
                margin: 10px 0; border: 1px solid #ccc; border-radius: 4px;
            }
            #custom_modal_btns { text-align: right; }
            #custom_modal_btns button {
                padding: 6px 12px; margin-left: 8px; cursor: pointer; border-radius: 4px; border: none;
            }
            .btn-confirm { background: #007bff; color: white; }
            .btn-cancel { background: #6c757d; color: white; }
        `;
        document.head.appendChild(style);
    }

    // 2. 创建并显示模态框
    const mask = document.createElement('div');
    mask.id = 'custom_modal_mask';
    mask.innerHTML = `
        <div id='custom_modal_box'>
            <h3>设置画布尺寸</h3>
            <input type='text' id='size_input' placeholder='宽度,高度 (如: 800,600)'>
            <div id='custom_modal_btns'>
                <button class='btn-cancel' onclick='document.getElementById(""custom_modal_mask"").remove()'>取消</button>
                <button class='btn-confirm' id='btn_resize_confirm'>确认</button>
            </div>
        </div>
    `;
    document.body.appendChild(mask);

    // 3. 绑定确认逻辑
    document.getElementById('btn_resize_confirm').onclick = () => {
        const val = document.getElementById('size_input').value;
        const parts = val.replace('，', ',').split(',').map(s => s.trim());

        if (parts.length === 2) {
            const w = parts[0];
            const h = parts[1];
            const el = document.getElementById('blocklyDiv');
            if (el) {
                el.style.width = w + 'px';
                el.style.height = h + 'px';
                // 刷新 Blockly
                if (window.Blockly) window.Blockly.svgResize(window.Blockly.getMainWorkspace());
            }
            mask.remove(); // 关闭模态框
        } else {
            alert('请输入正确的格式：宽,高');
        }
    };
})();
            ");

            Console.WriteLine("所有脚本注入完成");
        }

        // ==================== JS-to-C# Communication ====================

        private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            if (string.IsNullOrEmpty(json)) return;

            WebMessage msg;
            try
            {
                msg = JsonConvert.DeserializeObject<WebMessage>(json);
            }
            catch
            {
                Console.WriteLine($"无法解析 WebMessage: {json}");
                return;
            }

            if (msg.Type == "onLanguageSubmit")
            {
                Console.WriteLine("收到语言提交数据，共 " + (msg.Values?.Length ?? 0) + " 项");
                _currentLanguageTcs?.TrySetResult(msg.Values ?? new string[0]);
                _languageDialogOpen = false;
                return;
            }

            if (msg.Type == "callCSharp")
            {
                await HandleCallCSharpAsync(msg.Tag);
                return;
            }

            Console.WriteLine($"未知消息类型: {msg.Type}");
        }

        // ==================== Button Handlers ====================

        private async Task HandleCallCSharpAsync(string tag)
        {
            Console.WriteLine($"点击了按钮 {tag}");

            switch (tag)
            {
                case "A":
                    await HandleButtonA();
                    break;
                case "B":
                    await HandleButtonB();
                    break;
                case "C":
                    await HandleButtonC();
                    break;
                case "D":
                    await HandleButtonD();
                    break;
                default:
                    Console.WriteLine($"未知按钮标签: {tag}");
                    break;
            }
        }

        /// <summary>
        /// Button A: Execute event script - read codeArea, send to game mod, bring game to front
        /// </summary>
        private async Task HandleButtonA()
        {
            try
            {
                await WaitForElementAsync("input#project");
                bool isChecked = await EvaluateAsync<bool>("document.getElementById('project').checked");

                if (isChecked)
                {
                    await ExecuteScriptAsync(@"
(() => {
    const cb = document.getElementById('project');
    if (!cb) return;
    cb.checked = false;
    cb.dispatchEvent(new Event('change', { bubbles: true }));
})()");
                    Console.WriteLine("工程模式已关闭");
                }

                string code = await EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})()");

                Console.WriteLine("codeArea 输入框内容：");
                Console.WriteLine(code);

                var dataJson = new EventEditorModMiddleware.Program.DataJson
                {
                    Type = "EventEditor_Text",
                    Text = code
                };

                // Send to game mod via named pipe (run off UI thread to avoid blocking)
                await Task.Run(() =>
                    EventEditorModMiddleware.Program.Send(
                        "MiaoAicMod_Mod",
                        JsonConvert.SerializeObject(dataJson, Newtonsoft.Json.Formatting.Indented)));

                await Task.Delay(500);
                EventEditorModMiddleware.Program.BringToFront(_gamePid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作异常: {ex.Message}");
            }
        }

        /// <summary>
        /// Button B: Copy project and save to game - extract Blockly blocks, show language editor dialog
        /// </summary>
        private async Task HandleButtonB()
        {
            string code = "";
            try
            {
                await WaitForElementAsync("input#project");
                bool isChecked = await EvaluateAsync<bool>("document.getElementById('project').checked");

                if (isChecked)
                {
                    await ExecuteScriptAsync(@"
(() => {
    const cb = document.getElementById('project');
    if (!cb) return;
    cb.checked = false;
    cb.dispatchEvent(new Event('change', { bubbles: true }));
})()");
                    Console.WriteLine("工程模式已关闭");
                }

                code = await EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})()");

                Console.WriteLine("codeArea 输入框内容：");
                Console.WriteLine(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作异常: {ex.Message}");
            }

            // Extract Blockly workspace data as JSON
            string blockJson = await EvaluateAsync<string>(@"
(() => {
    const ws = Blockly.getMainWorkspace();
    if (!ws) return JSON.stringify([]);
    const blocks = ws.getAllBlocks(false);
    return JSON.stringify(blocks.map(b => {
        const fieldsData = {};
        if (b.inputList) {
            b.inputList.forEach(input => {
                if (input.fieldRow) {
                    input.fieldRow.forEach(field => {
                        if (field.name) {
                            fieldsData[field.name] = field.getValue();
                        }
                    });
                }
            });
        }
        return { type: b.type, id: b.id, values: fieldsData };
    }));
})()
            ");

            // Parse the JSON result - WebView2 returns JSON-stringified value
            string blockJsonParsed = JsonConvert.DeserializeObject<string>(blockJson);
            var blockList = JsonConvert.DeserializeObject<List<dynamic>>(blockJsonParsed);

            foreach (var block in blockList)
            {
                // Check if it's an entrance block
                if (block.type == "entrance")
                {
                    string eventId = block.values.String_0;
                    string isExportRaw = block.values.Bool_0;

                    Console.WriteLine($"事件ID: {eventId}");
                    EventEditorModMiddleware.Program.ExportToUtf8(
                        $"{_gameDirectory}\\AliceInCradle_Data\\StreamingAssets\\evt\\{eventId}.cmd", code);

                    if (isExportRaw == "TRUE")
                    {
                        Console.WriteLine("对话单独导出已开启");
                        Console.WriteLine("获取单独对话内容");

                        // Click the compile button to generate dialogue
                        await ExecuteScriptAsync(@"
(() => {
    const btn = document.querySelector('button[onclick=""compile()""]');
    if (btn) btn.click();
})()
                        ");

                        string compiledCode = await EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})()");

                        Console.WriteLine("codeArea 输入框内容：");
                        Console.WriteLine(compiledCode);

                        Console.WriteLine("显示多语言编辑框。");

                        // Guard: only one language dialog at a time
                        if (_languageDialogOpen)
                        {
                            Console.WriteLine("语言编辑框已打开，跳过");
                            break;
                        }
                        _languageDialogOpen = true;
                        _currentLanguageTcs = new TaskCompletionSource<string[]>();

                        // Inject the language editor dialog
                        await ExecuteScriptAsync($@"
(() => {{
    const code_init = {JsonConvert.ToString(compiledCode)};

    // 1. 清理旧弹窗
    const oldOverlay = document.getElementById('my-custom-overlay');
    if (oldOverlay) oldOverlay.remove();

    // 2. 配置字段
    const fieldsConfig = [
        {{ label: '英语 (English)', id: 'en' }},
        {{ label: '韩语 (Korean)', id: 'ko' }},
        {{ label: '泰语 (Thai)', id: 'th' }},
        {{ label: '简体中文 (Simplified Chinese)', id: 'zh-cn' }},
        {{ label: '繁体中文 (Traditional Chinese)', id: 'zh-tw' }},
        {{ label: '日语 (Japanese)', id: 'ja' }}
    ];

    const safeCode = code_init ? String(code_init) : '';
    const dataValues = fieldsConfig.map(() => safeCode);
    let activeIndex = -1;

    // 3. 样式定义
    const overlayStyle = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 99999; display: flex; justify-content: center; align-items: center;';
    const containerStyle = 'background-color: white; border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.2); display: flex; width: 900px; height: 600px; overflow: hidden; font-family: sans-serif;';
    const sidebarStyle = 'width: 220px; background-color: #f5f5f5; border-right: 1px solid #ddd; padding: 15px; display: flex; flex-direction: column; gap: 8px; overflow-y: auto;';
    const navBtnStyle = 'padding: 10px 15px; cursor: pointer; border-radius: 4px; border: none; background: transparent; text-align: left; font-size: 14px; color: #333; transition: all 0.2s; outline: none;';
    const navBtnActiveStyle = 'background-color: #2196F3; color: white; font-weight: bold; box-shadow: 0 2px 5px rgba(33, 150, 243, 0.3);';
    const contentStyle = 'flex: 1; padding: 25px; display: flex; flex-direction: column; background-color: #fff;';
    const labelStyle = 'font-size: 18px; font-weight: bold; margin-bottom: 15px; color: #444; border-bottom: 1px solid #eee; padding-bottom: 10px;';
    const textareaStyle = 'flex: 1; width: 100%; padding: 15px; font-family: monospace; font-size: 14px; line-height: 1.5; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; resize: none; outline: none; margin-bottom: 15px;';
    const footerStyle = 'display: flex; justify-content: flex-end; gap: 10px;';

    // 4. 创建 DOM 结构
    const overlay = document.createElement('div');
    overlay.id = 'my-custom-overlay';
    overlay.style.cssText = overlayStyle;

    const container = document.createElement('div');
    container.style.cssText = containerStyle;

    const sidebar = document.createElement('div');
    sidebar.style.cssText = sidebarStyle;

    const contentArea = document.createElement('div');
    contentArea.style.cssText = contentStyle;

    const currentLabel = document.createElement('div');
    currentLabel.style.cssText = labelStyle;

    const textarea = document.createElement('textarea');
    textarea.style.cssText = textareaStyle;
    textarea.placeholder = '在此输入翻译内容...';

    const footer = document.createElement('div');
    footer.style.cssText = footerStyle;

    const confirmBtn = document.createElement('button');
    confirmBtn.innerText = '确认保存并覆盖';
    confirmBtn.style.cssText = 'padding: 10px 25px; cursor: pointer; background-color: #2196F3; color: white; border: none; border-radius: 4px; font-size: 15px; font-weight: bold; box-shadow: 0 2px 5px rgba(0,0,0,0.2);';

    const cancelBtn = document.createElement('button');
    cancelBtn.innerText = '关闭';
    cancelBtn.style.cssText = 'padding: 10px 20px; cursor: pointer; background-color: #e0e0e0; color: #333; border: none; border-radius: 4px; font-size: 15px; margin-right: 10px;';
    cancelBtn.onclick = () => overlay.remove();

    footer.appendChild(cancelBtn);
    footer.appendChild(confirmBtn);

    contentArea.appendChild(currentLabel);
    contentArea.appendChild(textarea);
    contentArea.appendChild(footer);

    const buttons = [];

    function switchLanguage(newIndex) {{
        if (activeIndex !== -1) {{
            dataValues[activeIndex] = textarea.value;
        }}

        activeIndex = newIndex;

        textarea.value = dataValues[activeIndex] || '';
        currentLabel.innerText = '正在编辑: ' + fieldsConfig[activeIndex].label;

        buttons.forEach((btn, idx) => {{
            if (idx === newIndex) {{
                btn.style.cssText = navBtnStyle + navBtnActiveStyle;
            }} else {{
                btn.style.cssText = navBtnStyle;
            }}
        }});

        textarea.focus();
    }}

    fieldsConfig.forEach((field, index) => {{
        const btn = document.createElement('button');
        btn.innerText = field.label;
        btn.onclick = () => switchLanguage(index);
        sidebar.appendChild(btn);
        buttons.push(btn);
    }});

    confirmBtn.onclick = () => {{
        if (activeIndex !== -1) {{
            dataValues[activeIndex] = textarea.value;
        }}

        // Use WebView2 postMessage bridge instead of Playwright's ExposeFunction
        window.chrome.webview.postMessage(
            JSON.stringify({{ type: 'onLanguageSubmit', values: dataValues }}));
        overlay.remove();
    }};

    container.appendChild(sidebar);
    container.appendChild(contentArea);
    overlay.appendChild(container);
    document.body.appendChild(overlay);

    // 9. 触发默认选中 (简体中文 - Index 3)
    switchLanguage(3);
}})();
                        ");

                        // Wait for the user to submit language values
                        string[] userInputs = await _currentLanguageTcs.Task;
                        Console.WriteLine("\n捕获到的内容：");
                        string[] labels = { "英语", "韩语", "泰语", "简中", "繁中", "日语" };
                        string[] languagePath = { "en", "ko-kr", "th", "zh-cn", "zh-tc", "_" };
                        for (int i = 0; i < userInputs.Length; i++)
                        {
                            Console.WriteLine($"[{labels[i]}] 内容长度: {userInputs[i].Length}");
                            Console.WriteLine(userInputs[i]);

                            string eventFileName = Path.GetFileName(eventId);
                            EventEditorModMiddleware.Program.ExportToUtf8(
                                $"{_gameDirectory}\\AliceInCradle_Data\\StreamingAssets\\localization\\{languagePath[i]}\\ev_{eventFileName}.txt",
                                userInputs[i]);
                            Console.WriteLine("-----------------------");
                        }
                    }

                    // For entrance blocks, process only the first one and break
                    break;
                }
            }
        }

        /// <summary>
        /// Button C: Save project to file
        /// </summary>
        private async Task HandleButtonC()
        {
            await WaitForElementAsync("input#project");
            bool isChecked = await EvaluateAsync<bool>("document.getElementById('project').checked");

            if (!isChecked)
            {
                await ExecuteScriptAsync(@"
(() => {
    const cb = document.getElementById('project');
    if (!cb) return;
    cb.checked = true;
    cb.dispatchEvent(new Event('change', { bubbles: true }));
})()");
                Console.WriteLine("工程模式已开启");
            }

            string code = await EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})()");

            // Show save dialog on UI thread (STA is satisfied since we're a WinForms form)
            EventEditorModMiddleware.Program.PromptAndSaveFile(code, "");
        }

        /// <summary>
        /// Button D: Test Ping
        /// </summary>
        private async Task HandleButtonD()
        {
            var dataJson = new EventEditorModMiddleware.Program.DataJson
            {
                Type = "Ping",
                Text = "来自MiaoAicMod_EventEditor"
            };

            await Task.Run(() =>
                EventEditorModMiddleware.Program.Send(
                    "MiaoAicMod_Mod",
                    JsonConvert.SerializeObject(dataJson, Newtonsoft.Json.Formatting.Indented)));
        }

        // ==================== Helper Methods ====================

        /// <summary>
        /// Execute JavaScript without returning a value.
        /// </summary>
        private async Task ExecuteScriptAsync(string script)
        {
            await _webViewControl.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Execute JavaScript and deserialize the return value to T.
        /// WebView2's ExecuteScriptAsync always returns a JSON string.
        /// </summary>
        private async Task<T> EvaluateAsync<T>(string javaScript)
        {
            string raw = await _webViewControl.CoreWebView2.ExecuteScriptAsync(javaScript);
            if (raw == null || raw == "null")
                return default(T);
            return JsonConvert.DeserializeObject<T>(raw);
        }

        /// <summary>
        /// Poll until a DOM element matching the selector exists.
        /// Replaces Playwright's WaitForSelectorAsync.
        /// </summary>
        private async Task WaitForElementAsync(string selector, int timeoutMs = 5000)
        {
            string escapedSelector = selector.Replace("'", "\\'");
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                string result = await _webViewControl.CoreWebView2
                    .ExecuteScriptAsync($"!!document.querySelector('{escapedSelector}')");
                if (result == "true") return;
                await Task.Delay(100);
            }
            Console.WriteLine($"警告: 元素 '{selector}' 在 {timeoutMs}ms 内未找到");
        }
    }
}
