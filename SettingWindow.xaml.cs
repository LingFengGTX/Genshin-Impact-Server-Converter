using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using IWshRuntimeLibrary;
using System.IO;
using System.Diagnostics;
//using GameValidator;
namespace GenshinImpact_ServerConverter
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            TargetPath.Text = DataOperat.GamePath;
            VersionShow.Content = (string)FindResource("SettingWindow_Label_TooleVersion") + AssemblyVersion;
            if (RunInCheck.EnableAssembly) {
                btn_GameValidator.Visibility = Visibility.Visible;
            }
            else {
                btn_GameValidator.Visibility = Visibility.Hidden;
            }

            if (RunInCheck.CanUseReStoreBack)
            {
                btn_ReSetBackStatues.Visibility = Visibility.Visible;
            }
            else {
                btn_ReSetBackStatues.Visibility = Visibility.Hidden;
            }
        }

        private void PlayClosingAu()
        {

            Storyboard aniu = (Storyboard)FindResource("Closing");
            aniu.Begin();
            InSystem.AppSleep(200);
        }

        private void btn_ExitButton_Click(object sender, RoutedEventArgs e)
        {
            PlayClosingAu();
            this.Close();
        }

        private void btn_ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataOperat.RequirePath(TargetPath.Text)) {
                GMessageBox.GMessageBoxClass.Show((string)FindResource("Error_MessageBoxTitle"), (string)FindResource("SettingWindow_Message_IsNotCheckedPath"), GMessageBox.GMessageBoxDialogType.Tip, this);
                return;
            }

            RunInCheck.IsCheckedPath = true;
            DataOperat.GamePath = TargetPath.Text;
            ScriptEngine.Script.InsertMapKey("<Game>", DataOperat.GamePath);
            InSystem.WriteConfig("Settings", "Install Path", DataOperat.GamePath);
            PlayClosingAu();
            this.Close();
        }

        private void btn_BorwerButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog tempDialog = new System.Windows.Forms.FolderBrowserDialog();
            tempDialog.Description = (string)FindResource("SettingWindow_Dialog_BrowseTarget") ;
            tempDialog.ShowNewFolderButton = false;
            if (tempDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                return;
            }
            TargetPath.Text = tempDialog.SelectedPath;
        }

        private void btn_sendToDesktop_Click(object sender, RoutedEventArgs e)
        {
            String shortcutPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),(string)FindResource("MainWindow_Title") + ".lnk");
            if (!System.IO.File.Exists(shortcutPath))
            {
                String exePath = Process.GetCurrentProcess().MainModule.FileName;
                IWshShell shell = new WshShell();
                foreach (var item in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "*.lnk"))
                {
                    WshShortcut tempShortcut = (WshShortcut)shell.CreateShortcut(item);
                    if (tempShortcut.TargetPath == exePath)
                    {
                        return;
                    }
                }
                WshShortcut shortcut = (WshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.Arguments = null;
                shortcut.Description = null;
                shortcut.WorkingDirectory = RunInCheck.UserDataDirectory;
                if (RunInCheck.IsCheckedPath)
                {
                    shortcut.IconLocation = DataOperat.GamePath+"\\Yuanshen.exe";
                }
                else {
                    shortcut.IconLocation = exePath;
                }
                shortcut.WindowStyle = 1;
                shortcut.Save();
            }
            GMessageBox.GMessageBoxClass.Show((string)FindResource("Tip_MessageBoxTitle"), (string)FindResource("Tip_CreateShotOK"), GMessageBox.GMessageBoxDialogType.Tip, this);
        }

        private void btn_RestoreBack_Click(object sender, RoutedEventArgs e)
        {

            if (!RunInCheck.IsCheckedPath) {
                return;
            }
            
            if (GMessageBox.GMessageBoxClass.Show((string)FindResource("Tip_MessageBoxTitle"), (string)FindResource("Tip_RestoreBackage"), GMessageBox.GMessageBoxDialogType.Ask, this) == GMessageBox.GMessageBoxDialogResult.Cancel) {
                return;
            }

            if (!System.IO.File.Exists(RunInCheck.UserDataDirectory+ "\\GameConfig.back")) {
                System.Windows.MessageBox.Show((string)FindResource("Error_RestoreBackage"),(string)FindResource("Error_MessageBoxTitle"),System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                InSystem.restoreBackup();
                RunInCheck.meLaunch.ReStoreThisBackup();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.ToString(), (string)FindResource("Error_MessageBoxTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        private void btn_ReSetBackStatues_Click(object sender, RoutedEventArgs e)
        {
            if (!RunInCheck.IsCheckedPath)
            {
                return;
            }
           
            if (GMessageBox.GMessageBoxClass.Show((string)FindResource("Tip_MessageBoxTitle"), (string)FindResource("Tip_ReSetBackage"), GMessageBox.GMessageBoxDialogType.Ask, this) == GMessageBox.GMessageBoxDialogResult.Cancel)
            {
                return;
            }

            try
            {
                InSystem.ResetBackup();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString(), (string)FindResource("Error_MessageBoxTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icos/CloseButtonPressed.png", UriKind.Absolute));
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icos/CloseButton.png", UriKind.Absolute));
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlayClosingAu();
            this.Close();
        }

        private void btn_OpenReDialog_Click(object sender, RoutedEventArgs e)
        {
            if (!RunInCheck.IsCheckedPath)
            {
                return;
            }

            Win_API.WinExec("explorer.exe \""+ DataOperat.GamePath + "\"", 1);
        }

        private void btn_GameValidator_Click(object sender, RoutedEventArgs e)
        {
            if (!RunInCheck.IsCheckedPath)
            {
                GMessageBox.GMessageBoxClass.Show("错误","若要使用此组件，请先设置游戏目录的位置。",GMessageBox.GMessageBoxDialogType.Tip,this);
                return;
            }
            /*
             * 该组件尚未完成开发，因此此功能将禁用。
             * 
                Validator.RunInCheckPath = DataOperat.GamePath;
                Validator.LaunchPlug(LaunchType.LaunchStart,this);
            */  
      }

        private void btn_OpenInTerminal_Click(object sender, RoutedEventArgs e)
        {
            using(Process cmdProcess = new Process())
            { cmdProcess.StartInfo.FileName = "cmd.exe";
                cmdProcess.StartInfo.Arguments = "/c " + RunInCheck.UserDataDirectory.Substring(0, 2) + "&cd \"" + RunInCheck.UserDataDirectory + "\"&cmd";
                cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.Start();
                if (!cmdProcess.HasExited) {
                    if (GMessageBox.GMessageBoxClass.Show((string)FindResource("SettingWindow_DialogTitle_TerminalClose"), (string)FindResource("SettingWindow_DialogContent_TerminalClose"), GMessageBox.GMessageBoxDialogType.Ask, this) == GMessageBox.GMessageBoxDialogResult.OK)
                    {
                        InSystem.AppExit(0);
                    }
                }
            }
            
        }
        //该部分来自Microsoft C#项目的About Window(WinFrom)
        #region 程序集特性访问器
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        #endregion

    }
}
