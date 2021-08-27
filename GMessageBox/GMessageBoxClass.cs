using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenshinImpact_ServerConverter.GMessageBox
{
    public enum GMessageBoxDialogResult {OK,Cancel};
    public enum GMessageBoxDialogType {Tip,Ask }

    public class GMessageBoxClass
    {

        public static GMessageBoxDialogResult Result=GMessageBoxDialogResult.Cancel;
        
        public static GMessageBoxDialogResult Show(string WindowTitle,string WindowContent,GMessageBoxDialogType DialogType,System.Windows.Window Parent) {

            switch (DialogType) {
                case GMessageBoxDialogType.Tip: {
                        MessageBoxTip tempWindow = new MessageBoxTip(WindowTitle,WindowContent);
                        tempWindow.Owner = Parent;
                        tempWindow.ShowDialog();

                    };break;
                case GMessageBoxDialogType.Ask:
                    {
                        MessageBoxAsk tempWindow = new MessageBoxAsk(WindowTitle, WindowContent);
                        tempWindow.Owner = Parent;
                        tempWindow.ShowDialog();
                    }; break;
            }
            return Result;
        }
    }
}
