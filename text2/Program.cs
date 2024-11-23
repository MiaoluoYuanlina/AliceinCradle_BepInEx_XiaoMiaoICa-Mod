using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;
    private const int VK_F7 = 0x76;

    static void Main()
    {
        IntPtr hWnd = FindWindow(null, "游戏窗口名称"); // 替换为游戏窗口的名称
        if (hWnd != IntPtr.Zero)
        {
            PostMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_F7, IntPtr.Zero);
            System.Threading.Thread.Sleep(500); // 延时 500 毫秒
            PostMessage(hWnd, WM_KEYUP, (IntPtr)VK_F7, IntPtr.Zero);
        }
        else
        {
            Console.WriteLine("找不到游戏窗口");
        }
    }
}
