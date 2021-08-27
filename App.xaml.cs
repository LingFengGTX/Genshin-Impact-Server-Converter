using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace GenshinImpact_ServerConverter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public void LaunchFuncation(object sender,StartupEventArgs args) {


            //程序启动方法，此方法中，操作系统会将参数传递到本方法中然后根据参数来影响程序的启动行为。
            if (args.Args.Count() == 0)
            {
                InSystem.HidConsoleWindow(System.Console.Title);
                RunInCheck.RestoreConsole = false;
            }

            int Loop = -1;
            bool AfterClose = true;
            
            foreach (string Command in args.Args) {
                Loop += 1;
                if (Command == "-NoExit")
                {
                    //执行命令后自动退出程序
                    AfterClose = false;
                    continue;
                }

                if (Command == "-hidden") {
                    //隐藏控制台窗口
                    //如果有参数为 false,则退出时不还原控制台窗口。因此你需要进入任务管理器手动结束控制台程序。
                    if (MoveCommand(Loop, 1, args.Args) == "false")
                    {
                        RunInCheck.RestoreConsole = false;
                    }
                    InSystem.HidConsoleWindow(System.Console.Title);
                }

                if (Command == "-set") {
                    /*
                     *  配置命令，用于配置工具内的设置
                     * 
                     */
                    string SetType = MoveCommand(Loop, 1, args.Args);

                    if (SetType == "directory")
                    {
                        string Path = MoveCommand(Loop, 2, args.Args);
                        if (DataOperat.RequirePath(Path)) {
                            Win_API.WritePrivateProfileString("Settings", "Install Path", Path, Environment.CurrentDirectory + "\\App.ini");
                        }
                    } else if (SetType=="first") {
                        string reset = MoveCommand(Loop, 2, args.Args);
                        if (reset == "yes")
                        {
                            Win_API.WritePrivateProfileString("Settings", "IsFirstLaunch", "0", Environment.CurrentDirectory + "\\App.ini");
                        }
                        else if (reset == "no") {
                            Win_API.WritePrivateProfileString("Settings", "IsFirstLaunch", "1", Environment.CurrentDirectory + "\\App.ini");
                        }
                    }

                    continue;
                }

                if (Command == "-openDirectory") {
                    /*
                     * 打开目标目录
                     * 
                     * this:工具目录
                     * 
                     * game:游戏目录 - 需要提前使用 -LoadConfigure 指令
                     */

                    if (MoveCommand(Loop, 1, args.Args) == "this") {
                        System.Console.WriteLine("open target:" + Environment.CurrentDirectory);
                        Win_API.WinExec("explorer.exe \""+Environment.CurrentDirectory+"\"",1);
                    } else if (MoveCommand(Loop, 1, args.Args) == "game") {
                        if (RunInCheck.IsCheckedPath)
                        {
                            System.Console.WriteLine("open target:" + DataOperat.GamePath);
                            Win_API.WinExec("explorer.exe \"" + DataOperat.GamePath + "\"", 1);
                        }
                        else {
                            System.Console.WriteLine("No configure useful,please use \"-loadConfigure\" command.");
                        }
                    }
                    else {
                        continue;
                    }
                    continue;
                }

                if (Command == "-loadConfigure")
                {
                    /*
                     * 此命令是载入本地配置文件，方便执行有关备份的操作
                     */
                    RunInCheck.LoadLocateConfigure();
                    if (!RunInCheck.IsCheckedPath)
                    {
                        System.Console.WriteLine("Load Configure Faild!");

                    }
                    else
                    {
                        System.Console.WriteLine("Load Configure Sucessful!");
                    }

                    continue;
                }

                if (Command == "-launch")
                {
                    /*
                     * 依据载入的配置启动游戏
                     * 
                     * 该指令应提前使用 -LoadConfigure
                     */
                    if (MoveCommand(Loop, 1, args.Args) == "wait")
                    {
                        InSystem.LaunchGame(true,false);
                    }
                    else {
                        InSystem.LaunchGame(false,false);
                    }
                    continue;
                }

                if (Command == "-resetSetting") {
                    //清除工具的配置，必须手动启动
                    try
                    {
                        InSystem.ResetSetting();
                    }
                    catch (Exception exp) {
                        System.Console.WriteLine("Error:"+exp.Message);
                    }
                    continue;
                }

                if (Command == "-clearBackup")
                {
                    //清除备份
                    try
                    {
                        InSystem.ClearBackup();
                    }
                    catch (Exception exp)
                    {
                        System.Console.WriteLine("Error:" + exp.Message);
                    }
                    continue;
                }

                if (Command == "-resetBackup") {
                    //重置备份文件 使用之前需要使用 -loadConfigure 指令加载本地配置
                    try
                    {
                        InSystem.ResetBackup();
                    }
                    catch (Exception exp)
                    {
                        System.Console.WriteLine("Error:" + exp.Message);
                    }
                    continue;
                }

                if (Command == "-restoreBackup") {
                    //还原备份文件 使用之前需要使用 -loadConfigure 指令加载本地配置
                    try
                    {
                        InSystem.restoreBackup();
                    }
                    catch (Exception exp)
                    {
                        System.Console.WriteLine("Error:" + exp.Message);
                    }
                    continue;
                }

                if (Command == "-language")
                {
                    /*
                     * 强制设置程序内显示语言
                     * 
                     * 一般情况下程序的语言显示为自动，则自动获取操作系统的语言设置
                     * 
                     * 参数默认只支持两种 "chinese"(简体中文),"english"(英语)
                     */
                    RunInCheck.lang = MoveCommand(Loop, 1, args.Args);
                    RunInCheck.SetInterfaceLanguage();//设置程序目标语言
                    continue;
                    
                }

                if (Command == "-gc") {
                    /*
                     * 启动Gc垃圾回收器，该命令是让程序周期性执行垃圾回收以节省内存的使用
                     * 
                     * 单位:秒
                     */
                    long time;
                    try
                    {
                        time = System.Convert.ToInt32(MoveCommand(Loop, 1, args.Args));
                    }
                    catch (Exception exp) {
                        System.Console.WriteLine("Error:"+exp.Message);
                        continue;
                    }
                    if (time >= 1 && time <= Int64.MaxValue)
                    {
                        RunInCheck.LaunchGcCollecter(time);
                    }//如果超出这个范围则不执行垃圾回收
                    else {
                        System.Console.WriteLine("Interval Error.");
                    }
                    continue;
                }

                if (Command == "-enableAssembly") {
                    //启动游戏内组件
                    if (MoveCommand(Loop, 1, args.Args) == "true")
                    {
                        System.Console.WriteLine("Experiment function has been started.");
                        RunInCheck.EnableAssembly = true;
                    }
                    continue;
                }

                if (Command == "-background") {
                    //设置应用背景，可设定PNG,JPG,BMP等常见文件格式
                    string FilePath = MoveCommand(Loop, 1, args.Args);
                    if (FilePath == "static") {
                        RunInCheck.LoadVideo = false;
                        continue;
                    }
                    try
                    {
                        if (FilePath.Split('|').Count() == 0)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        System.Console.WriteLine("The Path has Error.You should like this:\n\"1.png|2.png|3.png\"");
                        continue;
                    }

                    FilePath = FilePath.Split('|')[new Random().Next(0,FilePath.Split('|').Count())];
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.Console.WriteLine("The BackGround has changed:"+FilePath);
                        RunInCheck.LoadVideo = false;//禁止加载视频
                        RunInCheck.ChangeTheBackGround = true;
                        RunInCheck.ImagePath = FilePath;
                    }
                    continue;
                }

                if (Command == "-disableXML") {
                    //禁用XML的游戏配置

                    /*
                     * 一般情况下如果未使用该命令，以及程序在运行目录下检测到 GameServer.xml 文件则会自动应用该配置文件
                     * 
                     * 如果该配置文件存在问题则程序会强制退出，该情况下移除该文件或者启用此命令即可
                     * 
                     * 关于配置文件的编辑可以使用 -enableEditorWindow 命令，启用XML文档编辑模式
                     */
                    if (MoveCommand(Loop, 1, args.Args) == "true")
                    {
                        System.Console.WriteLine("XML File disabled.");
                        RunInCheck.EnableXMLConfigure = false;
                    }
                    continue;
                }

                if (Command == "-enableEditorWindow") {
                    //启动配置文件编辑器
                    XmlDocumentEditor xmlDocumentEditor;
                    if (System.IO.File.Exists(MoveCommand(Loop, 1, args.Args)))
                    {
                        xmlDocumentEditor = new XmlDocumentEditor(MoveCommand(Loop, 1, args.Args));
                    }
                    else {
                        xmlDocumentEditor = new XmlDocumentEditor(null);
                    }
                    
                    xmlDocumentEditor.ShowActivated = true;
                    xmlDocumentEditor.ShowDialog();
                }

                if (Command =="-include") { 
                    /*
                     * 引用指定的XML文档配置文件
                     * 
                     * 如果之前使用了-disableXML则此参数不会生效
                     */
                    string tempPath= MoveCommand(Loop, 1, args.Args);
                    if (System.IO.File.Exists(tempPath) == true)
                    {
                        System.Console.WriteLine("Included the target:"+tempPath);
                        RunInCheck.includeXML = tempPath;
                    }
                    continue;
                }

                if (Command == "-server") {
                    if (!RunInCheck.IsCheckedPath) {
                        System.Console.WriteLine("No configure useful,please use \"-loadConfigure\" command.");
                        continue;
                    }
                    int Target = InSystem.StringIndex(MoveCommand(Loop, 2, args.Args));
                    if (Target == -1) {
                        System.Console.WriteLine("Noknow Server type.");
                        continue;
                    }
                    string type = MoveCommand(Loop, 1, args.Args);
                    string XmlFile= MoveCommand(Loop, 3, args.Args);
                    if (type=="mem") {
                        RunInCheck.memSetter = new MemConfigure();
                        RunInCheck.memSetter.LoadDefaultConfigure(null);
                        RunInCheck.memSetter.ApplyTargetServer(Target);
                        System.Console.WriteLine("Sucessful to change target server.");
                    } else if (type=="xml") {
                        RunInCheck.xmlSetter = new XMLConfigure();
                        if (System.IO.File.Exists(XmlFile))
                        {
                            RunInCheck.xmlSetter.LoadDefaultConfigure(XmlFile);
                            System.Diagnostics.Debug.WriteLine("Successful to load:" +XmlFile);
                            System.Console.WriteLine("Sucessful to change target server.");
                        }
                        else {
                            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\GameServer.xml"))
                            {
                                RunInCheck.xmlSetter.LoadDefaultConfigure(Environment.CurrentDirectory + "\\GameServer.xml");
                                RunInCheck.xmlSetter.ApplyTargetServer(Target);
                                System.Diagnostics.Debug.WriteLine("Successful to load:" + Environment.CurrentDirectory + "\\GameServer.xml");
                                System.Console.WriteLine("Sucessful to change target server.");
                            }
                            else {
                                System.Diagnostics.Debug.WriteLine("Fail to change.");
                            }
                        }
                    }
                    else {
                        continue;
                    }
                    continue;
                }

            }

            if (args.Args.Count() != 0)
            {
                if (AfterClose)
                {
                    InSystem.AppExit(0);
                }
            }
            //启动窗口 

            //如果用户输入了-language参数，那么自动会初始化用户语言
            if (RunInCheck.lang == null) {
                RunInCheck.SetInterfaceLanguage();//初始化语言
            }

            //检测 GameServer.XML文件
            if (RunInCheck.EnableXMLConfigure != false) {
                if (RunInCheck.includeXML == null) {
                    if (!System.IO.File.Exists(Environment.CurrentDirectory + "\\GameServer.xml"))
                    {
                        RunInCheck.EnableXMLConfigure = false;
                    }
                }
            }
            
            //载入预设
            if (RunInCheck.EnableXMLConfigure)
            {
                if (RunInCheck.xmlSetter == null) { 
                    RunInCheck.xmlSetter = new XMLConfigure();//初始化自定义类
                }
                if (RunInCheck.includeXML != null)
                {
                    if (RunInCheck.xmlSetter == null)
                    {
                        RunInCheck.xmlSetter.LoadDefaultConfigure(RunInCheck.includeXML);//载入目标配置
                    }
                }
                else {
                        RunInCheck.xmlSetter.LoadDefaultConfigure(Environment.CurrentDirectory + "\\GameServer.xml");
                }
            }
            else {
                if (RunInCheck.memSetter == null)
                {

                    RunInCheck.memSetter = new MemConfigure();//初始化预设类
                }
                RunInCheck.memSetter.LoadDefaultConfigure(null);
            }

            //如果运行时没有运行 -LoadConfigure命令则再次尝试加载本地配置
            if (!RunInCheck.IsCheckedPath) {
                RunInCheck.LoadLocateConfigure();
            }

            //创建主窗口类
            MainWindow mainWindow = new MainWindow();
            RunInCheck.LaunchApplication(mainWindow);
            mainWindow.ShowActivated = true;
            mainWindow.Show();
            
        }
        /// <summary>
        /// 获取指定命令后的命令数
        /// </summary>
        /// <param name="StartPos"></param>
        /// <param name="MoveSpace"></param>
        /// <param name="MainStringArray"></param>
        /// <returns></returns>
        private static string MoveCommand(int StartPos, int MoveSpace, String[] MainStringArray)
        {
            int TotalCount = MainStringArray.Count() - 1;
            if (StartPos > TotalCount)
            {
                return null;

            }
            else if (MoveSpace > TotalCount)
            {
                return null;
            }
            else if ((StartPos + MoveSpace) > TotalCount)
            {
                return null;
            }
            else
            {
                return MainStringArray[StartPos + MoveSpace];
            }
        }
    }
}
