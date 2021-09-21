using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using ScriptEngine;
namespace GenshinImpact_ServerConverter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public void LaunchFuncation(object sender,StartupEventArgs args) {

            //尝试初始化脚本类
            try
            {
                InitializeScriptClass();
            }
            catch (Exception exp) {
                System.Console.WriteLine("Error:{0}",exp.Message);
                InSystem.AppExit(-1);
            }


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
                            InSystem.WriteConfig("Settings", "Install Path", Path);
                        }
                    } else if (SetType=="first") {
                        string reset = MoveCommand(Loop, 2, args.Args);
                        if (reset == "yes")
                        {
                            InSystem.WriteConfig("Settings", "IsFirstLaunch", "0");
                        }
                        else if (reset == "no") {
                            InSystem.WriteConfig("Settings", "IsFirstLaunch", "1");
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
                        Win_API.WinExec("explorer.exe \""+ Environment.CurrentDirectory + "\"",1);
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
                    try
                    {
                        InSystem.CheckedPath();
                    }
                    catch (Exception exp) {

                        System.Console.WriteLine(exp.Message);
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
                    int GameIndex = RunInCheck.meLaunch.GetServerType();
                    if (GameIndex == -1) {
                        System.Console.WriteLine("Error:Failed to load settings.");
                        continue;
                    }
                    if (MoveCommand(Loop, 1, args.Args) == "wait")
                    {
                        InSystem.LaunchGame(GameIndex, true,false);
                    }
                    else {
                        InSystem.LaunchGame(GameIndex, false,false);
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

                if (Command == "-showServerList") {
                    System.Console.WriteLine("Servers:\tName");
                    System.Console.WriteLine("---------------------------");
                    foreach (System.Xml.XmlNode tempNode in RunInCheck.meLaunch.ServerNodes) {
                        System.Console.WriteLine("{0}\t{1}",tempNode.Attributes["Name"].Value,tempNode.Attributes["Text"].Value);
                    }
                    System.Console.WriteLine("---------------------------");
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
                        System.Console.WriteLine("The cycle setting is wrong.");
                    }
                    continue;
                }

                if (Command == "-enableAssembly") {
                    //启动实验性组件功能
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

                if (Command == "-server") {


                    if (!RunInCheck.IsCheckedPath) {
                        System.Console.WriteLine("Cann't know locate configure!");
                        continue;
                    }
                    
                    //服务器切换操作
                    if (RunInCheck.meLaunch.IfIndexIsTrue(MoveCommand(Loop, 1, args.Args)) != -1)
                    {
                        RunInCheck.meLaunch.ApplyTargetServer(RunInCheck.meLaunch.IfIndexIsTrue(MoveCommand(Loop, 1, args.Args)));
                    }
                    else {
                        System.Console.WriteLine("The target server not true!");
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
            

            //如果用户没有输入-language参数，那么自动会初始化用户语言
            if (RunInCheck.lang == null) {
                RunInCheck.SetInterfaceLanguage();//初始化语言
            }


            //载入预设

            //如果运行时没有运行 -LoadConfigure命令则再次尝试加载本地配置
            if (!RunInCheck.IsCheckedPath) {
                RunInCheck.LoadLocateConfigure();
            }
            
            //如果用户没有使用-gc命令则自动创建一个gc定时回收器 默认 15分钟(900秒)执行一次垃圾回收
            if (RunInCheck.GcCollecter==null) {
                RunInCheck.LaunchGcCollecter(900);
            }

            //是否启用恢复功能
            RunInCheck.CanUseReStoreBack = RunInCheck.meLaunch.CanUseBackUpFuncation();
            //创建主窗口类
            MainWindow mainWindow = new MainWindow();
            RunInCheck.LaunchApplication(mainWindow);
            mainWindow.ShowActivated = true;
            mainWindow.Show();
            
        }

        public static void InitializeScriptClass() {
            RunInCheck.meLaunch = new ScriptOpreate();
            if (RunInCheck.meLaunch == null) {
                throw new Exception("Important classes are not initialized.");
            }
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GenshinImpact_ServerConverter")) {
                try
                {
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GenshinImpact_ServerConverter");
                }
                catch (Exception exp) {
                    System.Console.WriteLine("Error:{0}",exp.Message) ;
                    InSystem.AppExit(-1);
                }
            }
            RunInCheck.UserDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\GenshinImpact_ServerConverter";
            
            try
            {
                RunInCheck.meLaunch.LoadDefaultConfigure(Environment.CurrentDirectory+"\\"+ System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".xml");
            }
            catch (Exception exp) {
                throw exp;
            }
        
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
