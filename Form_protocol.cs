using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Windows;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
namespace AIC_XiaoMiaoICa_Mod_DLL
{
    public partial class Form_protocol : Form
    {
        #region 窗口效果
        // 常量定义
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const uint LWA_ALPHA = 0x2;

        // API声明 (32位/64位兼容)
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetLayeredWindowAttributes(
            IntPtr hwnd,
            uint crKey,
            byte bAlpha,
            uint dwFlags
        );
        #endregion

        public Form_protocol()
        {
            this.FormBorderStyle = FormBorderStyle.None; // 隐藏边框
            this.TopMost = true; // 始终置顶
            // 全屏颜色设置
            //this.BackColor = Color.White;

            InitializeComponent();

            // 获取主屏幕 长宽
            Screen primaryScreen = Screen.PrimaryScreen;
            Rectangle bounds = primaryScreen.Bounds;
            Console.WriteLine("主屏幕的宽度: " + bounds.Width);
            Console.WriteLine("主屏幕的高度: " + bounds.Height);

            

            label2.Text = "欧尼酱~这是~超级宇宙声明！";
            label3.Text = "Mod开源免费！禁止盗卖\n点击同意即可继续运行！";
            button1.Text = "同意使用协议";
            button4.Text = "开源地址";
            button3.Text = "不同意";

            #region 根据windows版本加载窗口效果
            var os = Environment.OSVersion;
            Console.WriteLine($"平台: {os.Platform}");
            Console.WriteLine($"版本: {os.Version}");
            var version = os.Version;
            if (version.Major == 10 && version.Build >= 22000)
            {
                Console.WriteLine("窗口效果:Windows 11");
                groupBox1.Location = new System.Drawing.Point((bounds.Width / 2) - (200), (bounds.Height / 2) - (150));
                this.FormBorderStyle = FormBorderStyle.None;
                //this.BackColor = Color.Magenta;  // 透明色需要与TransparencyKey匹配
                //this.TransparencyKey = Color.Magenta;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Size = new Size(400, 300);
                this.WindowState = FormWindowState.Maximized; // 最大化窗口

                // 应用透明效果
                ApplyModernEffects();
            }
            else if (version.Major == 10)
            {
                Console.WriteLine("窗口效果:Windows 10");
                groupBox1.Location = new System.Drawing.Point((bounds.Width / 2) - (200), (bounds.Height / 2) - (150));
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized; // 最大化窗口
                //this.BackColor = Color.Magenta;  // 透明色需要与TransparencyKey匹配
                //this.TransparencyKey = Color.Magenta;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Size = new Size(400, 300);

                // 应用透明效果
                ApplyModernEffects();
            }
            else if (version.Major == 6)
            {
                switch (version.Minor)
                {
                    case 1: Console.WriteLine("窗口效果:Windows 7"); break;
                    case 2: Console.WriteLine("窗口效果:Windows 8"); break;
                    case 3: Console.WriteLine("窗口效果:Windows 8.1"); break;
                }
            }
            #endregion



        }

        #region 窗口效果
        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (Environment.Is64BitProcess)
            {
                return GetWindowLong64(hWnd, nIndex);
            }
            return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        private void ApplyModernEffects()
        {
            try
            {
                // 获取当前扩展样式
                IntPtr extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);

                // 设置分层窗口样式
                int newStyle = (int)(extendedStyle.ToInt64() | WS_EX_LAYERED);
                if (SetWindowLong(this.Handle, GWL_EXSTYLE, newStyle) == 0)
                {
                    ThrowLastWin32Error();
                }

                // 设置透明度（255~0 不透明度）
                if (!SetLayeredWindowAttributes(this.Handle, 0, 200, LWA_ALPHA))
                {
                    ThrowLastWin32Error();
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"窗口特效初始化失败：\n错误代码：0x{ex.ErrorCode:X8}\n{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生未预期错误：{ex.Message}");
            }
        }

        private void ThrowLastWin32Error()
        {
            int lastError = Marshal.GetLastWin32Error();
            throw new Win32Exception(lastError, "Windows API调用失败");
        }

        // 可选：添加控件时的特殊处理
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            // 防止子控件继承透明背景
            e.Control.BackColor = Color.Transparent;
        }
        #endregion




        private void Form1_Load(object sender, EventArgs e)
        {

        }


 

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/MiaoluoYuanlina/AliceinCradle_BepInEx_XiaoMiaoICa-Mod") { UseShellExecute = true });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\user_agreement", "user_agreement", "true");
            this.Close();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 定义要执行的命令
            string command = "echo msgbox \"不同意使用协议将无法进行游戏，或者选择卸载mod！\", 48, \"欧尼酱，不同意将不能使用mod~喵~\" > warning.vbs && cscript //nologo warning.vbs && del warning.vbs";

            // 创建 ProcessStartInfo 对象
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe", // 指定要运行的程序（CMD）
                Arguments = $"/c {command}", // /c 表示执行完命令后关闭 CMD
                UseShellExecute = true, // 使用系统 Shell 执行
                CreateNoWindow = false // 显示 CMD 窗口
            };

            // 创建 Process 对象
            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;

                // 启动进程
                process.Start();
            }


            Process.GetCurrentProcess().Kill();
        }
    }
}
