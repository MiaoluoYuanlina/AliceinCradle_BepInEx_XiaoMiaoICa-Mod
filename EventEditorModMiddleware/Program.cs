using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

class EventEditorModMiddleware
{
    // --- Windows API 导入 ---
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // 常量定义
    private const int SW_RESTORE = 9;
    private const byte VK_MENU = 0x12; // Alt 键
    private const uint KEYEVENTF_KEYUP = 0x02;


    internal class Program
    {
        static int Game_PID = 0;
        static string Game_directory = "";
        [STAThread]
        static void Main(string[] args)
        {

            //Start("chrome", "https://api.ica.wiki/AIC/EventEditor/");
            new Program().Receive("MiaoAicMod_EventEditor");
        }
        public class RequestDto
        {
            public string Command { get; set; }
            public int Value { get; set; }
        }
        public class ResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
        public bool Send(string Objective, string text)
        {
            using (var client = new NamedPipeClientStream(
            ".",
            Objective,
            PipeDirection.InOut))
            {
                client.Connect(3000);

                using (var reader = new StreamReader(client))
                using (var writer = new StreamWriter(client))
                {
                    writer.AutoFlush = true;

                    var req = new RequestDto
                    {
                        Command = text,
                        Value = 123
                    };

                    writer.WriteLine(System.Text.Json.JsonSerializer.Serialize(req));

                    string resp = reader.ReadLine();
                    Console.WriteLine("返回：" + resp);
                }
            }


            return true;
        }
        public bool Receive(string Objective)
        {
            Console.WriteLine("服务端启动：" + Objective);

            while (true)
            {
                NamedPipeServerStream server = null;
                StreamReader reader = null;
                StreamWriter writer = null;

                try
                {
                    server = new NamedPipeServerStream(
                        Objective,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message);

                    server.WaitForConnection();

                    reader = new StreamReader(server);
                    writer = new StreamWriter(server) { AutoFlush = true };

                    string json = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Console.WriteLine("收到空数据");
                        continue;
                    }

                    var req = System.Text.Json.JsonSerializer.Deserialize<RequestDto>(json);
                    Console.WriteLine("收到命令：" + req.Command);

                    DataJson Json = JsonConvert.DeserializeObject<DataJson>(req.Command);

                    if (Json.Type == "EventEditor_Start")
                    {
                        Game_directory = Json.directory;
                        Game_PID = Json.Pid;
                        Console.WriteLine("#XiaoMiaoICa: Game_PID:" + Game_PID);
                        Console.WriteLine("#XiaoMiaoICa: Game_directory:" + Game_directory);

                        Task.Run(() =>
                        {
                            Start(Json.Objective,Json.EditorUrl);
                        });
                    }

                    var resp = new ResponseDto
                    {
                        Success = true,
                        Message = "处理完成"
                    };

                    if (server.IsConnected)
                    {
                        writer.WriteLine(System.Text.Json.JsonSerializer.Serialize(resp));
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("管道已断开，等待下一个连接");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("未知异常：" + ex);
                }
                finally
                {
                    try { writer?.Dispose(); } catch { }
                    try { reader?.Dispose(); } catch { }
                    try { server?.Dispose(); } catch { }
                }
            }
        }

        static void Start(string Channel, string EditorUrl)
        {
            TaskCompletionSource<string[]> currentLanguageTcs = null;// 捕获多语言输入结果

            using (var playwright = Playwright.CreateAsync().Result)
            {
                Console.WriteLine("正在启动浏览器：" + Channel);
                var browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    Channel = Channel,
                    SlowMo = 50,
                    Args = new[] { "--start-maximized" }//最大化
                }).Result;


                // 创建页面
                var page = browser.NewPageAsync(new BrowserNewPageOptions
                {
                    ViewportSize = ViewportSize.NoViewport // 移除 1280x720 限制
                }).Result;

                //输入框回调
                page.ExposeFunctionAsync("onLanguageSubmit", (string[] values) =>
                {
                    currentLanguageTcs?.TrySetResult(values);
                }).Wait();



                page.ExposeFunctionAsync<string>("callCSharp", async (tag) =>
                {
                    Console.WriteLine($"点击了按钮 {tag}");

                    switch (tag)
                    {
                        case "A":
                            {
                                try
                                {
                                    await page.WaitForSelectorAsync("input#project");
                                    bool isChecked = await page.EvaluateAsync<bool>("() => document.getElementById('project').checked");

                                    if (isChecked)
                                    {
                                        await page.EvaluateAsync(@"() => {
                                const cb = document.getElementById('project');
                                if (!cb) return;
                                cb.checked = false;
                                cb.dispatchEvent(new Event('change', { bubbles: true }));
                            }");
                                        Console.WriteLine("工程模式已关闭");
                                    }

                                    string code = await page.EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})();
");

                                    Console.WriteLine("codeArea 输入框内容：");
                                    Console.WriteLine(code);

                                    DataJson Json = new DataJson
                                    {
                                        Type = "EventEditor_Text",
                                        Text = code
                                    };

                                    new Program().Send("MiaoAicMod_Mod", JsonConvert.SerializeObject(Json, Formatting.Indented));

                                    await Task.Delay(500);
                                    BringToFront(Game_PID);

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"操作异常: {ex.Message}");
                                }



                                break;
                            }

                        case "B":
                            {
                                string code2 = "";
                                try
                                {
                                    await page.WaitForSelectorAsync("input#project");
                                    bool isChecked = await page.EvaluateAsync<bool>("() => document.getElementById('project').checked");

                                    if (isChecked)
                                    {
                                        await page.EvaluateAsync(@"() => {
                                const cb = document.getElementById('project');
                                if (!cb) return;
                                cb.checked = false;
                                cb.dispatchEvent(new Event('change', { bubbles: true }));
                            }");
                                        Console.WriteLine("工程模式已关闭");
                                    }

                                    string code = await page.EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})();
");

                                    Console.WriteLine("codeArea 输入框内容：");
                                    Console.WriteLine(code);
                                    code2 = code;


                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"操作异常: {ex.Message}");
                                }




                                currentLanguageTcs = new TaskCompletionSource<string[]>();

                                var result = await page.EvaluateAsync<dynamic>(@"
() => {
    const ws = Blockly.getMainWorkspace();
    if (!ws) return []; 
    const blocks = ws.getAllBlocks(false);
    return blocks.map(b => {
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
        return {
            type: b.type,
            id: b.id,
            values: fieldsData 
        };
    });
}
");
                                var blockList = (IEnumerable<dynamic>)result;
                                foreach (var block in blockList)
                                {
                                    // 判断是否是入口块
                                    if (block.type == "entrance")
                                    {
                                        // 访问 values 下的 String_0 和 Bool_0
                                        string eventId = block.values.String_0;
                                        string isExportRaw = block.values.Bool_0; // 注意：Blockly 复选框通常返回字符串 "TRUE" 或 "FALSE"
                                                                                  // 进行你的业务判断
                                        Console.WriteLine($"事件ID: {eventId}");
                                        ExportToUtf8($"{Game_directory}\\AliceInCradle_Data\\StreamingAssets\\evt\\{eventId}.cmd", code2);
                                        if (isExportRaw == "TRUE")
                                        {
                                            Console.WriteLine("对话单独导出已开启");
                                            Console.WriteLine("获取单独对话内容");

                                            await page.EvaluateAsync(@"
() => {
    const btn = document.querySelector('button[onclick=""compile()""]');
    if (btn) btn.click();
}
");

                                            string code = await page.EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})();
");

                                            Console.WriteLine("codeArea 输入框内容：");
                                            Console.WriteLine(code);

                                            Console.WriteLine("显示多语言编辑框。");
                                            var tcs = new TaskCompletionSource<string[]>();
                                            try
                                            {
                                                await page.ExposeFunctionAsync("onLanguageSubmit", (string[] values) =>
                                                {
                                                    tcs.TrySetResult(values);
                                                });
                                            }
                                            catch (Microsoft.Playwright.PlaywrightException ex) when (ex.Message.Contains("already registered"))
                                            {
                                            }
                                            await page.EvaluateAsync(@"(code_init) => {
    // 1. 清理旧弹窗
    const oldOverlay = document.getElementById('my-custom-overlay');
    if (oldOverlay) oldOverlay.remove();

    // 2. 配置字段
    const fieldsConfig = [
        { label: '英语 (English)', id: 'en' },
        { label: '韩语 (Korean)', id: 'ko' },
        { label: '泰语 (Thai)', id: 'th' },
        { label: '简体中文 (Simplified Chinese)', id: 'zh-cn' }, // Index 3
        { label: '繁体中文 (Traditional Chinese)', id: 'zh-tw' },
        { label: '日语 (Japanese)', id: 'ja' }
    ];

    // 初始化数据：所有语言默认值都设为传入的 code_init
    // 使用 String(code_init) 确保它是字符串，防止 undefined 报错
    const safeCode = code_init ? String(code_init) : '';
    const dataValues = fieldsConfig.map(() => safeCode);
    
    // 【修复点1】初始索引设为 -1，表示尚未选中任何语言
    // 这样 switchLanguage 函数第一次运行时，就不会尝试去“保存”空内容覆盖掉默认值
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

    function switchLanguage(newIndex) {
        if (activeIndex !== -1) {
            dataValues[activeIndex] = textarea.value;
        }

        // 更新索引
        activeIndex = newIndex;

        // 加载数据到文本框
        textarea.value = dataValues[activeIndex] || '';
        currentLabel.innerText = '正在编辑: ' + fieldsConfig[activeIndex].label;

        // 更新按钮样式
        buttons.forEach((btn, idx) => {
            if (idx === newIndex) {
                btn.style.cssText = navBtnStyle + navBtnActiveStyle;
            } else {
                btn.style.cssText = navBtnStyle;
            }
        });
        
        textarea.focus();
    }

    fieldsConfig.forEach((field, index) => {
        const btn = document.createElement('button');
        btn.innerText = field.label;
        btn.onclick = () => switchLanguage(index);
        sidebar.appendChild(btn);
        buttons.push(btn);
    });

    confirmBtn.onclick = () => {
        // 保存最后一次编辑的内容
        if (activeIndex !== -1) {
            dataValues[activeIndex] = textarea.value;
        }
        
        if (window.onLanguageSubmit) {
            window.onLanguageSubmit(dataValues); 
        }
        overlay.remove();
    };

    container.appendChild(sidebar);
    container.appendChild(contentArea);
    overlay.appendChild(container);
    document.body.appendChild(overlay);

    // 9. 触发默认选中 (简体中文 - Index 3)
    switchLanguage(3);

}", code);
                                            //获取结果并打印
                                            string[] userInputs = await currentLanguageTcs.Task;
                                            Console.WriteLine("\n捕获到的内容：");
                                            string[] labels = { "英语", "韩语", "泰语", "简中", "繁中", "日语" };
                                            string[] languagePath = { "en", "ko-kr", "th", "zh-cn", "zh-tc", "_" };
                                            for (int i = 0; i < userInputs.Length; i++)
                                            {
                                                Console.WriteLine($"[{labels[i]}] 内容长度: {userInputs[i].Length}");
                                                Console.WriteLine(userInputs[i]); // 打印具体内容


                                                ExportToUtf8($"{Game_directory}\\AliceInCradle_Data\\StreamingAssets\\localization\\{languagePath[i]}\\ev_{Path.GetFileName(eventId)}.txt", userInputs[i]);
                                                Console.WriteLine("-----------------------");
                                            }

                                        }
                                        else
                                        {
                                        }
                                        // 如果你只需要处理 entrance 块，可以在处理完后 break
                                        // break; 
                                    }
                                }
                                // 打印完整 JSON 供调试 (可选)
                                // Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                                break;
                            }

                        case "C":
                            {
                                //Console.WriteLine("执行 C 逻辑");

                                await page.WaitForSelectorAsync("input#project");
                                bool isChecked = await page.EvaluateAsync<bool>("() => document.getElementById('project').checked");

                                if (!isChecked)
                                {
                                    await page.EvaluateAsync(@"() => {
        const cb = document.getElementById('project');
        if (!cb) return;
        cb.checked = true; // 设置为 true
        // 触发 change 事件以确保网页监听到状态改变
        cb.dispatchEvent(new Event('change', { bubbles: true }));
    }");
                                    Console.WriteLine("工程模式已开启"); 
                                }

                                string code = await page.EvaluateAsync<string>(@"
(() => {
    const ta = document.getElementById('codeArea');
    return ta ? ta.value : '';
})();
");

                                Thread newThread = new Thread(() => {
                                    PromptAndSaveFile(code, "");
                                });
                                newThread.SetApartmentState(ApartmentState.STA); // 强制设置为 STA
                                newThread.Start();
                                break;
                            }
                        case "D":
                            {
                                DataJson Json = new DataJson
                                {
                                    Type = "Ping",
                                    Text = "来自MiaoAicMod_EventEditor"
                                };

                                new Program().Send("MiaoAicMod_Mod", JsonConvert.SerializeObject(Json, Formatting.Indented));

                                break;
                            }



                    }
                }).Wait();

                //主页面
                page.GotoAsync(EditorUrl).Wait();
                page.WaitForLoadStateAsync(LoadState.DOMContentLoaded).Wait();

                Console.WriteLine("启动完成");

                //注入 修改文字

                // 改原按钮文字
                page.EvaluateAsync(@"
(() => {
    const btn = document.querySelector('button[onclick*=""saveRaw(true)""]');
    if (btn) btn.textContent = '复制工程到剪切板';
})();
").Wait();

                // 注入按钮
                page.EvaluateAsync(@"
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
").Wait();

                //注入 css
                page.EvaluateAsync(@"
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
").Wait();

                //添加导入功能
                page.EvaluateAsync(@"
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
").Wait();

                // 修改缩放
                page.EvaluateAsync(@"
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
").Wait();

                Console.WriteLine("界面已插入");
                Console.WriteLine("点击回车关闭");
                Console.ReadLine();


                browser.CloseAsync().Wait();
            }
        }

        public class DataJson
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public int Pid { get; set; }
            public string Objective { get; set; }
            public string EditorUrl { get; set; }
            public string directory { get; set; }
        }
        /// <summary>
        /// 强制以 UTF-8 编码导出文件
        /// </summary>
        /// <param name="filePath">完整的保存路径 (例如: @"D:\Exports\English.txt")</param>
        /// <param name="content">要保存的字符串内容</param>
        public static void ExportToUtf8(string filePath, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("路径不能为空");

                // 获取目录信息并确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                // 使用 UTF-8 编码（不带 BOM）写入文件
                Encoding utf8 = new UTF8Encoding(false);

                File.WriteAllText(filePath, content, utf8);

                Console.WriteLine($"[成功] 文件已保存至: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[导出失败] 路径: {filePath}");
                Console.WriteLine($"错误原因: {ex.Message}");
            }
        }

        /// <summary>
        /// 弹出系统对话框并执行导出
        /// </summary>
        /// <param name="content">要保存的文本内容</param>
        public static void PromptAndSaveFile(string content,string FillName)
        {
            // 使用 using 确保资源释放
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "请选择保存位置";
                sfd.Filter = "json文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                //sfd.FileName = DateTime.Now.ToString("yyyyMMdd");
                sfd.FileName = FillName;

                // 在静态方法中，不能使用 this。
                // 直接调用 ShowDialog() 或者使用 Form.ActiveForm 寻找当前活动窗口
                if (sfd.ShowDialog(Form.ActiveForm) == DialogResult.OK)
                {
                    try
                    {
                        // 确保你的 ExportToUtf8 也是 static 的，否则这里也会报错
                        ExportToUtf8(sfd.FileName, content);
                        MessageBox.Show("导出成功！");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"报错了: {ex.Message}");
                    }
                }
            }
        }


        /// <summary>
        /// 根据进程 PID 激活并置顶窗口
        /// </summary>
        /// <param name="pid">进程的 ID</param>
        /// <returns>是否成功找到并尝试激活</returns>
        public static void BringToFront(int pid)
        {
            Process proc = Process.GetProcessById(pid);
            IntPtr handle = proc.MainWindowHandle;

            if (handle == IntPtr.Zero) return;

            ShowWindow(handle, SW_RESTORE);

            keybd_event(VK_MENU, 0, 0, 0);

            SetForegroundWindow(handle);

            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);
        }
    }



}
