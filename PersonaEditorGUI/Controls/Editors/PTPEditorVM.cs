using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.FileStructure.PTP;

namespace PersonaEditorGUI.Controls.Editors
{
    class PTPEditorVM
    {
        PTP ptp;
        public Classes.Media.Visual.Backgrounds BackImage { get; } = new Classes.Media.Visual.Backgrounds();

        public CharList OldCharList { get; set; }
        public CharList NewCharList { get; set; }

        public ObservableCollection<PTP.Names> Names
        {
            get { return ptp.names; }
        }

        public ObservableCollection<PTP.MSG> MSG
        {
            get { return ptp.msg; }
        }

        public PTPEditorVM(PTP ptp)
        {
            this.ptp = ptp;

            OldCharList = ptp.OldCharList;
            NewCharList = ptp.NewCharList;
        }
    }
}