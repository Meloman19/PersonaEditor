using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditor.Controls.ToolBox
{
    public enum ToolBoxType
    {
        SaveAsPTP,
        OpenPTP
    }

    public enum ToolBoxResult
    {
        Ok,
        Cancel,
        None
    }

    public class ToolBox
    {
        public static ToolBoxResult Show(ToolBoxType toolBoxType)
        {
            if(toolBoxType == ToolBoxType.SaveAsPTP)
            {
                if (ApplicationSettings.AppSetting.Default.SaveAsPTP_NeverAskAgain)
                    return ToolBoxResult.Ok;

                SaveAsPTP saveAsPTP = new SaveAsPTP();
                saveAsPTP.ShowDialog();
                return saveAsPTP.Result;
            }
            else if (toolBoxType == ToolBoxType.OpenPTP)
            {
                if (ApplicationSettings.AppSetting.Default.OpenPTP_NeverAskAgain)
                    return ToolBoxResult.Ok;

                OpenPTP openPTP = new OpenPTP();
                openPTP.ShowDialog();
                return openPTP.Result;
            }
            else
            {
                return ToolBoxResult.None;
            }
        }
    }
}