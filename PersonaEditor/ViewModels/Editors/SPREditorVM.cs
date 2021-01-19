using System.Collections.ObjectModel;
using System.Windows;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.SpriteContainer;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class SPREditorVM : BindingObject, IEditor
    {
        public ObservableCollection<SPRTextureVM> TextureList { get; } = new ObservableCollection<SPRTextureVM>();

        public SPREditorVM(SPR spr)
        {
            
            if (spr == null)
                throw new System.ArgumentNullException(nameof(spr));

            for (int i = 0; i < spr.SubFiles.Count; i++)
                TextureList.Add(new SPRTextureVM(spr.SubFiles[i], spr.KeyList.List, i));
        }

        private bool isFirst = true;
        private bool isPCFormat;
        public bool IsPCFormat
        {
            get
            {
                return isPCFormat;
            }
            set
            {
                if (isFirst == true)
                {
                    isFirst = false;
                    //foreach (var texture in TextureList)
                    //{
                    //    foreach (var key in texture.KeyList)
                    //    {
                    //        key.X1 /= 2;
                    //        key.X2 /= 2;
                    //        key.Y1 /= 2;
                    //        key.Y2 /= 2;
                    //    }
                    //}
                }
                else
                {

                    isPCFormat = value;

                    if (isPCFormat == true)
                    {
                        foreach (var texture in TextureList)
                        {
                            foreach (var key in texture.KeyList)
                            {
                                key.X1 *= 2;
                                key.X2 *= 2;
                                key.Y1 *= 2;
                                key.Y2 *= 2;
                            }
                        }

                    }
                    else
                    {
                        foreach (var texture in TextureList)
                        {
                            foreach (var key in texture.KeyList)
                            {
                                key.X1 /= 2;
                                key.X2 /= 2;
                                key.Y1 /= 2;
                                key.Y2 /= 2;
                            }
                        }
                    }
                    //if (isPCFormat == false)
                    //{
                    //    Key.X1 /= 2;
                    //    Key.X2 /= 2;
                    //    Key.Y1 /= 2;
                    //    Key.Y2 /= 2;
                    //}

                    Notify("IsPCFormat");
                }
            }
        }

        public bool Close()
        {
            return true;
        }
    }
}