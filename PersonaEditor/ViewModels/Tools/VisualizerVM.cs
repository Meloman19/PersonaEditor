using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using PersonaEditor.Common;
using PersonaEditor.Common.Visual;
using PersonaEditorLib;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Tools
{
    public sealed class VisualizerVM : BindingObject
    {
        private ImageSource _textVisualize = null;

        private PersonaEncoding _selectedEncoding;
        private PersonaFont _selectedFont;

        private TextVisual _textVisual;

        private string _inputOuputText = "";
        private string _inputOutputHex = "";
        private bool _twoBytesAscii = false;

        private int _selectedFontIndex = -1;

        public VisualizerVM()
        {
            _textVisual = new TextVisual();
            _textVisual.ImageChanged += TextVisual_ImageChanged;

            SelectedFontIndex = 0;
        }

        public ReadOnlyObservableCollection<string> AvailableFontList => Static.EncodingManager.EncodingList;

        public int SelectedFontIndex
        {
            get { return _selectedFontIndex; }
            set
            {
                if (SetProperty(ref _selectedFontIndex, value))
                {
                    _selectedEncoding = Static.EncodingManager.GetPersonaEncoding(_selectedFontIndex)?.Clone();
                    _selectedFont = Static.FontManager.GetPersonaFont(Static.EncodingManager.GetPersonaEncodingName(_selectedFontIndex));
                    _textVisual.UpdateFont(_selectedEncoding, _selectedFont);
                    UpdateText2Hex();
                }
            }
        }

        public ImageSource TextVisualize
        {
            get => _textVisualize;
            set => SetProperty(ref _textVisualize, value);
        }

        public bool TwoBytesASCII
        {
            get => _twoBytesAscii;
            set
            {
                if (SetProperty(ref _twoBytesAscii, value))
                {
                    if (_selectedEncoding != null)
                    {
                        _selectedEncoding.TwoByteASCII = value;
                    }
                    _twoBytesAscii = value;
                    UpdateText2Hex();
                }
            }
        }

        public string InputOutputText
        {
            get { return _inputOuputText; }
            set
            {
                if (SetProperty(ref _inputOuputText, value))
                {
                    UpdateText2Hex();
                    _textVisual.UpdateText(value);
                }
            }
        }

        public string InputOutputHex
        {
            get => _inputOutputHex;
            set
            {
                if (SetProperty(ref _inputOutputHex, value))
                {
                    // UpdateHex2Text();
                }
            }
        }

        private void UpdateText2Hex()
        {
            var temp = InputOutputText.GetTextBases(_selectedEncoding).GetByteArray();
            InputOutputHex = BitConverter.ToString(temp).Replace('-', ' ');
        }

        private string UpdateHex2Text()
        {
            return "";
        }

        private void TextVisual_ImageChanged()
        {
            TextVisualize = _textVisual.Image;
        }
    }
}