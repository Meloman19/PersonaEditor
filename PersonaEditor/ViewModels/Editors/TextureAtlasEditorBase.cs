using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PersonaEditor.Common;
using PersonaEditor.Common.Settings;
using PersonaEditor.Views.Tools;

namespace PersonaEditor.ViewModels.Editors
{
    public abstract class TextureAtlasEditorBase<T, R> : BindingObject, IEditor
        where T : TextureAtlasBase<R>
        where R : TextureObjectBase
    {
        private readonly SettingsProvider _settingsProvider;

        private T _selectedTextureAtlas;

        public TextureAtlasEditorBase()
        {
            _settingsProvider = Static.SettingsProvider;
            SelectColorCommand = new RelayCommand(SelectColor);
        }

        public ObservableCollection<T> TextureAtlasList { get; } = new ObservableCollection<T>();

        public T SelectedTextureAtlas
        {
            get => _selectedTextureAtlas;
            set => SetProperty(ref _selectedTextureAtlas, value);
        }

        public int ObjectBorderThickness
        {
            get => _settingsProvider.AppSettings.SPDObjectBorderThickness;
            set
            {
                if (_settingsProvider.AppSettings.SPDObjectBorderThickness != value)
                {
                    _settingsProvider.AppSettings.SPDObjectBorderThickness = value;
                    _ = _settingsProvider.SaveAsync();
                    Notify(nameof(ObjectBorderThickness));
                }
            }
        }

        public Color ObjectBorderColor
        {
            get => _settingsProvider.AppSettings.SPDObjectBorderColor;
            set
            {
                if (_settingsProvider.AppSettings.SPDObjectBorderColor != value)
                {
                    _settingsProvider.AppSettings.SPDObjectBorderColor = value;
                    _ = _settingsProvider.SaveAsync();
                    Notify(nameof(ObjectBorderColor));
                }
            }
        }

        public Color ObjectSelectionColor
        {
            get => _settingsProvider.AppSettings.SPDObjectSelectionColor;
            set
            {
                if (_settingsProvider.AppSettings.SPDObjectSelectionColor != value)
                {
                    _settingsProvider.AppSettings.SPDObjectSelectionColor = value;
                    _ = _settingsProvider.SaveAsync();
                    Notify(nameof(ObjectSelectionColor));
                }
            }
        }

        public Color EditorBackground
        {
            get => _settingsProvider.AppSettings.SPDEditorBackgroundColor;
            set
            {
                if (_settingsProvider.AppSettings.SPDEditorBackgroundColor != value)
                {
                    _settingsProvider.AppSettings.SPDEditorBackgroundColor = value;
                    _ = _settingsProvider.SaveAsync();
                    Notify(nameof(EditorBackground));
                }
            }
        }

        public ICommand SelectColorCommand { get; }

        private void SelectColor(object arg)
        {
            if (arg is string s)
            {
                Color selectColor;

                switch (s)
                {
                    case "Background":
                        selectColor = EditorBackground;
                        break;
                    case "Border":
                        selectColor = ObjectBorderColor;
                        break;
                    case "Selected":
                        selectColor = ObjectSelectionColor;
                        break;
                    default:
                        return;
                }

                var tool = new ColorPickerTool()
                {
                    SelectedColor = selectColor
                };
                if (tool.ShowDialog() != true)
                    return;

                selectColor = tool.SelectedColor;

                switch (s)
                {
                    case "Background":
                        EditorBackground = selectColor;
                        break;
                    case "Border":
                        ObjectBorderColor = selectColor;
                        break;
                    case "Selected":
                        ObjectSelectionColor = selectColor;
                        break;
                    default:
                        return;
                }
            }
        }

        public bool Close()
        {
            if (TextureAtlasList.Any(x => x.HasAnyChanges()))
            {
                var result = MessageBox.Show("Save changes?", "Saving", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var item in TextureAtlasList)
                        item.SaveChanges();
                }
                else if (result == MessageBoxResult.No)
                    return true;
                else
                    return false;
            }

            return true;
        }
    }
}