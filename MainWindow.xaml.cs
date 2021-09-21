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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GenshinImpact_ServerConverter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool IsMoveSignel = false;//鼠标拖动触发信号
        public MainWindow()
        {
            InitializeComponent();
            
            {//初始类构造:用于初始化此应用中必要的类 
                RunInCheck.ChangeBackGround(this);//随机应用背景

                RunInCheck.meLaunch.RefreshServerList(this.ServerSelectBox);

                if (!RunInCheck.IsCheckedPath)
                {
                    ServerSelectBox.IsEnabled = false;
                    Error_TipBox.Visibility = Visibility.Visible;
                }
                else {
                    ServerSelectBox.IsEnabled = true;
                    Error_TipBox.Visibility = Visibility.Hidden;
                    ReloadSetting();
                }
            }
        }
        private void ReloadSetting() {
            int LocationConfigureType=RunInCheck.meLaunch.GetServerType();

            if (LocationConfigureType==-1) {
                LogoImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/Icos/NoConfig.jpg", UriKind.Absolute)); RunInCheck.NoknowConfig = true;
            }
            ServerSelectBox.SelectedIndex = LocationConfigureType;
            if (!RunInCheck.NoknowConfig) {
                RunInCheck.IsFirstLaunch();
            }
            StringBuilder gameVersion = new StringBuilder();
            Win_API.GetPrivateProfileString("General", "game_version", "null", gameVersion, 255, DataOperat.GamePath + "\\Config.ini");
            Label_GameVersion.Content =(string)FindResource("MainWindow_Label_GameVersion") + gameVersion.ToString();
        }

        
        private void MainWindowGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip) return;
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                IsMoveSignel = WindowState == WindowState.Maximized;
                DragMove();

            }
        }

        private void MainWindowGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //此方法是用于结束鼠标拖动事件
            IsMoveSignel = false;
        }

        private void MainWindowGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMoveSignel)
            {
                IsMoveSignel = false;
                WindowState = WindowState.Normal;
                var point = e.MouseDevice.GetPosition(this);
                this.Left =(point.X - MainWindowGrid.ActualWidth * point.X)/ SystemParameters.WorkArea.Width;
                this.Top = (point.Y - MainWindowGrid.ActualHeight * point.Y) / SystemParameters.WorkArea.Height;
                this.DragMove();
            }
        }

        private void btn_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            InSystem.AppExit(0);
        }

        private void btn_LaunchGame_Click(object sender, RoutedEventArgs e)
        {
            
            if (!RunInCheck.IsCheckedPath) {
                GMessageBox.GMessageBoxClass.Show((string)FindResource("Error_MessageBoxTitle"), (string)FindResource("MainWindow_Label_PathError"), GMessageBox.GMessageBoxDialogType.Tip,this);
                return;
            }

            this.MP4Player.Pause();
            this.Hide();
            RunInCheck.IsWindowMinSize = true;//激活最小化以启用虚拟内存
            try
            {
                InSystem.LaunchGame(ServerSelectBox.SelectedIndex, true, true);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, (string)FindResource("Error_MessageBoxTitle"), MessageBoxButton.OK,MessageBoxImage.Error);
            }
            finally {
                RunInCheck.IsWindowMinSize = false;
                this.Show();
                this.MP4Player.Play();
            }
            
        }

        private void ServerSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerSelectBox.SelectedIndex == -1) {
                return;
            }
            
            if (RunInCheck.NoknowConfig)
            {
                if (GMessageBox.GMessageBoxClass.Show((string)FindResource("Tip_MessageBoxTitle"), (string)FindResource("Tip_NoKnoewConfig"), GMessageBox.GMessageBoxDialogType.Ask, this) == GMessageBox.GMessageBoxDialogResult.Cancel)
                {
                    ServerSelectBox.SelectedIndex = -1;
                    return;
                }
                RunInCheck.NoknowConfig = false;

            }

            try
            {
                RunInCheck.meLaunch.ApplyTargetServer(ServerSelectBox.SelectedIndex);
            }
            catch {
                return;
            }
            LogoImage.ImageSource = new BitmapImage(new Uri(RunInCheck.meLaunch.GetServerIco(ServerSelectBox.SelectedIndex),UriKind.RelativeOrAbsolute));
        }

        private void LogoImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ServerSelectBox.SelectedIndex == -1) {
                return;
            }
            Win_API.WinExec("explorer.exe \""+RunInCheck.meLaunch.GetServerURL(ServerSelectBox.SelectedIndex) +" \"", 15);

        }

        private void btn_SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow tempWindow = new SettingWindow();
            tempWindow.ShowActivated = true;
            this.MP4Player.Pause();
            this.Visibility = Visibility.Hidden;
            tempWindow.ShowDialog();
            if (!RunInCheck.IsCheckedPath)
            {
                ServerSelectBox.IsEnabled = false;
                Error_TipBox.Visibility = Visibility.Visible;
            }
            else
            {
                ServerSelectBox.IsEnabled = true;
                Error_TipBox.Visibility = Visibility.Hidden;
                ReloadSetting();
            }
            this.Visibility = Visibility.Visible;
            this.MP4Player.Play();
        }

        private void btn_MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MP4Player_Loaded(object sender, RoutedEventArgs e)
        {
            this.MP4Player.Play();
        }

        private void MP4Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.MP4Player.Stop();
            this.MP4Player.Play();
        }
    }
}
