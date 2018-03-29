using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditorGUI.Classes
{
    public static class Utilities
    {
        public static string GetString(this Collection<ResourceDictionary> resourceDictionaries, string key)
        {
            string returned = "";

            if (Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Contains(key)) is var a)
                returned = a[key] as string;

            return returned;
        }
    }
}