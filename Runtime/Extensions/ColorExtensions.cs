using UnityEngine;

namespace UrbanFox.GameObjectPainter
{
    public static class ColorExtensions
    {
        public static Color SetRed(this Color color, float r)
        {
            return new Color(r, color.g, color.b, color.a);
        }

        public static Color SetGreen(this Color color, float g)
        {
            return new Color(color.r, g, color.b, color.a);
        }

        public static Color SetBlue(this Color color, float b)
        {
            return new Color(color.r, color.g, b, color.a);
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color SetHue(this Color color, float hue)
        {
            Color.RGBToHSV(color, out _, out float s, out float v);
            return Color.HSVToRGB(hue, s, v);
        }

        public static Color SetSaturation(this Color color, float saturation)
        {
            Color.RGBToHSV(color, out float h, out _, out float v);
            return Color.HSVToRGB(h, saturation, v);
        }

        public static Color SetValue(this Color color, float value)
        {
            Color.RGBToHSV(color, out float h, out float s, out _);
            return Color.HSVToRGB(h, s, value);
        }
    }
}
