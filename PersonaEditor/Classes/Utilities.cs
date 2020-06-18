using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditor.Classes
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

        public static Drawing DrawBitmap(int width, int height, Color[] colors)
        {
            Pen pen = new Pen(new SolidColorBrush(Colors.Gray), 0.1);

            DrawingGroup drawingGroup = new DrawingGroup();
            var context = drawingGroup.Append();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Rect temp = new Rect(new Point(x, y), new Point(x + 1, y + 1));
                    context.DrawRectangle(
                        new SolidColorBrush(colors[y * width + x]),
                        pen,
                        temp);
                }

            context.Close();

            drawingGroup.Freeze();

            return drawingGroup;
        }
    }
}