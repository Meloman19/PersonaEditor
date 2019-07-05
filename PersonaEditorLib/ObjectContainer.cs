using System;
using System.Collections.Generic;

namespace PersonaEditorLib
{
    public class ObjectContainer
    {
        private object @object;
        private string name;

        public object Object
        {
            get => @object;
            set => @object = value ?? throw new ArgumentNullException(nameof(Object));
        }

        public string Name
        {
            get => name;
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Null or WhiteSpace", nameof(Name));
                name = value;
            }
        }

        public object Tag { get; set; }

        public ObjectContainer(string name, object obj)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Null or WhiteSpace", nameof(name));

            Name = name;
            Object = obj ?? throw new ArgumentNullException(nameof(obj));
        }

        public IEnumerable<ObjectContainer> GetAllObjectFiles(FormatEnum fileType)
        {
            if (Object is IGameFile pFile)
            {
                if (pFile.Type == fileType)
                    yield return this;
                foreach (var sub in pFile.SubFiles)
                    foreach (var obj in sub.GetAllObjectFiles(fileType))
                        yield return obj;
            }
        }
    }
}