using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls
{
    enum TabItemType
    {
        Null,
        Image,
        SPR,
        PTP,
        BMD,
        FNT,
        HEX
    }

    class ClosableTabItemVM : BindingObject
    {
        public ICommand FileClose { get; }
        private void CloseFile()
        {
            Close();
        }

        private TabItemType dataContextType = TabItemType.Null;
        private object dataContext = null;

        public string TabTitle { get; set; }

        public string DataContextType => dataContextType.ToString();
        public object DataContext
        {
            get { return dataContext; }
            set
            {
                if (dataContext != value)
                {
                    dataContext = value;
                    Notify("DataContext");
                }
            }
        }
        public bool IsClosable { get; set; } = true;

        public void SetDataContextType(TabItemType tabItemType)
        {
            dataContextType = tabItemType;
            Notify("DataContextType");
        }

        public bool Close()
        {
            if (IsClosable)
            {
                if (DataContext is IViewModel vm)
                    if (!vm.Close())
                        return false;

                Notify("Close");
            }

            SetDataContextType(TabItemType.Null);
            DataContext = null;
            return true;
        }

        public ClosableTabItemVM()
        {
            FileClose = new RelayCommand(CloseFile);
        }
    }
}