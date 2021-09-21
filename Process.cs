using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
namespace GenshinImpact_ServerConverter
{

    /// <summary>
    /// 程序配置类，该类用于存放程序运行时的配置
    /// </summary>
    class RunInCheck
    {
        private static System.Threading.Mutex instance = null; //程序线程检查
        public static bool IsCheckedPath = false;
        public static bool NoknowConfig = false;
        
        
        //启用组件组件
        public static bool EnableAssembly = false;

        //设置应用背景
        public static bool ChangeTheBackGround = false;
        public static string ImagePath = null;    

        public static string lang = null;

        public static System.Windows.Threading.DispatcherTimer GcCollecter = null;
        public static string VideoFile = null;
        public static bool LoadVideo = true;

        //是否在结束之前显示控制台窗口。
        public static bool RestoreConsole = true;
        public static IntPtr intptr;

        public static bool IsWindowMinSize = false; //程序是否处于最小化状态

        public static string UserDataDirectory = null;
        public static bool CanUseReStoreBack = false;

        public static ScriptOpreate meLaunch = null;

        public static bool IsGenShinDir() {
            string LaunchDirectory = RunInCheck.UserDataDirectory;
            if (System.IO.File.Exists(LaunchDirectory + "\\YuanShen.exe"))
            {
                return false;
            }
            else if (System.IO.File.Exists(LaunchDirectory + "\\GenshinImpact.exe")){
                return false;
            }
            else
            {
                if (System.IO.File.Exists(LaunchDirectory + "\\config.ini")){
                    return true;
                }
                else {
                    return false;
                }
                
            }
        }

        public static void LaunchGcCollecter(long second)
        {
            GcCollecter = new System.Windows.Threading.DispatcherTimer();
            GcCollecter.Interval = new TimeSpan(0,0,(int)second);
            GcCollecter.Tick += GcCollecter_Tick;
            GcCollecter.Start();//启动垃圾回收器
        }

        public static void LoadLocateConfigure() {
            //获取本地配置信息
            DataOperat.GamePath = InSystem.ReadConfig("Settings", "Install Path");

            //检查配置是否正确
            RunInCheck.IsCheckedPath = DataOperat.RequirePath(DataOperat.GamePath);
            if (RunInCheck.IsCheckedPath) {
                ScriptEngine.Script.InsertMapKey("<Game>", DataOperat.GamePath); ScriptEngine.Script.InsertMapKey("<Data>", Environment.CurrentDirectory + "\\Data");
            }
        }

        private static void GcCollecter_Tick(object sender, EventArgs e)
        {
            //执行垃圾回收
            GC.Collect();
            GC.WaitForFullGCComplete();
            //如果处于最小化状态则可以使用虚拟内存来暂存部分数据,而不是物理内存
            if (IsWindowMinSize&&(Environment.OSVersion.Platform == PlatformID.Win32NT)) {
                Win_API.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,-1,-1);
            }
        }

        public static void IsFirstLaunch() {
            if (InSystem.ReadConfig("Settings", "IsFirstLaunch") != "0") {
                
                try{
                    System.IO.File.Copy(DataOperat.GamePath+"\\config.ini",RunInCheck.UserDataDirectory+"\\GameConfig.back");
                }
                catch{ 
                   
                }
                InSystem.WriteConfig("Settings", "IsFirstLaunch", "0");
            }
        
        }

        public static void IfOtherRun(MainWindow tempWindow) {
            bool createdNew;
            instance = new System.Threading.Mutex(true, "Application", out createdNew);
            if (createdNew)
            {
                return;
            }
            else
            {
                System.Windows.MessageBox.Show((string)tempWindow.FindResource("Error_LaunchOlne"), (string)tempWindow.FindResource("Error_MessageBoxTitle"),System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Warning,System.Windows.MessageBoxResult.OK);
                InSystem.AppExit(-1);
            }
        }
        
        public static void SetInterfaceLanguage() {

            if (lang == "chinese") {
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary { Source = new Uri("pack://application:,,,/Languages/Chinese.xaml", UriKind.Absolute) };
                return;
            }
            if (lang == "english") {
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary { Source = new Uri("pack://application:,,,/Languages/English.xaml", UriKind.Absolute) };
                return;
            }
            //如果没有目标项则为自动获取语言
            uint LangCode = Win_API.GetOEMCP();//获取操作系统语言代码
            switch (LangCode) {//根据所传递的值来决定使用那种语言
                case 936:
                    {
                        //Chinese
                        System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary { Source = new Uri("pack://application:,,,/Languages/Chinese.xaml", UriKind.Absolute) };
                    }; break;
                case 437: {
                        //English
                        System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary { Source = new Uri("pack://application:,,,/Languages/English.xaml", UriKind.Absolute) };
                    }; break;
                default: {
                        System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary { Source = new Uri("pack://application:,,,/Languages/English.xaml", UriKind.Absolute) };
                    }; break;

            }
        }
        public static void ChangeBackGround(MainWindow Target) {
            if (!LoadVideo)
            {
                Target.MP4Player.Visibility = System.Windows.Visibility.Hidden;
                if (RunInCheck.ChangeTheBackGround)
                {
                    Target.BackGroundImage.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(RunInCheck.ImagePath));
                }
                else
                {
                    Target.BackGroundImage.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/Interface.jpg", UriKind.Absolute));
                }
            }
            else {
                //视频背景资源释放
                RunInCheck.VideoFile = RunInCheck.UserDataDirectory + "\\Background.mp4";
                if (!System.IO.File.Exists(RunInCheck.VideoFile))
                {
                    InSystem.CopyTempFile();
                }
                Target.MP4Player.Source = new Uri(RunInCheck.VideoFile, UriKind.Absolute);
                Target.MP4Player.LoadedBehavior =System.Windows.Controls.MediaState.Manual;
            } 
        }

        //启动程序
        public static void LaunchApplication(MainWindow Target)
        {

            RunInCheck.IfOtherRun(Target);
            if (RunInCheck.IsGenShinDir())
            {
                System.Windows.MessageBox.Show((string)Target.FindResource("Error_NotTargetDir"),(string)Target.FindResource("Error_MessageBoxTitle"), System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                InSystem.AppExit(-1);
            }

        }

    }

    /// 在程序中注册系统的API
    public class Win_API {
        //转移至虚拟内存
        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr process,int minSize,int maxSize);

        //执行函数
        [DllImport("kernel32.dll")]
        public static extern void WinExec(string AppString, int RunType);

        //获取操作系统语言代码
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetOEMCP();

        //获取文本类型的INI配置参数
        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        //写入INI配置参数
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        //设置窗体显示状态
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        //查找指定标题窗口句柄
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //计时器
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

    }

    /// <summary>
    /// 数据操作类，该类存放程序中数据操作的行为
    /// </summary>

    public class DataOperat
    {

        public static string GamePath = null;

        //判断是否为游戏目录。
        public static bool RequirePath(string Path)
        {
            if (Path == "null")
            {
                return false;
            }
            else if (System.IO.File.Exists(Path + "\\YuanShen.exe") && System.IO.File.Exists(Path + "\\config.ini"))
            {
                return true;
            }
            else if (System.IO.File.Exists(Path + "\\GenshinImpact.exe") && System.IO.File.Exists(Path + "\\config.ini"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }


    public class ScriptOpreate : IDataOpreat
    {
        public  XmlDocument ExpenedData = null;
        public  XmlNodeList ServerNodes = null;

        public int IfIndexIsTrue(string Title) {
            int Loop = -1;
            foreach (XmlNode tempNode in  ServerNodes) {
                Loop += 1;
                if (tempNode.Attributes["Name"].Value == Title) {
                    return Loop;
                }
            }
            return -1;
        }

        public void ReStoreThisBackup() {
            ScriptEngine.Script.LaunchScriptFromFile(ExpenedData.SelectSingleNode("Converter").SelectSingleNode("Head").SelectSingleNode("Script").Attributes["ReBackup"].Value, this.GetScriptFileName());
        }

        public void LoadDefaultConfigure(string Path) {
            if (!System.IO.File.Exists(Path)) {
                throw new Exception("The configure XML file can't to be loaded.");
            }
            this.ExpenedData = new XmlDocument();
            XmlReaderSettings ReadingRules = new XmlReaderSettings();
            ReadingRules.IgnoreComments = true;
            try {
                this.ExpenedData.Load(XmlReader.Create(Path, ReadingRules)) ;
            }
            catch (Exception exp) {
                throw exp;
            }
            this.ServerNodes = this.ExpenedData.SelectSingleNode("Converter").SelectSingleNode("Server").ChildNodes;
        }

        public bool CanUseBackUpFuncation() {
            if (this.ExpenedData.SelectSingleNode("Converter").SelectSingleNode("Head").SelectSingleNode("Script").Attributes["EnbaleRestore"].Value== "1")
            {
                return true;
            }
            else {
                return false;
            }
        }

        public string GetScriptFileName() {
            try {
                return ExpenedData.SelectSingleNode("Converter").SelectSingleNode("Head").SelectSingleNode("Script").Attributes["Using"].Value;
            }
            catch (Exception exp) {
                throw exp;
            }
        }

        public void RefreshServerList(System.Windows.Controls.ComboBox Target) {
            foreach (XmlNode tempNode in this.ServerNodes)
            {
                try
                {
                    Target.Items.Add(new System.Windows.Controls.ComboBoxItem().Content = tempNode.Attributes["Text"].Value);
                }
                catch {
                    continue;
                }
            }
        }

        public string GetServerNameFromIndex(int Index) {
            if (Index < 0 && Index > (this.ServerNodes.Count - 1))
            {
                return null;
            }
            return this.ServerNodes[Index].Attributes["Name"].Value;
        }

        public void ApplyTargetServer(int Target)
        {
            ScriptEngine.Script.LaunchScriptFromFile(this.GetServerNameFromIndex(Target), this.GetScriptFileName());
            InSystem.WriteConfig("Settings", "Server",this.GetServerNameFromIndex(Target));
        }

        public string GetServerIco(int Index) {
            if (Index < 0 && Index > (this.ServerNodes.Count - 1))
            {
                return null;
            }
            return this.ServerNodes[Index].Attributes["Ico"].Value;
        }

        public int GetServerType() {
            if (!RunInCheck.IsCheckedPath) {
                return -1;
            }
            
            string ServerCode = InSystem.ReadConfig("Settings", "Server");
            int Loop = -1;
            foreach (XmlNode tempNode in this.ServerNodes) {
                Loop += 1;
                if (ScriptEngine.Script.LaunchCheckScriptFromFile(tempNode.Attributes["CheckPoint"].Value,this.GetScriptFileName())) {
                    return Loop;
                }
            }
            return -1;
        }

        public string GetServerURL(int Index) {
            if (Index < 0 && Index > (this.ServerNodes.Count - 1)) {
                return null;
            }
            return this.ServerNodes[Index].Attributes["Url"].Value;
        }

        public string StartupExe(int Index) {
            if (Index < 0 && Index > (this.ServerNodes.Count - 1))
            {
                return null;
            }
            return this.ServerNodes[Index].Attributes["Startup"].Value;
        }
    }


    public class InSystem
    {        
        public static void ResetBackup()
        {
            try
            {
                if (RunInCheck.IsCheckedPath)
                {
                    System.IO.File.Copy(DataOperat.GamePath + "\\config.ini", RunInCheck.UserDataDirectory + "\\GameConfig.back", true);
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public static void CheckedPath() {
            if (!RunInCheck.IsCheckedPath)
            {
                throw new Exception("Failed to load settings!");

            }
            else
            {
                throw new Exception("Successfully loaded settings!");
            }
        }
        public static void ResetSetting()
        {
            try
            {
                if (System.IO.File.Exists(RunInCheck.UserDataDirectory + "\\App.ini"))
                {
                    System.IO.File.Delete(RunInCheck.UserDataDirectory + "\\App.ini");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public static void CopyTempFile()
        {
            byte[] byDll = global::GenshinImpact_ServerConverter.Properties.Resources.Interface;
            using (System.IO.FileStream fileWriter = new System.IO.FileStream(RunInCheck.VideoFile, System.IO.FileMode.Create))
            {
                fileWriter.Write(byDll, 0, byDll.Length);
                fileWriter.Close();
            }
        }
        public static void ClearBackup()
        {
            try
            {
                if (System.IO.File.Exists(RunInCheck.UserDataDirectory + "\\GameConfig.back"))
                {
                    System.IO.File.Delete(RunInCheck.UserDataDirectory + "\\GameConfig.back");
                }

            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public static void LaunchGame(int GameIndex,bool WaitFor,bool Collect) {
            if (!RunInCheck.IsCheckedPath) {
                throw new Exception("Cann't load locate configure.");
            }
            string GameFile = DataOperat.GamePath + "\\" + RunInCheck.meLaunch.StartupExe(GameIndex);
            if (!System.IO.File.Exists(GameFile)) {
                throw new Exception("Cann't Find The Target:"+ GameFile);
            }
            System.Diagnostics.Process GameProcess = new System.Diagnostics.Process();
            //
            GameProcess.StartInfo.FileName = GameFile;
            GameProcess.StartInfo.WorkingDirectory = DataOperat.GamePath;

            //执行游戏前进行一次垃圾回收释放内存空间。
            if (Collect) {
                GC.Collect();
            }
            
            try
            {
                GameProcess.Start();
            }
            catch
            {
                return;
            }

            if (!WaitFor) {
                return;
            }

            while (!GameProcess.HasExited)
            {
                System.Threading.Thread.Sleep(1000);//设置延迟设置为1秒，避免后台占用CPU过大
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public static void HidConsoleWindow(string ConsoleTitle) {
             RunInCheck.intptr = Win_API.FindWindow("ConsoleWindowClass", ConsoleTitle);
                         if (RunInCheck.intptr != IntPtr.Zero)
                             {
                              Win_API.ShowWindow(RunInCheck.intptr, 0);//隐藏这个窗口
                           }
        }

        public static void AppSleep(uint ms)
        {
            uint start = Win_API.GetTickCount();
            while (Win_API.GetTickCount() - start < ms)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public static void WriteConfig(string App,string Key,string Value) {
            Win_API.WritePrivateProfileString(App, Key, Value, RunInCheck.UserDataDirectory + "\\App.ini");
        }

        public static string ReadConfig(string App,string Key) {
            StringBuilder tempString = new StringBuilder();
            Win_API.GetPrivateProfileString(App,Key,"null",tempString,255, RunInCheck.UserDataDirectory + "\\App.ini");
            return tempString.ToString();
        }
        public static void AppExit(int Code) {
            if (RunInCheck.RestoreConsole)
            {
                Win_API.ShowWindow(RunInCheck.intptr, 1);
            }
            Environment.Exit(Code);
        }
        public static void restoreBackup() {
            try
            {
                if (!RunInCheck.IsCheckedPath)
                {
                    return;
                }

                if (System.IO.File.Exists(RunInCheck.UserDataDirectory + "\\GameConfig.back"))
                {
                    System.IO.File.Copy(RunInCheck.UserDataDirectory + "\\GameConfig.back", DataOperat.GamePath + "\\config.ini", true);
                }

            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

    }

}
