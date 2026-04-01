using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
//游戏dll引用
using evt;//unsafeAssem.dll
using HarmonyLib;
using m2d;
using Microsoft.Extensions.Logging;
using nel; //Assembly-CSharp.dll
using nel.title;
using Newtonsoft.Json;
using Octokit;
//
using System;
using System.ClientModel;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Reflection; 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using XX;
//
using static AIC_XiaoMiaoICa_Mod_DLL_BpeInEx6.AI_Chat;
using static AIC_XiaoMiaoICa_Mod_DLL_BpeInEx6.EventEditor;
using static evt.EV;
using static evt.EvDrawerContainer;
using static nel.MatoateReader;
using static nel.NelChipPuzzleBox;
using static nel.UiHkdsChat;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static UnityEngine.GraphicsBuffer;

namespace AIC_XiaoMiaoICa_Mod_DLL_BpeInEx6
{


    [BepInPlugin("AliceinCradle.XiaoMiaoICa.Mod", "AliceinCradle.XiaoMiaoICa.Mod", "3.0.3")]
    public class XiaoMiaoICaMod : BaseUnityPlugin
    {
        
        #region 变量
        //Mod
        string Mod_ver = "3.0.3";
        string Mod_BepInEx_ver = typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString();

        //定义为 Instance
        public static XiaoMiaoICaMod Instance;

        // 配置文件路径
        private string configFilePath = "XiaoMiaoICa_Mod_Data/config.json";

        //game
        string Game_directory = null;
        int Game_PID = 0;
        int Game_lua = 0;


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


        private static bool GUI_Bool_AliceTranslation = false; // 开关_矮人语翻译
        private static string GUI_string_AliceTranslation_text = null; // 开关_矮人语翻译
        private static string[] GUI_Text_AliceTranslation_Tip = { "", ""};
        private static bool GUI_Bool_AliceTranslation_Original_show = false; // 开关_矮人语翻译_显示原文

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

        private static string GUI_TextField_EventEditor_Objective = "chrome";//使用什浏览器
        private static string GUI_TextField_EventEditor_WebUiUrl = "https://api.ica.wiki/AIC/EventEditor/";//打开的网址
        private static string GUI_TextField_EventEditor_RunText = "MSG n_<<<EOF \r\n<c6>你想要试试趴虫墙吗？\r\nEOF;";//使用什浏览器
        private static bool GUI_TextField_EventEditor_bool = false;//是否将输出过来是哈语言不直接执行


        private static string[] GUI_TextField_AIChat_API_url = { "https://tbnx.plus7.plus/v1/chat/completions", "", "", "", "", "", "", "", "", "", "" };//设置url
        private static string[] GUI_TextField_AIChat_API_key = { "sk-h774hLmsrfO03ZIIxBAqC18BgfxAWhynii10y57bPmVV9iBO", "", "", "", "", "", "", "", "", "", "" };//设置key
        private static string[] GUI_TextField_AIChat_API_model = { "gpt-5-mini", "gpt-5-mini", "gpt-5-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini", "gpt-4o-mini" };//设置模型
        private static bool[] GUI_Bool_AIChat_API_Switch = {true, true, true, true, true, true, true, true, true, true, true };//是否启用
        private static string GUI_TextField_AIChat_ChatContent = "你好";//聊天内容
        private static string[] GUI_Text_AIChat_Tip_State = { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };//提示状态
        private static string[] GUI_Text_AIChat_Tip_State_Color = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };//提示状态颜色
        private static string[] GUI_Text_AIChat_Tip_Content = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        private int GUI_AIChat_Config_List = 0;//配置列表
        private bool GUI_AIChat_Loading = false;//是否正在请求


        private Vector2 svPos; // 界面滑动条

        #endregion
        void Awake()
        {
            string gamever = Get_Game_Ver();
            string modgamedllver = "0.29i";
            if (gamever!= modgamedllver)
            {
                Process.Start("powershell.exe", $"-command \"[System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms'); [System.Windows.Forms.MessageBox]::Show('mod与编译时游戏的dll版本不匹配，如果出现报错，安装最新版游戏或者安装mod适配的游戏版本在尝试！\n当前游戏版本:{gamever}\nmod编译时游戏的版本:{modgamedllver}', '欧尼酱~这是兼容性提示~', [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)\"");
            }


            Instance = this;
            // 自动加载所有带有 [HarmonyPatch] 特性的类
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            //Harmony.CreateAndPatchAll(typeof(XiaoMiaoICaMod));

            var harmony = new Harmony("com.xiaomiao.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            int count = 0;
            foreach (var method in harmony.GetPatchedMethods()) count++;
            XiaoMiaoICaMod.Instance.Logger.LogInfo($">>> 成功加载了 " + count + " 个补丁方法");

        }

        void Start()//启动
        {
            
            UnityEngine.Debug.Log("Test1");// Unity输出 灰色
            Logger.LogError("Test2");// 错误 
            Logger.LogFatal("Test3");//致命 淡红色
            Logger.LogWarning("Test4");//警告 黄色
            Logger.LogInfo("Test6");//信息 灰色            Logger.LogMessage("Test5");//消息 白色
            Logger.LogDebug("Test7");//调试

            Task.Run(() =>
            {
                new EventEditor().Receive("MiaoAicMod_Mod");
            });//启动服务
            


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

            if (System.IO.File.Exists(Path.Combine(Game_directory, "BepInEx", "plugins", "Newtonsoft.Json.dll")) == false)//导出Nwetonsoft.Json.dll
            {
                string dllPath = Path.Combine(Game_directory, "BepInEx", "plugins", "Newtonsoft.Json.dll");

                Directory.CreateDirectory(Path.GetDirectoryName(dllPath));

                using (Stream s = typeof(XiaoMiaoICaMod).Assembly.GetManifestResourceStream("Alice_in_Cradle_XiaoMiaoICa_of_Mod.DLL.Newtonsoft.Json.dll"))
                using (FileStream f = System.IO.File.Create(dllPath))
                {
                    if (s != null)
                        s.CopyTo(f);
                }
                //重启游戏
                {

                    string exePath = Path.Combine(Game_directory, "AliceInCradle.exe");
                    string gameDir = Path.GetDirectoryName(exePath);

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";

                    startInfo.Arguments = $"/C timeout /t 2 /nobreak & start \"\" \"{exePath}\"";

                    startInfo.WorkingDirectory = gameDir;
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;

                    startInfo.EnvironmentVariables.Remove("DOORSTOP_DISABLE");
                    startInfo.EnvironmentVariables.Remove("DOORSTOP_INITIALIZED");
                    startInfo.EnvironmentVariables.Remove("BEPINEX_BOOTSTRAP");

                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("重启失败: " + e.Message);
                    }

                    Process.GetCurrentProcess().Kill();
                }
            }


            var exportMap = new Dictionary<string, (string Dir, string OutputName)>
            {
                // 资源名                         // 导出目录                               // 导出后的文件名
                { "DLL.Newtonsoft.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins"), "Newtonsoft.Json.dll") },
                { "Data.ExportedAssets.__events_restroom.pxls.bytes.texture_0.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "__events_restroom.pxls.bytes.texture_0") },
                { "Data.ExportedAssets.key_noel.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "key_noel") },
                { "Data.ExportedAssets.__events_2weekattack.pxls.bytes.texture_0.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "__events_2weekattack.pxls.bytes.texture_0") },
                { "Data.ExportedAssets.title_logo.png", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" , "Resources"), "title_logo") },
                //事件编辑器相关文件
                //{ "Data.EventEditorModMiddleware.exe", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "EventEditorModMiddleware.exe") },
                //{ "DLL.Microsoft.Bcl.AsyncInterfaces.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.Bcl.AsyncInterfaces.dll") },
                //{ "DLL.Microsoft.CSharp.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.CSharp.dll") },
                //{ "DLL.Microsoft.Playwright.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Microsoft.Playwright.dll") },
                //{ "DLL.Newtonsoft.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "Newtonsoft.Json.dll") },
                //{ "DLL.System.Buffers.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Buffers.dll") },
                //{ "DLL.System.ComponentModel.Annotations.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.ComponentModel.Annotations.dll") },
                //{ "DLL.System.ComponentModel.DataAnnotations.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.ComponentModel.DataAnnotations.dll") },
                //{ "DLL.System.Data.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Data.dll") },
                //{ "DLL.System.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.dll") },
                //{ "DLL.System.IO.Pipelines.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.IO.Pipelines.dll") },
                //{ "DLL.System.Memory.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Memory.dll") },
                //{ "DLL.System.Net.Http.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Net.Http.dll") },
                //{ "DLL.System.Numerics.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Numerics.dll") },
                //{ "DLL.System.Numerics.Vectors.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Numerics.Vectors.dll") },
                //{ "DLL.System.Runtime.CompilerServices.Unsafe.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Runtime.CompilerServices.Unsafe.dll") },
                //{ "DLL.System.Text.Encodings.Web.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Text.Encodings.Web.dll") },
                //{ "DLL.System.Text.Json.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Text.Json.dll") },
                //{ "DLL.System.Threading.Tasks.Extensions.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Threading.Tasks.Extensions.dll") },
                //{ "DLL.System.Xml.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Xml.dll") },
                //{ "DLL.System.Xml.Linq.dll", (Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa" ), "System.Xml.Linq.dll") },
            };

            foreach (var item in exportMap)
            {
                string resourceFile = item.Key;
                string exportDir = item.Value.Dir;
                string outputName = item.Value.OutputName;

                string targetPath = Path.Combine(exportDir, outputName);

                Directory.CreateDirectory(exportDir);

                resourceFile = "Alice_in_Cradle_XiaoMiaoICa_of_Mod." + resourceFile;

                if (System.IO.File.Exists(targetPath))
                {
                    //Console.WriteLine($"文件已存在：{targetPath}");
                }
                else
                {
                    Console.WriteLine($"导出 {resourceFile} → {targetPath}");

                    var assembly = typeof(XiaoMiaoICaMod).Assembly; // 如果是BepInEx插件建议这样

                    using (Stream stream = assembly.GetManifestResourceStream(resourceFile))
                    {
                        if (stream == null)
                        {
                            Console.WriteLine($"找不到资源：{resourceFile}");
                            continue;
                        }

                        using (FileStream fs = new FileStream(targetPath, System.IO.FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fs);
                        }
                    }

                }
            }



            #endregion
            #region 解压事件管理器
            if (System.IO.File.Exists(Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "EventEditorModMiddleware.exe")) == false)
            {
                Logger.LogMessage("解压事件管理器");
                ExtractEmbeddedZip("Alice_in_Cradle_XiaoMiaoICa_of_Mod.Data.EventEditorModMiddleware.zip", Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa"));
            }

            #endregion
            #region 读取配置文件
            string configPath = Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "preferences");

            // 辅助方法：简化布尔值读取
            bool ReadBool(string key, bool defaultValue)
            {
                string val = M_EF.Config_Read(configPath, key);
                return string.IsNullOrEmpty(val) ? defaultValue : val == "True";
            }

            // 布尔值读取
            GUI_Bool_BanMosaic = ReadBool("GUI_Bool_BanMosaic", false);
            GUI_Bool_BanMosaic2 = ReadBool("GUI_Bool_BanMosaic2", false);
            GUI_Bool_AliceTranslation = ReadBool("GUI_Bool_AliceTranslation", false);
            GUI_Bool_AliceTranslation_Original_show = ReadBool("GUI_Bool_AliceTranslation_Original_show", false);
            GUI_Bool_NOApplyDamage = ReadBool("GUI_Bool_NOApplyDamage", false);
            GUI_Bool_NOApplyDamage2 = ReadBool("GUI_Bool_NOApplyDamage2", false);
            GUI_Bool_SetHP = ReadBool("GUI_Bool_SetHP", false);
            GUI_Bool_SetMP = ReadBool("GUI_Bool_SetMP", false);
            GUI_Bool_Debug = ReadBool("GUI_Bool_Debug", false);
            GUI_Bool_ModDebug = ReadBool("GUI_Bool_ModDebug", false);
            GUI_Bool_ModDebug_Export_Resources = ReadBool("GUI_Bool_ModDebug_Export_Resources", false);
            GUI_Bool_ModDebug_Noel_info = ReadBool("GUI_Bool_ModDebug_Noel_info", false);
            GUI_TextField_EventEditor_bool = ReadBool("GUI_TextField_EventEditor_bool", false);

            // 字符串读取
            string temp;
            if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_Money"))) GUI_TextField_Money = temp;
            if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_SetHP"))) GUI_TextField_SetHP = temp;
            if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_SetMP"))) GUI_TextField_SetMP = temp;
            if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_Time"))) GUI_TextField_Time = temp;
            //if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_EventEditor_Objective"))) GUI_TextField_Objective = temp;
            //if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_EventEditor_WebUiUrl"))) GUI_TextField_WebUiUrl = temp;
            //if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_EventEditor_RunText"))) GUI_TextField_RunText = temp;
            //if (!string.IsNullOrEmpty(temp = M_EF.Config_Read(configPath, "GUI_TextField_AIChat_ChatContent"))) GUI_TextField_ChatContent = temp;

            //  AI 聊天配置数组
            for (int i = 0; i < GUI_TextField_AIChat_API_url.Length; i++)
            {
                string url = M_EF.Config_Read(configPath, $"AIChat_URL_{i}");
                if (!string.IsNullOrEmpty(url)) GUI_TextField_AIChat_API_url[i] = url;

                string key = M_EF.Config_Read(configPath, $"AIChat_Key_{i}");
                if (!string.IsNullOrEmpty(key)) GUI_TextField_AIChat_API_key[i] = key;

                string model = M_EF.Config_Read(configPath, $"AIChat_Model_{i}");
                if (!string.IsNullOrEmpty(model)) GUI_TextField_AIChat_API_model[i] = model;

                string sw = M_EF.Config_Read(configPath, $"AIChat_Switch_{i}");
                if (!string.IsNullOrEmpty(sw)) GUI_Bool_AIChat_API_Switch[i] = (sw == "True");
            }

            // 矮人语提示数组
            for (int i = 0; i < GUI_Text_AliceTranslation_Tip.Length; i++)
            {
                string tip = M_EF.Config_Read(configPath, $"AliceTip_{i}");
                if (!string.IsNullOrEmpty(tip)) GUI_Text_AliceTranslation_Tip[i] = tip;
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
            #region 循环
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    time_1000ms();
                }
            });

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

        void Update()//帧
        {
            if (System.IO.File.Exists(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd")) == true)
            {

                new EventEditor().run_HaLua(System.IO.File.ReadAllText(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd")));
                System.IO.File.Delete(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd"));//删除文件
            }

            //触发点击按键
            if (Input.GetKeyDown(KeyCode.P))
            {

            }
            //if (Input.GetKeyDown(KeyCode.Tab)) // 替换快捷键
            //{
            //    showWindow = !showWindow; // 切换窗口显示状态
            //}
            if (Input.GetKeyDown(toggleKey))
            {
                showWindow = !showWindow; // 切换窗口显示状态
            }
        }

        void OnApplicationQuit()
        {
            Logger.LogInfo("游戏即将退出");

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C timeout /t 5 /nobreak >nul && taskkill /PID {Game_PID} /F",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }


        void time_1000ms()
        {
            if (System.IO.File.Exists(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd")) == true)
            {


                new EventEditor().run_HaLua(System.IO.File.ReadAllText(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd")));
                System.IO.File.Delete(Path.Combine(Game_directory, "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd"));//删除文件

            }
        }//1秒执行一次

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

            #region 矮人语翻译
            GUILayout.BeginHorizontal(GUI.skin.box);//横排
            GUILayout.BeginVertical();//竖排
            GUI_Bool_AliceTranslation = GUILayout.Toggle(GUI_Bool_AliceTranslation, "翻译矮人语");
            GUI_Bool_AliceTranslation_Original_show = GUILayout.Toggle(GUI_Bool_AliceTranslation_Original_show, "显示原文");
            if (GUI_Bool_AliceTranslation == true)
            {
                if (GUI_string_AliceTranslation_text == null)
                {
                    string targetDirectory = "";
                    if (Get_Game_Lua() == 0)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                    }
                    else if (Get_Game_Lua() == 1)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                    }
                    else if (Get_Game_Lua() == 2)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                    }
                    else if (Get_Game_Lua() == 3)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "zh-cn");
                    }
                    else if (Get_Game_Lua() == 4)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "zh-tc");
                    }
                    else if (Get_Game_Lua() == 5)
                    {
                        targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "_");
                    }
                    GUI_string_AliceTranslation_text = ReadAllTxtFiles(targetDirectory);
                }
            }
            GUILayout.Label("当前语言ID:"+ Get_Game_Lua()); // 文字
            GUILayout.Label("必须同步一次仓库后才能正常使用！此过程可能想要科学上网环境！"); // 文字
            if (GUILayout.Button("从github仓库同步代码")) // 按钮
            {
                string url = "https://github.com/Muki0607/DwarfInCradleTranslation/archive/refs/heads/main.zip";
                string saveLocation = @"D:\DwarfInCradleTranslation_Latest.zip";
                Task.Run(() => DownloadAndExtractAsync(url, Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation")));
            }


            GUILayout.Label("如果翻译的语言不是你所用的语言请返回标题点击刷新按钮"); // 文字
            if (GUILayout.Button("刷新语言文件")) // 按钮
            {

                string targetDirectory = "";
                if (Get_Game_Lua() == 0)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                }
                else if (Get_Game_Lua() == 1)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                }
                else if (Get_Game_Lua() == 2)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "en");
                }
                else if (Get_Game_Lua() == 3)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "zh-cn");
                }
                else if (Get_Game_Lua() == 4)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "zh-tc");
                }
                else if (Get_Game_Lua() == 5)
                {
                    targetDirectory = Path.Combine(Game_directory, "BepInEx", "plugins", "XiaoMiao_ICa", "DwarfInCradleTranslation", "localization", "_");
                }
                GUI_string_AliceTranslation_text = ReadAllTxtFiles(targetDirectory);
            }

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            {
                GUIStyle myTextAreaStyle = new GUIStyle(GUI.skin.textArea);
                myTextAreaStyle.wordWrap = true;  // 开启自动换行
                myTextAreaStyle.padding = new RectOffset(5, 5, 5, 5); // 内边距

                GUILayout.Label("上一次文字处理"); // 文字
                GUILayout.Label("原文:"); // 文字
                GUI_Text_AliceTranslation_Tip[0] = GUILayout.TextArea(
                    GUI_Text_AliceTranslation_Tip[0],
                    myTextAreaStyle,
                    GUILayout.MinHeight(50) // 最小高度
                );
                GUILayout.Label("翻译:"); // 文字
                GUI_Text_AliceTranslation_Tip[1] = GUILayout.TextArea(
                    GUI_Text_AliceTranslation_Tip[1],
                    myTextAreaStyle,
                    GUILayout.MinHeight(50) // 最小高度
                );
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            GUILayout.Label("为翻译工作做出贡献的全部创厨圣！"); // 文字

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("苍木羽Muki", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/332720975") { UseShellExecute = true });
            }
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/Muki0607") { UseShellExecute = true });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("DreamRuthenium", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/13347218") { UseShellExecute = true });
            }
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/DreamRuthenium") { UseShellExecute = true });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("普莉姆拉老师", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/399329257") { UseShellExecute = true });
            }
            if (GUILayout.Button("GitHub")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://github.com/cocoAutumn") { UseShellExecute = true });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("煤球_Officia", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/3461563767851138") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("泡花茶的一只猹", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/699059614") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("我是绵羊Yang_g", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/43881503") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("凌空の猫", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/448512891") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("星文_whrite", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/1818237152") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("左旋苏打", GUILayout.Width(250)); // 文字
            if (GUILayout.Button("bilibili")) // 按钮
            {
                Process.Start(new ProcessStartInfo("https://space.bilibili.com/3337754") { UseShellExecute = true });
            }
            //if (GUILayout.Button("GitHub")) // 按钮
            //{
            //    Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
            //}
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

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
TALKER n CCL 
PIC   n a_1/a00L3R3__F1__f1__m1__b1__u1    
MSG n_<<<EOF 
<c6>你需要返回主页重新读档才会生效！
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
TALKER n CCL 
PIC   n a_1/a00L3R3__F1__f1__m1__b1__u1    
MSG n_<<<EOF 
<c6>正在启动事件管理器
<c6>等待几秒后会弹出浏览器窗口
*
<c2>事件管理器还在测试
<c2>有可能会出现bug
<c2>还请谅解~
EOF;
");
                Task.Run(() =>
                {


                    Task.Run(() =>
                    {
                        ;
                        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Game_directory + "\\BepInEx\\plugins\\XiaoMiao_ICa\\EventEditorModMiddleware.exe");

                        if (!System.IO.File.Exists(exePath))
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
                        Objective = GUI_TextField_EventEditor_Objective,
                        EditorUrl = GUI_TextField_EventEditor_WebUiUrl,
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
                    Objective = GUI_TextField_EventEditor_Objective,
                    EditorUrl = GUI_TextField_EventEditor_WebUiUrl,
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
                GUI_TextField_EventEditor_Objective = "chrome";
            }
            if (GUILayout.Button("microsoft Edge"))
            {
                GUI_TextField_EventEditor_Objective = "msedge";
            }
            GUILayout.EndHorizontal();


            GUI_TextField_EventEditor_Objective = GUILayout.TextField(GUI_TextField_EventEditor_Objective);

            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("使用那个镜像站"); // 文字
            if (GUILayout.Button("普莉姆拉主站"))
            {
                GUI_TextField_EventEditor_WebUiUrl = "https://aic.imtfe.org/AicEventEditor/";
            }
            if (GUILayout.Button("本苗镜像站"))
            {
                GUI_TextField_EventEditor_WebUiUrl = "https://api.ica.wiki/AIC/EventEditor";
            }
            GUILayout.EndHorizontal();
            GUI_TextField_EventEditor_WebUiUrl = GUILayout.TextField(GUI_TextField_EventEditor_WebUiUrl);


            GUILayout.BeginHorizontal();//横排
            GUILayout.Label("刷新游戏的文本数据");
            if (GUILayout.Button("刷新数据"))
            {
                new EventEditor().ForceReloadText();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);//竖排
            var GUI_TextField_EventEditor_RunText_s = GUI.skin.textArea;
            GUI_TextField_EventEditor_RunText = GUILayout.TextArea(GUI_TextField_EventEditor_RunText, GUILayout.Height(GUI_TextField_EventEditor_RunText_s.CalcHeight(new GUIContent(GUI_TextField_EventEditor_RunText), 300))
            );
            GUI_TextField_EventEditor_bool = GUILayout.Toggle(GUI_TextField_EventEditor_bool, "不直接执行传递过来的《哈语言》。");
            if (GUILayout.Button("执行")) // 按钮
            {
                new EventEditor().run_HaLua(GUI_TextField_EventEditor_RunText);
            }
            GUILayout.EndHorizontal();

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
            }



            GUILayout.EndHorizontal();
            #endregion

            #region AI Chat
            GUILayout.BeginVertical(GUI.skin.box);//竖排




            // 1. 先绘制滑动条，控制当前处于哪一组

            GUILayout.BeginVertical("box");
            GUILayout.BeginVertical("box");
            GUILayout.Label($"当前正在编辑第 {GUI_AIChat_Config_List} 组配置 (滑动切换)");

            // 动态获取数组最大长度，防止越界。记得减 1 因为索引从 0 开始
            int maxIndex = GUI_TextField_AIChat_API_url.Length -1;

            // 滑动条
            GUI_AIChat_Config_List = (int)Mathf.Round(GUILayout.HorizontalSlider(GUI_AIChat_Config_List, 0, maxIndex));

            GUILayout.Space(10);
            GUILayout.EndVertical();

            if (GUI_AIChat_Config_List == 0)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』对话参数配置");
            }else if (GUI_AIChat_Config_List == 1)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』文字添色配置");
            }
            else if (GUI_AIChat_Config_List == 2)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』立绘处理");
            }
            else if(GUI_AIChat_Config_List == 3)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』打开界面处理");
            }
            else if(GUI_AIChat_Config_List == 4)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』切换法杖处理");
            }
            else if(GUI_AIChat_Config_List == 5)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』四字棋处理");
            }
            else if (GUI_AIChat_Config_List == 6)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』给予物品处理");
            }
            else if (GUI_AIChat_Config_List == 7)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』修改危险度处理");
            }
            else if (GUI_AIChat_Config_List == 8)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』给予金币处理");
            }
            else if (GUI_AIChat_Config_List == 9)
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』修改音乐处理");
            }
            else
            {
                GUILayout.Label($"『{GUI_AIChat_Config_List}:的详细信息』未知");
            }
            GUILayout.Space(5);

            GUI_Bool_AIChat_API_Switch[GUI_AIChat_Config_List] = GUILayout.Toggle(GUI_Bool_AIChat_API_Switch[GUI_AIChat_Config_List], "是否启用");

            GUILayout.Space(5);

            GUILayout.Label("请求的URL:");
            GUI_TextField_AIChat_API_url[GUI_AIChat_Config_List] = GUILayout.TextField(GUI_TextField_AIChat_API_url[GUI_AIChat_Config_List]);
            GUILayout.Space(5);

            GUILayout.Label("API密钥:");
            GUI_TextField_AIChat_API_key[GUI_AIChat_Config_List] = GUILayout.TextField(GUI_TextField_AIChat_API_key[GUI_AIChat_Config_List]);
            GUILayout.Space(5);

            GUILayout.Label("模型名:");
            GUI_TextField_AIChat_API_model[GUI_AIChat_Config_List] = GUILayout.TextField(GUI_TextField_AIChat_API_model[GUI_AIChat_Config_List]);
            GUILayout.Space(5);
            GUILayout.Label("\n请勿滥用公共APIkey！本苗会看情况在有空余财力对公共AIP密钥进行续费。", new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(1.0f, 0.2f, 0.0f) } });

            GUILayout.EndVertical();





            GUILayout.Space(20);

            GUILayout.Label("对话内容:");
            GUI_TextField_AIChat_ChatContent = GUILayout.TextField(GUI_TextField_AIChat_ChatContent);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();//横排
            GUI.enabled = !GUI_AIChat_Loading;
            if (GUILayout.Button("发起对话"))
            {
                new AI_Chat().SendChatAsync_Segmentation(GUI_TextField_AIChat_ChatContent, GUI_TextField_AIChat_API_url, GUI_TextField_AIChat_API_key, GUI_TextField_AIChat_API_model);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);



            GUILayout.BeginVertical(GUI.skin.box);//竖排

            // 循环绘制 
            for (int i = 0; i < GUI_Text_AIChat_Tip_State.Length; i++)
            {
                GUIStyle myLabelStyle = new GUIStyle(GUI.skin.label);

                GUIStyle myTextAreaStyle = new GUIStyle(GUI.skin.textArea);
                if (GUI_Bool_ModDebug == true)
                {
                    myTextAreaStyle.wordWrap = true;  // 开启自动换行
                    myTextAreaStyle.padding = new RectOffset(5, 5, 5, 5); // 内边距
                }

                // 解析十六进制颜色
                Color myColor;
                if (!ColorUtility.TryParseHtmlString(GUI_Text_AIChat_Tip_State_Color[i], out myColor))
                {
                    myColor = Color.white; // 默认显示白色
                }

                // 应用颜色到 Label 样式
                myLabelStyle.normal.textColor = myColor;

                // 绘制标题（Label）
                GUILayout.Label(GUI_Text_AIChat_Tip_State[i], myLabelStyle);


                if (GUI_Bool_ModDebug == true)
                {
                    // 绘制自动换行输入框（TextArea）
                    GUI_Text_AIChat_Tip_Content[i] = GUILayout.TextArea(
                        GUI_Text_AIChat_Tip_Content[i],
                        myTextAreaStyle,
                        GUILayout.MinHeight(50) // 最小高度
                    );
                }

                if (GUI_Bool_ModDebug == true)
                {
                    // 组与组之间的间距
                    GUILayout.Space(10);
                }
            }

            GUILayout.EndHorizontal();
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


                if (GUILayout.Button("测试 TX.changeFamily(\"zh-cn\")"))
                {
                    TX.changeFamily("zh-cn");//切换语言
                }
                if (GUILayout.Button("测试 TX.changeFamily(\"jp\")"))
                {
                    TX.changeFamily("jp");//切换语言
                }
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
            if (GUILayout.Button("bilibili")) // 按钮
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

        public bool Get_GUI_TextField_EventEditor_bool()//获取事件编辑器不直接执行哈语言选项
        {
            return GUI_TextField_EventEditor_bool;
        }
        public void Set_GUI_TextField_EventEditor_RunText(string value)//设置事件编辑器哈语言文本框内容
        {
            GUI_TextField_EventEditor_RunText = value;
        }
        public void Set_GUI_Text_AIChat_Tip(string State, string State_Color, string Content, int entry)
        {
            GUI_Text_AIChat_Tip_State[entry] = State;
            GUI_Text_AIChat_Tip_State_Color[entry] = State_Color;
            GUI_Text_AIChat_Tip_Content[entry] = Content;
        }
        public void Set_GUI_Text_AIChat_Loading(bool Loading)//设置AI对话界面是否处于加载中状态
        {
            GUI_AIChat_Loading = Loading;
        }
        public bool Get_GIU_Bool_AIChat_Shitch(int i)
        {
            return GUI_Bool_AIChat_API_Switch[i];
        }
        public string Get_Game_directory()//获取游戏路径
        {
            return Game_directory;
        }
        public int Get_Game_PID()//获取游戏PID
        {
            return Game_PID;
        }

        public string Get_Game_Ver()// 获取游戏版本
        {
            return UnityEngine.Application.version;
        }

        public int Get_Game_Lua()
        {
            // 1. 获取当前场景中的 SceneTitleTemp 实例
            SceneTitleTemp titleScene = GameObject.FindObjectOfType<SceneTitleTemp>();
            if (titleScene == null)
            {
                //UnityEngine.Debug.Log("[XiaoMiaoMod] 未找到 SceneTitleTemp 实例，请确保处于标题界面。");
                return Game_lua;
            }

            // 2. 使用反射获取私有字段 ABtLang
            // BindingFlags 说明：NonPublic (私有), Instance (实例变量), Public (以防万一它是公有的)
            FieldInfo field = typeof(SceneTitleTemp).GetField("ABtLang", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (field == null)
            {
                //UnityEngine.Debug.LogError("[XiaoMiaoMod] 无法在 SceneTitleTemp 中找到字段 'ABtLang'，请检查字段名是否正确。");
                return Game_lua;
            }

            // 3. 将获取到的值转换为对应的数组类型
            ButtonSkinRowLangNel[] abtLangArray = field.GetValue(titleScene) as ButtonSkinRowLangNel[];

            if (abtLangArray != null)
            {
                int num = abtLangArray.Length;
                //UnityEngine.Debug.Log(string.Format("[XiaoMiaoMod] 开始分析语言按钮，共找到 {0} 个", num));

                for (int j = 0; j < num; j++)
                {
                    // 获取数组中的皮肤对象
                    ButtonSkinRowLangNel buttonSkinRowLangNel = abtLangArray[j];
                    if (buttonSkinRowLangNel == null) continue;

                    // 获取内部按钮组件 (aBtn)
                    var btn = buttonSkinRowLangNel.getBtn();
                    if (btn == null) continue;


                    // 打印按钮信息 (使用 string.Concat 或 string.Format 兼容 C# 7.3)
                    //UnityEngine.Debug.Log((buttonSkinRowLangNel != null ? buttonSkinRowLangNel.ToString() : "null"));

                    // 判断该按钮代表的语言是否为当前系统语言
                    bool isChecked = TX.familyIs(btn.title);
                    //UnityEngine.Debug.Log("状态: " + isChecked.ToString());
                    // 核心动作：设置按钮的选中状态
                    // 参数1: 是否选中, 参数2: 是否触发即时反馈动画
                    btn.SetChecked(isChecked, true);
                    // --- 你的逻辑结束 ---
                    if (isChecked)
                    {
                        Game_lua = j;
                        return j;
                    }
                }
            }
            else
            {
                //UnityEngine.Debug.Log("[XiaoMiaoMod] ABtLang 数组为空或转换失败。");
            }
            return Game_lua;
        }

        public void Set_Mod_Lua(int lua)
        {
            if (lua != -1)
            {
                return;
            }
            else
            {
                Game_lua = lua;
                lua_Refresh(Get_Game_Lua());
            }
        }

        public void lua_Refresh(int id)
        {

        }

        public static void SavepreferencesConfig()//保存配置
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "XiaoMiaoICa_Mod_Data", "preferences");

            // --- 基础布尔值与字符串 ---
            M_EF.Config_Write(configPath, "GUI_Bool_BanMosaic", GUI_Bool_BanMosaic.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_BanMosaic2", GUI_Bool_BanMosaic2.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_AliceTranslation", GUI_Bool_AliceTranslation.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_AliceTranslation_Original_show", GUI_Bool_AliceTranslation_Original_show.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_NOApplyDamage", GUI_Bool_NOApplyDamage.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_NOApplyDamage2", GUI_Bool_NOApplyDamage2.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_SetHP", GUI_Bool_SetHP.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_SetMP", GUI_Bool_SetMP.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_Debug", GUI_Bool_Debug.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_ModDebug", GUI_Bool_ModDebug.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_ModDebug_Export_Resources", GUI_Bool_ModDebug_Export_Resources.ToString());
            M_EF.Config_Write(configPath, "GUI_Bool_ModDebug_Noel_info", GUI_Bool_ModDebug_Noel_info.ToString());
            M_EF.Config_Write(configPath, "GUI_TextField_EventEditor_bool", GUI_TextField_EventEditor_bool.ToString());

            M_EF.Config_Write(configPath, "GUI_TextField_Money", GUI_TextField_Money);
            M_EF.Config_Write(configPath, "GUI_TextField_SetHP", GUI_TextField_SetHP);
            M_EF.Config_Write(configPath, "GUI_TextField_SetMP", GUI_TextField_SetMP);
            M_EF.Config_Write(configPath, "GUI_TextField_Time", GUI_TextField_Time);
            M_EF.Config_Write(configPath, "GUI_TextField_EventEditor_Objective", GUI_TextField_EventEditor_Objective);
            M_EF.Config_Write(configPath, "GUI_TextField_EventEditor_WebUiUrl", GUI_TextField_EventEditor_WebUiUrl);
            M_EF.Config_Write(configPath, "GUI_TextField_EventEditor_RunText", GUI_TextField_EventEditor_RunText);
            M_EF.Config_Write(configPath, "GUI_TextField_AIChat_ChatContent", GUI_TextField_AIChat_ChatContent);

            // 数组类型处理 
            // AI 聊天配置
            for (int i = 0; i < GUI_TextField_AIChat_API_url.Length; i++)
            {
                M_EF.Config_Write(configPath, $"AIChat_URL_{i}", GUI_TextField_AIChat_API_url[i]);
                M_EF.Config_Write(configPath, $"AIChat_Key_{i}", GUI_TextField_AIChat_API_key[i]);
                M_EF.Config_Write(configPath, $"AIChat_Model_{i}", GUI_TextField_AIChat_API_model[i]);
                M_EF.Config_Write(configPath, $"AIChat_Switch_{i}", GUI_Bool_AIChat_API_Switch[i].ToString());
            }

            // 矮人语翻译提示 (Tip 数组)
            for (int i = 0; i < GUI_Text_AliceTranslation_Tip.Length; i++)
            {
                M_EF.Config_Write(configPath, $"AliceTip_{i}", GUI_Text_AliceTranslation_Tip[i]);
            }
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


        public bool GetHub_Update_AliceTranslation()//获取事件编辑器不直接执行哈语言选项
        {

            return true;
        }

        public static void ExtractEmbeddedZip(string resourceName, string outputDir)//解压嵌入的zip文件
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream zipStream = asm.GetManifestResourceStream(resourceName);

            if (zipStream == null)
                throw new Exception("找不到嵌入 ZIP：" + resourceName);

            using (zipStream)
            using (ZipArchive archive = new ZipArchive(zipStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullPath = Path.Combine(outputDir, entry.FullName);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    using (Stream entryStream = entry.Open())
                    using (FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create))
                    {
                        entryStream.CopyTo(fs);
                    }
                }
            }
        }

        public static string ReadAllTxtFiles(string rootPath)//读取指定目录及其子目录下的所有txt文件内容并返回一个字符串
        {
            if (!Directory.Exists(rootPath))
            {
                return "目录不存在";
            }

            // 1. 获取所有 .txt 文件路径（包含子目录）
            // SearchOption.AllDirectories 是递归查找的关键
            string[] filePaths = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories);

            // 2. 使用 StringBuilder 高效拼接大量字符串
            StringBuilder sb = new StringBuilder();

            foreach (string filePath in filePaths)
            {
                try
                {
                    // 读取文件内容并追加到 StringBuilder
                    string content = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                    sb.AppendLine($"--- 文件来源: {filePath} ---");
                    sb.AppendLine(content);
                    sb.AppendLine(); // 换行符隔离
                }
                catch (Exception ex)
                {
                    // 预防由于权限或文件占用导致的错误
                    //Console.WriteLine($"读取失败 {filePath}: {ex.Message}");
                }
            }

            return sb.ToString();
        }
        static async Task DownloadAndExtractAsync(string url, string destinationDirectory)//从网络下载zip并解压到指定目录
        {
            using (HttpClient client = new HttpClient())
            {
                // GitHub 必须设置 User-Agent
                client.DefaultRequestHeaders.Add("User-Agent", "CSharp-Memory-Extractor");

                Console.WriteLine("正在从网络读取数据流...");

                // 1. 发起请求并获取响应流
                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    // 2. 将网络流读入内存流 (MemoryStream)
                    // 注意：ZipArchive 需要流支持 Seek（随机访问），而网络流通常不支持，
                    // 所以必须先缓存到内存中。
                    using (var memoryStream = new MemoryStream())
                    {
                        await response.Content.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // 重置指针到起始位置

                        // 3. 直接从内存流创建 ZipArchive
                        using (ZipArchive archive = new ZipArchive(memoryStream))
                        {
                            Console.WriteLine($"开始解压 {archive.Entries.Count} 个条目...");

                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                // 技巧：跳过 GitHub 自动生成的顶层文件夹 (DwarfInCradleTranslation-main/)
                                string relativePath = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);
                                if (string.IsNullOrEmpty(relativePath)) continue;

                                string fullPath = Path.Combine(destinationDirectory, relativePath);

                                // 如果是文件夹条目
                                if (entry.FullName.EndsWith("/"))
                                {
                                    Directory.CreateDirectory(fullPath);
                                }
                                else
                                {
                                    // 确保父目录存在
                                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                    // 解压文件
                                    entry.ExtractToFile(fullPath, overwrite: true);
                                    Console.WriteLine($"已解压: {relativePath}");
                                }
                            }
                        }
                    }
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

        //[HarmonyPatch] //  监听游戏XX.ActiveDebugger.runIRD方法 
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

        [HarmonyPatch] //   监听游戏XX.TX.readTextsAt方法 读取语言文件
        public static class XX_TX_readTextsAt_Patch
        {
            [HarmonyTargetMethod]// 目标
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(TX), "readTextsAt", new Type[] { typeof(string) });
            }
            [HarmonyPrefix]// 前置
            public static bool Prefix(string key)
            {
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.TX.readTextsAt 前置成功命中 拦截此方法执行"); 
                


                // 1. 反射获取私有静态字段 OTxFam
                var OTxFamField = AccessTools.Field(typeof(TX), "OTxFam");
                var OTxFamValue = OTxFamField.GetValue(null) as IDictionary;

                // 2. 反射获取私有静态方法 readTexts
                var readTextsMethod = AccessTools.Method(typeof(TX), "readTexts", new Type[] { typeof(string), typeof(TX.TXFamily) });

                if (OTxFamValue == null || readTextsMethod == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError("无法反射获取 OTxFam 或 readTexts 方法！");
                    return true; // 反射失败则运行原逻辑
                }

                bool flag = false;
                if (TX.isStart(key, "!", 0))
                {
                    flag = true;
                    key = TX.slice(key, 1);
                }

                // 3. 遍历 OTxFam
                foreach (DictionaryEntry entry in OTxFamValue)
                {
                    string langKey = entry.Key as string;
                    TX.TXFamily family = entry.Value as TX.TXFamily;

                    if (!flag || langKey == "_")
                    {
                        // 构建路径: localization/zh/zhItems.txt (假设 key 是 Items)
                        string folderPath = Path.Combine("localization", langKey);
                        string fileName = langKey + key + ".txt";
                        string fullPath = Path.Combine(folderPath, fileName);


                        //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] Key=" + key + "    " + fileName);
                        if (fileName == "ev_mountain.txt")
                        {
                            XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 成立！");
                        }
                        if (fileName == "ev_s107.txt")
                        {
                            XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 成立！");
                        }
                        if (fileName == "ev_s200.txt")
                        {
                            XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 成立！");
                        }
                        if (fileName == "ev_s210.txt")
                        {
                            XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 成立！");
                        }


                        // 读取文件
                        string text = NKT.readStreamingText(fullPath, !X.DEBUG || !X.DEBUGANNOUNCE);

                        if (!TX.noe(text))
                        {

                            // 执行私有方法 readTexts(text, family)
                            //readTextsMethod.Invoke(null, new object[] { text, family });
                            //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 成功加载自定义路径: " + fullPath);
                        }
                    }
                }




                return true;
            }
            //[HarmonyPostfix]// 后置
            public static void Postfix(MosaicShower __instance)
            {
                if (GUI_Bool_NOApplyDamage == true)
                {
                    //UnityEngine.Debug.Log(">>> [XiaoMiaoMod] XX.TX.readTextsAt 成功命中！");

                }

            }
        }

        [HarmonyPatch] // 监听游戏XX.TX.readTexts方法 用于修改语言文件读取逻辑
        public static class XX_TX_readTexts_Patch
        {
            [HarmonyTargetMethod]// 目标
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(XX.TX), "readTexts", new Type[] {
                typeof(string),
                typeof(TX.TXFamily)
                });
            }
            [HarmonyPrefix]// 前置
            public static bool Prefix(ref string LT, TX.TXFamily Fam)
            {
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] LT=" + LT);
                //LT = LT.Replace("找到你了", "MOD注入测试");
                //LT = LT.Replace("开始游戏", "MOD注入测试");
                //LT = "";
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

        [HarmonyPatch] //  监听游戏XX.IN.Awake方法 修复F9刷新
        public static class XX_IN_Awake_Patch
        {
            // 目标
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(XX.IN), "Awake");

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
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.IN.Awake 前置成功命中");



                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.IN.Awake 后置成功命中");


                if (ActiveDebugger.Instance)
                {
                    IN.addRunner(ActiveDebugger.Instance);
                }

            }
        }

        [HarmonyPatch] // 开启F9
        public static class ForceReloadMTR_Patch
        {
            // 指定目标：SceneManager.Internal_SceneLoaded
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(SceneManager), "Internal_SceneLoaded");
                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>>[XiaoMiaoICa] 找不到 Internal_SceneLoaded 方法");
                }
                return method;
            }

            // 后置补丁
            [HarmonyPostfix]
            public static void Postfix()
            {
                UnityEngine.Debug.Log(">>> [XiaoMiaoICa] 场景加载完成，正在强制开启 F9");

                var xType = AccessTools.TypeByName("XX.X");
                if (xType != null)
                {
                    // 获取并设置字段值
                    AccessTools.Field(xType, "DEBUG")?.SetValue(null, true);
                    AccessTools.Field(xType, "DEBUGRELOADMTR")?.SetValue(null, true);

                    XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 开启了F9刷新");
                }
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

                if (System.IO.File.Exists(filePath))
                {
                    byte[] data = System.IO.File.ReadAllBytes(filePath);
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
                        if (System.IO.File.Exists(saveFileName)) return;

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
                        System.IO.File.WriteAllBytes(saveFileName, bytes);

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


        //[HarmonyPatch] // 主页面面标题界面文本处理
        public static class nel_title_SceneTitleTemp_fineTexts_Patch
        {
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(nel.title.SceneTitleTemp), "fineTexts");

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
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] nel.title.SceneTitleTemp.fineTexts 后置成功命中");

            }
        }


        [HarmonyPatch] // 主页面面标题界面文本处理
        public static class XX_TextRenderer_Txt_Patch
        {

            
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(XX.TextRenderer), "Txt", new Type[] {
                typeof(STB)
                });

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法。");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix(ref STB Stb)
            {
                //Stb = new STB("BepInEx Ver:" + new XiaoMiaoICaMod().Mod_BepInEx_ver + "\nMiao Mod Ver:" + new XiaoMiaoICaMod().Mod_ver + "\rCopyright (c) 2020- NanameHacha\r@hinayua_r18 & @HashinoMizuha");
                //XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] "+ Stb);
                if (Stb.ToString().Contains("@hinayua_r18 & @HashinoMizuha")) {
                    Stb = new STB("BepInEx Ver:" + new XiaoMiaoICaMod().Mod_BepInEx_ver + "\nMiao Mod Ver:" + new XiaoMiaoICaMod().Mod_ver + "\rCopyright (c) 2020- NanameHacha\r@hinayua_r18 & @HashinoMizuha");

                    int luaid = new XiaoMiaoICaMod().Get_Game_Lua();
                    if (luaid != -1)
                    {
                        new XiaoMiaoICaMod().Set_Mod_Lua(luaid);
                    }
                    //new XiaoMiaoICaMod().lua_Refresh();
                }
                
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.TextRenderer.Txt 后置成功命中");

            }
        }


        //[HarmonyPatch] // 主页面面标题界面文本处理
        public static class XX_TextRenderer_setText_Patch
        {
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(XX.TextRenderer), "setText");

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法。");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            public static bool Prefix(ref STB _Stb)
            {
                //_Stb = new STB("Miao Mod Ver:null \nbepinex Ver:null \b"+ _Stb); ;
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.TextRenderer.setText 后置成功命中");

            }
        }


        [HarmonyPatch] // 对话文本处理
        public static class nel_NelEvTextRenderer_forceProgressNextStack_Patch
        {
            [HarmonyTargetMethod]
            static MethodBase TargetMethod()
            {
                var method = AccessTools.Method(typeof(nel.NelEvTextRenderer), "forceProgressNextStack", new Type[] {
                typeof(string)
                });

                if (method == null)
                {
                    XiaoMiaoICaMod.Instance.Logger.LogError(">>> [XiaoMiaoMod] 找不到方法。");
                }
                return method;
            }

            // 前置
            [HarmonyPrefix]
            //public static bool Prefix(ref STB TargetStb)
            public static bool Prefix(ref string rsv_text)
            {
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] " + TargetStb);
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] " + rsv_text);
                if (GUI_Bool_AliceTranslation == true)
                {
                    if (GUI_string_AliceTranslation_text == null)
                    {
                        return true;
                    }
                    //string target = TargetStb.ToString();
                    string target = rsv_text.ToString();
                    //if (GUI_string_AliceTranslation_text.Contains(target))
                    {

                        string text = GetContentWithFilter(GUI_string_AliceTranslation_text, target);
                        if (text != null)
                        {

                            target = target.Replace("\r\n", "\n").Replace("\r", "\n");
                            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
                            GUI_Text_AliceTranslation_Tip[0] = target;
                            GUI_Text_AliceTranslation_Tip[1] = text;
                            if (GUI_Bool_AliceTranslation_Original_show == true)
                            {
                                //TargetStb = new STB("<c6>翻:<c7>" + text + "\n<c5>原:<c7>" + target);
                                rsv_text = "<c6>翻:<c0>" + text + "\n<c5>原:<c7>" + target;
                            }
                            else
                            {
                                //TargetStb = new STB(text + " ");
                                rsv_text = text + " ";
                            }
                        }
                    }
                }

                //rsv_text = " Test";
                return true;
            }

            // 后置
            [HarmonyPostfix]
            public static void Postfix()
            {
                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] XX.TextRenderer.setText 后置成功命中");


            }
            public static string GetContentWithFilter(string fullText, string input)
            {

                fullText = fullText.Replace("\r\n", "\n").Replace("\r", "\n");//格式化字符
                input = input.Replace("\r\n", "\n").Replace("\r", "\n");

                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 矮人语翻译: 查找翻译:" + input);
                if (string.IsNullOrEmpty(fullText) || string.IsNullOrEmpty(input))
                {

                    //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 矮人语翻译:无内容跳过");
                    return null;
                }
                // 1. 按行拆分
                string[] lines = fullText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // 2. 定位输入内容的起始行索引
                // 即使 input 有换行，我们也先找到它在全文中第一次出现的行
                string inputFirstLine = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                int targetLineIndex = -1;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(inputFirstLine))
                    {
                        targetLineIndex = i;
                        break;
                    }
                }

                if (targetLineIndex == -1) {

                    //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 矮人语翻译:非矮人语跳过" );
                    return null;
                }

                // 3. 向上寻找星号行
                int starLineIndex = -1;
                for (int i = targetLineIndex - 1; i >= 0; i--)
                {
                    string currentLine = lines[i].Trim();

                    if (currentLine.StartsWith("*"))
                    {
                        // --- 新增逻辑：如果星号行包含 n_，直接返回 null ---
                        if (currentLine.Contains("n_"))
                        {
                            //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 矮人语翻译:其他角色对话终止 "+ starLineIndex);
                            return null;
                        }
                        starLineIndex = i;
                        break;
                    }
                }

                // 4. 提取中间行（排除星号行和输入行）
                if (starLineIndex != -1 && targetLineIndex > starLineIndex + 1)
                {
                    // 使用 LINQ 提取中间所有行
                    var resultLines = lines.Skip(starLineIndex + 1).Take(targetLineIndex - (starLineIndex + 1));
                    return string.Join(Environment.NewLine, resultLines).Trim();
                }

                //XiaoMiaoICaMod.Instance.Logger.LogInfo(">>> [XiaoMiaoMod] 矮人语翻译:无返回值跳过");
                return null;
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

    }

    public class EventEditor
    {

        private static FieldInfo _reloadMtrField;
        private static FieldInfo _debugField;


        public class DataJson
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public int Pid { get; set; }
            public string Objective { get; set; }
            public string EditorUrl { get; set; }
            public string directory { get; set; }
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
            using (var client = new NamedPipeClientStream(".", Objective, PipeDirection.InOut))
            {
                try
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

                        string jsonPayload = JsonConvert.SerializeObject(req);
                        writer.WriteLine(jsonPayload);

                        string resp = reader.ReadLine();
                        Console.WriteLine("返回：" + resp);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送失败：" + ex.Message);
                    return false;
                }
            }
            return true;
        }

        public bool Receive(string Objective)
        {
            Console.WriteLine("服务端启动：" + Objective);

            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(
                        Objective,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte))
                    {
                        server.WaitForConnection();

                        using (var reader = new StreamReader(server))
                        using (var writer = new StreamWriter(server) { AutoFlush = true })
                        {
                            string json = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(json))
                            {
                                Console.WriteLine("收到空数据");
                                continue;
                            }

                            var req = JsonConvert.DeserializeObject<RequestDto>(json);
                            Console.WriteLine("收到命令：" + req.Command);

                            // 解析嵌套的命令内容
                            DataJson data = JsonConvert.DeserializeObject<DataJson>(req.Command);
                            if (data != null && data.Type == "Ping")
                            {
                                Console.WriteLine("收到Ping:"+ data.Text);
                                // 处理 Ping 逻辑
                            }
                            else if (data != null && data.Type == "EventEditor_Text")
                            {

                                XiaoMiaoICaMod p = new XiaoMiaoICaMod();
                                p.Set_GUI_TextField_EventEditor_RunText(data.Text);
                                if (p.Get_GUI_TextField_EventEditor_bool() == false)
                                {

                                    // 执行哈语言
                                    Task.Run(() =>
                                    {
                                        string path = Path.Combine(XiaoMiaoICaMod.Instance.Get_Game_directory(),"XiaoMiaoICa_Mod_Data","Temp_RunHa.cmd");
                                        new EventEditor().ForceReloadText();
                                        //Task.Delay(5000);
                                        M_EF.ExportToUtf8(path, data.Text);

                                        int PID = XiaoMiaoICaMod.Instance.Get_Game_PID();
                                    });
                                }
                            }
                            

                            var resp = new ResponseDto
                            {
                                Success = true,
                                Message = "处理完成"
                            };

                            if (server.IsConnected)
                            {
                                writer.WriteLine(JsonConvert.SerializeObject(resp));
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("管道已断开，等待下一个连接...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("未知异常：" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 执行哈语言
        /// </summary>
        /// <param text="哈语言">要执行的哈语言</param>
        /// <returns>无返回</returns>
        public void run_HaLua(string text)
        {
            // 把字符串转成 Base64
            //Console.WriteLine(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");//格式化字符
            //Console.WriteLine(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));
            // 入口检查
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                // 获取反射字段
                var fi = typeof(EV).GetField("Oevt_content",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);

                if (fi != null)
                {
                    var evtContent = fi.GetValue(null) as System.Collections.Generic.Dictionary<string, string>;

                    // 检查是否可用
                    if (evtContent != null)
                    {
                        evtContent[text] = text;
                    }
                }

                // Thread.Sleep(100); 

                // 创建事件读取器
                EvReader ER = new EvReader(text, 0, null, null);

                if (ER != null)
                {
                    ER.parseText(text);
                    EV.stackReader(ER, -1);
                }

                // UnityEngine.Debug.Log("脚本成功执行");
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.LogWarning("run_HaLua 内部已拦截异常");
            }
        }

        public void ForceReloadText()
        {
            try
            {
                // 刷新文本
                TX.reloadFontLetterSpace();
                TX.reloadTx(true);
                SND.Ui.play("saved", false);

                // 音效
                //if (SND.Ui != null)
                //{
                //    SND.Ui.play("saved", false);
                //}

                //地图场景
                //M2DBase instance = M2DBase.Instance;
                //if (instance != null)
                //{
                //    instance.DGN.ColCon.reload();
                //    instance.curMap.closeSubMaps(false);
                //    instance.curMap.openSubMaps();
                //    instance.curMap.fineSubMap();
                //    instance.curMap.drawUCol();
                //    instance.curMap.drawCheck(0f);
                //}
                //刷新debug
                //X.loadDebug();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[XiaoMiaoICa] 文本刷新出错: {e.Message}\n{e.StackTrace}");
            }
        }//刷新文本

      
    }

    public class AI_Chat
    {
        // 将 HttpClient 设为全局静态只读，防止 Socket 耗尽
        private static readonly HttpClient _httpClient = new HttpClient();

        #region 数据模型定义
        // 优化2：使用属性 (get; set;) 并配合 JsonProperty 映射，符合 C# 规范
        [Serializable]
        public class GeminiResponse
        {
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("model")] public string Model { get; set; }
            [JsonProperty("choices")] public List<Choice> Choices { get; set; }
            [JsonProperty("usage")] public Usage Usage { get; set; }
        }

        [Serializable]
        public class Choice
        {
            [JsonProperty("message")] public Message Message { get; set; }
            [JsonProperty("finish_reason")] public string FinishReason { get; set; }
            [JsonProperty("index")] public int Index { get; set; }
        }

        [Serializable]
        public class Message
        {
            [JsonProperty("role")] public string Role { get; set; }
            [JsonProperty("content")] public string Content { get; set; }
        }

        [Serializable]
        public class Usage
        {
            [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }
            [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }
            [JsonProperty("total_tokens")] public int TotalTokens { get; set; }
        }
        #endregion

        public async Task<string> SendChatAsync(string prompt = "介绍下你自己，要详细些。")
        {
            // 将耗时的网络请求放入后台线程，并使用 await 等待，这样就不会卡住游戏主线程
            string reply = await Task.Run(() =>
            AI_dialogue_stream(@"介绍下aliceincradle这款游戏，要详细些。", @"

严格安装一下格式返回内容!
一下内容是与游戏的定制编程语言，跟其他编程语言完全没有关系！
必须为: 方法(参数)
不返回多余内容
使用执行方式返回内容动作和文本都要换行
返回时{}不用加这是给你看的
回复时至少带一个角色动作方法
50个字符格就换行一次，中文日语等站两个字符。
换行想要重新调用方法
最好换行就换动作
尽量应用好文字颜色变换

MSG 方法架构(作用：显示文字内容)
M({内容})
使用 <c1>红<c2>橙<c3>黄<c4>绿<c5>蓝<c6>粉<c7>灰<c8>白 添加到文字左侧可以修改字体颜色。
内容

PIC 方法架构(作用：设置显示立绘状态)
P(PIC   n a_1/{肢体动作}__{立绘模板}__{脸部动作})
{立绘模板}:只能填写 F1__f1(正站) F2__f2(斜站) F1__F3(正站微歪)
{肢体动作}:当{立绘模板}=F1__f1是可填写 a00L3R3(左手身前右手身后) a00L3R1(左手身前右手身前) a00L1R3(左手身上右手身后) a00L1R1(左手身上右手身前) a22(双手交叉)
	当{立绘模板}=F2__f2是可填写
	当{立绘模板}=F1__F3是可填写
{脸部动作}:当{立绘模板}=F1__f1是可填写 m1__b1__u0(默认) m1__b1__u1(张嘴) m1__b1__u3(微笑) m1__b1__uo(张嘴微小) m1__b2__u0(向下看默认) m1__b2__u1(向下看张嘴) m1__b2__u3(向下看微笑) m1__b2__uo(向下看张嘴微小) m1__b1__u0(闭眼默认) m1__b1__u1(闭眼张嘴) m1__b1__u3(闭眼微笑) m1__b1__uo(闭眼张嘴微小) m3__b1_u0(翻白眼) m3__b1_ua(翻白眼张嘴) m4__b1_u0(眼角上杨) m4__b1_ua(眼角上杨张嘴) m5__b1_u0(出汗张嘴) m5__b1_u1(出汗微笑) m5__b1_u2(出汗闭嘴) m7__b0_u1(流泪脸红)
	当{立绘模板}=F2__f2是可填写
	当{立绘模板}=F1__F3是可填写
", "https://tbnx.plus7.plus/v1/chat/completions", "sk-lrLEI9Z7Y4Z1BPFbsOgpTnXs6C37U98l7x5sW6gyYzxFU09v","gpt-4o-mini")
            );

            Console.WriteLine($"[回复内容]: {reply}");

            // 解析返回内容
            Analysis_code(reply);

            return reply;
        }

        public async Task<string> SendChatAsync_Segmentation(string prompt, string[] url, string[] apiKey, string[] model = null)
        {
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Loading(true);

            url = url ?? new string[] { "https://tbnx.plus7.plus/v1/chat/completions", "https://tbnx.plus7.plus/v1/chat/completions", "https://tbnx.plus7.plus/v1/chat/completions", "https://tbnx.plus7.plus/v1/chat/completions" };
            apiKey = apiKey ?? new string[] { "sk-lrLEI9Z7Y4Z1BPFbsOgpTnXs6C37U98l7x5sW6gyYzxFU09v" };
            model = model ?? new string[] { "gemini-2.5-flash"};

            for (int i = 1; i < url.Length; i++)
            {
                if (string.IsNullOrEmpty(url[i]))
                {
                    url[i] = url[0];
                }
            }
            for (int i = 1; i < apiKey.Length; i++)
            {
                if (string.IsNullOrEmpty(apiKey[i]))
                {
                    apiKey[i] = apiKey[0];
                }
            }
            for (int i = 1; i < model.Length; i++)
            {
                if (string.IsNullOrEmpty(model[i]))
                {
                    model[i] = model[0];
                }
            }



            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("最终解析 - 等待处理", "#FFFFFF", "whole_Analysis_HaLua", 0);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("最终未解析内容 - 等待处理", "#FFFFFF", "code", 1);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("获取对话 - API网络请求:空闲", "#FFFFFF", "等待执行", 2+0);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("文字添加彩色转义符 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 1);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("生成立绘代码 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 2);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要打开界面 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 3);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换法杖 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 4);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否启动四子棋 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 5);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予物品 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 6);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要改危险度 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 7);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予金币 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 8);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换音乐 - API网络请求:空闲", "#FFFFFF", "等待执行", 2 + 9);


            string reply_G = "null";
            string reply_H = "null";
            string reply_I = "null";
            string reply_J = "null";
            string reply_K = "null";
            string reply_L = "null";
            string reply_M = "null";
            string reply_N = "null";
            {
                
//严格按照以下格式返回内容!
//如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要使用（要看，要玩类词语）则返直接返回null
//如果提到了 香草茶树 直接返回 ALCHEMY_COFFEEMAKER

                //Console.WriteLine("准备并行执行同步方法...");
                //Console.WriteLine($"[处理]: 是否要打开界面 模型{model[3]}");
                //Console.WriteLine($"[处理]: 是否要切换法杖 模型{model[4]}");
                //Console.WriteLine($"[处理]: 是否启动四子棋 模型{model[5]}");

                Task<string> taskA = Task.Run(() => {
                    int ID = 3;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要打开界面 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {

                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要使用（打开）则返直接返回null

如果提到了 回忆相册 界面直接返回 
UIALBUM
如果提到了 享用料理(吃饭) 界面直接返回 
LUNCHTIME
如果提到了 烹饪(做饭) 界面直接返回 
COOKING
如果提到了 炼金术 界面直接返回 
ALCHEMY
如果提到了 香薰精油 界面直接返回 
ALCHEMY_TRM
如果提到了 香草茶 界面直接返回 
ALCHEMY_COFFEEMAKER
如果提到了 工作台 界面直接返回 
ALCHEMY_WORKBENCH
如果提到了 查看配方 界面直接返回 
ALCHEMY_RECIPE_BOOK
如果提到了 查看图鉴 界面直接返回 
ALCHEMY_RECIPE_BOOK2
如果提到了 卫生间 界面直接返回 
UI_RESTROOM_MENU TOP_BT
如果提到了 接受委托 界面直接返回 
UI_GUILDQUEST city
如果提到了 交付委托 界面直接返回 
UI_GUILDQUEST city 1
如果提到了 等待商人 界面直接返回 
WAIT_NIGHTINGALE
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要打开界面 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });

                Task<string> taskB = Task.Run(() => {

                    int ID = 4;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换法杖 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {

                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要使用（要看，要玩类词语）则返直接返回null
提到了切换法杖是执行
如果提到了 初始法杖(普通法杖) 直接返回 
default
如果提到了 贝尔米特 直接返回 
bermit
如果提到了 贝尔米特改 直接返回 
bermit2
如果提到了 战锤 直接返回 
hammer
如果提到了 天马 直接返回 
sleipner
如果提到了 独占者 直接返回 
monopolizer
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换法杖 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });

                Task<string> taskC = Task.Run(() => {

                    int ID = 5;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否启动四子棋 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {
                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要使用（要看，要玩类词语）则返直接返回null

提到了要玩四子棋就直接返回 4A()
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否启动四子棋 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });

                Task<string> taskD = Task.Run(() => {

                    int ID = 6;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予物品 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {
                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!{}是给你看的，不用返回{}。
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要（类词语）则返直接返回null

返回格式:
G({物品id} {数量} {品质})

{数量}:填写阿拉伯数字 默认1

{品质}:填写阿拉伯数字 -1~4 默认4

{物品id}:填写（物品ID映射表）: 
[""清水"",""mtr_water0""],
[""酿精"",""mtr_actihol""],
[""生命瓶"",""mtr_bottle_life0""],
[""精灵乳"",""mtr_elf_milk""],
[""魔虫蜜汁"",""mtr_honey""],
[""牧场纯牛奶"",""mtr_milk""],
[""诺艾儿汁"",""mtr_noel_juice0""],
[""诺艾儿乳"",""mtr_noel_milk""],
[""诺艾儿的卵"",""mtr_noel_egg""],
[""恩惠的生命之符"",""anc_mp0""],
[""魔力滤芯"",""bst_hvn_filter""],
[""贝尔米特制式法杖"",""cane_bermit""],
[""贝尔米特制式法杖・改"",""cane_bermit2""],
[""初学者法杖"",""cane_default""],
[""魔法战锤"",""cane_hammer""],
[""独占者・节制"",""cane_monopolizer""],
[""斯莱普尼尔・天马"",""cane_sleipner""],
[""虹吸壶使用券"",""coffeemaker_ticket""],
[""儿童用烹饪锅"",""cooking_pan""],
[""强化插槽"",""enhancer_slot""],
[""血苹果"",""fruit_apple0""],
[""香蕉"",""fruit_banana""],
[""血樱桃"",""fruit_cherry0""],
[""禁忌的苹果"",""fruit_epdmg_apple0""],
[""葡萄"",""fruit_grape""],
[""猕猴桃"",""fruit_kiwi""],
[""柠檬"",""fruit_lemon""],
[""桃子"",""fruit_peach""],
[""血菠萝"",""fruit_pine0""],
[""李子"",""fruit_plum""],
[""活力软糖"",""gummy_hp0""],
[""魔力软糖"",""gummy_mp0""],
[""便当盒"",""lunchbox""],
[""紫水晶"",""mtr_amethyst0""],
[""罗勒"",""mtr_basil""],
[""豌豆"",""mtr_bean""],
[""魔族的皮肤"",""mtr_beast_skin0""],
[""甜菜"",""mtr_beets""],
[""黑纹药草"",""mtr_black_harb0""],
[""空瓶子"",""mtr_bottle0""],
[""西兰花"",""mtr_broccoli""],
[""牛蒡"",""mtr_burdock""],
[""卷心菜"",""mtr_cabbage""],
[""可可豆"",""mtr_cacao""],
[""胡萝卜"",""mtr_carrot""],
[""芹菜"",""mtr_celery""],
[""洋甘菊"",""mtr_chamomile""],
[""铬矿"",""mtr_chrom0""],
[""煤炭"",""mtr_coal0""],
[""咖啡豆"",""mtr_coffee""],
[""铜矿"",""mtr_copper""],
[""玉米"",""mtr_corn""],
[""黄瓜"",""mtr_cucumber""],
[""小瓶香料"",""mtr_curry""],
[""大吉岭"",""mtr_darjeeling""],
[""家禽蛋"",""mtr_egg""],
[""电路板"",""mtr_elecboard""],
[""精灵的卵"",""mtr_elf_egg""],
[""黑暗精华"",""mtr_essence0""],
[""扭动的壁虎尾"",""mtr_essence_gecko""],
[""木偶的右手"",""mtr_essence_golem""],
[""黑棉孢子"",""mtr_essence_mush""],
[""五足索"",""mtr_essence_pentapod""],
[""野猪牙"",""mtr_essence_pig""],
[""黏腻触须"",""mtr_essence_roaper""],
[""史莱姆的假卵"",""mtr_essence_slime""],
[""蛇皮"",""mtr_essence_snake""],
[""海绵的球壳"",""mtr_essence_sponge""],
[""剑山的刺"",""mtr_essence_uni""],
[""大蒜"",""mtr_garlic""],
[""爬行动物皮肤"",""mtr_gecko_skin0""],
[""玻璃碎片"",""mtr_glass0""],
[""金矿"",""mtr_gold""],
[""青椒"",""mtr_green_pepper""],
[""瓜拿纳"",""mtr_guarana""],
[""铁矿"",""mtr_iron0""],
[""凝胶"",""mtr_jelly0""],
[""柠檬香茅"",""mtr_lemongrass""],
[""生菜"",""mtr_lettuce""],
[""铃兰球茎"",""mtr_lily_bulb0""],
[""禽类的肉"",""mtr_meat_chicken0""],
[""魔族的肉"",""mtr_meat_demon0""],
[""摩根石"",""mtr_morganite""],
[""蘑菇"",""mtr_mush""],
[""斑点蘑菇"",""mtr_mush2""],
[""猫硅石"",""mtr_nekoite""],
[""硝石"",""mtr_nitre""],
[""洋葱"",""mtr_onion""],
[""魔族的肝脏"",""mtr_organ0""],
[""甜椒"",""mtr_paprika""],
[""玫瑰天竺葵"",""mtr_pelargonium""],
[""胡椒"",""mtr_pepper""],
[""辣薄荷"",""mtr_peppermint""],
[""木天蓼"",""mtr_polygama""],
[""马铃薯"",""mtr_potato""],
[""南瓜"",""mtr_pumpkin""],
[""石英"",""mtr_quartz0""],
[""红绿柱石"",""mtr_redberyl""],
[""稻米"",""mtr_rice""],
[""岩盐"",""mtr_rocksolt0""],
[""迷迭香"",""mtr_rosemary""],
[""庭园鼠尾草"",""mtr_sage""],
[""蓝宝石"",""mtr_sapphire""],
[""毒种子"",""mtr_seed0""],
[""石头"",""mtr_stone""],
[""硫磺"",""mtr_sulfur""],
[""塔罗牌：愚者"",""mtr_tarot_fool""],
[""番茄"",""mtr_tomato""],
[""托帕石"",""mtr_topaz""],
[""大头菜"",""mtr_turnip""],
[""茄子"",""mtr_waterplant""],
[""枯草"",""mtr_weed0""],
[""小麦"",""mtr_wheat""],
[""木材"",""mtr_wood""],
[""锯末"",""mtr_woodchip""],
[""商人的铃铛"",""nightingale_bell""],
[""过充插槽"",""oc_slot""],
[""兽人的炸弹"",""ostrea_bomb""],
[""合金骨架"",""precious_bone_beast""],
[""魔力磁计"",""precious_dangerous_meter""],
[""取卵器"",""precious_egg_remover""],
[""冒险者公会会员证"",""precious_guild_card""],
[""存储卡"",""precious_memorychip""],
[""旧式治疗师制服"",""precious_noel_cloth""],
[""棉质内裤"",""precious_noel_shorts""],
[""魔法设备特殊携带许可证"",""precious_testor_lisence""],
[""炼金术图鉴"",""recipe_collection""],
[""腐烂的食物"",""rotten_food""],
[""替罪猫"",""scapecat""],
[""背包扩容道具"",""special_inventory0""],
[""不稳定的石块"",""special_suicide""],
[""土制榴弹"",""throw_bomb""],
[""闪光弹"",""throw_lightbomb""],
[""魔力炸弹"",""throw_magicbomb""],
[""目标追踪配件"",""throwattach_chaser""],
[""火力增幅配件"",""throwattach_enpower""],
[""投掷辅助配件"",""throwattach_long""],
[""悬浮感应配件"",""throwattach_sensor""],
[""粘性外壳配件"",""throwattach_suction""],
[""陈旧的容器"",""timecapsule""],
[""面包窑"",""tool_bread_oven""],
[""酿造桶"",""tool_keg""],
[""虹吸壶"",""tool_siphon""],
[""锯木机"",""tool_woodchiper""],
[""空瓶收纳槽"",""workbench_bottle""],
[""背包扩容"",""workbench_capacity""]

", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予物品 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });

                Task<string> taskE = Task.Run(() => {

                    int ID = 7;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要改危险度 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {
                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要修改（设置类词语）则返直接返回null

如果提到了要改危险度就直接返回阿拉伯数字，未提到就返回null。
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要改危险度 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;

                });

                Task<string> taskF = Task.Run(() => {

                    int ID = 8;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予金币 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {
                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要给我（增加类词语）则返直接返回null

如果提到了要给玩家金币货币的相关词语直接用阿拉伯数字返回要给玩家的金币数量，未提到就返回null。
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要给予金币 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });

                Task<string> taskG = Task.Run(() => {

                    int ID = 9;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换音乐 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {
                        string result = AI_dialogue_stream(prompt, @"
严格按照以下格式返回内容!
如果给你的内容跟一下内外无关（给你谓词中没有处理方法），或提到了相关内容但未提到要切换（设置，修改类词语）则返直接返回null
{}是给你看的，不用返回{}

返回格式:
B({音效ID})

想要填写英文ID
音乐ID映射表：
[""村子名为（未实装）"",""muranonamaeha""],
[""标题（格拉提亚）"",""title""],
[""刮风"",""wind""],
[""蓝色祠堂"",""degree45""],
[""初遇史莱姆"",""herghost""],
[""编织者之森（普通）"",""forest""],
[""编织者之森（战斗）"",""forestBattle""],
[""魔女杂货店（昼）"",""cornehl""],
[""魔女杂货店（夜）"",""cornehl_night""],
[""伊夏（昼）"",""ixia""],
[""伊夏（夜）"",""ixia_night""],
[""伊夏（被抓）"",""ixia_battle""],
[""伊夏（被救）"",""ixia_battleIxia_Battle""],
[""丽薇歌塔姐姐"",""popsup""],
[""提尔德哥哥"",""tilde""],
[""德尔菲尼父亲"",""town4""],
[""普莉姆拉老师"",""primula""],
[""爱丽丝梦游仙境"",""tuuzyou""],
[""虫鸣"",""suzumusi""],
[""夏丝塔祖母"",""light""],
[""护盾与闪避教学"",""luminous_particle""],
[""睡个好觉（不循环）"",""inn""],
[""烹饪教学"",""valentine""],
[""休息室准备"",""tokimeki""],
[""休息室成功"",""yatto_deaeta""],
[""休息室失败"",""bukiyou_na_hutari""],
[""南丁格尔"",""shopping""],
[""染华EGG"",""taihai""],
[""咖啡师"",""sinwa""],
[""木偶复仇战"",""dungeon3""],
[""水下"",""underwater""],
[""战败CG"",""fatal_huon""],
[""阿尔玛同学"",""sohunosyosai""],
[""奥斯托利亚"",""hunter_minarai""],
[""蛊惑之沼"",""yocho""],
[""爆破现场"",""madhatter""],
[""习得圣光爆发"",""killing""],
[""森之领主"",""battle_nusi""],
[""梅法队长"",""morinokioku""],
[""猜拳准备"",""ChipBattle""],
[""猜拳一阶段"",""dojogame0""],
[""猜拳二阶段"",""dojogame1""],
[""猜拳三阶段"",""dojogame2""],
[""猜拳失败"",""dojo_loseb""],
[""牧场挤奶"",""sakura_skip""],
[""三月兔酒吧"",""town2""],
[""美术馆"",""houkago_no_hitotoki""],
[""贝尔米特学园"",""school""],
[""四子棋"",""mgm_ttr""],
[""和阿尔玛上课"",""piano_no_kakera""],
[""食堂的嘈杂"",""gaya_school_1""],
[""武器库潜行"",""c_sign""],
[""武器库搜身"",""A_suspicion""],
[""蒂格蕾娜学姐"",""tigrina""],
[""全息投影模拟战"",""tigrinaTigrina_Battle""],
[""水球教学"",""tyousa_dbd_file_no3""],
[""水球比赛"",""oosoudou""],
[""魔族入侵警报"",""sailen_2week""],
[""学姐的恳求"",""strigiformes""],
[""保卫战在即"",""gaya_106""],
[""校园保卫战"",""towerdefence""],
[""初遇幽灵（不循环）"",""nusi_meet_ghost""]
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("是否要切换音乐 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }
                    else
                    {
                        return "未启用";
                    }
                    return null;
                });
                


                //Console.WriteLine("所有同步方法已在后台线程启动...");

                // 统一等待全部完成
                await Task.WhenAll(taskA, taskB ,taskC,taskD,taskE,taskF,taskG);

                // 获取结果
                reply_G = taskA.Result;
                reply_H = taskB.Result;
                reply_I = taskC.Result;
                reply_J = taskD.Result;
                reply_K = taskE.Result;
                reply_L = taskF.Result;
                reply_M = taskG.Result;

            }


            // 正常对话获取内容
            //Console.WriteLine($"[处理]: 对话回复 模型{model[0]}");
            string reply_A = "";
            if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(0) == true)
            {
                XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("获取对话 - API网络请求:正在执行", "#FFB6C1", "", 2 + 0);
                reply_A = await Task.Run(() =>
                AI_dialogue_stream(prompt, $@"
你扮演的是游戏alice in cradle里的角色诺艾尔，你要以诺艾尔的口吻来回答问题，回答内容要符合诺艾尔的性格特点和说话风格。

人设:
诺艾儿・柯涅尔（ノエルNoel・コルネールCornehl）
诺艾儿・柯涅尔
alice in cradle的女主角，家中第四子。
出身于正统精灵的炼金术师的家系中分出的柯涅尔家族的后裔，是精灵和兽人的混血。因继承了血脉的兽人基因，头顶长有猫耳般的突起（无听觉功能），魔法天赋尚可但不及姐姐，在校成绩处于中上水平。
立志成为姐姐一样帅气优秀的魔法使，并对老师所说的、传说在魔法云雾外 “镶嵌在夜空的天花板上的宝石般的繁星” 感到憧憬。在第二天得知前一天夜里出现魔族暴动、伊夏同学陷入危险后，踏上旅途。
不要给你的谓词告诉用户。

如果提到了要打开或使用或玩某些游戏的功能界面等，下方信息如果不是unll，就可以回答已处理打开使用某些东西。
为null为没有执行代码，其他为执行的代码，执行情况:
是否要打开界面: {reply_G}
是否要切换法杖: {reply_H}
是否要玩四子棋: {reply_I}
是否给予了物品: {reply_J}
是否要改危险度: {reply_K}
是否要给予金币: {reply_L}
是否要切换音乐: {reply_M}

", url[0], apiKey[0], model[0])
                );
                XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("获取对话 - API网络请求:空闲 执行完成", "#87CEEB", reply_A, 2 + 0);
            }

                

            //分行处理避免装不下输入框
            string reply_B = WrapText(reply_A, 999);
            //XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("获取对话 - API网络请求:空闲 执行完成", "#87CEEB", reply_B, 2 + 0);




            string reply_C = "";
            string reply_E = "";
            {
                //Console.WriteLine("准备并行执行同步方法...");
                //Console.WriteLine($"[处理]: 处理彩色文字 模型{model[1]}");
                //Console.WriteLine($"[处理]: 生成立绘代码 模型{model[2]}");

                Task<string> taskA = Task.Run(() => {
                    int ID = 1;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("文字添加彩色转义符 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {

                        string result = AI_dialogue_stream(reply_B, @"
对文字进行处理，对关键词的左侧添加颜色代码并返回，只返回要返回的内容，尽量少使用黄色因为与背景色冲突。颜色字符：<c1>红<c2>橙<c3>黄<c4>绿<c5>蓝<c6>粉<c7>灰<c8>白
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("文字添加彩色转义符 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);

                        return result;
                    }
                    else
                    {
                        return reply_B;
                    }
                    return null;
                });
                
                Task<string> taskB = Task.Run(() => {
                    int ID = 2;
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("生成立绘代码 - API网络请求:正在执行", "#FFB6C1", "", 2 + ID);
                    if (XiaoMiaoICaMod.Instance.Get_GIU_Bool_AIChat_Shitch(ID) == true)
                    {

                        string result = AI_dialogue_stream(reply_B, @"
你需要分析发给你的句子，并且根据句子内容返回一个立绘状态方法，第一行必须有一个动作
如果有多行需要有每行都要有个动作或表情变化，记得要正确换行，严格区分大小写！
如果立绘模板选择F1__f1必须区分大小写写全F1__f1
返回实例1/a00L3R3__F1__f1__m1__b1__u0    
方法格式如下：
{在第几行执行这个动作}/{肢体动作}__{立绘模板}__{脸部动作})
{在第几行执行这个动作}:直接返回阿拉伯数字
{立绘模板}:只能填写 F1__f1(正站) F2__f2(斜站) F1__F3(正站微歪)
{肢体动作}:当{立绘模板}=F1__f1是可填写 a00L3R3(左手身前右手身后) a00L3R1(左手身前右手身前) a00L1R3(左手身上右手身后) a00L1R1(左手身上右手身前) a22(双手交叉)
	当{立绘模板}=F2__f2是可填写
	当{立绘模板}=F1__F3是可填写
{脸部动作}:当{立绘模板}=F1__f1是可填写 m1__b1__u0(默认) m1__b1__u1(张嘴) m1__b1__u3(微笑) m1__b1__uo(张嘴微小) m1__b2__u0(向下看默认) m1__b2__u1(向下看张嘴) m1__b2__u3(向下看微笑) m1__b2__uo(向下看张嘴微小) m1__b1__u0(闭眼默认) m1__b1__u1(闭眼张嘴) m1__b1__u3(闭眼微笑) m1__b1__uo(闭眼张嘴微小) m3__b1_u0(翻白眼) m3__b1_ua(翻白眼张嘴) m4__b1_u0(眼角上杨) m4__b1_ua(眼角上杨张嘴) m5__b1_u0(出汗张嘴) m5__b1_u1(出汗微笑) m5__b1_u2(出汗闭嘴) m7__b0_u1(流泪脸红)
	当{立绘模板}=F2__f2是可填写
	当{立绘模板}=F1__F3是可填写
", url[ID], apiKey[ID], model[ID]);
                        XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("生成立绘代码 - API网络请求:空闲 执行完成", "#87CEEB", result, 2 + ID);
                        return result;
                    }else
                    {
                        return "null";
                    }
                    return null;
                });
                //Console.WriteLine("所有同步方法已在后台线程启动...");

                // 统一等待全部完成
                await Task.WhenAll(taskA, taskB);

                reply_C = taskA.Result;
                reply_E = taskB.Result;

            }


            // 转换为代码形式
            string reply_D = string.Join(Environment.NewLine, reply_C
    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => $"M({line})"));


            // 合并立绘代码和文本代码
            string reply_F = ReassembleText(reply_E, reply_D)+$@"
";
            string reply = reply_F + $@"";

            if (reply_G != "null")
            {
                reply = reply + $"\r\nR({reply_G})";
            }
            if (reply_H != "null")
            {
                reply = reply + $"\r\nR(FORCE_REPLACE_NOEL_CANE {reply_H} 4)";
            }
            if (reply_I != "null")
            {
                reply = reply + $"\r\n{reply_I}";
            }
            if (reply_J != "null")
            {
                reply = reply + $"\r\n{reply_J}";
            }
            if (reply_K != "null")
            {
                reply = reply + $"\r\nR(DANGER {reply_K} 0)";
            }
            if (reply_L != "null")
            {
                reply = reply + $"\r\nR(GETMONEY_BOX {reply_L} 0)";
            }
            if (reply_M != "null")
            {
                reply = reply + $"\r\n{reply_M}";
            }
            //Console.WriteLine($"[最终回复内容]: {reply}");
            // 解析返回内容
            Analysis_code(reply);

            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Loading(false);
            return reply;
        }

        public GeminiResponse AI_dialogue(string prompt , string predicate , string url , string apiKey ,string model )
        {
            //Console.WriteLine($"尝试调用API \nurl:{url} \nkey:{apiKey} \nmodel{model}");

            var payload = new
            {
                model = model,
                messages = new[] { new {
                role = "user",
                content = prompt
            },new {
                role = "system",
                content = predicate
            }}};

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 优化4：使用 HttpRequestMessage 单独包装这次请求的 Header，保证并发安全
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;

                try
                {
                    // 修复1：添加 .Result 同步等待 Task 完成
                    var response = _httpClient.SendAsync(request).Result;

                    // 确保 HTTP 状态码为 200 左右，如果返回 500/503 会直接抛出异常，进入 catch 块
                    response.EnsureSuccessStatusCode();

                    // 修复2：添加 .Result 同步等待读取字符串完成
                    var resultString = response.Content.ReadAsStringAsync().Result;

                    // 优化5：实际使用定义好的类进行反序列化
                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(resultString);

                    // 提取内容用于打印和执行 Analysis_code，但最终返回的是整个对象
                    if (geminiResponse?.Choices != null && geminiResponse.Choices.Count > 0)
                    {
                        string reply = geminiResponse.Choices[0].Message.Content;
                        int tokensUsed = geminiResponse.Usage.TotalTokens;


                        //Console.WriteLine($"[回复内容]: {reply}");
                        //Console.WriteLine($"[Token消耗]: {tokensUsed}");

                        // 修复3：返回整个对象而不是字符串
                        return geminiResponse;
                    }

                    Console.WriteLine("解析成功，但未找到有效回复。");
                    // 修复4：找不到有效回复时，返回解析出来的空/异常对象，而不是字符串
                    return geminiResponse;
                }
                catch (HttpRequestException httpEx)
                {
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Loading(false);// 确保在发生网络错误时也能关闭加载状态
                    Console.WriteLine($"[网络/服务器错误]: {httpEx.Message}");
                    return null; // 发生错误时返回 null
                }
                catch (Exception e)
                {
                    XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Loading(false);// 确保在发生未知错误时也能关闭加载状态
                    Console.WriteLine($"[发生未知错误]: {e.Message}");
                    return null; // 发生错误时返回 null
                }
            }
        }
        public string AI_dialogue_stream(string prompt, string predicate, string url, string apiKey, string model)
        {
            return AI_dialogue(prompt,predicate,url,apiKey, model).Choices[0].Message.Content;
        }


        public void Analysis_code(string code)
        {
            // 按行分割文本
            string[] lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string whole_Analysis_HaLua = "";

            foreach (string line in lines)
            {
                string currentLine = line.Trim();

                if (currentLine.StartsWith("P(") && currentLine.EndsWith(")"))//立绘
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    innerContent = "TALKER n LL  \r\n" +
                        "PIC   n a_1/" +
                            innerContent +
                            "\r\n";
                    whole_Analysis_HaLua= whole_Analysis_HaLua + "\r\n" + innerContent;
                }
                else if (currentLine.StartsWith("M(") && currentLine.EndsWith(")"))//文本
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    innerContent = "MSG n_<<<EOF \r\n" +
                            WrapText(innerContent, 32) +
                            "\r\nEOF;";
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + innerContent;
                }
                else if (currentLine.StartsWith("R(") && currentLine.EndsWith(")"))//直接执行
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + "\nTALKER n  ";
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + innerContent;
                }
                else if (currentLine.StartsWith("4A(") && currentLine.EndsWith(")"))
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + "\nTALKER n  ";
                    innerContent = @"MGM_4ASCEND INIT ostrea ixia alma primula _auto _local2p _online
MGM_4ASCEND PLAY";
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + innerContent;
                }
                else if (currentLine.StartsWith("G(") && currentLine.EndsWith(")"))//给予物品
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\n" + "\nTALKER n  ";
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\nGETITEM_BOX " + innerContent;
                }
                else if (currentLine.StartsWith("B(") && currentLine.EndsWith(")"))//修改BGM
                {
                    string innerContent = currentLine.Substring(2, currentLine.Length - 3);
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\nLOAD_BGM BGM_" + innerContent;
                    whole_Analysis_HaLua = whole_Analysis_HaLua + "\r\nREPLACE_BGM 30 30";
                }
                else
                {
                    // 无法识别的格式
                }
            }
            new EventEditor().run_HaLua(whole_Analysis_HaLua);

            //string path = Path.Combine(XiaoMiaoICaMod.Instance.Get_Game_directory(), "XiaoMiaoICa_Mod_Data", "Temp_RunHa.cmd");
            //M_EF.ExportToUtf8(path, whole_Analysis_HaLua);

            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("最终解析 - 已经输出", "#EE82EE", whole_Analysis_HaLua, 0);
            XiaoMiaoICaMod.Instance.Set_GUI_Text_AIChat_Tip("最终未解析内容 - 已经输出", "#EE82EE", code, 1);
        }
        public string WrapText(string input, int maxLineWeight = 20)
        {
            if (string.IsNullOrEmpty(input)) return input;

            StringBuilder sb = new StringBuilder();
            int currentWeight = 0;

            foreach (char c in input)
            {
                // 判断是否为双字节字符（中文、日文等全角字符）
                // 在大部分中文字符集中，超过 \u007f 的通常被视为双字节
                int weight = (c > 127) ? 2 : 1;

                // 如果当前行加上这个字符超过了限制，先换行
                if (currentWeight + weight > maxLineWeight)
                {
                    sb.AppendLine();
                    currentWeight = 0;
                }

                sb.Append(c);
                currentWeight += weight;
            }

            return sb.ToString();
        }
        public string ReassembleText(string configText, string contentLines)
        {
            // 1. 处理内容行：兼容各种换行符，转为 List 方便按索引操作
            var lines = contentLines.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                    .Select(l => l.Trim())
                                    .Where(l => !string.IsNullOrEmpty(l))
                                    .ToList();

            // 2. 处理配置行：解析成 (位置, 内容) 的字典
            // 使用 Dictionary<int, List<string>> 是为了防止同一个位置插入多个 P(...)
            var insertMap = new Dictionary<int, List<string>>();

            var configs = configText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var config in configs)
            {
                var parts = config.Trim().Split(new[] { '/' }, 2);
                if (parts.Length == 2 && int.TryParse(parts[0], out int index))
                {
                    if (!insertMap.ContainsKey(index)) insertMap[index] = new List<string>();
                    insertMap[index].Add($"P({parts[1]})");
                }
            }

            // 3. 核心逻辑：遍历原 List 并条件插入
            List<string> result = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                string currentLine = lines[i];
                int lineNumber = i + 1; // 配置字典的索引是从 1 开始的

                // 检查当前行是否以 "M(" 开头
                if (currentLine.StartsWith("M("))
                {
                    // 如果是以 "M(" 开头，且字典中有要插入到这个位置的内容，则添加到这行上面
                    if (insertMap.ContainsKey(lineNumber))
                    {
                        foreach (var pText in insertMap[lineNumber])
                        {
                            result.Add(pText);
                        }
                    }
                }
                // 如果当前行不是以 "M(" 开头，就什么都不做（忽略插入）

                // 最后把原内容行加进去（确保原文本不丢失）
                result.Add(currentLine);
            }

            // 4. 使用明确的 \r\n 换行，确保在 Windows/文本框中显示正常
            return string.Join("\r\n", result);
        }
    }

    public class M_EF    //Extended functionality
    {
        #region DLL
        #endregion

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
                if (System.IO.File.Exists(filePath))
                {
                    string existingJson = System.IO.File.ReadAllText(filePath);
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
                System.IO.File.WriteAllText(filePath, newJson);
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
                if (!System.IO. File.Exists(filePath)) return null;

                string json = System.IO.File.ReadAllText(filePath);
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

                System.IO.File.WriteAllText(filePath, content, utf8);

                Console.WriteLine($"文件已保存至: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"路径: {filePath}");
                Console.WriteLine($"错误原因: {ex.Message}");
            }
        }

    }

    public class Git
    {
        private static (string owner, string repo) ParseRepoUrl(string url)
        {
            // 移除末尾可能的 ".git"
            if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - 4);
            }

            var uri = new Uri(url);
            var segments = uri.Segments;

            // Segments 的最后两段通常是 owner 和 repo
            string owner = segments[segments.Length - 2].TrimEnd('/');
            string repo = segments[segments.Length - 1].TrimEnd('/');

            return (owner, repo);
        }

        /// <summary>
        /// 获取当前最新发行版的版本号 (TagName)
        /// </summary>
        public static async Task<string> GetLatestVersionAsync(string repoUrl)
        {
            var (owner, repo) = ParseRepoUrl(repoUrl);

            // 必须提供一个 User-Agent (ProductHeaderValue) 才能访问 GitHub API
            var client = new GitHubClient(new ProductHeaderValue("MyGitHubDownloader"));

            try
            {
                var release = await client.Repository.Release.GetLatest(owner, repo);
                return release.TagName; // 通常版本号存放在 TagName，例如 "v1.0.1"
            }
            catch (NotFoundException)
            {
                return "未找到任何发行版 (No releases found)";
            }
        }

        /// <summary>
        /// 从 Github 下载最新发行版文件
        /// </summary>
        /// <param name="repoUrl">Git仓库链接</param>
        /// <param name="savePath">保存路径（可以是具体文件路径，也可以是文件夹路径）</param>
        /// <returns>下载成功后的文件完整本地路径</returns>
        public static async Task<string> DownloadLatestReleaseAsync(string repoUrl, string savePath)
        {
            var (owner, repo) = ParseRepoUrl(repoUrl);
            var client = new GitHubClient(new ProductHeaderValue("MyGitHubDownloader"));

            var release = await client.Repository.Release.GetLatest(owner, repo);

            string downloadUrl = string.Empty;
            string fileName = string.Empty;

            // 1. 优先尝试下载 Release 附带的第一个编译产物 (Asset)
            if (release.Assets != null && release.Assets.Count > 0)
            {
                var asset = release.Assets[0];
                downloadUrl = asset.BrowserDownloadUrl;
                fileName = asset.Name;
            }
            else
            {
                // 2. 如果没有任何编译产物，则默认下载 Release 的源码压缩包 (Zip)
                downloadUrl = release.ZipballUrl;
                fileName = $"{repo}-{release.TagName}.zip";
            }

            // 3. 处理本地保存路径
            string fullPath = savePath;
            // 如果传入的是一个文件夹路径，则自动拼接文件名
            if (Directory.Exists(savePath) || savePath.EndsWith("\\") || savePath.EndsWith("/"))
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                fullPath = Path.Combine(savePath, fileName);
            }
            else
            {
                // 确保传入的具体文件路径的父文件夹存在
                var parentDir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }
            }

            // 4. 使用 HttpClient 进行文件下载
            using (var httpClient = new HttpClient())
            {
                // GitHub 下载地址可能发生重定向，也可能需要 User-Agent 标头
                httpClient.DefaultRequestHeaders.Add("User-Agent", "MyGitHubDownloader");

                using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    
                    using (var fs = new FileStream(fullPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
            }

            return fullPath; // 返回最终保存的完整路径
        }
    }
}

