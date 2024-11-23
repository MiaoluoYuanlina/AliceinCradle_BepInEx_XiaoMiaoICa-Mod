using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security.Policy;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using nel;
using BepInEx.Logging;
using evt;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
///命名空间
namespace Alice_in_Cradle_XiaoMiaoICa_of_Mod
{
    //Mod信息
    [BepInPlugin("AliceinCradle.XiaoMiaoICa.Mod", "AliceinCradle.XiaoMiaoICa.Mod", "1.0.0")]
    //类
    public class XiaoMiaoICaMod : BaseUnityPlugin
    {

        [DllImport("user32.dll", SetLastError = true)]//模拟按键
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);//模拟按键

        /// <summary>
        /// 启动
        /// </summary>
        /// 
        string Game_directory = null;
        int Game_PID = 0;

        void Start()
        {
            //获取PID
            Process currentProcess = Process.GetCurrentProcess();
            Game_PID = currentProcess.Id;
        // 获取当前目录
        string currentDirectory = Directory.GetCurrentDirectory();
            Game_directory = currentDirectory;
            // 组合路径
            string path = Path.Combine(currentDirectory, "XiaoMiaoICa_Mod_Data");
            XM_log(path, 2);
            // 检查文件夹是否存在
            if (!Directory.Exists(path))
            {
                // 创建文件夹
                Directory.CreateDirectory(path);
            }

            const string Py = "XiaoMiao_ICa";

            UnityEngine.Debug.Log("#XiaoMiaoICa:Heloo World");
            Logger.LogInfo("#XiaoMiaoICa:Heloo World");
            UnityEngine.Debug.Log("#XiaoMiaoICa: Game_PID:"+ Game_PID);
            UnityEngine.Debug.Log("#XiaoMiaoICa: Game_directory:" + Game_directory);
            //Logger.LogInfo("\a");
            //Logger.LogInfo(@"\a");

            Harmony.CreateAndPatchAll(typeof(XiaoMiaoICaMod));

            string savedKey = PlayerPrefs.GetString("toggleKey", "F1");//快捷键
            toggleKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedKey);


        }

        #region GUI
        
        /// <summary>
        /// 可视化窗口
        /// </summary>
        #region 窗口_变量
        private Rect WindowsRect = new Rect(50, 50, 500, 400); // 主窗口
        private bool showWindow = true; // 控制窗口显示/隐藏的标志
        private KeyCode toggleKey = KeyCode.Tab; // 用户定义的快捷键

        private string GUI_Textstring = ""; // 输入框
        private bool GUI_TextBool = false; // 开关
        private float GUI_TextInt = 1; // 滑动条


        private string GUI_string_toggleKey = "目前快捷键:";//快捷键


        private string GUI_string_debuggingText = "下载适用用开发的Mod进游戏中。"; // 调试_提示

        private static bool GUI_Bool_BanMosaic = false; // 开关_禁用马赛克


        private static bool GUI_Bool_CurrentIiving = false; // 开关_当前生命值
        private float GUI_string_CurrentIiving = 100; // 输入框_当前生命值

        private static bool GUI_Bool_CurrentMagic = false; // 开关_当前生魔力
        private float GUI_string_CurrentMagic = 100; // 输入框_当前魔力


        private static bool GUI_Bool_CurrentMoney = false; // 开关_当前金币
        private float GUI_string_CurrentMoney = 1; // 输入框_当前金币

        private static bool GUI_Bool_Debug = false; // 开关_游戏自带调试
        private string GUI_string_Debug = ""; // 文字_游戏自带调试

        private Vector2 svPos; // 界面滑动条


        #endregion


        void OnGUI()
        {
            

            //WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:Hello World");
            if (showWindow)
            {
                WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:Hello World");


            }
        }

        public void WindowsFunc(int id)
        {

            // 添加控件
            GUIStyle style = new GUIStyle(GUI.skin.label);
            Color redColor = new Color32(255, 0, 0, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("本Mod包括游戏本体完全免费！为爱发电，如果你是购买而来，证明你被骗啦！", style); // 文字

            // 开始滚动视图
            svPos = GUILayout.BeginScrollView(svPos);

            #region 控件_尝试代码

            /*
            GUILayout.Label("Hello World"); // 文字

            if (GUILayout.Button("哦？按钮!")) // 按钮
            {
                XM_log("单击了按钮", 2);
            }

            TextBool = GUILayout.Toggle(TextBool, "开关"); // 开关

            TextInt = GUILayout.HorizontalSlider(TextInt, 1, 10); // 滑动条
            GUILayout.Label(TextInt.ToString());



            */
            /*
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("BiliBili")) // 按钮
            {
                XM_log("单击了按钮", 2);
            }
            if (GUILayout.Button("bilibili")) // 按钮
            {
                XM_log("单击了按钮", 2);
            }
            if (GUILayout.Button("QQ")) // 按钮
            {
                XM_log("单击了按钮", 2);
            }
            if (GUILayout.Button("X")) // 按钮
            {
                XM_log("单击了按钮", 2);
            }
            GUILayout.EndHorizontal();
            */
            #endregion

            #region 控件_mod功能


            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("设置窗口隐藏显示快捷键")) // 按钮
            {
                StartCoroutine(SetCustomKey());//调用函数
            }
            GUILayout.Label(GUI_string_toggleKey + toggleKey); // 文字
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUI_Bool_BanMosaic = GUILayout.Toggle(GUI_Bool_BanMosaic, "禁止马赛克生成");
            GUILayout.Label("有的部分是在游戏CG上就已经除了过了，所以不一定全部地方生效。"); // 文字
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("设置金币")) // 按钮
            {
                int textint = Mathf.RoundToInt(GUI_string_CurrentMoney); // 转换为整数
                Mod_Set_Money(textint); // 设置金币
            }
            GUILayout.Label(GUI_string_CurrentMoney.ToString());
            GUILayout.EndHorizontal();
            GUI_string_CurrentMoney = GUILayout.HorizontalSlider(GUI_string_CurrentMoney, 1, 10000);
            GUI_string_CurrentMoney = Mathf.RoundToInt(GUI_string_CurrentMoney);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            GUI_Bool_CurrentIiving = GUILayout.Toggle(GUI_Bool_CurrentIiving, "锁定生命值 目前无效功能，等等本苗更新吧~");
            GUILayout.Label(GUI_string_CurrentIiving.ToString());
            GUILayout.EndHorizontal();
            GUI_string_CurrentIiving = GUILayout.HorizontalSlider(GUI_string_CurrentIiving, 1, 1000);
            GUI_string_CurrentIiving = Mathf.RoundToInt(GUI_string_CurrentIiving);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            GUI_Bool_CurrentMagic = GUILayout.Toggle(GUI_Bool_CurrentMagic, "锁定魔力 目前无效功能，等等本苗更新吧~");
            GUILayout.Label(GUI_string_CurrentMagic.ToString());
            GUILayout.EndHorizontal();
            GUI_string_CurrentMagic = GUILayout.HorizontalSlider(GUI_string_CurrentMagic, 1, 1000);
            GUI_string_CurrentMagic = Mathf.RoundToInt(GUI_string_CurrentMagic);
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("使用游戏原版调试Debug");
            if (GUILayout.Button("启用该功能")) // 按钮
            {
                // 定义文件路径
                string filePath = $"{Game_directory}\\AliceInCradle_Data\\StreamingAssets\\_debug.txt";
                // 读取文件内容
                string content = File.ReadAllText(filePath);
                // 替换文本
                string modifiedContent = content.Replace("timestamp 0", "timestamp 1");
                modifiedContent = modifiedContent.Replace("announce 0", "announce 1");
                // 将修改后的内容写回文件
                File.WriteAllText(filePath, modifiedContent);
                XM_log("_debug.txt 文件内容已成功更新！",2);
                GUI_string_Debug = "你需要重启游戏，此功能才会生效！";

            }
            if (GUILayout.Button("关闭该功能")) // 按钮
            {
                // 定义文件路径
                string filePath = $"{Game_directory}\\AliceInCradle_Data\\StreamingAssets\\_debug.txt";
                // 读取文件内容
                string content = File.ReadAllText(filePath);
                // 替换文本
                string modifiedContent = content.Replace("timestamp 1", "timestamp 0");
                // 将修改后的内容写回文件
                File.WriteAllText(filePath, modifiedContent);
                XM_log("_debug.txt 文件内容已成功更新！", 1);
                GUI_string_Debug = "你需要重启游戏，此功能才会生效！";
            }
            //if (GUILayout.Button("打开/关闭 GUI F7")) // 按钮
            //{
            //    strikeF7();
            //}
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("启用后点击F7开启关闭GUI！");
            GUILayout.Label(GUI_string_Debug, style);
            GUILayout.EndHorizontal();
            GUILayout.Label("此功能是AliceInCradle开发者留下的调试功能。" +
                "\n╔< 汉化栏" +
                "\n╠══╦⇒ ? ↴ " +
                "\n╟    ╠═⇒ mighty ⇄ 大幅度增加攻击力" +
                "\n╟    ╠═⇒ nodamage ⇄ 不会收到伤害" +
                "\n╟    ╠═⇒ weak ⇄ 受到1下伤害就会倒下" +
                "\n╟    ╠═⇒ IF文で停止 ⇄ 获取全部魔法" +
                "\n╟    ╠═⇒ IF语句停止 ⇄ 停止使用 IF 语句。" +
                "\n╟    ╠═⇒ <BREAK>で停止 ⇄ 停在<BREAK>" +
                "\n╟    ╚═⇒ seed ⇄ 种子" +
                "\n╠══╦⇒ HP/MP ⇄ 生命值/魔力值 ↴" +
                "\n╟    ╠═⇒ Noel ⇄ 诺艾尔    kill ⇄ 杀死(点了你就直接死了)" +
                "\n╟    ╠═⇒ HP ⇄ 生命值    MP ⇄ 魔力值 " +
                "\n╟    ╠═⇒ pos ⇄ 坐标" +
                "\n╟    ╚══> 右边的敌队生物翻译一样。" +
                "\n╠══╦⇒ item ⇄ 物品" +
                "\n╟    ╠═⇒ Grade ⇄ 数量" +
                "\n╟    ╠═⇒ Money ⇄ 钱币" +
                "\n╟    ╠═⇒ All ⇄ 全部物品" +
                "\n╟    ╠═⇒ CURE ⇄ 治疗" +
                "\n╟    ╠═⇒ BOMB ⇄ 炸弹" +
                "\n╟    ╠═⇒ MTR ⇄ 材料" +
                "\n╟    ╠═⇒ INGREDIENT ⇄ 原料" +
                "\n╟    ╠═⇒ WATER ⇄ 水" +
                "\n╟    ╠═⇒ BOTTLE ⇄ 瓶装" +
                "\n╟    ╠═⇒ FRUIT ⇄ 水果" +
                "\n╟    ╠═⇒ DUST ⇄ 腐烂的食物" +
                "\n╟    ╠═⇒ PRECIOUS ⇄ 贵重物品" +
                "\n╟    ╠═⇒ TOOL ⇄ 工具" +
                "\n╟    ╠═⇒ ENHANCER ⇄ 插件" +
                "\n╟    ╠═⇒ SKILL ⇄ 技能" +
                "\n╟    ╠═⇒ RECIPE ⇄ 宝箱" +
                "\n╟    ╚═⇒ SPCONFIG ⇄ 不明" +
                "\n⇓" +
                "\n待更新");
            GUILayout.EndHorizontal();
            #endregion

            #region 控件_MID信息


            

            // 在顶部插空白空间
            GUILayout.Space(50);


            // 设置一个水平布局，用来控制垂直布局的位置
            GUILayout.BeginHorizontal();
            // 在左侧插入一个可伸缩的空白空间，使垂直布局水平居中
            GUILayout.FlexibleSpace();
            // 设置垂直布局
            GUILayout.BeginVertical();
            // 在垂直布局的顶部插入一个可伸缩的空白空间，使内容垂直居中
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(100));//竖排
            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("隐藏窗口")) // 按钮
            {
                showWindow = !showWindow; // 切换窗口显示状态
            }
            if (GUILayout.Button("游戏官网")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://aliceincradle.dev") { UseShellExecute = true });
            }
            if (GUILayout.Button("Mod官网")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://www.xiaomiao-ica.top/index.php/alice-in-cradle-bepinex-mod/") { UseShellExecute = true });
            }
            if (GUILayout.Button("ModGitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/MiaoluoYuanlina/AliceinCradle_BepInEx_XiaoMiaoICa-Mod") { UseShellExecute = true });
            }
            if (GUILayout.Button("重启游戏")) // 按钮
            {
                RestartApplication();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.Label("出现问题可以联系本苗哦~ 作者信息:"); // 文字
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("BiliBili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/1775750067") { UseShellExecute = true });
            }
            if (GUILayout.Button("X(Twitter)")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://x.com/XiaoMiao_ICa") { UseShellExecute = true });
            }
            if (GUILayout.Button("QQ")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://user.qzone.qq.com/2966095351") { UseShellExecute = true });
            }
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/MiaoluoYuanlina") { UseShellExecute = true });
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.Label(GUI_string_debuggingText); // 文字
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("sinai-dev-UnityExplorer")) // 按钮
            {
                
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            // 在垂直布局的底部插入一个可伸缩的空白空间
            GUILayout.FlexibleSpace();
            // 结束垂直布局
            GUILayout.EndVertical();
            // 在右侧插入一个可伸缩的空白空间，使垂直布局水平居中
            GUILayout.FlexibleSpace();
            // 结束水平布局
            GUILayout.EndHorizontal();

            //if (GUILayout.Button("Text")) // 按钮
            //{
            //}

            // 在顶部插空白空间
            GUILayout.Space(50);

            // 结束滚动视图
            GUILayout.EndScrollView();

            // 允许拖动窗口
            GUI.DragWindow();


            

            #endregion
        }

        public static void RestartApplication()//重启游戏
        {
            try
            {
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                string workingDirectory = Path.GetDirectoryName(appPath);

                // 确保路径不为空，并且应用程序文件存在
                if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
                {
                    // 设置工作目录为当前应用程序的目录，以便 BepInEx 正常加载
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = appPath,
                        WorkingDirectory = workingDirectory, // 设置工作目录
                        UseShellExecute = true,
                        Verb = "runas" // 确保重新启动时以管理员权限运行
                    };

                    Process.Start(startInfo);
                }

                // 强制终止当前进程
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                // 记录日志或显示错误信息
                Console.WriteLine($"重启失败：{ex.Message}");
            }
        }
        #endregion
        //快捷键设置
        IEnumerator SetCustomKey()
        {
            XM_log("请点击一个按键！", 1);
            while (!Input.anyKeyDown)
            {
                GUI_string_toggleKey = "请点击键盘上的一个按键 目前快捷键:";
                yield return null; // 等待用户按下一个键
            }

            // 获取用户按下的键并保存
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    toggleKey = keyCode;
                    PlayerPrefs.SetString("toggleKey", keyCode.ToString());
                    PlayerPrefs.Save();
                    XM_log ("快捷键设置为了: " + keyCode , 1);
                    GUI_string_toggleKey = "目前快捷键:";
                    if (keyCode == KeyCode.Mouse0)
                    {
                        toggleKey = KeyCode.Tab;
                        GUI_string_toggleKey = "此快捷不可用 目前快捷键:";

                    }
                    break;
                }
            }
        }

        /// <summary>
        ///触发点击按键
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {

            }
            //if (Input.GetKeyDown(KeyCode.Tab)) // 替换为你想要的快捷键
            //{
            //    showWindow = !showWindow; // 切换窗口显示状态
            //}
            if (Input.GetKeyDown(toggleKey))
            {
                showWindow = !showWindow; // 切换窗口显示状态
            }
        }

        /// <summary>
        /// 监听游戏MosaicShower函数 前置 
        /// 删马赛克
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
            public static bool MosaicShower_FnDrawMosaic_Prefix(MosaicShower __instance)
            {
            if (GUI_Bool_BanMosaic == true)
            {
                XM_log("拦截了MosaicShower函数的执行。", 2);

                return false;
            }
            else
            {
                return true;
            }
            return true;
        }

        /// <summary>
        /// 监听游戏MosaicShower函数 后置
        /// 删马赛克
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public static void MosaicShower_FnDrawMosaic_Postfix(MosaicShower __instance ,Camera Cam)
        {
            if (GUI_Bool_BanMosaic == true)
            {
                XM_log($"MosaicShower传入函数的变量信息:[Cam={Cam}", 2);
            }
        }

        /// <summary>
        /// 监听游戏initMagicItem函数
        /// 魔法
        /// </summary>
        [HarmonyPatch(typeof(MDAT), "initMagicItem")]
        public static class MDAT_initMagicItem_Patch
        {
            public static void Postfix(bool init_aimpos_to_d)
            {
                XM_log($"initMagicItem传入函数的变量信息:[init_aimpos_to_d={init_aimpos_to_d}]", 2);
            }
        }

        /// <summary>
        /// 监听游戏initMagicItem函数
        /// 调试
        /// </summary>
        [HarmonyPatch(typeof(EV), "initDebugger")]
        public static class EV_initDebugger_Patch
        {
            public static void Postfix(bool execute_debugger_initialize)
            {
                XM_log($"initMagicItem传入函数的变量信息:[init_aimpos_to_d={execute_debugger_initialize}]", 2);
            }
        }

        void Mod_Set_Money(int desiredCoinAmount)
        {
            // 通过 addCount 方法增加金币
            // 在这个示例中，我们先计算需要增加的金币数
            int currentGold = (int)CoinStorage.getCount(CoinStorage.CTYPE.GOLD);
            int amountToAdd = desiredCoinAmount - currentGold;

            // 使用 addCount 方法增加金币数量
            if (amountToAdd > 0)
            {
                CoinStorage.addCount(amountToAdd, CoinStorage.CTYPE.GOLD);
                XM_log("金币已修改为: " + desiredCoinAmount, 1);
            }
            else if (amountToAdd < 0)
            {
                CoinStorage.reduceCount(-amountToAdd, CoinStorage.CTYPE.GOLD);
                XM_log("金币已修改为: " + desiredCoinAmount, 1);

            }
        }



        /// <summary>
        /// 本苗日志
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool XM_log(string text, int type)
        {
            if (type == 1)
            {
                UnityEngine.Debug.Log("#XiaoMiaoICa: " + text);
            }
            else if (type == 2)
            {
                XiaoMiaoICaMod instance_log = new XiaoMiaoICaMod();
                instance_log.XM_log2(text ,2);
            }
            return false;
        }
        public void XM_log2(string text, int type)
        {
            if (type == 1)
            {
                UnityEngine.Debug.Log("#XiaoMiaoICa: " + text);
            }
            else if (type == 2)
            {
                Logger.LogInfo("#XiaoMiaoICa: " + text);
            }
            
        }
        

        

    }

}
