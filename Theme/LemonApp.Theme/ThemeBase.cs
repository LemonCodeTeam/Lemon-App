using System.Windows.Controls;
using System.Windows.Media;

namespace LemonApp.Theme
{
    public abstract class ThemeBase : UserControl
    {
        public string ThemeName { get; set; }
        public static string NameSpace;
        public Color ThemeColor { get; set; }
        public string FontColor { get; set; }
        public abstract ThemeBase GetPage();
        public abstract void Draw();
    }
}
