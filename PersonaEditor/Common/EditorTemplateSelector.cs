using PersonaEditor.ViewModels;
using PersonaEditor.ViewModels.Editors;
using PersonaEditor.Views;
using PersonaEditor.Views.Editors;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PersonaEditor.Common
{
    internal sealed class EditorTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Type controlType;
            switch (item)
            {
                case ImagePreviewVM:
                    controlType = typeof(ImagePreview);
                    break;
                case SPRTextureAtlasEditor:
                case SPDTextureAtlasEditor:
                    controlType = typeof(TextureAtlasEditor);
                    break;
                case BMDEditorVM:
                    controlType = typeof(BMDEditor);
                    break;
                case PTPEditorVM:
                    controlType = typeof(PTPEditor);
                    break;
                case HEXEditorVM:
                    controlType = typeof(HEXEditor);
                    break;
                case FTDEditorVM:
                    controlType = typeof(FTDEditor);
                    break;
                case FNTEditorVM:
                case FNT0EditorVM:
                    controlType = typeof(FNTEditor);
                    break;
                default:
                    return null;
            }

            var factory = new FrameworkElementFactory(controlType);
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.AppendChild(factory);

            var template = new DataTemplate();
            template.DataType = item.GetType();
            template.VisualTree = borderFactory;
            return template;
        }
    }
}