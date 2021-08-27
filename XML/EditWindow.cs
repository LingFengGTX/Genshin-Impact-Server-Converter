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
    public partial class EditWindow : Form
    {
        public EditWindow(XmlDocumentEditor.ConfigureItem Target)
        {
            InitializeComponent();
            this.Target = Target;
            KeyString.Text = this.Target.Key;
            ValueString.Text = this.Target.Value;
        }
        private XmlDocumentEditor.ConfigureItem Target;
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Target.Key = KeyString.Text;
            Target.Value = ValueString.Text;
            
            this.Close();
        }
    }
}
