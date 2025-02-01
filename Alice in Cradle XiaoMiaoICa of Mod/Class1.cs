using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

//游戏dll引用
using nel; //Assembly-CSharp.dll
using evt;//unsafeAssem.dll


namespace AIC_XiaoMiaoICa_Mod_DLL
{
    [BepInPlugin("AliceinCradle.XiaoMiaoICa.Mod", "AliceinCradle.XiaoMiaoICa.Mod", "1.0.0")]
    public class XiaoMiaoICaMod : BaseUnityPlugin
    {
        #region 变量
        // 配置文件路径
        private string configFilePath = "XiaoMiaoICa_Mod_Data/config.json";

        //game
        string Game_directory = null;
        int Game_PID = 0;

        //UI
        private Rect WindowsRect = new Rect(50, 50, 500, 400); // 主窗口
        private bool showWindow = true; // 控制窗口显示/隐藏的标志
        private KeyCode toggleKey = KeyCode.Tab; // 用户定义的快捷键

        private string GUI_Textstring = ""; // 输入框
        private bool GUI_TextBool = false; // 开关
        private float GUI_TextInt = 1; // 滑动条

        private string GUI_string_toggleKey = "目前快捷键:";//快捷键

        private static bool GUI_Bool_BanMosaic = false; // 开关_禁用马赛克

        private static string GUI_TextField_Money = "10000";//金币

        private static string GUI_TextField_Time = "1"; //变速齿轮

        private static bool GUI_Bool_Debug = false; // 开关_游戏自带调试
        private string GUI_string_Debug = ""; // 文字_游戏自带调试

        private Vector2 svPos; // 界面滑动条

        #endregion


        void Start()//启动
        {
            Harmony.CreateAndPatchAll(typeof(XiaoMiaoICaMod));
            UnityEngine.Debug.Log("Test1");// Unity输出 灰色
            Logger.LogError("Test2");// 错误 
            Logger.LogFatal("Test3");//致命 淡红色
            Logger.LogWarning("Test4");//警告 黄色
            Logger.LogMessage("Test5");//消息 白色
            Logger.LogInfo("Test6");//信息 灰色
            //Logger.LogDebug("Test7");//调试


            //获取PID
            Process currentProcess = Process.GetCurrentProcess();
            Game_PID = currentProcess.Id;
            // 获取当前目录
            string currentDirectory = Directory.GetCurrentDirectory();
            Game_directory = currentDirectory;
            // 组合路径
            string path = Path.Combine(currentDirectory, "XiaoMiaoICa_Mod_Data");
            Logger.LogMessage(path);
            // 检查文件夹是否存在
            if (!Directory.Exists(path))
            {
                // 创建文件夹
                Directory.CreateDirectory(path);
            }

            const string Py = "XiaoMiao_ICa";

            Logger.LogMessage("#XiaoMiaoICa: Game_PID:" + Game_PID);
            Logger.LogMessage("#XiaoMiaoICa: Game_directory:" + Game_directory);

        }

        void OnGUI()
        {
            if (showWindow)
            {
                WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:Hello World");


            }
        }
        public void WindowsFunc(int id)
        {
            #region 控件

            #region TIP
            GUIStyle style = new GUIStyle(GUI.skin.label);
            Color redColor = new Color32(255, 0, 0, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("本Mod包括游戏本体完全免费！为爱发电，如果你是购买而来，证明你被骗啦！", style); // 文字
            #endregion
            
            svPos = GUILayout.BeginScrollView(svPos);// 开始滚动视图

            #region 快捷键
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("设置窗口隐藏显示快捷键")) // 按钮
            {
                StartCoroutine(SetCustomKey());//调用函数
            }
            GUILayout.Label(GUI_string_toggleKey + toggleKey); // 文字
            GUILayout.EndHorizontal();
            #endregion

            #region 马赛克
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUI_Bool_BanMosaic = GUILayout.Toggle(GUI_Bool_BanMosaic, "禁止马赛克生成");
            GUILayout.Label("有的部分是在游戏CG上就已经除了过了，所以不一定全部地方生效。"); // 文字
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            #endregion

            #region 金币
            GUILayout.BeginHorizontal(GUI.skin.box);//横排

            //GUILayout.Label("text");
            if (GUILayout.Button("修改金币")) // 按钮
            {
                int textint = Mathf.RoundToInt(int.Parse(GUI_TextField_Money));
                Mod_Set_Money(textint);
                GUI_TextField_Money = textint.ToString();
            }
            GUI_TextField_Money = GUILayout.TextField(GUI_TextField_Money);

            GUILayout.EndHorizontal();
            #endregion

            #region 变速齿轮
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("修改游戏速度")) // 按钮
            {
                int textint = Mathf.RoundToInt(int.Parse(GUI_TextField_Time));
                Time.timeScale = textint; // 游戏加速
                GUI_TextField_Time = textint.ToString();
            }
            GUI_TextField_Time = GUILayout.TextField(GUI_TextField_Time);
            GUILayout.EndHorizontal();
            #endregion

            #region debug
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
                Logger.LogMessage("_debug.txt 文件内容已成功更新！");
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
                Logger.LogMessage("_debug.txt 文件内容已成功更新！");
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

            #region 保存配置
            //if (GUILayout.Button("保存配置到配置文件"))
            //{
            //    SaveConfig();
            //}
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
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("强制关闭游戏")) // 按钮
            {
                Process.GetCurrentProcess().Kill();
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



            #endregion

            GUILayout.Space(50);// 在顶部插空白空间
            GUILayout.EndScrollView();// 结束滚动视图
            GUI.DragWindow();// 允许拖动窗口
            #endregion
        }
        

        //更改快捷键
        IEnumerator SetCustomKey()
        {
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
                    Logger.LogInfo("快捷键设置为了: " + keyCode);
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

        //触发点击按键
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



        

        // 监听游戏MosaicShower函数 前置 
        [HarmonyPrefix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public static bool MosaicShower_FnDrawMosaic_Prefix(MosaicShower __instance)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_BanMosaic == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - 方法 nel - 类 FnDrawMosaic - 前置 - 返回true");
                return false;
            }
            else
            {
                return true;
            }
            return true;
        }

        // 监听游戏MosaicShower函数 后置
        [HarmonyPostfix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public static void MosaicShower_FnDrawMosaic_Postfix(MosaicShower __instance, Camera Cam)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_BanMosaic == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - 方法 nel - 类 FnDrawMosaic - 后置 - 传递:[Cam=" + Cam +"]");
            }
        }



        // 监听游戏initMagicItem函数
        [HarmonyPatch(typeof(MDAT), "initMagicItem")]
        public static class MDAT_initMagicItem_Patch
        {
            public static void Postfix(bool init_aimpos_to_d)
            {
                UnityEngine.Debug.Log($"initMagicItem传入函数的变量信息:[init_aimpos_to_d={init_aimpos_to_d}]");
            }
        }


        // 监听游戏initMagicItem函数
        [HarmonyPatch(typeof(EV), "initDebugger")]
        public static class EV_initDebugger_Patch
        {
            public static void Postfix(bool execute_debugger_initialize)
            {
                UnityEngine.Debug.Log($"initMagicItem传入函数的变量信息:[init_aimpos_to_d={execute_debugger_initialize}]");
            }
        }


        //修改金币
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
                Logger.LogInfo("金币已修改为: " + desiredCoinAmount);
            }
            else if (amountToAdd < 0)
            {
                CoinStorage.reduceCount(-amountToAdd, CoinStorage.CTYPE.GOLD);
                Logger.LogInfo("金币已修改为: " + desiredCoinAmount);

            }
        }


    }
}
