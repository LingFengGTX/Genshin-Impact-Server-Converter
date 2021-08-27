using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace GenshinImpact_ServerConverter
{
    /// <summary>
    /// XmlDocumentEditor.xaml 的交互逻辑
    /// </summary>
    public partial class XmlDocumentEditor : Window
    {
        public XmlDocumentEditor(string FileName)
        {
            InitializeComponent();
            Official_Configure = new List<ConfigureItem>();
            BiliBili_Configure = new List<ConfigureItem>();

            if (FileName != null) {
                LoadXmlFile(FileName);
            }
        }

        private int ServerType = 0;
        public class ConfigureItem {

            public string Key { set; get; }
            public string Value { set; get; }
        }

        private List<ConfigureItem> Official_Configure = null;
        private List<ConfigureItem> BiliBili_Configure = null;
        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show((string)FindResource("Xml_Window_CancelDialog_title"),(string)FindResource("Tip_MessageBoxTitle"),MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK) {
                this.Close();
            }
        }


        private void RefreshList() {
            MainGrid.ItemsSource = null;
            switch (ServerType) {
                case 0: {
                        MainGrid.ItemsSource = Official_Configure;
                        
                    } break;
                case 1: {
                        MainGrid.ItemsSource = BiliBili_Configure;
                        
                    } break;
            }
            MainGrid.UpdateLayout();
        }
        private void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog OpenFileDialog=new System.Windows.Forms.OpenFileDialog()) {
                OpenFileDialog.Title = (string)FindResource("Xml_Window_OpenFileDialog_title");
                OpenFileDialog.Filter = "XML Document File|*.xml";
                if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                    return;
                }
                LoadXmlFile(OpenFileDialog.FileName);
                
            }
        }

        private void LoadXmlFile(string FileName) {
            //清空以前的数据信息
            BiliBili_Configure.Clear();
            Official_Configure.Clear();
            //加载官方服务器配置
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(FileName);
                XmlNodeList MainNode = xmlDocument.SelectSingleNode("Servers").SelectSingleNode("Official").ChildNodes;

                ConfigureItem tempItem = null;


                //遍历本体列表
                foreach (XmlNode tempNode in MainNode)
                {
                    tempItem = new ConfigureItem();
                    tempItem.Key = tempNode.Attributes["ConfigMember"].Value;
                    tempItem.Value = tempNode.InnerText;
                    Official_Configure.Add(tempItem);
                    tempItem = null;
                }

                //加载BiliBili服务器配置

                MainNode = xmlDocument.SelectSingleNode("Servers").SelectSingleNode("BiliBili").ChildNodes;

                tempItem = null;


                //遍历本体列表
                foreach (XmlNode tempNode in MainNode)
                {
                    tempItem = new ConfigureItem();
                    tempItem.Key = tempNode.Attributes["ConfigMember"].Value;
                    tempItem.Value = tempNode.InnerText;
                    BiliBili_Configure.Add(tempItem);
                    tempItem = null;
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message,"Error");
            }
            //刷新界面列表
            RefreshList();
        }

        private void Menu_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            using(System.Windows.Forms.SaveFileDialog SaveDialog=new System.Windows.Forms.SaveFileDialog()){ 
                SaveDialog.Title=(string)FindResource("Xml_Window_SaveFileDialog_title");
                SaveDialog.Filter = "XML Document File|*.xml";
                if (SaveDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                System.Xml.XmlTextWriter XWriter = new System.Xml.XmlTextWriter(SaveDialog.FileName, Encoding.UTF8);
                XWriter.WriteStartDocument(false);
                XWriter.WriteComment((string)FindResource("Xml_Window_Log_XMLintro"));
                XWriter.WriteStartElement("Servers");

                //写入自定义官方配置文件
                XWriter.WriteComment((string)FindResource("Xml_Window_Log_XMLintro_Official"));
                XWriter.WriteStartElement("Official");
                foreach (ConfigureItem tempData in Official_Configure)
                {
                    XWriter.WriteStartElement("Configure");
                    XWriter.WriteAttributeString("ConfigMember", tempData.Key);
                    XWriter.WriteString(tempData.Value);
                    XWriter.WriteEndElement();
                }
                XWriter.WriteEndElement();

                //写入自定义BiliBili配置文件
                XWriter.WriteComment((string)FindResource("Xml_Window_Log_XMLintro_BiliBili"));
                XWriter.WriteStartElement("BiliBili");
                foreach (ConfigureItem tempData in BiliBili_Configure)
                {
                    XWriter.WriteStartElement("Configure");
                    XWriter.WriteAttributeString("ConfigMember", tempData.Key);
                    XWriter.WriteString(tempData.Value);
                    XWriter.WriteEndElement();
                }
                XWriter.WriteEndElement();
                XWriter.WriteEndElement();
                XWriter.Flush();
                XWriter.Close();
            }
        }

        private void Menu_DeleteTarget_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.SelectedIndex == -1)
            {
                MessageBox.Show((string)FindResource("Xml_Window_Tip_NoSelect"), (string)FindResource("Error_MessageBoxTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show((string)FindResource("Xml_Window_DeleteDialog_Context"), (string)FindResource("Tip_MessageBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }

            switch (ServerType) {

                case 0: {
                        Official_Configure.RemoveAt(MainGrid.SelectedIndex);
                    } break;
                case 1: {
                        BiliBili_Configure.RemoveAt(MainGrid.SelectedIndex);
                    }
                    break;
            }
            RefreshList();
        }

        private void Menu_ToOfficialServer_Click(object sender, RoutedEventArgs e)
        {
            ServerType = 0;
            Menu_ToBiliBiliServer.IsChecked = false;
            Menu_ToOfficialServer.IsChecked = true;
            RefreshList();
        }

        private void Menu_ToBiliBiliServer_Click(object sender, RoutedEventArgs e)
        {
            ServerType = 1;
            Menu_ToOfficialServer.IsChecked = false;
            Menu_ToBiliBiliServer.IsChecked = true;
            RefreshList();
        }

        private void Menu_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show((string)FindResource("Xml_Window_ClearDialog_Context"), (string)FindResource("Tip_MessageBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }

                switch (ServerType)
            {

                case 0:
                    {
                        Official_Configure.Clear();
                    }
                    break;
                case 1:
                    {
                        BiliBili_Configure.Clear();
                    }
                    break;
            }
            RefreshList();
        }

        private void Menu_Add_Click(object sender, RoutedEventArgs e)
        {
            switch (ServerType) {
                case 0: {
                        using (XML.AddNewItemWindow tempWindow = new XML.AddNewItemWindow(Official_Configure))
                        {
                            tempWindow.ShowDialog();
                        }
                    } break;
                case 1: {
                        using (XML.AddNewItemWindow tempWindow = new XML.AddNewItemWindow(BiliBili_Configure))
                        {
                            tempWindow.ShowDialog();
                        }
                    } break;
            
            }
            RefreshList();
        }

        private void Menu_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (MainGrid.SelectedIndex == -1)
            {
                MessageBox.Show((string)FindResource("Xml_Window_Tip_NoSelect"), (string)FindResource("Error_MessageBoxTitle"),MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }          

            switch (ServerType)
            {

                case 0:
                    {
                        using (XML.EditWindow tempWindow=new XML.EditWindow(Official_Configure[MainGrid.SelectedIndex])) {
                            tempWindow.ShowDialog();
                        }   
                    }
                    break;
                case 1:
                    {
                        using (XML.EditWindow tempWindow = new XML.EditWindow(BiliBili_Configure[MainGrid.SelectedIndex]))
                        {
                            tempWindow.ShowDialog();
                        }
                    }
                    break;
            }
            RefreshList();
        }
    }
}
