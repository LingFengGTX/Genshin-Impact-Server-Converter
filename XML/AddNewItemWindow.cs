using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinImpact_ServerConverter.XML
{
    public partial class AddNewItemWindow : Form
    {
        public AddNewItemWindow(List<XmlDocumentEditor.ConfigureItem> Target)
        {
            InitializeComponent();
            this.Target = Target;
        }
        private List<XmlDocumentEditor.ConfigureItem> Target;
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            XmlDocumentEditor.ConfigureItem configureItem = new XmlDocumentEditor.ConfigureItem();
            configureItem.Key = KeyString.Text;
            configureItem.Value = ValueString.Text;
            Target.Add(configureItem);
            this.Close();
        }
    }
}
