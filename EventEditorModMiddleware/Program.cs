using EventEditorUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        // UI thread synchronization context for marshaling form creation
        private static SynchronizationContext _uiSyncContext;
        // Each editor window owns its own WebView and editing state.
        private static readonly List<webView> _openForms = new List<webView>();
        private static int _nextWindowId;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Explicitly create WindowsFormsSynchronizationContext before Application.Run
            // (SynchronizationContext.Current is null until the message loop starts)
            _uiSyncContext = new WindowsFormsSynchronizationContext();

            // Start the named pipe server on a background thread
            var program = new Program();
            Task.Run(() => program.Receive("MiaoAicMod_EventEditor"));

            Console.WriteLine("EventEditorModMiddleware 启动完成，等待连接...");

            // Run the WinForms message loop (blocks until Application.Exit())
            Application.Run();

            Console.WriteLine("EventEditorModMiddleware 已退出");
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

        /// <summary>
        /// Send a message to the game mod via named pipe (client side).
        /// Made static so it can be called from webView.cs.
        /// </summary>
        public static bool Send(string Objective, string text)
        {
            try
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
            catch (TimeoutException)
            {
                Console.WriteLine("发送失败：连接超时 (pipe: " + Objective + ")");
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine("发送失败：Pipe is broken. (" + ex.Message + ")");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送失败：" + ex.Message);
                return false;
            }
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
                        string gameDirectory = Json.directory;
                        int gamePid = Json.Pid;
                        Console.WriteLine("#XiaoMiaoICa: Game_PID:" + gamePid);
                        Console.WriteLine("#XiaoMiaoICa: Game_directory:" + gameDirectory);

                        // Marshal the form creation to the UI thread
                        string editorUrl = Json.EditorUrl;
                        _uiSyncContext.Post(_ =>
                        {
                            StartForm(editorUrl, gameDirectory, gamePid);
                        }, null);
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

        /// <summary>
        /// Create and show the WebView2 form on the UI thread.
        /// Keep existing editors open so each window can edit a different project.
        /// </summary>
        public static void StartForm(string editorUrl, string gameDirectory, int gamePid)
        {
            var form = new webView(editorUrl, gameDirectory, gamePid);
            form.Text += " - 窗口 " + (++_nextWindowId);
            _openForms.Add(form);
            form.FormClosed += (s, e) =>
            {
                _openForms.Remove(form);
                Console.WriteLine("WebView 表单已关闭，剩余窗口: " + _openForms.Count);
            };
            form.Show();
            Console.WriteLine("WebView 表单已打开，当前窗口: " + _openForms.Count);
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
        public static void PromptAndSaveFile(string content, string FillName)
        {
            // 使用 using 确保资源释放
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "请选择保存位置";
                sfd.Filter = "json文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                sfd.FileName = FillName;

                if (sfd.ShowDialog(Form.ActiveForm) == DialogResult.OK)
                {
                    try
                    {
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
