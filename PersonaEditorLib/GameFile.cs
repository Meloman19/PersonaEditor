using System;
using System.Collections.Generic;

namespace PersonaEditorLib
{
    public class GameFile
    {
        //public static GameFile GetEmpty() => new GameFile();

        #region Field

        private string name;
        private IGameData gameData;

        #endregion

        #region Properties

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Null or WhiteSpace", nameof(Name));
                name = value;
            }
        }

        public IGameData GameData
        {
            get => gameData;
            set => gameData = value ?? throw new ArgumentNullException(nameof(GameData));
        }

        public object Tag { get; set; }

        #endregion

        #region Constructors

        private GameFile()
        {
            name = "";
            gameData = null;
        }

        private GameFile(string name) : this()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Null or WhiteSpace", nameof(name));

            Name = name;
        }

        public GameFile(string name, IGameData gameFile) : this(name)
        {
            GameData = gameFile ?? throw new ArgumentNullException(nameof(gameFile));
        }

        #endregion

        public IEnumerable<GameFile> GetAllObjectFiles(FormatEnum fileType)
        {
            if (GameData.Type == fileType)
                yield return this;
            foreach (var sub in GameData.SubFiles)
                foreach (var gameFile in sub.GetAllObjectFiles(fileType))
                    yield return gameFile;
        }
    }
}