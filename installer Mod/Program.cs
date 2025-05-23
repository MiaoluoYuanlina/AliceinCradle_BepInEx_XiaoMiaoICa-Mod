﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Http;
using System.Net;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Security.Policy;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;




namespace installer_Mod
{
    internal class Program
    {
        #region 常量
        readonly string OFFLINE_MOD_MOD5 = "b8ce560c83a5aefdc00d6e2ca35ee763";
        readonly string OFFLINE_BPEEX_MOD5 = "d42de011d504ea560cbb940318403489";
        readonly string ONLINE_DOWNLOAD_URL_BEPEX = "https://builds.bepinex.dev/projects/bepinex_be/571/BepInEx_UnityMono_x64_3a54f7e_6.0.0-be.571.zip";
        readonly string ONLINE_MD5_BEPEX = "d42de011d504ea560cbb940318403489";
        readonly string ONLINE_DOWNLOAD_URL_MOD_DOWNLOADTEXT = "http://miaoluoyuanlina.github.io/AIC/Mod/Latest_version_URL.txt";
        readonly string ONLINE_MD5_URL_MOD = "http://miaoluoyuanlina.github.io/AIC/Mod/MD5.txt";
        #endregion

        static string LOG_save = "";//日志
        static bool abort(int return_int)//结束程序
        {
            DeletePath(Directory.GetCurrentDirectory() + "/Temp");//删除Temp文件夹
            string LogID = "-1";
            if (return_int != 0)
            {
                LogID = GetUrlTxt("https://api.xiaomiao-ica.top/AIC/log/exe/ID_error/?gain=");
                if (return_int == 1)
                {
                    MessageBox.Show("无法链接至主要服务器!\n你可以尝试更改DNS服务器在尝试运行本程序.", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    WriteLine_color("无法链接至主要服务器!\n你可以尝试更改DNS服务器在尝试运行本程序.", ConsoleColor.Red);
                }
                else if (return_int == 2)
                {
                    WriteLine_color("无法链接至主要服务器!\n你可以尝试更改DNS服务器在尝试运行本程序.", ConsoleColor.Red);
                    MessageBox.Show("无法链接至主要服务器!\n你可以尝试更改DNS服务器在尝试运行本程序.", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 3)
                {
                    WriteLine_color("BepExMD5哈希验证失败。\n这可能是在下载文件时出现了网络波动等情况,你可以尝试重新运行本程序.", ConsoleColor.Red);
                    MessageBox.Show("BepExMD5哈希验证失败。\n这可能是在下载文件时出现了网络波动等情况,你可以尝试重新运行本程序.", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 4)
                {
                    WriteLine_color("ModMD5哈希验证失败。\n这可能是在下载文件时出现了网络波动等情况,你可以尝试重新运行本程序.", ConsoleColor.Red);
                    MessageBox.Show("ModMD5哈希验证失败。\n这可能是在下载文件时出现了网络波动等情况,你可以尝试重新运行本程序.", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 5)
                {
                    WriteLine_color("游戏路径包含中文!BepEx不支持中文路径!\n请将游戏复制到没有中文的目录!", ConsoleColor.Red);
                    MessageBox.Show("游戏路径包含中文!BepEx不支持中文路径!\n请将游戏复制到没有中文的目录!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 6)
                {
                    WriteLine_color("获取MD5验证值失败!", ConsoleColor.Red);
                    MessageBox.Show("获取MD5验证值失败!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 7) 
                {
                    WriteLine_color("获取Mod下载链接失败!", ConsoleColor.Red);
                    MessageBox.Show("获取Mod下载链接失败!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 8)
                {
                    WriteLine_color("解析GitHub下载地址失败!", ConsoleColor.Red);
                    MessageBox.Show("解析GitHub下载地址失败!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 9)
                {
                    WriteLine_color("下载游戏本体失败！", ConsoleColor.Red);
                    MessageBox.Show("下载游戏本体失败！", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 10)
                {
                    WriteLine_color("解压游戏本体失败！", ConsoleColor.Red);
                    MessageBox.Show("解压游戏本体失败！", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 11)
                {
                    WriteLine_color("BepExMD5导出失败。\n这可能是文件被某些其他程序占用", ConsoleColor.Red);
                    MessageBox.Show("BepExMD5导出失败。\n这可能是文件被某些其他程序占用", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 12)
                {
                    WriteLine_color("MOD导出失败。\n这可能是文件被某些其他程序占用", ConsoleColor.Red);
                    MessageBox.Show("MOD导出失败。\n这可能是文件被某些其他程序占用", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 13)
                {
                    WriteLine_color("下载游戏失败！", ConsoleColor.Red);
                    MessageBox.Show("可能是由于官方更新了新版本，而脚本还没更新。\n请尝试自行下载游戏本体，或者等待更新！\n游戏官网 https://cn.aliceincradle.dev", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (return_int == 14)
                {
                    WriteLine_color("未知错误!", ConsoleColor.Red);
                    MessageBox.Show("未知错误!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    WriteLine_color("未知错误!", ConsoleColor.Red);
                    MessageBox.Show("未知错误!", "欧尼酱~出错啦~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (MessageBox.Show("你在运行本程序时出现了问题!\n你可以尝试联系本苗!本苗会帮助你解决一些问题~\n记得带上你的终端截图!要不从雨来了也帮不了你啦~\n\n是否要联系本苗?", "欧尼酱~你想要帮助吗?", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://xiaomiao-ica.top") { UseShellExecute = true });
                }

            }
            else
            {
                LogID = GetUrlTxt("https://api.xiaomiao-ica.top/AIC/log/exe/ID_normalcy/?gain=");
            }
            Thread.Sleep(100);
            WriteLine_color("你的本次运行被分配了ID:"+LogID, ConsoleColor.Blue);
            SendDataToServer(LOG_save, return_int.ToString());
            Thread.Sleep(1000);
            //WriteLine_color("运行结束！点击任意键结束程序!", ConsoleColor.Green);
            //Console.ReadKey();//等待点击
            Environment.Exit(return_int);
            return true;
        }
        static int Name_Get_PID(string processName) // 获取进程的PID
        {
            try
            {
                // 获取匹配的进程
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    //Console.WriteLine($"未找到名为 \"{processName}\" 的进程。");
                    return 0; // 返回0表示未找到进程
                }
                else if (processes.Length > 1)
                {
                    //Console.WriteLine($"找到多个名为 \"{processName}\" 的进程，请确认唯一性。");
                    return 0; // 返回0表示找到多个进程
                }

                // 返回唯一进程的 PID
                return processes[0].Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"方法 Name_Get_PID:获取进程 PID 时出错：{ex.Message}");
                return 0; // 出错时返回0
            }
        }
        static string Pid_Get_Path(int pid)//获取进程的路径
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                return process.MainModule.FileName;
            }
            catch (ArgumentException)
            {
                //Console.WriteLine("方法 Kill_Pid:指定 PID 的进程不存在。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"方法 Kill_Pid:错误: {ex.Message}");
            }

            return null;  // 返回 null 如果获取路径失败
        }
        static void Kill_Pid(int pid)//通过PID结束进程
        {
            try
            {
                // 获取指定 PID 的进程
                Process process = Process.GetProcessById(pid);

                // 调用 Kill 方法终止进程
                process.Kill();
                //Console.WriteLine($"方法 Kill_Pid:成功终止 PID 为 {pid} 的进程。");
            }
            catch (Exception ex)
            {
                // 捕获并显示异常信息
                Console.WriteLine($"方法 Kill_Pid:终止 PID 为 {pid} 的进程时出错：{ex.Message}");
            }
        }
        static string Get_Parent_Directory(string filePath)//获取文件路径的父目录
        {
            try
            {
                // 获取父目录路径
                string parentDirectory = Directory.GetParent(filePath)?.FullName;

                if (parentDirectory == null)
                {
                    //throw new Exception("方法 Get_Parent_Directory:无法获取父目录路径。");
                }

                return parentDirectory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"方法 Get_Parent_Directory:错误: {ex.Message}");
                return string.Empty;
            }
        }
        static async Task<double> GetUrlResponseTimeAsync(string url)//获取 URL 的响应时间
        {
            try
            {
                // 创建 HttpClient 实例
                using (HttpClient client = new HttpClient())
                {
                    // 记录请求开始时间
                    var startTime = DateTime.Now;

                    // 发送请求并获取响应
                    HttpResponseMessage response = await client.GetAsync(url);

                    // 记录响应时间
                    var responseTime = DateTime.Now - startTime;

                    // 返回响应时间（毫秒）
                    return responseTime.TotalMilliseconds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"请求 URL 时出错: {ex.Message}");
                return -1;  // 返回-1表示发生了错误
            }
        }
        static int WriteLine_color(string text , ConsoleColor color)//输出带颜色的文字
        {
            LOG_save = LOG_save + text + "\n";
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            return 0;
        }
        static string GetUrlTxt(string url)//获取文本文件内容
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // 获取文本文件内容
                    string content = client.DownloadString(url);
                    return content;
                }
            }
            catch (Exception ex)
            {
                return $"发生错误：{ex.Message}";
            }
        }
        static void ExtractZipWithProgress(string zipFilePath, string extractPath)//解压文件
        {
            try
            {
                using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zipFilePath))
                {
                    long totalEntries = archive.Entries.Count;
                    long currentEntry = 0;

                    foreach (var entry in archive.Entries)
                    {
                        currentEntry++;
                        string destinationPath = Path.Combine(extractPath, entry.FullName);

                        // 检查并创建目录
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }

                        // 显示解压进度
                        Console.Write($"\r解压进度: {currentEntry}/{totalEntries} ({(currentEntry * 100) / totalEntries}%)");
                    }
                }

                Console.WriteLine("\n解压完成!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n解压失败: {ex.Message}");
            }
        }
        static void DownloadFile(string fileUrl, string savePath)//下载文件
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        // 格式化单位显示
                        string received = FormatSize(e.BytesReceived);
                        string total = e.TotalBytesToReceive > 0 ? FormatSize(e.TotalBytesToReceive) : "未知大小";

                        Console.Write($"\r下载进度: {e.ProgressPercentage}% ({received} / {total})");
                    };

                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        Console.WriteLine("\n下载完成!");
                    };

                    // 开始下载文件
                    client.DownloadFileAsync(new Uri(fileUrl), savePath);

                    // 防止程序过早退出，等待下载完成
                    //Console.WriteLine("正在下载，请稍候...");
                    while (client.IsBusy)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n下载失败: {ex.Message}");
            }
        }
        private static string FormatSize(long bytes)//格式化文件大小格式
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;
            const long TB = GB * 1024;

            if (bytes >= TB)
                return $"{bytes / (double)TB:F2} TB";
            if (bytes >= GB)
                return $"{bytes / (double)GB:F2} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:F2} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:F2} KB";
            return $"{bytes} B";
        }
        static void CreatePath(string path)//创建目录
        {
            try
            {
                Directory.CreateDirectory(path);
                //Console.WriteLine($"目录已成功创建: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建目录时发生错误: {ex.Message}");
            }
        }
        static void DeletePath(string folderPath)//删除目录
        {
            // 判断文件夹是否存在
            if (Directory.Exists(folderPath))
            {
                // 删除文件夹中的所有文件
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    // 删除单个文件
                    System.IO.File.Delete(file);
                }
                // 删除文件夹中的所有子文件夹
                foreach (var subDirectory in Directory.GetDirectories(folderPath))
                {
                    // 递归调用删除子文件夹
                    DeletePath(subDirectory);
                }
                // 删除文件夹本身
                Directory.Delete(folderPath);
            }
            else
            {
                // 如果文件夹不存在，输出提示信息
                Console.WriteLine("指定的文件夹不存在.");
            }
        }
        static string GetFileMd5(string filePath)//获取文件MD5值
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("指定的文件不存在", filePath);

            using (var md5 = MD5.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }
        static string RemoveEmptyLines(string input)//移除文本中的空行
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 按行拆分，去掉空白行后重新拼接
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, lines);
        }
        static bool ContainsCJKCharacters(string input)//判断字符串是否包含中日韩字符
        {
            // 正则表达式，匹配中日韩字符的Unicode范围
            string pattern = @"[\u4e00-\u9fff\u3040-\u309f\u30a0-\u30ff\uac00-\ud7af]";

            // 使用正则表达式进行匹配
            return Regex.IsMatch(input, pattern);
        }
        static async Task SendDataToServer(string text, string data) // 发送数据到服务器
        {
            Console.WriteLine($"尝试上传日志...");
            string url = "https://api.xiaomiao-ica.top/AIC/log/index.php";  // PHP文件URL

            // 构造POST请求的数据
            var payload = new Dictionary<string, string>
            {
                { "text", text },
                { "data", data }
            };

            using (var client = new HttpClient())
            {
                try
                {
                    // 设置 HttpClient 的配置，忽略 SSL 证书验证
                    client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                    // 发送 POST 请求
                    var content = new FormUrlEncodedContent(payload);
                    var response = await client.PostAsync(url, content);

                    // 确保响应成功
                    response.EnsureSuccessStatusCode();

                    // 获取响应内容
                    string responseText = await response.Content.ReadAsStringAsync();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(responseText);  // 输出服务器响应内容
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"请求出错: {e.Message}");
                    Console.ResetColor();
                }
            }
        }
        static int GetStringWidth(string str)// 计算字符串的宽度
        {
            int width = 0;
            foreach (char c in str)
            {
                // 中文字符宽度为 2，其他字符宽度为 1
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter ||
                    char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol)
                {
                    width += 2;  // 中文、符号等宽度为2
                }
                else
                {
                    width += 1;  // 英文字符宽度为1
                }
            }
            return width;
        }
        static string GetGitHubDownloadUrl(string url)//获取GitHub下载链接
        {
            using (HttpClient client = new HttpClient())
            {
                // 设置请求头
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
                client.DefaultRequestHeaders.Add("User-Agent", "C# App"); // GitHub API 需要 User-Agent

                try
                {
                    // 发送 GET 请求
                    HttpResponseMessage response = client.GetAsync(url).Result; 

                    // 如果响应成功，返回最终的 URL
                    if (response.IsSuccessStatusCode)
                    {
                        return response.RequestMessage.RequestUri.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return null; // 如果失败，返回 null
        }
        static void CreateShortcut(string targetPath, string shortcutPath)//创建快捷方式
        {
            // 创建 WScript.Shell 对象
            var wshShell = new WshShell();

            // 创建快捷方式对象
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcutPath);

            // 设置快捷方式属性
            shortcut.TargetPath = targetPath; // 目标路径
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath); // 工作目录
            shortcut.Description = ""; // 快捷方式描述
            shortcut.IconLocation = targetPath + ", 0"; // 设置快捷方式图标（使用程序的图标）

            // 保存快捷方式
            shortcut.Save();
        }
        static bool ExportEmbedResources(string FileName, string Path) // 导出嵌入资源
        {
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 构造嵌入资源的完整名称
                string resourceName = $"installer_Mod.{FileName}";

                // 检查资源是否存在
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        Console.WriteLine($"嵌入资源未找到：{resourceName}");
                        Console.WriteLine($"资源加载失败，可用资源列表:\n{string.Join("\n", assembly.GetManifestResourceNames())}");
                        return false;
                    }

                    // 将嵌入的资源导出到文件系统
                    using (FileStream fileStream = new FileStream(Path, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    Console.WriteLine($"资源已成功导出到: {Path}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出失败: {ex.Message}");
                return false;
            }
        }


        static async Task Main(string[] args)//主
        {
            #region Text
            if (false)
            {
                abort(999);

            }

            #endregion
            #region 变量定义
            Program program = new Program();
            #endregion
            #region 启动参数
            bool start_DownloadGame = false;
            string start_Path = "";
            bool start_ExportEmbed_Mod = false;
            bool start_ExportEmbed_BepEx = false;
            string DownloadGameFileName = "CN_HgV1znzi_Win_ver027h.zip";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--Path")//路径
                {
                    start_Path = args[i + 1];
                    i++;
                }
                else if (args[i] == "-DownloadGame")//下载游戏
                {
                    start_DownloadGame = true;
                }
                else if (args[i] == "-ExportEmbed_Mod")//导出切入资源
                {
                    start_ExportEmbed_Mod = true;
                }
                else if (args[i] == "-ExportEmbed_BepEx")//导出切入资源
                {
                    start_ExportEmbed_BepEx = true;

                } 
                else if (args[i] == "--DownloadGameFileName")
                {
                    DownloadGameFileName = args[i + 1];
                    i++;
                }

            }
            #endregion
            #region 声明
            string[] lines = {
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Py：XiaoMiao_ICa or 苗萝缘莉雫",
            "GPL-3.0 开源许可",
            "适用于 Alice In Cradle 的 bepinex 框架Mod一键安装程序",//10
            "理论适配游戏全部版本！",
            "本程序仅供学习参考 GitHub项目:repo:MiaoluoYuanlina/AliceinCradle_BepInEx_XiaoMiaoICa-Mod",
            "Ciallo～(∠・ω< )⌒☆​",
            "Mod及游戏本体都是免费的，如果你是购买而来，证明你被骗啦~",
            "本程序会收集你的日志来更好的维护，如果您不同意，请立即关闭此程序。",
            "",
            "MOD官网：https://xiaomiao-ica.top/2024/12/01/alice-in-cradle-bepinex-mod/",
            "GitHub项目:https://github.com/MiaoluoYuanlina/AliceinCradle_BepInEx_XiaoMiaoICa-Mod",
            "原游戏官网:https://aliceincradle.com/",
            "",//20
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""//30
            };
            ConsoleColor[] colors = {
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen,
            ConsoleColor.Blue,//10
            ConsoleColor.White,
            ConsoleColor.Yellow,
            ConsoleColor.White,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
            ConsoleColor.White,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkYellow,
            ConsoleColor.Magenta,
            ConsoleColor.White,//20
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White,
            ConsoleColor.White//30
            };
            // 获取终端窗口的宽度
            int consoleWidth = Console.WindowWidth;
            // 遍历每一行文本
            foreach (string line in lines)
            {
                int lineWidth = GetStringWidth(line);
                int startPosition = (consoleWidth - lineWidth) / 2;
                Console.ForegroundColor = colors[Array.IndexOf(lines, line)];
                Console.SetCursorPosition(startPosition, Console.CursorTop);
                Console.WriteLine(line);
                Console.ResetColor();
            }
            for (int i = 0; i < 5; i++)
            {

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(""+(5-i)+"秒后开始运行");
                Thread.Sleep(1000); // 等待
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("               ");
            for (int i = 0; i < 30; i++)
            {
                //Console.WriteLine("\n");
            }
            #endregion
            #region 下载游戏
            WriteLine_color("\n程序运行目录" + Directory.GetCurrentDirectory(), ConsoleColor.Cyan);//显示程序运行目录
            CreatePath(Directory.GetCurrentDirectory() + "/Temp");//创建Temp文件夹
            if (start_DownloadGame == true)
            {
                WriteLine_color("尝试下载游戏本体......", ConsoleColor.Blue);
                //CN_HgV1znzi_Win_ver027h
                string URL_FILE_NAME = DownloadGameFileName;
                string GAME_V = "027";
                WriteLine_color("目前尝试下载版本:"+ URL_FILE_NAME, ConsoleColor.Blue);


                for (int i = 1; i <= 9; i++)
                {
                    WriteLine_color("尝试使用官方 CDN0" + i + " 进行下载。", ConsoleColor.Blue);
                    DownloadFile("https://minazuki.shiro.dev/d/CN0"+ i +"/AliceInCradle_Latest/" + URL_FILE_NAME, Directory.GetCurrentDirectory() + "/Temp/" + URL_FILE_NAME);
                    FileInfo fileInfo = new FileInfo(Directory.GetCurrentDirectory() + "/Temp/" + URL_FILE_NAME);
                    WriteLine_color("官方 CDN0" + i + " 不可用。", ConsoleColor.Yellow);


                    if (fileInfo.Length >= 1024 * 1024)
                    {
                        break;
                    }

                    string fileContent = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Temp", URL_FILE_NAME));
                    WriteLine_color("URL返回内容：", ConsoleColor.Yellow);
                    WriteLine_color(fileContent, ConsoleColor.Yellow);

                    if (i == 9)
                    {
                        abort(13);//无可用的 CDN 
                    }
                }

                



                if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "/Temp/"+ URL_FILE_NAME))
                {
                    WriteLine_color("下载成功！", ConsoleColor.Blue);
                }
                else
                {
                    abort(9);
                    WriteLine_color("下载游戏本体失败！", ConsoleColor.Red);
                }


                CreatePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/AppData/Local/Alice in Cradle");
                WriteLine_color("解压游戏......", ConsoleColor.Blue);
                ExtractZipWithProgress(Directory.GetCurrentDirectory() + "/Temp/"+ URL_FILE_NAME, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/AppData/Local/Alice in Cradle");
                start_Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/AppData/Local/Alice in Cradle/Win ver"+ GAME_V + "/AliceInCradle_ver" + GAME_V+ "/AliceInCradle.exe";
                if (System.IO.File.Exists(start_Path))
                {
                    WriteLine_color("解压成功！", ConsoleColor.Blue);
                }
                else
                {
                    abort(10);
                    WriteLine_color("解压游戏本体失败！", ConsoleColor.Red);
                }
                CreateShortcut(start_Path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/Alice In Cradle.lnk");
                CreateShortcut(start_Path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Alice In Cradle.lnk");
                
                WriteLine_color(start_Path, ConsoleColor.DarkCyan);
            }
            #endregion
            #region 获取进程
            int Game_pid = 0;
            int while_a = 49;
            string Gmae_Path_incomplete = "";
            string Gmae_Path = "";
            if (start_Path == "")
            {
                while (true)
                {
                    Game_pid = Name_Get_PID("AliceInCradle"); // 查找进程
                    if (Game_pid != 0)
                    {
                        Console.WriteLine("\n");
                        WriteLine_color("GamePID: " + Game_pid, ConsoleColor.Blue);
                        Gmae_Path_incomplete = Pid_Get_Path(Game_pid);
                        Gmae_Path = Get_Parent_Directory(Gmae_Path_incomplete);
                        WriteLine_color("GmaePath " + Gmae_Path_incomplete, ConsoleColor.Blue);
                        Kill_Pid(Game_pid); // 结束进程
                        break; // 找到进程，退出循环
                    }
                    while_a = while_a + 1;
                    if (while_a >= 30)
                    {
                        while_a = 0;
                        Console.SetCursorPosition(0, Console.CursorTop);

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"游戏未运行，请开启游戏！");
                        Console.ResetColor();
                    }
                    if (while_a == 20)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"游戏未运行，请开启游戏！");
                        Console.ResetColor();
                    }
                    if (while_a == 10)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write($"游戏未运行，请开启游戏！");
                        Console.ResetColor();
                    }
                    Thread.Sleep(100); // 等待
                }
                if (ContainsCJKCharacters(Gmae_Path) == true)
                {
                    abort(5);
                }
            }else{
                Gmae_Path_incomplete = start_Path;
                Gmae_Path = Get_Parent_Directory(Gmae_Path_incomplete);
                WriteLine_color("GamePID: " + Game_pid, ConsoleColor.Blue);
                WriteLine_color("GmaePath " + Gmae_Path_incomplete, ConsoleColor.Blue);
            }
            
            #endregion
            #region 动态获取URL
            WriteLine_color("检查本苗服务器可用性......", ConsoleColor.Blue);
            #region 测试延迟
            double URL_delay = 0;
            if (start_ExportEmbed_BepEx == false && start_ExportEmbed_Mod == false)
            {
                
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    double responseTime = await GetUrlResponseTimeAsync("https://api.xiaomiao-ica.top");
                    WriteLine_color("ping:" + responseTime + "ms", ConsoleColor.Blue);
                    if (responseTime >= 0)
                    {
                        URL_delay = URL_delay + responseTime;
                    }
                    else
                    {
                        abort(1);
                        break; // 发生错误时退出循环
                    }
                }
                if (URL_delay != -1)
                {
                    URL_delay = URL_delay / 3;
                }
                if (URL_delay < 2000 && URL_delay != 0)
                {
                    WriteLine_color("检测本苗服务器可用! 平均延迟：" + URL_delay, ConsoleColor.Blue);
                }
                else
                {
                    WriteLine_color("本苗服务器不可用或延迟过高! URL_delay:" + URL_delay, ConsoleColor.Red);
                    abort(2);
                }
            }
            string download_url_BepEx = program.ONLINE_DOWNLOAD_URL_BEPEX;
            string MD5_BepEx = program.ONLINE_MD5_BEPEX;
            string download_url_Mod_downloadText = program.ONLINE_DOWNLOAD_URL_MOD_DOWNLOADTEXT;
            string MD5_url_Mod = program.ONLINE_MD5_URL_MOD;
            string agentURL_Mod = "";
            if (start_ExportEmbed_BepEx == false)
            {
                URL_delay = 0;
                WriteLine_color("检查github官网可用性......", ConsoleColor.Blue);
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    double responseTime = await GetUrlResponseTimeAsync("https://miaoluoyuanlina.github.io");
                    WriteLine_color("ping:" + responseTime + "ms", ConsoleColor.Blue);
                    if (responseTime >= 0)
                    {
                        URL_delay = URL_delay + responseTime;
                    }
                    else
                    {
                        URL_delay = -1;
                        break; // 发生错误时退出循环
                    }
                }
                if (URL_delay != -1)
                {
                    URL_delay = URL_delay / 3;
                }
                if (URL_delay < 1500 && URL_delay != -1)
                {
                    WriteLine_color("github官网可用! 平均延迟：" + URL_delay, ConsoleColor.Blue);
                }
                else
                {
                    URL_delay = 0;
                    WriteLine_color("github官网不可用或延迟过高! URL_delay:" + URL_delay, ConsoleColor.Yellow);
                    WriteLine_color("改用代理Url", ConsoleColor.Yellow);
                    agentURL_Mod = "https://api.xiaomiao-ica.top/agent/index.php?fileUrl=";
                    download_url_Mod_downloadText = agentURL_Mod + download_url_Mod_downloadText;
                    MD5_url_Mod = agentURL_Mod + MD5_url_Mod;

                }
            }
            if (start_ExportEmbed_Mod == false)
            {
                URL_delay = 0;
                WriteLine_color("检查BepEx官网可用性......", ConsoleColor.Blue);
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    double responseTime = await GetUrlResponseTimeAsync("https://builds.bepinex.dev");
                    WriteLine_color("ping:" + responseTime + "ms", ConsoleColor.Blue);
                    if (responseTime >= 0)
                    {
                        URL_delay = URL_delay + responseTime;
                    }
                    else
                    {
                        URL_delay = 99999;
                        break; // 发生错误时退出循环
                    }
                }
                if (URL_delay != -1)
                {
                    URL_delay = URL_delay / 3;
                }
                if (URL_delay < 1500 && URL_delay != -1)
                {
                    WriteLine_color("BepEx官网可用! 平均延迟：" + URL_delay, ConsoleColor.Blue);
                }
                else
                {
                    WriteLine_color("BepEx官网不可用或延迟过高! URL_delay:" + URL_delay, ConsoleColor.Yellow);
                    WriteLine_color("改用代理Url", ConsoleColor.Yellow);
                    download_url_BepEx = "https://api.xiaomiao-ica.top/agent/index.php?fileUrl=" + download_url_BepEx;
                }
            }
            #endregion
            #region URL获取
            string MD5_Mod = "";
            string download_url_Mod = "";
            if (start_ExportEmbed_Mod == false)
            {
                for (int i = 0; i < 3; i++)
                {
                    MD5_Mod = GetUrlTxt(MD5_url_Mod);
                    //Console.WriteLine(MD5_Mod.Contains("发生错误："));
                    if (MD5_Mod.Contains("发生错误：") == false)
                    {
                        break;
                    }
                    if (i == 2)
                    {
                        WriteLine_color("动态获取MD5失败", ConsoleColor.Red);
                        abort(6);
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    download_url_Mod = agentURL_Mod + GetUrlTxt(download_url_Mod_downloadText);
                    download_url_Mod = GetUrlTxt(download_url_Mod_downloadText);
                    //Console.WriteLine(MD5_Mod.Contains("发生错误："));
                    if (download_url_Mod.Contains("发生错误：") == false)
                    {
                        break;
                    }
                    if (i == 2)
                    {
                        WriteLine_color("获取Mod下载链接失败！", ConsoleColor.Red);
                        abort(7);
                    }
                }
                MD5_Mod = RemoveEmptyLines(MD5_Mod);
                download_url_Mod = RemoveEmptyLines(download_url_Mod);
            }
            else
            {
                MD5_Mod = program.OFFLINE_MOD_MOD5;
                MD5_Mod = RemoveEmptyLines(MD5_Mod); 

            }
            if (start_ExportEmbed_BepEx == false) { } else
            {
                MD5_BepEx = program.OFFLINE_BPEEX_MOD5;
                MD5_BepEx = RemoveEmptyLines(MD5_BepEx);
            }
            WriteLine_color("分配的URL信息", ConsoleColor.Cyan);
            WriteLine_color("download_url_BepEx:" + download_url_BepEx, ConsoleColor.Cyan);
            WriteLine_color("MD5_BepEx:" + MD5_BepEx, ConsoleColor.Cyan);
            WriteLine_color("download_url_Mod:" + download_url_Mod, ConsoleColor.Cyan);
            WriteLine_color("MD5_Mod:" + MD5_Mod, ConsoleColor.Cyan);
            #endregion
            #endregion
            #region 安装mod
            if (start_ExportEmbed_BepEx == false)
            {
                DownloadFile(download_url_BepEx, Directory.GetCurrentDirectory() + "/Temp/BepInEx_UnityMono_x64.zip");//下载BepEx 
                string Downloaded_BepExMD5 = GetFileMd5(Directory.GetCurrentDirectory() + "/Temp/BepInEx_UnityMono_x64.zip");//获取下载BepEx文件的MD5
                WriteLine_color("下载文件的MD5哈希值:" + Downloaded_BepExMD5, ConsoleColor.Blue);
                if (string.Equals(MD5_BepEx, Downloaded_BepExMD5, StringComparison.OrdinalIgnoreCase))//判断MD5
                {
                    WriteLine_color("BepExMD5哈希验证成功。", ConsoleColor.Blue);
                }
                else
                {
                    abort(3);
                    WriteLine_color("BepExMD5哈希验证失败。", ConsoleColor.Red);
                }
            }//BepEx
            else
            {
                CreatePath(Directory.GetCurrentDirectory() + "/Temp");
                if (ExportEmbedResources("file.BepInEx_UnityMono_x64_3a54f7e_6.0.0-be.571.zip", Directory.GetCurrentDirectory() + "/Temp/BepInEx_UnityMono_x64.zip")) 
                {
                    WriteLine_color("BepExMD5导出成功！", ConsoleColor.Blue);
                }
                else
                {
                    WriteLine_color("BepExMD5导出失败。", ConsoleColor.Red);
                    abort(11);
                }
                string Downloaded_BepExMD5 = GetFileMd5(Directory.GetCurrentDirectory() + "/Temp/BepInEx_UnityMono_x64.zip");//获取下载BepEx文件的MD5
                WriteLine_color("导出文件的MD5哈希值:" + Downloaded_BepExMD5, ConsoleColor.Blue);
                if (string.Equals(MD5_BepEx, Downloaded_BepExMD5, StringComparison.OrdinalIgnoreCase))//判断MD5
                {
                    WriteLine_color("BepExMD5哈希验证成功。", ConsoleColor.Blue);
                }
                else
                {
                    abort(3);
                    WriteLine_color("BepExMD5哈希验证失败。", ConsoleColor.Red);
                }
            }
            ExtractZipWithProgress(Directory.GetCurrentDirectory() + "/Temp/BepInEx_UnityMono_x64.zip", Gmae_Path);//解压BepEx
            CreatePath(Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa");//创建BepEx的Mod文件夹
            if (start_ExportEmbed_Mod == false)
            {
                WriteLine_color("正在尝试解析的下载github地址......", ConsoleColor.Blue);
                string Git_download_url_Mod = GetGitHubDownloadUrl(download_url_Mod);
                //Console.WriteLine("A "+ download_url_Mod);
                //Console.WriteLine("B "+ Git_download_url_Mod);
                if (Git_download_url_Mod == null)
                {
                    abort(8);
                    WriteLine_color("解析GitHub链接失败", ConsoleColor.Red);
                }
                WriteLine_color("解析到的下载地址:" + Git_download_url_Mod, ConsoleColor.Blue);
                DownloadFile(Git_download_url_Mod, Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa/XiaoMiaoICa_AIC_Mod.dll");//下载Mod
                string Downloaded_ModMD5 = GetFileMd5(Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa/XiaoMiaoICa_AIC_Mod.dll");//获取下载mod文件的MD5
                WriteLine_color("下载文件的MD5哈希值:" + Downloaded_ModMD5, ConsoleColor.Blue);
                if (string.Equals(MD5_Mod, Downloaded_ModMD5, StringComparison.OrdinalIgnoreCase))//判断MD5
                {
                    WriteLine_color("ModMD5哈希验证成功。", ConsoleColor.Blue);
                }
                else
                {
                    abort(4);
                    WriteLine_color("ModMD5哈希验证失败。", ConsoleColor.Red);
                }
            }//MOD
            else
            {
                CreatePath(Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa");
                if (ExportEmbedResources("file.AliceInCradle_Miao_Mod_Dll.dll", Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa/XiaoMiaoICa_AIC_Mod.dll"))
                {
                    Console.WriteLine("MOD导出成功！");
                }
                else
                {
                    abort(12);
                    Console.WriteLine("MOD导出失败。");
                }
                string Downloaded_ModMD5 = GetFileMd5(Gmae_Path + "/BepInEx/plugins/XiaoMiao_ICa/XiaoMiaoICa_AIC_Mod.dll");//获取下载mod文件的MD5
                WriteLine_color("导出文件的MD5哈希值:" + Downloaded_ModMD5, ConsoleColor.Blue);
                
                if (string.Equals(RemoveEmptyLines(program.OFFLINE_MOD_MOD5), Downloaded_ModMD5, StringComparison.OrdinalIgnoreCase))//判断MD5
                {
                    WriteLine_color("ModMD5哈希验证成功。", ConsoleColor.Blue);
                }
                else
                {
                    abort(4);
                    WriteLine_color("ModMD5哈希验证失败。", ConsoleColor.Red);
                }
            } 
            #endregion
            Process.Start(Gmae_Path_incomplete);//启动游戏
            abort(0);//结束程序
        }
    }
}
