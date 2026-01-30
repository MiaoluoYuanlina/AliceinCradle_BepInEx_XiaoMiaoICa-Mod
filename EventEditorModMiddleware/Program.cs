using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class EventEditorModMiddleware
{
    internal class Program
    {
        static int Game_PID = 0;
        static string Game_directory = "";
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
                }).Result;

                var page = browser.NewPageAsync().Result;

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

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"操作异常: {ex.Message}");
                            }



                            break;

                        case "B":
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
                                        await page.EvaluateAsync(@"(code_en) => {
    // 1. 如果页面上已经存在旧的弹窗，先移除它
    const oldOverlay = document.getElementById('my-custom-overlay');
    if (oldOverlay) oldOverlay.remove();
    // 2. 样式定义
    const containerStyle = 'background-color: white; padding: 25px; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.3); display: flex; flex-direction: column; gap: 15px; width: 600px; max-height: 90vh; overflow-y: auto; font-family: sans-serif;';
    const textareaStyle = 'padding: 8px; width: 100%; height: 80px; font-family: monospace; resize: vertical; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;';
    // 3. 配置字段与默认值
    const fieldsConfig = [
        { label: '英语 (English)', val: code_en },
        { label: '韩语 (Korean)', val: code_en },
        { label: '泰语 (Thai)', val: code_en },
        { label: '简体中文 (Simplified Chinese)', val: code_en },
        { label: '繁体中文 (Traditional Chinese)', val: code_en },
        { label: '日语 (Japanese)', val: code_en }
    ];
    // 4. 创建遮罩层
    const overlay = document.createElement('div');
    overlay.id = 'my-custom-overlay';
    overlay.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 99999; display: flex; justify-content: center; align-items: center;';
    const container = document.createElement('div');
    container.style.cssText = containerStyle;
    const textareas = [];
    // 5. 循环生成控件
    fieldsConfig.forEach((field) => {
        const row = document.createElement('div');
        row.style.display = 'flex';
        row.style.flexDirection = 'column';
        row.style.gap = '5px';
        const label = document.createElement('label');
        label.innerText = field.label + ':';
        label.style.fontWeight = 'bold';
        const textarea = document.createElement('textarea');
        textarea.style.cssText = textareaStyle;
        // --- 【修复点】将默认内容赋值给输入框 ---
        textarea.value = field.val; 
        // 可选：添加占位提示
        textarea.placeholder = '请输入' + field.label + '内容...';
        textareas.push(textarea);
        row.appendChild(label);
        row.appendChild(textarea);
        container.appendChild(row);
    });
    // 6. 确认按钮
    const btn = document.createElement('button');
    btn.innerText = '确认保存';
    btn.style.cssText = 'margin-top: 10px; padding: 12px; cursor: pointer; background-color: #2196F3; color: white; border: none; border-radius: 4px; font-size: 16px; font-weight: bold;';
    btn.onclick = () => {
        const resultValues = textareas.map(t => t.value);
        if (window.onLanguageSubmit) {
            window.onLanguageSubmit(resultValues); 
        }
        overlay.remove();
    };
    container.appendChild(btn);
    overlay.appendChild(container);
    document.body.appendChild(overlay);
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


                                            ExportToUtf8( $"{Game_directory}\\AliceInCradle_Data\\StreamingAssets\\localization\\{languagePath[i]}\\ev_{Path.GetFileName(eventId)}.txt", userInputs[i]);
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



                        case "C":
                            Console.WriteLine("执行 C 逻辑");
                            DataJson json = new DataJson
                            {
                                Type = "EventEditor_Ping"
                            };

                            string payload = JsonConvert.SerializeObject(json, Formatting.Indented);

                            new Program().Send("MiaoAicMod_Mod", payload);



                            break;
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

                // 再注入你自己的按钮
                page.EvaluateAsync(@"
(() => {
    // 找到包含“导出对话”的那一行 p
    const targetP = Array.from(document.querySelectorAll('p'))
        .find(p => p.innerText.includes('导出对话'));

    if (!targetP) return;

    // 防止重复注入
    if (document.getElementById('__csharp_btn_A__')) return;

    // 新建一个 p（第二行）
    const newP = document.createElement('p');
    newP.id = '__csharp_toolbar__';

    // 创建按钮的工厂
    const makeBtn = (id, text, tag) => {
        const btn = document.createElement('button');
        btn.id = id;
        btn.textContent = text;
        btn.onclick = () => window.callCSharp(tag);
        return btn;
    };

    // 加入你的按钮
    newP.appendChild(makeBtn('__csharp_btn_A__', '执行', 'A'));
    newP.appendChild(makeBtn('__csharp_btn_B__', '保存工程到指定文件夹', 'B'));
    newP.appendChild(makeBtn('__csharp_btn_C__', '复制项目并保存到游戏', 'C'));

    // 插入到 targetP 的下一行（第二行）
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

    }



}
