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

            DrawingVisual drawingVisual = new DrawingVisual();
            var render = drawingVisual.RenderOpen();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Rect temp = new Rect(new Point(x, y), new Point(x + 1, y + 1));
                    render.DrawGeometry(
                        new SolidColorBrush(colors[y * width + x]),
                        pen,
                        new RectangleGeometry(temp));
                }

            render.Close();

            drawingVisual.Drawing.Freeze();

            return drawingVisual.Drawing;
        }
    }
}