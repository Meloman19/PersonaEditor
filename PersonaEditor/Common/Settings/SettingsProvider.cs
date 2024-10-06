using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonaEditor.Common.Settings
{
    public sealed class SettingsProvider
    {
        private readonly object _saveLocker = new object();
        private readonly string _filePath;
        private readonly JsonSerializerOptions _serializerOptions;

        public SettingsProvider(string filePath)
        {
            _filePath = filePath;
            _serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new GridLengthJsonConverter(),
                    new ColorJsonConverter(),
                }
            };
        }

        public async Task LoadAsync()
        {
            if (File.Exists(_filePath))
            {
                var model = await Task.Run(() =>
                {
                    var text = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<SettingsStoreModel>(text, _serializerOptions);
                });
                AppSettings = model.AppSettings;
                UISettings = model.UISettings;
            }
            else
            {
                AppSettings = new AppSettings();
                UISettings = new UISettings();
            }
        }

        public void Load()
        {
            if (File.Exists(_filePath))
            {
                var text = File.ReadAllText(_filePath);
                var model = JsonSerializer.Deserialize<SettingsStoreModel>(text, _serializerOptions);
                AppSettings = model.AppSettings;
                UISettings = model.UISettings;
            }
            else
            {
                AppSettings = new AppSettings();
                UISettings = new UISettings();
            }
        }

        public async Task SaveAsync()
        {
            var model = new SettingsStoreModel();
            model.AppSettings = AppSettings.Clone();
            model.UISettings = UISettings.Clone();

            await Task.Run(() =>
            {
                lock (_saveLocker)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                    var text = JsonSerializer.Serialize(model, _serializerOptions);
                    File.WriteAllText(_filePath, text);
                }
            });
        }

        public void Save()
        {
            var model = new SettingsStoreModel();
            model.AppSettings = AppSettings.Clone();
            model.UISettings = UISettings.Clone();

            lock (_saveLocker)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                var text = JsonSerializer.Serialize(model, _serializerOptions);
                File.WriteAllText(_filePath, text);
            }
        }

        public AppSettings AppSettings { get; set; } = new AppSettings();

        public UISettings UISettings { get; private set; } = new UISettings();
    }
}