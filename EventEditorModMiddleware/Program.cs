using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Remoting.Channels;
using System.Text.Json;
using System.Threading.Tasks;

class EventEditorModMiddleware
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Start("chrome");
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
                        Task.Run(() =>
                        {
                            Console.WriteLine("启动浏览器：" + Json.Objective);
                            Start(Json.Objective);
                        });
                    }

                    var resp = new ResponseDto
                    {
                        Success = true,
                        Message = "处理完成"
                    };

                    // ★ 写回时也要防断
                    if (server.IsConnected)
                    {
                        writer.WriteLine(System.Text.Json.JsonSerializer.Serialize(resp));
                    }
                }
                catch (IOException)
                {
                    // ★ 管道断开 = 正常情况
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

        static void Start(string Channel)
        {
            using (var playwright = Playwright.CreateAsync().Result)
            {
                var browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    Channel = Channel
                }).Result;

                var page = browser.NewPageAsync().Result;

                page.ExposeFunctionAsync<string>("callCSharp", async (tag) =>
                {
                    Console.WriteLine($"点击了按钮 {tag}");

                    switch (tag)
                    {
                        case "A":
                            try
                            {
                                // 1. 使用 await 等待元素
                                await page.WaitForSelectorAsync("input#project");

                                // 2. 使用 await 获取状态
                                bool isChecked = await page.EvaluateAsync<bool>("() => document.getElementById('project').checked");

                                if (isChecked)
                                {
                                    // 3. 使用 await 执行 JS
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
                            var result = await page.EvaluateAsync<dynamic>(@"
() => {
    const ws = Blockly.getMainWorkspace();
    if (!ws) return null;
    const blocks = ws.getAllBlocks(false);
    return blocks.map(b => ({
        type: b.type,
        id: b.id
    }));
}
");

                            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

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
                page.GotoAsync("https://api.ica.wiki/AIC/EventEditor/").Wait();
                page.WaitForLoadStateAsync(LoadState.DOMContentLoaded).Wait();



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
        }

    }



}
