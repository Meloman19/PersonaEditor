using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace PersonaEditor.Classes
{
    // Thanks Mr.PoorEnglish
    // site: https://www.codeproject.com/Articles/852756/Wpf-Tabcontrol-saving-its-visual-TabItem-States-Al

    [ContentProperty("DataTemplate")]
    public class CachedContentPresenter : Decorator
    {
        //ConditionalWeakTable is a special Dictionary, which doesn't prohibit garbage-collection of its keys. Instead it automatically removes garbaged Elements
        private ConditionalWeakTable<object, ContentPresenter> _PresenterCache = new ConditionalWeakTable<object, ContentPresenter>();

        public CachedContentPresenter()
        {
            DataContextChanged += (s, e) => UpdatePresentation(e.NewValue);
        }

        private void UpdatePresentation(object item)
        {
            ContentPresenter ctp = null;
            if (item != null)
            {
                if (!_PresenterCache.TryGetValue(item, out ctp))
                {
                    ctp = new ContentPresenter { ContentTemplate = DataTemplate };
                    ctp.SetBinding(ContentPresenter.ContentProperty, new Binding());
                    _PresenterCache.Add(item, ctp);
                }
            }
            this.Child = ctp;
        }

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(CachedContentPresenter), new FrameworkPropertyMetadata(DataTemplate_Changed));
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)this.GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        private static void DataTemplate_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            
            //clear cache before update Presentation
            var ccp = (CachedContentPresenter)sender;
            ccp._PresenterCache = new ConditionalWeakTable<object, ContentPresenter>();
            ccp.UpdatePresentation(ccp.DataContext);
        }
    }
}