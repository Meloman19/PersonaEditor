using System.Collections.Generic;

namespace PersonaEditorLib
{
    public class ObjectContainer
    {
        public object Object { get; set; } = null;
        public object Tag { get; set; } = null;
        public string Name { get; set; } = "";

        public ObjectContainer(string name, object obj)
        {
            Name = name;
            Object = obj;
        }

        public ObjectContainer[] GetAllObjectFiles(FormatEnum fileType)
        {
            List<ObjectContainer> objectFiles = new List<ObjectContainer>();

            if (Object is IGameFile pFile)
            {
                if (pFile.Type == fileType)
                    objectFiles.Add(this);
                foreach (var sub in pFile.SubFiles)
                    objectFiles.AddRange(sub.GetAllObjectFiles(fileType));
            }

            return objectFiles.ToArray();
        }
    }
}