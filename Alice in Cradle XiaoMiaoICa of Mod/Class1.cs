using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
//游戏dll引用
using evt;//unsafeAssem.dll
using HarmonyLib;
using m2d;
using nel; //Assembly-CSharp.dll

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI; 
using XX;
using static AIC_XiaoMiaoICa_Mod_DLL_BpeInEx6.EventEditor;
using static evt.EvDrawerContainer;
using static nel.MatoateReader;
using static nel.NelChipPuzzleBox;
using static nel.UiHkdsChat;
using static System.Net.Mime.MediaTypeNames;

namespace AIC_XiaoMiaoICa_Mod_DLL_BpeInEx6
{


    [BepInPlugin("AliceinCradle.XiaoMiaoICa.Mod", "AliceinCradle.XiaoMiaoICa.Mod", "2.2.1")]
    public class XiaoMiaoICaMod : BaseUnityPlugin
    {
        #region DLL
        #endregion
        #region 变量

        //定义为 Instance
        public static XiaoMiaoICaMod Instance;

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
        private static bool GUI_Bool_BanMosaic2 = false; // 开关_禁用马赛克

        private static bool GUI_Bool_NOApplyDamage = false; // 开关_免疫伤害
        private static bool GUI_Bool_NOApplyDamage2 = false; // 开关_不受伤害

        private static string GUI_TextField_Money = "10000";//金币


        private static string GUI_TextField_SetHP = "100";//HP
        private static string GUI_TextField_SetMP = "100";//MP
        private static bool GUI_Bool_SetHP = false; // 开关_设置HP
        private static bool GUI_Bool_SetMP = false; // 开关_设置MP

        private static string GUI_TextField_Time = "1"; //变速齿轮

        private static bool GUI_Bool_Debug = false; // 开关_游戏自带调试
        private string GUI_string_Debug = ""; // 文字_游戏自带调试

        private static bool GUI_Bool_ModDebug = false; // 开关_Mod调试
        private static bool GUI_Bool_ModDebug_Export_Resources = false; // 开关_Mod调试_导出资源
        private static bool GUI_Bool_ModDebug_Noel_info = false; //开关_Mod调试_显示Noel信息

        private static string GUI_TextField_Objective = "chrome";//使用什浏览器
        private static string GUI_TextField_WebUiUrl = "https://api.ica.wiki/AIC/EventEditor/";//打开的网址

        private Vector2 svPos; // 界面滑动条

        #endregion

        void Start()//启动
        {
            Harmony.CreateAndPatchAll(typeof(XiaoMiaoICaMod));
            UnityEngine.Debug.Log("Test1");// Unity输出 灰色
            Logger.LogError("Test2");// 错误 
            Logger.LogFatal("Test3");//致命 淡红色
            Logger.LogWarning("Test4");//警告 黄色
            Logger.LogInfo("Test6");//信息 灰色            Logger.LogMessage("Test5");//消息 白色
            Logger.LogDebug("Test7");//调试

            Instance = this;

            var harmony = new Harmony("com.xiaomiao.mod");
            // 强制指定程序集加载，防止扫描不到
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            // 打印已加载的补丁数量来确认
            var patchedMethods = harmony.GetPatchedMethods();
            int count = 0;
            foreach (var method in patchedMethods) count++;
            Logger.LogInfo($"Harmony 成功加载了 "+count+" 个补丁方法");


            //获取PID
            Process currentProcess = Process.GetCurrentProcess();
            Game_PID = currentProcess.Id;
            // 获取当前目录
            Game_directory = Directory.GetCurrentDirectory();
            // 组合路径
            string path = Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data");
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

            #region 导出dll和文件
           
            var exportMap = new Dictionary<string, (string Dir, string OutputName)>
            {
                // 资源名                         // 导出目录                               // 导出后的文件名
                //{ "DLL.Newtonsoft.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins"), "Newtonsoft.Json.dll") },
                { "Data.ExportedAssets.__events_restroom.pxls.bytes.texture_0.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "__events_restroom.pxls.bytes.texture_0") },
                { "Data.ExportedAssets.key_noel.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "key_noel") },
                { "Data.ExportedAssets.__events_2weekattack.pxls.bytes.texture_0.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "__events_2weekattack.pxls.bytes.texture_0") },
                { "Data.ExportedAssets.title_logo.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "title_logo") },
                //事件编辑器相关文件
                { "Data.EventEditorModMiddleware.exe", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "EventEditorModMiddleware.exe") },
                { "DLL.Microsoft.Bcl.AsyncInterfaces.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.Bcl.AsyncInterfaces.dll") },
                { "DLL.Microsoft.CSharp.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.CSharp.dll") },
                { "DLL.Microsoft.Playwright.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.Playwright.dll") },
                { "DLL.Newtonsoft.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Newtonsoft.Json.dll") },
                { "DLL.System.Buffers.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Buffers.dll") },
                { "DLL.System.ComponentModel.Annotations.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.ComponentModel.Annotations.dll") },
                { "DLL.System.ComponentModel.DataAnnotations.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.ComponentModel.DataAnnotations.dll") },
                { "DLL.System.Data.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Data.dll") },
                { "DLL.System.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.dll") },
                { "DLL.System.IO.Pipelines.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.IO.Pipelines.dll") },
                { "DLL.System.Memory.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Memory.dll") },
                { "DLL.System.Net.Http.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Net.Http.dll") },
                { "DLL.System.Numerics.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Numerics.dll") },
                { "DLL.System.Numerics.Vectors.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Numerics.Vectors.dll") },
                { "DLL.System.Runtime.CompilerServices.Unsafe.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Runtime.CompilerServices.Unsafe.dll") },
                { "DLL.System.Text.Encodings.Web.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Text.Encodings.Web.dll") },
                { "DLL.System.Text.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Text.Json.dll") },
                { "DLL.System.Threading.Tasks.Extensions.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Threading.Tasks.Extensions.dll") },
                { "DLL.System.Xml.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Xml.dll") },
                { "DLL.System.Xml.Linq.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Xml.Linq.dll") },
            };

            foreach (var item in exportMap)
            {
                string resourceFile = item.Key;
                string exportDir = item.Value.Dir;
                string outputName = item.Value.OutputName;

                string targetPath = Path.Combine(exportDir, outputName);

                Directory.CreateDirectory(exportDir);

                if (File.Exists(targetPath))
                {
                    Console.WriteLine($"文件已存在：{targetPath}");
                }
                else
                {
                    Console.WriteLine($"导出 {resourceFile} → {targetPath}");

                    ExportEmbedResources(
                        "Alice_in_Cradle_XiaoMiaoICa_of_Mod." + resourceFile,
                        targetPath
                    );
                }
            }



            #endregion
            #region 用户协议
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
            #endregion
            #region 读取配置文件
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic") == "True")
            {
                GUI_Bool_BanMosaic = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic2") == "True")
            {
                GUI_Bool_BanMosaic2 = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage") == "True")
            {
                GUI_Bool_NOApplyDamage = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage2") == "True")
            {
                GUI_Bool_NOApplyDamage2 = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug") == "True")
            {
                GUI_Bool_ModDebug = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Export_Resources") == "True")
            {
                GUI_Bool_ModDebug_Export_Resources = true;
            }
            if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            {
                GUI_Bool_ModDebug_Noel_info = true;
            }

            //if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            //{
            //    GUI_Bool_ModDebug_Noel_info = true;
            //}
            //if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            //{
            //    GUI_Bool_ModDebug_Noel_info = true;
            //}
            //if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            //{
            //    GUI_Bool_ModDebug_Noel_info = true;
            //}
            //if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            //{
            //    GUI_Bool_ModDebug_Noel_info = true;
            //}
            //if (M_EF.Config_Read(Game_directory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info") == "True")
            //{
            //    GUI_Bool_ModDebug_Noel_info = true;
            //}
            #endregion
        }
        
        void OnGUI()//绘制UI
        {
            if (showWindow)
            {
                WindowsRect = GUILayout.Window(0721, WindowsRect, WindowsFunc, "苗萝缘莉雫:这是mod窗口哦~");

            }
            if (WindowsRect_utilization_agreement_showWindow)
            {
                WindowsRect_utilization_agreement = GUILayout.Window(0720, WindowsRect_utilization_agreement, WindowsFunc_utilization_agreement, "苗萝缘莉雫:《Alice in Cradle》第三方Mod使用协议");
            }
        }

        void Update()//触发点击按键
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
            GUILayout.Label("Mod由第三方非官方开发者开发！与原始游戏官方无关联，请不要将mod引起的游戏崩溃日志提供给官方，因为官方不会为mod提供支持，还会因为未知的错误影响官方的开发！\n", style);


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
            GUI_Bool_BanMosaic = GUILayout.Toggle(GUI_Bool_BanMosaic, "禁止由代码生成的马赛克生成");
            GUILayout.Label("代码生成的马赛克是动态加载的，如在长椅上0721的时候，生成的马赛克就是动态生成的。"); // 文字
            GUI_Bool_BanMosaic2 = GUILayout.Toggle(GUI_Bool_BanMosaic2, "替换被马赛克修改过的图片");
            GUILayout.Label("被马赛克修改过的图片修改的图是指CG。哈酱在把画完的CG放进游戏的时候，马赛克已经被涂在游戏CG上了，所以这是不可逆的。"); // 文字
            GUILayout.Label("所以本苗只能手绘，或者使用AI，但是我还没弄明白怎么用AI去除涩图上的马赛克。现在看来就只能进行手绘了，我也没什么绘画技术，只能先凑合用吧。目前我只手绘了少量图片馁，并没有覆盖游戏的全部CG，毕竟时间画技有限。"); // 文字
            GUILayout.EndHorizontal(); 
            GUILayout.EndHorizontal();
            #endregion

            #region 免疫伤害
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUILayout.Label("此选项可能影响的不止玩家操作的角色,如果出现杀不死的魔族请关闭此选项。"); // 文字
            GUI_Bool_NOApplyDamage2 = GUILayout.Toggle(GUI_Bool_NOApplyDamage2, "免疫伤害");
            GUILayout.Label("免疫魔族和环境对你造成伤害"); // 文字
            GUI_Bool_NOApplyDamage = GUILayout.Toggle(GUI_Bool_NOApplyDamage, "不受伤害");
            GUILayout.Label("使你的生命值不被修改"); // 文字
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            #endregion

            #region HPMP
            GUILayout.BeginHorizontal(GUI.skin.box);//横排

            //GUILayout.Label("text");
            if (GUILayout.Button("设置HP") || GUI_Bool_SetHP == true) // 按钮
            {
                int textint = Mathf.RoundToInt(int.Parse(GUI_TextField_SetHP));
                Mod_Noel.SetHp(textint);
                GUI_TextField_SetHP = textint.ToString(); 

            }
            GUI_Bool_SetHP = GUILayout.Toggle(GUI_Bool_SetHP, "锁定HP");
            GUI_TextField_SetHP = GUILayout.TextField(GUI_TextField_SetHP, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            if (GUILayout.Button("设置MP") || GUI_Bool_SetMP == true) // 按钮
            {
                int textint = Mathf.RoundToInt(int.Parse(GUI_TextField_SetMP));
                Mod_Noel.SetMp(textint);
                GUI_TextField_SetMP = textint.ToString();
            }
            GUI_Bool_SetMP = GUILayout.Toggle(GUI_Bool_SetMP, "锁定MP");
            GUI_TextField_SetMP = GUILayout.TextField(GUI_TextField_SetMP, GUILayout.Width(100));


            GUILayout.EndHorizontal();
            #endregion

            #region 金币
            GUILayout.BeginHorizontal(GUI.skin.box);//横排

            //GUILayout.Label("text");
            if (GUILayout.Button("修改金币")) // 按钮
            {
                int textint = Mathf.RoundToInt(int.Parse(GUI_TextField_Money));
                Mod_Noel.SetMoney(textint);
                GUI_TextField_Money = textint.ToString();
            }
            GUI_TextField_Money = GUILayout.TextField(GUI_TextField_Money, GUILayout.Width(100));

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
            GUI_TextField_Time = GUILayout.TextField(GUI_TextField_Time, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            #endregion

            #region debug
            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("使用游戏原版调试Debug");
            if (GUILayout.Button("启用该功能")) // 按钮
            {
                Patch_X_LoadDebug.GameDeBug (true);
                new EventEditor().run_HaLua(@"
TX_BOARD <<<EOF 
<c6>你想要返回主页面重新读档才能生效！
EOF;
");

            }
            if (GUILayout.Button("关闭该功能")) // 按钮
            {
                Patch_X_LoadDebug.GameDeBug (false);
                new EventEditor().run_HaLua(@"
TX_BOARD <<<EOF 
<c6>你想要返回主页面重新读档才能生效！
EOF;
");
            }
            //if (GUILayout.Button("打开/关闭 GUI F7")) // 按钮
            //{
            //    strikeF7();
            //}
            GUILayout.EndHorizontal();
            //if (GUILayout.Button("Test")) // 按钮
            //{
            //    Patch_X_LoadDebug.SetBool("timestamp", true);
            //    Patch_X_LoadDebug.SetBool("announce", true);
            //}
            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("启用后点击F7开启关闭GUI！");
            GUILayout.Label(GUI_string_Debug, style);
            GUILayout.EndHorizontal();
            //GUILayout.Label("此功能是AliceInCradle开发者留下的调试功能。" +
            //    "\n╔< 汉化栏" +
            //    "\n╠══╦⇒ ? ↴ " +
            //    "\n╟    ╠═⇒ mighty ⇄ 大幅度增加攻击力" +
            //    "\n╟    ╠═⇒ nodamage ⇄ 不会收到伤害" +
            //    "\n╟    ╠═⇒ weak ⇄ 受到1下伤害就会倒下" +
            //    "\n╟    ╠═⇒ IF文で停止 ⇄ 获取全部魔法" +
            //    "\n╟    ╠═⇒ IF语句停止 ⇄ 停止使用 IF 语句。" +
            //    "\n╟    ╠═⇒ <BREAK>で停止 ⇄ 停在<BREAK>" +
            //    "\n╟    ╚═⇒ seed ⇄ 种子" +
            //    "\n╠══╦⇒ HP/MP ⇄ 生命值/魔力值 ↴" +
            //    "\n╟    ╠═⇒ Noel ⇄ 诺艾尔    kill ⇄ 杀死(点了你就直接死了)" +
            //    "\n╟    ╠═⇒ HP ⇄ 生命值    MP ⇄ 魔力值 " +
            //    "\n╟    ╠═⇒ pos ⇄ 坐标" +
            //    "\n╟    ╚══> 右边的敌队生物翻译一样。" +
            //    "\n╠══╦⇒ item ⇄ 物品" +
            //    "\n╟    ╠═⇒ Grade ⇄ 数量" +
            //    "\n╟    ╠═⇒ Money ⇄ 钱币" +
            //    "\n╟    ╠═⇒ All ⇄ 全部物品" +
            //    "\n╟    ╠═⇒ CURE ⇄ 治疗" +
            //    "\n╟    ╠═⇒ BOMB ⇄ 炸弹" +
            //    "\n╟    ╠═⇒ MTR ⇄ 材料" +
            //    "\n╟    ╠═⇒ INGREDIENT ⇄ 原料" +
            //    "\n╟    ╠═⇒ WATER ⇄ 水" +
            //    "\n╟    ╠═⇒ BOTTLE ⇄ 瓶装" +
            //    "\n╟    ╠═⇒ FRUIT ⇄ 水果" +
            //    "\n╟    ╠═⇒ DUST ⇄ 腐烂的食物" +
            //    "\n╟    ╠═⇒ PRECIOUS ⇄ 贵重物品" +
            //    "\n╟    ╠═⇒ TOOL ⇄ 工具" +
            //    "\n╟    ╠═⇒ ENHANCER ⇄ 插件" +
            //    "\n╟    ╠═⇒ SKILL ⇄ 技能" +
            //    "\n╟    ╠═⇒ RECIPE ⇄ 宝箱" +
            //    "\n╟    ╚═⇒ SPCONFIG ⇄ 不明" +
            //    "\n⇓" +
            //    "\n待更新");
            GUILayout.EndHorizontal();
            #endregion

            #region 事件管理器

            GUILayout.BeginVertical(GUI.skin.box);//竖排


            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("启动事件管理器"))
            {
                new EventEditor().run_HaLua(@"
TX_BOARD <<<EOF 
<c6>正在启动事件管理器
<c6>等待几秒后会弹出浏览器窗口
<c6>
<c2>事件管理器还在测试
<c2>有可能会出现bug
<c2>还请谅解~
EOF;
");
                Task.Run(() =>
                {


                    Task.Run(() =>
                    {
                        new EventEditor().Receive("MiaoAicMod_Mod");

                    });
                    Task.Run(() =>
                    {
                        ;
                        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Game_directory + "\\BepInEx\\plugins\\XiaoMiao_ICa\\EventEditorModMiddleware.exe");

                        if (!File.Exists(exePath))
                            return;

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(exePath)
                        });
                    });
                    Thread.Sleep(5000); // 卡住当前线程
                    DataJson json = new DataJson
                    {
                        Type = "EventEditor_Start",
                        Text = "",
                        Pid = Game_PID,
                        Objective = GUI_TextField_Objective,
                        EditorUrl = GUI_TextField_WebUiUrl,
                        directory = Game_directory,
                    };

                    string payload = JsonConvert.SerializeObject(json, Formatting.Indented);

                    new EventEditor().Send("MiaoAicMod_EventEditor", payload);




                });
            }
            GUI.enabled = true;
            if (GUILayout.Button("重新连接事件管理器"))
            {

                DataJson json = new DataJson
                {
                    Type = "EventEditor_Start",
                    Text = "",
                    Pid = Game_PID,
                    Objective = GUI_TextField_Objective,
                    EditorUrl = GUI_TextField_WebUiUrl,
                    directory = Game_directory,
                };

                string payload = JsonConvert.SerializeObject(json, Formatting.Indented);

                new EventEditor().Send("MiaoAicMod_EventEditor", payload);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("使用什么浏览器启动"); 
            if (GUILayout.Button("Google Chrome"))
            {
                GUI_TextField_Objective = "chrome";
            }
            if (GUILayout.Button("microsoft Edge"))
            {
                GUI_TextField_Objective = "msedge";
            }
            GUILayout.EndHorizontal();


            GUI_TextField_Objective = GUILayout.TextField(GUI_TextField_Objective);

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("使用那个镜像站"); // 文字
            if (GUILayout.Button("普莉姆拉主站"))
            {
                GUI_TextField_WebUiUrl = "https://aic.imtfe.org/AicEventEditor/";
            }
            if (GUILayout.Button("本苗镜像站"))
            {
                GUI_TextField_WebUiUrl = "https://api.ica.wiki/AIC/EventEditor";
            }
            GUILayout.EndHorizontal();
            GUI_TextField_WebUiUrl = GUILayout.TextField(GUI_TextField_WebUiUrl);


            if (GUILayout.Button("刷新数据(不知道为什么没效果暂时放这吧)"))
            {
                new EventEditor().ForceReloadText();
            }
            GUILayout.Label("\nWenUI部分由 B站@普莉姆拉老师开发", new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.8f, 0.4f, 1f) } });
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("事件编辑器WebUI GitHub项目")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/cocoAutumn/AicEventEditor") { UseShellExecute = true });
            }
            GUILayout.EndHorizontal();


            if (GUI_Bool_ModDebug == true)
            {
                if (GUILayout.Button("Test1"))
                {
                    try
                    {
                        // 1. 获取 STB 对象
                        STB stb = TX.PopBld(null, 0);

                        // 2. 构造事件脚本
                        stb.Add(@"
MSG n_<<<EOF 
<c1>红<c2>橙<c3>黄<c4>绿<c5>蓝<c6>粉<c7>灰<c8>白
EOF;
");

                        // 3. 创建 EvReader
                        EvReader evReader = new EvReader("%BENCH_EVENT", 0, null, null);

                        // 4. 解析 STB 脚本
                        evReader.parseText(stb);

                        // 5. 放入事件队列执行
                        EV.stackReader(evReader, -1);

                        // 6. 释放 STB
                        TX.ReleaseBld(stb);

                        Logger.LogWarning("[STBExecutor] 脚本已执行");
                    }
                    catch (System.Exception ex)
                    {

                        Logger.LogWarning("[STBExecutor] 执行出错: " + ex);
                    }
                }
                if (GUILayout.Button("Test2"))
                {
                    new EventEditor().run_HaLua(@"
TX_BOARD <<<EOF 
<c1>红<c2>橙<c3>黄<c4>绿<c5>蓝<c6>粉<c7>灰<c8>白
EOF;
");
                }
            }


            GUILayout.EndHorizontal();
            #endregion

            #region ModDebug
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUI_Bool_ModDebug = GUILayout.Toggle(GUI_Bool_ModDebug, "当前选项全部为Mod调试选项，请勿随意启用。");
            GUILayout.BeginVertical();//竖排
            if (GUI_Bool_ModDebug == true)
            {
                GUILayout.BeginVertical();//竖排
                GUI_Bool_ModDebug_Export_Resources = GUILayout.Toggle(GUI_Bool_ModDebug_Export_Resources, "导出正在加载的资源文件");
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();//竖排
                GUI_Bool_ModDebug_Noel_info = GUILayout.Toggle(GUI_Bool_ModDebug_Noel_info, "显示Noel数据");
                if (GUI_Bool_ModDebug == true)
                {
                    if (GUI_Bool_ModDebug_Noel_info == true)
                    {
                        GameObject player = GameObject.Find("Noel");
                        if (player == null)
                        {
                            GUILayout.Label("未找到 Noel ;"); // 文字
                        }
                        else
                        {
                            PRNoel pr = player.GetComponent<PRNoel>();

                            if (pr == null)
                            {

                                GUILayout.Label("未找到 PRNoel 组件 ;"); // 文字
                            }
                            else if (pr == null)
                            {
                                GUILayout.Label("未找到 PRNoel 组件 ;" + pr.ToString()); // 文字
                            }
                            else
                            {

                                int UI_X = 100;

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_carry_vx 运动速度X:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_carry_vx().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_carry_vy 运动速度Y:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_carry_vy().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_walk_xspeed 水平移动速度:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_walk_xspeed().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_hp 生命值:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_hp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_maxhp 最大生命:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_maxhp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_maxmp 最大魔力:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_maxmp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_mp 魔力值:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_mp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_current_state 当前状态:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_current_state().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_state_time 状态持续时间:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_state_time().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_ep 兴奋度:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_ep().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_FootBCC 碰撞相关:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_FootBCC().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_knockback_time:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_knockback_time().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_LastBCC 碰撞相关:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_LastBCC().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_sizex 碰撞箱尺寸X:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_sizex().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_sizey 碰撞箱尺寸Y:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_sizey().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_temp_puzzle_max_mp 临时/谜题用魔力值上限:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_temp_puzzle_max_mp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal(GUI.skin.box);
                                GUILayout.Label("get_temp_puzzle_mp 临时/谜题用魔力值:", GUILayout.Width(UI_X), GUILayout.ExpandWidth(true));
                                try
                                {
                                    GUILayout.Label(pr.get_temp_puzzle_mp().ToString());
                                }
                                catch
                                {
                                    GUILayout.Label("获取出错");
                                }
                                GUILayout.EndHorizontal();
                            }


                        }

                    }
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();

            
            GUILayout.EndHorizontal();
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
                Process.Start(new ProcessStartInfo("https://www.xiaomiaoica.wiki/index.php/alice-in-cradle-bepinex-mod/") { UseShellExecute = true });
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
            GUILayout.Label("事件编辑器WebUi部分开发者:"); // 文字
            GUILayout.BeginHorizontal();//横排
            if (GUILayout.Button("b站")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/399329257") { UseShellExecute = true });
            }
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/cocoAutumn/AicEventEditor") { UseShellExecute = true });
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

        public static void SavepreferencesConfig()//保存配置
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic", GUI_Bool_BanMosaic.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_BanMosaic2", GUI_Bool_BanMosaic2.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage", GUI_Bool_NOApplyDamage.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_NOApplyDamage2", GUI_Bool_NOApplyDamage2.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug", GUI_Bool_ModDebug.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Export_Resources", GUI_Bool_ModDebug_Export_Resources.ToString());
            M_EF.Config_Write(currentDirectory + @"\XiaoMiaoICa_Mod_Data\preferences", "GUI_Bool_ModDebug_Noel_info", GUI_Bool_ModDebug_Noel_info.ToString());
        }

        IEnumerator SetCustomKey() //更改快捷键
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

        [HarmonyPatch] // 监听游戏nel.MosaicShower.FnDrawMosaic方法 用于清除马赛克
        public static class nel_MosaicShower_FnDrawMosaic_Patch
        {
            [HarmonyTargetMethod]// 目标
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(nel.MosaicShower), "FnDrawMosaic", new Type[] {
                typeof(object).MakeByRefType(),
                typeof(ProjectionContainer),
                typeof(Camera)
                });
            }
            [HarmonyPrefix]// 前置
            public static bool Prefix()
            {
                if (GUI_Bool_BanMosaic == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] nel.MosaicShower.FnDrawMosaic 前置成功命中 拦截此方法执行");
                    return false;
                }
                else
                {
                    return true;
                }
                return true;
            }
            [HarmonyPostfix]// 后置
            public static void Postfix(MosaicShower __instance)
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    //UnityEngine.Debug.Log(">>> [XiaoMiaoMod] Prefix 成功命中！");

                }

            }
        }

        [HarmonyPatch] // 监听游戏m2d.M2Attackable.applyHpDamage方法 用于免疫伤害
        public static class m2d_M2Attackable_applyHpDamage_Patch
        {
            [HarmonyTargetMethod]// 目标
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(m2d.M2Attackable), "applyHpDamage", new Type[] {
                typeof(int),
                typeof(bool),
                typeof(AttackInfo)
                });
            }
            [HarmonyPrefix]// 前置
            public static bool Prefix()
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] m2d.M2Attackable.applyHpDamage 前置成功命中 拦截此方法执行");
                    return false;

                }
                return true;
            }
            [HarmonyPostfix]// 后置
            public static void Postfix(MosaicShower __instance)
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] m2d.M2Attackable.applyHpDamage 后置成功命中");
                }

            }
        }

        [HarmonyPatch] // 监听游戏nel.M2PrADmg.applyDamage方法 用于不受伤害
        public static class M2PrADmg_Patch 
        {
            // 目标
            //[HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(nel.M2PrADmg), "applyDamage", new Type[] {
                typeof(NelAttackInfo),
                typeof(HITTYPE).MakeByRefType(),
                typeof(bool),
                typeof(string),
                typeof(bool),
                typeof(bool)
                });
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix()
            {
                if (GUI_Bool_NOApplyDamage2 == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] nel.M2PrADmg.applyDamage方法 前置成功命中 拦截此方法执行");
                    return false;

                }
                return true;
               
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {

            }
        }

        [HarmonyPatch] // 监听游戏evt.EV.evStart方法
        public static class evt_EV_evStart_Patch
        {

            [HarmonyTargetMethod]// 目标
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(evt.EV), "evStart", new Type[] {
                });
            }
            [HarmonyPrefix]// 前置
            public static bool Prefix()
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod]  evt.EV.evStart 前置成功命中");
                }
                return true;
            }
            [HarmonyPostfix]// 后置
            public static void Postfix(MosaicShower __instance)
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod]  evt.EV.evStart 后置成功命中");
                }

            }
        }

        [HarmonyPatch] // 监听游戏nel.M2PrADmg.applyWormTrapDamage方法 用于免疫虫墙
        public static class nel_M2PrADmg_applyWormTrapDamage_Patch
        {
            // 目标
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(nel.M2PrADmg), "applyWormTrapDamage", new Type[] {
                typeof(NelAttackInfo),
                typeof(int),
                typeof(bool)
                });
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix()
            {
                //if (GUI_Bool_ == true)
                //{
                //    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] nel.M2PrADmg.applyWormTrapDamage 前置成功命中");
                //    return false;
                //}
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix(NelAttackInfo Atk, int phase_count, bool decline_additional_effect)
            {
                //UnityEngine.Debug.Log($">>> [XiaoMiaoMod] Postfix 成功命中！");
                //UnityEngine.Debug.Log($">>> {Atk}");
                //UnityEngine.Debug.Log($">>> {phase_count}");
                //UnityEngine.Debug.Log($">>> {decline_additional_effect}");
            }
        }

        [HarmonyPatch] // 监听游戏nel.M2PrADmg.press_damage_state_skip方法 失败后跳过
        public static class nel_M2PrADmg_press_damage_state_skip_Patch
        {
            // 目标
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(nel.M2PrADmg), "press_damage_state_skip");

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法 press_damage_state_skip！");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix()
            {
                UnityEngine.Debug.Log(">>> [XiaoMiaoMod] Prefix 成功命中！");
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix(float def_wait_t, float t_state)
            {
                UnityEngine.Debug.Log($">>> [XiaoMiaoMod] Postfix 成功命中！");
                UnityEngine.Debug.Log($">>> {def_wait_t}");
                UnityEngine.Debug.Log($">>> {t_state}");
            }
        }

        [HarmonyPatch] //  监听游戏nel.M2PrADmg.resetFlagsForGameOver方法 失败重置·
        public static class nel_M2PrADmg_resetFlagsForGameOver_Patch
        {
            // 目标
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(nel.M2PrADmg), "resetFlagsForGameOver");

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法 resetFlagsForGameOver！请确认方法名拼写。");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] m2d.M2Attackable.resetFlagsForGameOver 后置成功命中 游戏识别重新读档");
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 杂鱼~杂鱼~又在看诺艾尔的战败CG~");
            }
        }



        [HarmonyPatch] //  监听游戏XX.ActiveDebugger.runIRD方法 修复刷新
        public static class XX_ActiveDebugger_runIRD_Patch
        {
            // 目标
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(XX.ActiveDebugger), "runIRD");

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法。");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.ActiveDebugger.runIRD 前置成功命中");
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.ActiveDebugger.runIRD 后置成功命中");
            }
        }

        [HarmonyPatch] // 多图贴图替换补丁类
        public static class MultiImagePatchHandler
        {
            // Key 是文件名，Value 是贴图对象
            private static Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

            // 获取自定义图片的通用方法
            public static Texture2D GetCustomTexture(string assetName)
            {
                if (_textureCache.ContainsKey(assetName)) return _textureCache[assetName];

                string filePath = Path.Combine(Paths.PluginPath, "XiaoMiao_ICa","Resources", assetName);

                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (UnityEngine.ImageConversion.LoadImage(tex, data))
                    {
                        tex.name = assetName + "_custom";
                        _textureCache[assetName] = tex;
                        XiaoMiaoICaMod.Instance.Logger.LogInfo($">>> [XiaoMiaoMod] 成功加载新图片: " + assetName);
                        return tex;
                    }
                }
                return null;
            }

            // 统一替换逻辑函数
            private static void TryReplace(string name, Type type, ref UnityEngine.Object result)
            {
                if (XiaoMiaoICaMod.GUI_Bool_BanMosaic2)
                {
                    // 待替换的资源关键字
                    string[] targetNames = {
                "title_logo",
                "key_noel",
                "__events_restroom.pxls.bytes.texture_0",
                "__events_2weekattack.pxls.bytes.texture_0"
            };

                    foreach (string target in targetNames)
                    {
                        if (name.ToLower().Contains(target.ToLower()))
                        {
                            Texture2D customTex = GetCustomTexture(target);
                            if (customTex == null) continue;

                            // 自动识别类型替换
                            if (type == typeof(Texture2D) || result is Texture2D)
                            {
                                result = customTex;
                            }
                            else if (type == typeof(Sprite) || result is Sprite)
                            {
                                // 包装成 Sprite
                                result = Sprite.Create(customTex,
                                    new Rect(0, 0, customTex.width, customTex.height),
                                    new Vector2(0.5f, 0.5f));

                                // 这一步很重要，防止某些脚本通过名字查找资源失败
                                result.name = target + "_custom_sprite";
                            }
                        }
                    }
                }
            }

            // 拦截同步加载
            private static readonly string ExportPath = Path.Combine(Paths.GameRootPath, "BepInEx/plugins/XiaoMiao_ICa/ExportedAssets");// ModDebug_资源导出目录
            [HarmonyPatch(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) })]
            [HarmonyPostfix]
            public static void PostfixSync(string name, Type type, ref UnityEngine.Object __result)
            {
                
                if (XiaoMiaoICaMod.GUI_Bool_BanMosaic2)
                {
                    TryReplace(name, type, ref __result);
                }
                if (GUI_Bool_ModDebug_Export_Resources)
                {
                    UnityEngine.Debug.Log("加载加载资源: " + name);
                    // 1. 基础校验：结果不能为空，且必须是贴图类型
                    if (__result == null || !(__result is Texture2D tex)) return;

                    try
                    {
                        // 2. 确保导出目录存在
                        if (!Directory.Exists(ExportPath)) Directory.CreateDirectory(ExportPath);

                        // 3. 处理文件名（防止路径字符冲突）
                        // 有些资源名带路径，如 assets/ui/logo.png，需要把斜杠替换掉
                        string safeName = name.Replace("/", "_").Replace("\\", "_");
                        if (string.IsNullOrEmpty(safeName)) safeName = tex.name;

                        string saveFileName = Path.Combine(ExportPath, safeName + ".png");

                        // 4. 如果文件已经导出过，就跳过（避免重复读写卡顿）
                        if (File.Exists(saveFileName)) return;

                        // 5. 将贴图转为可读状态并导出
                        // 注意：有些贴图在内存中是不可读的（Read/Write Disabled），直接 Encode 会报错
                        // 我们需要创建一个临时的可读副本
                        RenderTexture tmp = RenderTexture.GetTemporary(
                            tex.width,
                            tex.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);

                        Graphics.Blit(tex, tmp);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = tmp;

                        Texture2D readableTex = new Texture2D(tex.width, tex.height);
                        readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                        readableTex.Apply();

                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(tmp);

                        // 6. 写入文件
                        byte[] bytes = ImageConversion.EncodeToPNG(readableTex);
                        File.WriteAllBytes(saveFileName, bytes);

                        UnityEngine.Object.Destroy(readableTex); // 及时销毁临时对象，防止内存泄漏

                        UnityEngine.Debug.Log($">>> [AssetDump] 成功导出资源: {saveFileName}");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($">>> [AssetDump] 导出 {name} 失败: {ex.Message}");
                    }
                }
            }

            // 拦截异步加载
            [HarmonyPatch(typeof(AssetBundleRequest), "asset", MethodType.Getter)]
            [HarmonyPostfix]
            public static void PostfixAsync(AssetBundleRequest __instance, ref UnityEngine.Object __result)
            {
                if (__result != null && XiaoMiaoICaMod.GUI_Bool_BanMosaic2)
                {
                    TryReplace(__result.name, __result.GetType(), ref __result);
                }
            }
        }

        [HarmonyPatch(typeof(XX.X), "loadDebug")]//XX.X.loadDebug 补丁类
        public class Patch_X_LoadDebug
        {
            public static void GameDeBug(bool i)
            {
                SetBool("announce", i);
                SetBool("timestamp", i);
            }

            public static void SetBool(string name, bool value)
            {
                var type = typeof(XX.X);

                // 优先 DEBUGXXX
                var field =
                    type.GetField("DEBUG" + name.ToUpper(),
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? type.GetField(name,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                {
                    field.SetValue(null, value);
                }
            }
        }
        



        /// <summary>
        /// 导出切入资源
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public bool ExportEmbedResources(string FileName, string Path) // 导出嵌入资源
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


    public class Mod_Noel
    {
        /// <summary>
        /// 获取玩家对象
        /// </summary>
        /// <returns>无返回</returns>
        public static GameObject FindPlayer()//获取玩家
        {
            GameObject player = GameObject.Find("Noel");
            if (player == null)
            {
                return null;
            }
            return player;
        }

        /// <summary>
        /// 设置生命值
        /// </summary>
        /// <param name="HP">第一个数</param>
        /// <param name="玩家对象(可选)">第二个数</param>
        /// <returns>返回bool</returns>
        public static bool SetHp(int HP, GameObject player = null)//设置生命值
        {
            if (player == null)
            {
                player = FindPlayer();
            }
            if (player == null)
            {
                return false;
            }

            PRNoel pr = player.GetComponent<PRNoel>();
            if (pr != null)
            {
                pr.debugSetHp(HP);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置魔力值
        /// </summary>
        /// <param name="MP">第一个数</param>
        /// <param name="玩家对象(可选)">第二个数</param>
        /// <returns>返回bool</returns>
        public static bool SetMp(int MP, GameObject player = null)//设置魔力值
        {
            if (player == null)
            {
                player = FindPlayer();
            }
            if (player == null)
            {
                return false;
            }
            PRNoel pr = player.GetComponent<PRNoel>();
            if (pr != null)
            {
                pr.debugSetMp(MP);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置生命中为最大
        /// </summary>
        /// <returns>返回bool</returns>
        public static bool SetHpMax(GameObject player = null)//生命值回满
        {
            if (player == null)
            {
                player = FindPlayer();
            }
            if (player == null)
            {
                return false;
            }
            PRNoel pr = player.GetComponent<PRNoel>();
            if (pr != null)
            {
                pr.debugSetHp(GetHP());
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置生命中为最大
        /// </summary>
        /// <returns>返回bool</returns>
        public static bool SetMpMax(GameObject player = null)//魔力值回满
        {
            if (player == null)
            {
                player = FindPlayer();
            }
            if (player == null)
            {
                return false;
            }
            PRNoel pr = player.GetComponent<PRNoel>();
            if (pr != null)
            {
                pr.debugSetMp(GetMP());
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前MP
        /// </summary>
        /// <returns>返回当前MP</returns>
        public static int GetMP(GameObject player = null)//获取魔力
        {
            if (player == null)
            {
                player = FindPlayer();
            }

            PRNoel pr = player.GetComponent<PRNoel>();
            return (int)pr.get_hp();
        }

        /// <summary>
        /// 获取当前HP
        /// </summary>
        /// <returns>返回当前HP</returns>
        public static int GetHP(GameObject player = null)//获取魔力
        {
            if (player == null)
            {
                player = FindPlayer();
            }

            PRNoel pr = player.GetComponent<PRNoel>();
            return (int)pr.get_hp();
        }

        /// <summary>
        /// 获取当前最多MP
        /// </summary>
        /// <returns>返回当前最多MP</returns>
        public static int GetMaxMP(GameObject player = null)//获取最大魔力
        {
            if (player == null)
            {
                player = FindPlayer();
            }

            PRNoel pr = player.GetComponent<PRNoel>();
            return (int)pr.get_maxhp();
        }

        /// <summary>
        /// 获取当前最多MP
        /// </summary>
        /// <returns>返回当前最多MP</returns>
        public static int GetManHP(GameObject player = null)//获取最大生命
        {
            if (player == null)
            {
                player = FindPlayer();
            }


            PRNoel pr = player.GetComponent<PRNoel>();
            return (int)pr.get_maxhp();
        }

        /// <summary>
        /// 设置金币
        /// </summary>
        /// <returns>无</returns>
        public static void SetMoney(int desiredCoinAmount)
        {
            // 通过 addCount 方法增加金币
            // 在这个示例中，我们先计算需要增加的金币数
            int currentGold = 0;
            currentGold = (int)nel.CoinStorage.getCount(CoinStorage.CTYPE.GOLD);
            int amountToAdd = desiredCoinAmount - currentGold;

            // 使用 addCount 方法增加金币数量
            if (amountToAdd > 0)
            {
                nel.CoinStorage.addCount(amountToAdd, nel.CoinStorage.CTYPE.GOLD);
            }
            else if (amountToAdd < 0)
            {
                nel.CoinStorage.reduceCount(-amountToAdd, nel.CoinStorage.CTYPE.GOLD);

            }
        }

        public static int test(GameObject player = null)
        {
            if (player == null)
            {
                player = FindPlayer();
            }


            PRNoel pr = player.GetComponent<PRNoel>();
            return (int)0;
        }

    }

    
    public class EventEditor
    {
        public static ActiveDebugger CachedAD;

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

        public bool Send(string Objective,string text)
        {

            using (var client = new NamedPipeClientStream(
                ".",
                Objective,
                PipeDirection.InOut))
            {
                Console.WriteLine($"向 {Objective} 发送" + text);
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
                    string json = JsonConvert.SerializeObject(req);
                    writer.WriteLine(json);

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
                using (var server = new NamedPipeServerStream(
                    Objective,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message))
                {
                    server.WaitForConnection();

                    using (var reader = new StreamReader(server))
                    using (var writer = new StreamWriter(server))
                    {
                        writer.AutoFlush = true;

                        string json = reader.ReadLine();

                        var req = JsonConvert.DeserializeObject<RequestDto>(json);

                        Console.WriteLine("收到命令：" + req.Command);

                        DataJson Json = JsonConvert.DeserializeObject<DataJson>(req.Command);

                        if (Json.Type == "EventEditor_Text")
                        {
                            Console.WriteLine("返回编辑器内容：\n" + Json.Text);
                            run_HaLua(Json.Text);
                        }

                        var resp = new ResponseDto
                        {
                            Success = true,
                            Message = "处理完成"
                        };

                        writer.WriteLine(JsonConvert.SerializeObject(resp));
                    }
                }
            }

            return true;
        }

        public void run_HaLua(string text)
        {
            Thread.Sleep(100);
            try
            {

                // 1 缓存到 EV.Oevt_content
                var fi = typeof(EV).GetField("Oevt_content", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                var evtContent = (System.Collections.Generic.Dictionary<string, string>)fi.GetValue(null);
                evtContent[text] = text;
                Thread.Sleep(100);
                // 2 创建事件读取器并解析执行
                EvReader ER = new EvReader(text, 0, null, null);
                ER.parseText(text);
                EV.stackReader(ER, -1);
                
                UnityEngine.Debug.Log("[CMDExecutor] 脚本执行完成: " + text);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("[CMDExecutor] 执行脚本出错: " + ex);
            }
        }



        public void ForceReloadText()
        {
            try
            {
                // 刷新文本
                TX.reloadFontLetterSpace();
                TX.reloadTx(true);

                // 音效
                if (SND.Ui != null)
                {
                    SND.Ui.play("saved", false);
                }

                //地图场景
                M2DBase instance = M2DBase.Instance;
                if (instance != null)
                {
                    instance.DGN.ColCon.reload();
                    instance.curMap.closeSubMaps(false);
                    instance.curMap.openSubMaps();
                    instance.curMap.fineSubMap();
                    instance.curMap.drawUCol();
                    instance.curMap.drawCheck(0f);
                }
                //刷新debug
                X.loadDebug();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[GameReloader] 文本刷新出错: {e.Message}\n{e.StackTrace}");
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
