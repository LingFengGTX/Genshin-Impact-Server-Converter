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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GenshinImpact_ServerConverter.GMessageBox
{
    /// <summary>
    /// MessageBoxTip.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxTip : Window
    {
        public MessageBoxTip(string Title,string Content)
        {
            InitializeComponent();
            WindowTitle.Content = (string)Title;//设置MessageBox的标题
            WindowContent.Content = (string)Content;//设置MessageBox的内容
            this.Title = (string)Title;
        }

        private void PlayClosingAu()
        {

            Storyboard aniu = (Storyboard)FindResource("Closing");
            aniu.Begin();
            InSystem.AppSleep(200);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            GMessageBoxClass.Result = GMessageBoxDialogResult.OK;
            PlayClosingAu();
            this.Close();
        }
    }
}
