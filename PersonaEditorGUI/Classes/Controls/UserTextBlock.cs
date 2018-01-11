using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PersonaEditorGUI.Classes.Controls
{
    class UserTextBlock : TextBlock, ISerializable
    {
        public UserTextBlock()
        {
            DataObject.AddCopyingHandler(this, Copy);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }

        private void Copy(object sender, DataObjectCopyingEventArgs e)
        {
        }
    }
}
