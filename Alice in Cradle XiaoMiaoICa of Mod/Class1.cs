using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;



using System.Windows.Forms;
using System.Threading;



//游戏dll引用
using nel; //Assembly-CSharp.dll
using evt;//unsafeAssem.dll


namespace AIC_XiaoMiaoICa_Mod_DLL
{
    [BepInPlugin("AliceinCradle.XiaoMiaoICa.Mod", "AliceinCradle.XiaoMiaoICa.Mod", "2.2.1")]
    public class XiaoMiaoICaMod : BaseUnityPlugin
    {
        #region 变量
        // 配置文件路径
        private string configFilePath = "XiaoMiaoICa_Mod_Data/config.json";

        //game
        string Game_directory = null;
        int Game_PID = 0;

        //用户协议
        private bool utilization_agreement = false; // 用户协议是否同意

        //UI_用户协议   
        private Rect WindowsRect_utilization_agreement = new Rect(100, 100, 400, 400); // 主窗口
        private bool WindowsRect_utilization_agreement_showWindow = false; // 控制窗口显示/隐藏的标志

        //UI
        private Rect WindowsRect = new Rect(50, 50, 500, 400); // 主窗口
        private bool showWindow = false; // 控制窗口显示/隐藏的标志
        private KeyCode toggleKey = KeyCode.Tab; // 用户定义的快捷键

        private string GUI_Textstring = ""; // 输入框
        private bool GUI_TextBool = false; // 开关
        private float GUI_TextInt = 1; // 滑动条

        private string GUI_string_toggleKey = "目前快捷键:";//快捷键

        private static bool GUI_Bool_BanMosaic = false; // 开关_禁用马赛克

        private static bool GUI_Bool_NOApplyDamage = false; // 开关_免疫伤害

        private static string GUI_TextField_Money = "10000";//金币

        private static string GUI_TextField_Time = "1"; //变速齿轮

        private static bool GUI_Bool_Debug = false; // 开关_游戏自带调试
        private string GUI_string_Debug = ""; // 文字_游戏自带调试

        private Vector2 svPos; // 界面滑动条

        #endregion
        void Start()//启动
        {
            Harmony.CreateAndPatchAll(typeof(XiaoMiaoICaMod));
            //UnityEngine.Debug.Log("Test1");// Unity输出 灰色
            //Logger.LogError("Test2");// 错误 
            //Logger.LogFatal("Test3");//致命 淡红色
            //Logger.LogWarning("Test4");//警告 黄色
            //Logger.LogMessage("Test5");//消息 白色
            //Logger.LogInfo("Test6");//信息 灰色
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


            string[] file =
            {
                "Newtonsoft.Json.dll"
            };
            // 循环检查每个文件是否存在
            foreach (var file2 in file)
            {
                if (File.Exists(Game_directory+ @"\BepInEx\plugins\" + file2))
                {
                    Console.WriteLine($"DLL文件 {file2} 存在。");
                }   
                else
                {
                    Console.WriteLine($"导出DLL {file2} 。");
                    ExportEmbedResources("Alice_in_Cradle_XiaoMiaoICa_of_Mod.DLL." + file2, Game_directory + @"\BepInEx\plugins\" + file2);
                    
                }
            }

            string initext = "";
            try
            {
                initext = M_EF.Config_Read(Directory.GetCurrentDirectory() + @"\XiaoMiaoICa_Mod_Data\user_agreement", "user_agreement");

            }
            catch
            {
                Logger.LogWarning("读取配置文件出错！");
            }
            if (initext == "true")
            {
                Logger.LogMessage("用户同意使用协议");
                utilization_agreement = true;
            }
            else
            {
                Logger.LogWarning("用户未同意使用协议");
                utilization_agreement = false;
            }
            if (utilization_agreement == true)
            {
                showWindow = true;
            }
            else
            {
                WindowsRect_utilization_agreement_showWindow = true;
            }
            #region 读取配置文件
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic") == "True")
            {
                GUI_Bool_BanMosaic = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage") == "True")
            {
                GUI_Bool_NOApplyDamage = true;
            }
            #endregion
        }

        void OnGUI()
        {
            if (showWindow)
            {
                WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:Hello World");
                
            }
            if (WindowsRect_utilization_agreement_showWindow)
            {
                WindowsRect_utilization_agreement = GUILayout.Window(0720, WindowsRect_utilization_agreement, WindowsFunc_utilization_agreement, "苗萝缘莉雫:《Alice in Cradle》第三方Mod使用协议");
            }
        }

        public void WindowsFunc_utilization_agreement(int id)//控件_用户协议
        {

            if (utilization_agreement == true)
            {
                M_EF.Config_Write(Directory.GetCurrentDirectory() + @"\XiaoMiaoICa_Mod_Data\user_agreement", "user_agreement", "true");
                showWindow = true; // 显示主窗口
                WindowsRect_utilization_agreement_showWindow = false; // 关闭窗口
            }

            //WindowsRect_utilization_agreement = new Rect(50, 50, 700, 500);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            Color redColor = new Color32(0, 0, 0, 0);

            redColor = new Color32(255, 153, 51, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("原始游戏:指《Alice in Cradle》原始游戏文件。\nMOD:指XiaoMiao_ICa开发的非官方开源衍生作品。", style);


            redColor = new Color32(255, 0, 0, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("禁止将本Mod用于商业用途，或者修改后用于商业用途。\n", style); // 文字

            redColor = new Color32(255, 255, 0, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("Mod由第三方非官方开发者开发！与原始游戏官方无关联，请不要将mod引起的游戏崩溃日志提供给官方，因为官方不会为mod提供支持！\n", style);


            // 在顶部插空白空间
            GUILayout.Space(10);

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/MiaoluoYuanlina/AliceinCradle_BepInEx_XiaoMiaoICa-Mod") { UseShellExecute = true });
            }
            if (GUILayout.Button("同意协议")) // 按钮
            {
                utilization_agreement = true; // 用户协议同意
            }
            if (GUILayout.Button("拒绝协议")) // 按钮
            {
                Process.Start("powershell.exe", "-command \"[System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms'); [System.Windows.Forms.MessageBox]::Show('如果不同意使用协议，请立刻卸载Mod！', '欧尼酱~你拒绝了用户协议呢~', [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)\"");
                Process.GetCurrentProcess().Kill();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();



            //if (GUILayout.Button("Text")) // 按钮
            //{
            //}




            
        }

        public void WindowsFunc(int id)//控件
        {
            #region 控件

            #region TIP
            GUIStyle style = new GUIStyle(GUI.skin.label);
            Color redColor = new Color32(255, 0, 0, 255);
            style.normal.textColor = redColor;
            GUILayout.Label("本Mod包括游戏本体完全免费！为爱发电，如果你是购买而来，证明你被骗啦！", style); // 文字
            #endregion
            
            svPos = GUILayout.BeginScrollView(svPos);// 开始滚动视图

            #region 保存配置
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("保存配置到配置文件"))
            {
                SavepreferencesConfig();
            }
            GUILayout.Label("将当前配置保存，下次启动自动读取。"); // 文字
            GUILayout.EndHorizontal();
            #endregion

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

            #region 免疫伤害 ApplyDamage
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUI_Bool_NOApplyDamage = GUILayout.Toggle(GUI_Bool_NOApplyDamage, "免疫伤害");
            GUILayout.Label("免疫魔物和环境对你造成伤害"); // 文字
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
            //if (GUILayout.Button("重启游戏")) // 按钮
            //{
            //    Process.Start(Game_directory + "/AliceInCradle.exe", "");
            //    Process.GetCurrentProcess().Kill();
            //}
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

        //保存配置
        public static void SavepreferencesConfig()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic", GUI_Bool_BanMosaic.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage", GUI_Bool_NOApplyDamage.ToString());
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


        // 监听游戏MosaicShower函数 前置 用于清除马赛克
        [HarmonyPrefix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public static bool MosaicShower_FnDrawMosaic_Prefix(MosaicShower __instance)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_BanMosaic == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - nel.MosaicShower_FnDrawMosaic - 前置 - 返回true");
                return false;
            }
            else
            {
                return true;
            }
            return true;
        }

        // 监听游戏MosaicShower函数 后置 用于清除马赛克
        [HarmonyPostfix, HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public static void MosaicShower_FnDrawMosaic_Postfix(MosaicShower __instance, Camera Cam)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_BanMosaic == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - nel.MosaicShower_FnDrawMosaic - 后置 - 传递:[Cam=" + Cam +"]");
            }
        }


        // 监听游戏M2PrADmg函数 前置 用于免疫伤害
        [HarmonyPrefix, HarmonyPatch(typeof(M2PrADmg), "applyDamage")]
        public static bool M2PrADmg_applyDamage_Prefix(MosaicShower __instance)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_NOApplyDamage == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - nel.M2PrADmg.applyDamage - 前置 - 返回true");
                return false;
            }
            else
            {
                return true;
            }
            return true;
        }

        // 监听游戏M2PrADmg函数 后置 用于免疫伤害
        [HarmonyPostfix, HarmonyPatch(typeof(M2PrADmg), "applyDamage")]
        public static void M2PrADmg_applyDamage_Postfix(MosaicShower __instance, NelAttackInfo Atk, bool force, string fade_key, bool decline_ui_additional_effect, bool from_press_damage)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            if (GUI_Bool_NOApplyDamage == true)
            {
                instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - nel.M2PrADmg.applyDamage - 后置 - 传递:[force=" + force + "]" + "[fade_key=" + fade_key + "]" + "[decline_ui_additional_effect=" + decline_ui_additional_effect + "]" + "[from_press_damage=" + from_press_damage + "]");
            }
        }

        // 监听游戏initMagicItem函数 用于用户协议

        [HarmonyPrefix, HarmonyPatch(typeof(EV), "evStart")]
        public static bool EV_evStart_Prefix(MosaicShower __instance)
        {
            //return true;
            // Create an instance of XiaoMiaoICaMod to access the non-static field
            //XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            //instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - evt.EV.Prefix - 前置 - 返回***");

            //instance_XiaoMiaoICaMod.utilization_agreement = true;
            //instance_XiaoMiaoICaMod.Logger.LogMessage("用户协议" + instance_XiaoMiaoICaMod.utilization_agreement);
            //if (instance_XiaoMiaoICaMod.utilization_agreement)
            //{

            //    instance_XiaoMiaoICaMod.showWindow = false; // 显示主窗口
            //    instance_XiaoMiaoICaMod.WindowsRect_utilization_agreement_showWindow = true; // 关闭窗口
            //    return true;
            //}
            //else
            //{
            //    instance_XiaoMiaoICaMod.showWindow = true; // 显示主窗口
            //    instance_XiaoMiaoICaMod.WindowsRect_utilization_agreement_showWindow = false; // 关闭窗口
            //    return false;
            //}


            return true;
        }

        // 监听游戏M2PrADmg函数 后置 用于用户协议
        [HarmonyPostfix, HarmonyPatch(typeof(EV), "evStart")]
        public static void EV_evStart_Postfix(MosaicShower __instance)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - evt.EV.Prefix - 后置 - 传递:无");


        }

        // 监听游戏 函数 后置 用于
        //[HarmonyPostfix, HarmonyPatch(typeof(aBtnNel), "ButtonSkin")]
        public static void aBtnNel_ButtonSkin_Postfix(MosaicShower __instance, string key)
        {
            XiaoMiaoICaMod instance_XiaoMiaoICaMod = new XiaoMiaoICaMod();
            instance_XiaoMiaoICaMod.Logger.LogInfo("DLL Assembly-CSharp.dll - nel.aBtnNel.ButtonSkin - 后置 - 传递:[force=" + key + "]");


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

        /// <summary>
        /// 导出切入资源
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static bool ExportEmbedResources(string FileName, string Path) // 导出嵌入资源
        {
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 构造嵌入资源的完整名称
                string resourceName = $"{FileName}";

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
    }
    public static class M_EF    //Extended functionality
    {
        /// <summary>
        /// 写入配置文件
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <param name="configName">配置项名称</param>
        /// <param name="content">配置内容</param>
        /// <returns>操作是否成功</returns>
        public static bool Config_Write(string filePath, string configName, string content)
        {
            
            
            try
            {
                Dictionary<string, string> configDict;

                // 创建目录（如果不存在）
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 读取现有配置
                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);
                    configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(existingJson)
                               ?? new Dictionary<string, string>();
                }
                else
                {
                    configDict = new Dictionary<string, string>();
                }

                // 更新配置项
                configDict[configName] = content;

                // 写入文件
                string newJson = JsonConvert.SerializeObject(configDict, Formatting.Indented);
                File.WriteAllText(filePath, newJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <param name="configName">配置项名称</param>
        /// <returns>配置内容（失败返回null）</returns>
        public static string Config_Read(string filePath, string configName)
        {
            try
            {
                if (!File.Exists(filePath)) return null;

                string json = File.ReadAllText(filePath);
                Dictionary<string, string> configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return configDict != null && configDict.TryGetValue(configName, out string value)
                       ? value
                       : null;
            }
            catch
            {
                return null;
            }

        }

    }
}
