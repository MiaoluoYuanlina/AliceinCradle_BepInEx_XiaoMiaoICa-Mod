using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using nel;
using System.Globalization;
using System.Security.Policy;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using BepInEx.Logging;


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


        }



        /// <summary>
        /// 可视化窗口
        /// </summary>

        private Rect WindowsRect = new Rect(50, 50, 500, 400); // 主窗口
        private string GUI_Textstring = ""; // 输入框
        private bool GUI_TextBool = false; // 开关
        private float GUI_TextInt = 1; // 滑动条

        private string GUI_string_debuggingText = "下载适用用开发的Mod进游戏中。"; // 调试_提示

        private static bool GUI_Bool_BanMosaic = false; // 开关_禁用马赛克


        private static bool GUI_Bool_CurrentIiving = false; // 开关_当前生命值
        private float GUI_string_CurrentIiving = 100; // 输入框_当前生命值

        private static bool GUI_Bool_CurrentMagic = false; // 开关_当前生魔力
        private float GUI_string_CurrentMagic = 100; // 输入框_当前魔力


        private static bool GUI_Bool_CurrentMoney = false; // 开关_当前金币
        private float GUI_string_CurrentMoney = 1; // 输入框_当前金币

        private Vector2 svPos; // 界面滑动条

        void OnGUI()
        {
            WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:Hello World");
        }

        public void WindowsFunc(int id)
        {

            // 开始滚动视图
            svPos = GUILayout.BeginScrollView(svPos);

            // 添加控件
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

            GUILayout.Label("本Mod包括游戏本体完全免费！为爱发的，如果你是购买而来，证明你被骗啦！"); // 文字

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
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
            GUILayout.Label(GUI_string_debuggingText); // 文字
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("下载")) // 按钮
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://github.com");
                    request.Method = "GET";
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            XM_log("github 可正常访问",1);

                        }
                        else
                        {
                            XM_log("github无法访问，状态码: " + response.StatusCode,1);
                        }
                    }
                }
                catch (WebException ex)
                {
                    GUI_string_debuggingText = GUI_string_debuggingText + " \ngithub无法访问因此无法下载重要文件。状态码: " + ex.Message;
                }

            }





            //if (GUILayout.Button("重启游戏到游玩模式")) // 按钮
            //{

            //    ProcessStartInfo startInfo = new ProcessStartInfo
            //    {
            //        FileName = "powershell.exe",
            //        Arguments = "-File \"./XiaoMiaoICa_Mod_Data/reopen_0.ps1\"", // 使用相对路径
            //        UseShellExecute = false,
            //        CreateNoWindow = true
            //    };
            //    Process process = new Process
            //    {
            //        StartInfo = startInfo
            //    };
            //    process.Start();// 不等待进程退出
            //}
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

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


            /*
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("Text")) // 按钮
            {
                //NelAttackInfoPatch();
            }
            if (GUILayout.Button("Text")) // 按钮
            {
            }
            if (GUILayout.Button("Text")) // 按钮
            {
            }
            GUILayout.EndHorizontal();

            */

            // 结束滚动视图
            GUILayout.EndScrollView();

            // 允许拖动窗口
            GUI.DragWindow();

        }

        /// <summary>
        ///触发点击按键
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {

                Logger.LogInfo("#XiaoMiaoICa.MOD:Heloo World");


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


       















        /*
        // 使用 Harmony 修补 addCount 方法，明确指定参数类型
        [HarmonyPatch(typeof(CoinStorage), "addCount", new Type[] { typeof(int), typeof(CoinStorage.CTYPE), typeof(bool) })]
        public static class CoinStorage_AddCount_Patch
        {
            static void Prefix(ref int v, CoinStorage.CTYPE ctype = CoinStorage.CTYPE.GOLD, bool show_log = true)
            {
                // 将金币数量加倍（或任何你想要的修改）
                v *= 2;
                XM_log($"金币返回值修改为: {v}",1);
            }
        }
        */

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
