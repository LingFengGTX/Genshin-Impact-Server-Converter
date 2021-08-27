using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Windows;

namespace GenshinImpact_ServerConverter
{
    
    //使用XML文件配置游戏设置
    
    public class XMLConfigure : IDataOpreat
    {
        private class ConfigureMember {
            public string header;
            public string member;
            public string value;
        }

        private static List<ConfigureMember> Official_Configure = null;
        private static List<ConfigureMember> BiliBili_Configure = null;
        public void ApplyTargetServer(int TargetType) {
            List<ConfigureMember> TargetConfigure = null;
            //设置配置引用指针
            switch (TargetType) {
                case 0: {
                        TargetConfigure = Official_Configure;
                        //应用配置
                        Win_API.WritePrivateProfileString("Location", "GameServer", "Official", Environment.CurrentDirectory + "\\App.ini");
                    };break;
                case 1: {
                        TargetConfigure = BiliBili_Configure;
                        //应用配置
                        Win_API.WritePrivateProfileString("Location", "GameServer", "BiliBili", Environment.CurrentDirectory + "\\App.ini");
                    }; break;
            
            }
            //写入配置
            foreach (ConfigureMember tempMember in TargetConfigure) {
                Win_API.WritePrivateProfileString(tempMember.header, tempMember.member, tempMember.value, DataOperat.GamePath + "\\Config.ini");
            }
        
        }

        public void LoadDefaultConfigure(string FilePath)
        {
            ConfigureMember tempMemberClass = null;
            try {
                //官方服务器配置载入
                XmlDocument XmlDocReader = new XmlDocument();
                XmlDocReader.Load(FilePath);

                System.Xml.XmlNode O_MainNode = XmlDocReader.SelectSingleNode("Servers");
                System.Xml.XmlNode O_parents = O_MainNode.SelectSingleNode("Official");
                Official_Configure = new List<ConfigureMember>();
                foreach (XmlNode tempNode in O_parents.ChildNodes) {
                    tempMemberClass = new ConfigureMember();
                    tempMemberClass.header = "General";
                    tempMemberClass.member = tempNode.Attributes["ConfigMember"].Value;
                    tempMemberClass.value = tempNode.InnerText;
                    
                    Official_Configure.Add(tempMemberClass);
                    tempMemberClass = null;
                }

               
                //BiliBili服务器配置载入;
                System.Xml.XmlNode B_MainNode = XmlDocReader.SelectSingleNode("Servers");
                System.Xml.XmlNode B_parents = B_MainNode.SelectSingleNode("BiliBili");
               BiliBili_Configure = new List<ConfigureMember>();
                foreach (XmlNode tempNode in B_parents.ChildNodes)
                {
                    tempMemberClass = new ConfigureMember();
                    tempMemberClass.header = "General";
                    tempMemberClass.member = tempNode.Attributes["ConfigMember"].Value;
                    tempMemberClass.value = tempNode.InnerText;

                    BiliBili_Configure.Add(tempMemberClass);
                    tempMemberClass = null;
                }

                XmlDocReader = null;

            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                InSystem.AppExit(-1);
            }
        }

        public int GetServerType()
        {
            StringBuilder RString = new StringBuilder();
            Win_API.GetPrivateProfileString("Location", "GameServer", "null", RString, 255, Environment.CurrentDirectory + "\\App.ini");
            if (RString.ToString()=="Official") {
                return 0;
            } else if (RString.ToString() == "BiliBili") {
                return 1;
            }
            else {
                return -1;
            }
            

        }
        
    }
}
