using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
namespace GenshinImpact_ServerConverter
{

    /// <summary>
    /// 程序配置类，该类用于存放程序运行时的配置
    /// </summary>
    class RunInCheck
    {
        private const int ImageCount = 5;//图像数量限制阀门
        private static System.Threading.Mutex instance = null; //程序线程检查
        public static bool IsCheckedPath = false;
        public static bool NoknowConfig = false;
        
        
        //启用组件组件
        public static bool EnableAssembly = false;

        //设置应用背景
        public static bool ChangeTheBackGround = false;
        public static string ImagePath = null;

        public static bool EnableXMLConfigure = true; //默认开启
        
        //引用文件路径
        public static string includeXML = null;

        public static string lang = null;

        public static System.Windows.Threading.DispatcherTimer GcCollecter = null;
        public static string VideoFile = null;
        public static bool LoadVideo = true;

        //是否在结束之前显示控制台窗口。
        public static bool RestoreConsole = true;
        public static IntPtr intptr;

        //数据操作类
        public static XMLConfigure xmlSetter = null;
        public static MemConfigure memSetter = null;

        public static bool IsGenShinDir() {
            string LaunchDirectory = Environment.CurrentDirectory;
            if (!System.IO.File.Exists(LaunchDirectory + "\\YuanShen.exe") || !System.IO.File.Exists(LaunchDirectory + "\\config.ini"))
            {
                return false;
            }
            else {
                return true;
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
            StringBuilder tempPathStringBulider = new StringBuilder();
            Win_API.GetPrivateProfileString("Settings", "Install Path", "null", tempPathStringBulider, 255, Environment.CurrentDirectory + "\\App.ini");
            DataOperat.GamePath = tempPathStringBulider.ToString();

            //检查配置是否正确
            RunInCheck.IsCheckedPath = DataOperat.RequirePath(DataOperat.GamePath);
        }

        private static void GcCollecter_Tick(object sender, EventArgs e)
        {
            //执行垃圾回收
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        public static void IsFirstLaunch() {
            StringBuilder RString = new StringBuilder();
            Win_API.GetPrivateProfileString("Settings","IsFirstLaunch","1",RString,255,Environment.CurrentDirectory+"\\App.ini");
            if (RString.ToString() == "1") {
                
                try{
                    System.IO.File.Copy(DataOperat.GamePath+"\\config.ini",Environment.CurrentDirectory+"\\GameConfig.back");
                }
                catch{ 
                   
                }
                Win_API.WritePrivateProfileString("Settings", "IsFirstLaunch", "0",Environment.CurrentDirectory + "\\App.ini");
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
                RunInCheck.VideoFile = System.IO.Path.GetTempPath() + "\\GenShenImpact_ServerConvert\\Background.mp4";
                if (!System.IO.Directory.Exists(System.IO.Path.GetTempPath() + "\\GenShenImpact_ServerConvert")) {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetTempPath() + "\\GenShenImpact_ServerConvert");

                }
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
                GMessageBox.GMessageBoxClass.Show((string)Target.FindResource("Error_MessageBoxTitle"), (string)Target.FindResource("Error_NotTargetDir"), GMessageBox.GMessageBoxDialogType.Tip, Target);
                InSystem.AppExit(-1);
            }

            //开启WinForm界面美化
            System.Windows.Forms.Application.EnableVisualStyles();
        }

    }

    /// 在程序中注册系统的API
    public class Win_API {
        [DllImport("kernel32.dll")]
        public static extern void WinExec(string AppString, int RunType);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetOEMCP();

        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
      [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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
            else if (!System.IO.File.Exists(Path + "\\YuanShen.exe") || !System.IO.File.Exists(Path + "\\config.ini"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
    public class MemConfigure : IDataOpreat
    {
        //预设配置数据
        private ConfigureData BiliBili_Configure;
        private ConfigureData Official_Configure;

        public void LoadDefaultConfigure(string Target)
        {
            BiliBili_Configure = new ConfigureData { sub_channel = "0", channel = "14", cps = "bilibili" };
            Official_Configure = new ConfigureData { sub_channel = "1", channel = "1", cps = "pcadbdpz" };
        }

        //将获取的配置数据与标准数据对比，判断游戏当前使用的是何种服务器
        public int GetServerType()
        {
            ConfigureData tempData = ReadInLocation();
            if (CompareStruct(tempData, Official_Configure))
            {
                return 0;
            }
            else if (CompareStruct(tempData, BiliBili_Configure))
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }

        public struct ConfigureData
        {
            public string channel;
            public string sub_channel;
            public string cps;

        }

        private bool CompareStruct(ConfigureData a, ConfigureData b)
        {
            if (a.channel != b.channel)
            {
                return false;
            }

            if (a.sub_channel != b.sub_channel)
            {
                return false;
            }
            return true;
        }

        private ConfigureData ReadInLocation()
        {
            ConfigureData tempData = new ConfigureData { };
            StringBuilder GetString = new StringBuilder();
            Win_API.GetPrivateProfileString("General", "sub_channel", "NULL", GetString, 255, DataOperat.GamePath + "\\Config.ini");
            tempData.sub_channel = GetString.ToString();

            Win_API.GetPrivateProfileString("General", "channel", "NULL", GetString, 255, DataOperat.GamePath + "\\Config.ini");
            tempData.channel = GetString.ToString();

            Win_API.GetPrivateProfileString("General", "cps", "NULL", GetString, 255, DataOperat.GamePath + "\\Config.ini");
            tempData.cps = GetString.ToString();
            return tempData;
        }

        //应用配置数据
        public void ApplyTargetServer(int Target)
        {
            switch (Target)
            {
                case 0:
                    {
                        Win_API.WritePrivateProfileString("General", "sub_channel", Official_Configure.sub_channel, DataOperat.GamePath + "\\Config.ini");
                        Win_API.WritePrivateProfileString("General", "cps", Official_Configure.cps, DataOperat.GamePath + "\\Config.ini");
                        Win_API.WritePrivateProfileString("General", "channel", Official_Configure.channel, DataOperat.GamePath + "\\Config.ini");
                    }; break;
                case 1:
                    {
                        Win_API.WritePrivateProfileString("General", "sub_channel", BiliBili_Configure.sub_channel, DataOperat.GamePath + "\\Config.ini");
                        Win_API.WritePrivateProfileString("General", "cps", BiliBili_Configure.cps, DataOperat.GamePath + "\\Config.ini");
                        Win_API.WritePrivateProfileString("General", "channel", BiliBili_Configure.channel, DataOperat.GamePath + "\\Config.ini");
                    }; break;
            }
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
                    System.IO.File.Copy(DataOperat.GamePath + "\\config.ini", Environment.CurrentDirectory + "\\GameConfig.back", true);
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public static void ResetSetting()
        {
            try
            {
                if (System.IO.File.Exists(Environment.CurrentDirectory + "\\App.ini"))
                {
                    System.IO.File.Delete(Environment.CurrentDirectory + "\\App.ini");
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
                if (System.IO.File.Exists(Environment.CurrentDirectory + "\\GameConfig.back"))
                {
                    System.IO.File.Delete(Environment.CurrentDirectory + "\\GameConfig.back");
                }

            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public static void LaunchGame(bool WaitFor,bool Collect) {
            if (!RunInCheck.IsCheckedPath) {
                return;
            }

            System.Diagnostics.Process GameProcess = new System.Diagnostics.Process();
            GameProcess.StartInfo.FileName = DataOperat.GamePath + "\\YuanShen.exe";
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
                System.Threading.Thread.Sleep(1000);//设置延迟，避免后台占用CPU过大
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public static int StringIndex(string stringCode) {
            if (stringCode == "official")
            {
                return 0;
            }
            else if (stringCode == "bilibili") {
                return 1;
            }
            else {
                return -1;
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

                if (System.IO.File.Exists(Environment.CurrentDirectory + "\\GameConfig.back"))
                {
                    System.IO.File.Copy(Environment.CurrentDirectory + "\\GameConfig.back", DataOperat.GamePath + "\\config.ini", true);
                }

            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

    }
}
