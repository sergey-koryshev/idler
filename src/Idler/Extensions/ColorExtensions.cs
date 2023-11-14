using System.Windows.Media;

namespace Idler.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToMediaColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
