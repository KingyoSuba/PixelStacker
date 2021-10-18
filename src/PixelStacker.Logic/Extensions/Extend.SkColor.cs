using PixelStacker.IO.Config;
using PixelStacker.Logic.Model;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelStacker.Extensions
{
    public static partial class Extend
    {
        /// <summary>
        /// Overlay the TOP color ontop of the BOTTOM color
        /// </summary>
        /// <param name="RGBA2_Bottom"></param>
        /// <param name="RGBA1_Top"></param>
        /// <returns></returns>
        public static ColorData OverlayColor(this ColorData RGBA2_Bottom, ColorData RGBA1_Top)
        {
            double alpha = Convert.ToDouble(RGBA1_Top.A) / 255;
            int R = (int)((RGBA1_Top.R * alpha) + (RGBA2_Bottom.R * (1.0 - alpha)));
            int G = (int)((RGBA1_Top.G * alpha) + (RGBA2_Bottom.G * (1.0 - alpha)));
            int B = (int)((RGBA1_Top.B * alpha) + (RGBA2_Bottom.B * (1.0 - alpha)));
            return ColorData.FromArgb(255, R, G, B);
        }

        /// <summary>
        /// Overlay the TOP color ontop of the BOTTOM color
        /// </summary>
        /// <param name="RGBA2_Bottom"></param>
        /// <param name="RGBA1_Top"></param>
        /// <returns></returns>
        public static SKColor OverlayColor(this SKColor RGBA2_Bottom, SKColor RGBA1_Top)
        {
            double alpha = Convert.ToDouble(RGBA1_Top.Alpha) / 255;
            int R = (int)((RGBA1_Top.Red * alpha) + (RGBA2_Bottom.Red * (1.0 - alpha)));
            int G = (int)((RGBA1_Top.Green * alpha) + (RGBA2_Bottom.Green * (1.0 - alpha)));
            int B = (int)((RGBA1_Top.Blue * alpha) + (RGBA2_Bottom.Blue * (1.0 - alpha)));
            return new SKColor((byte)R, (byte)G, (byte)B, 255);
        }

        public static float GetDegreeDistance(float alpha, float beta)
        {
            float phi = Math.Abs(beta - alpha) % 360;       // This is either the distance or 360 - distance
            float distance = phi > 180 ? 360 - phi : phi;
            return distance;
        }

        public static double GetDegreeDistance(double alpha, double beta)
        {
            double phi = Math.Abs(beta - alpha) % 360;       // This is either the distance or 360 - distance
            double distance = phi > 180 ? 360 - phi : phi;
            return distance;
        }

        /// <summary>
        /// Use for SUPER accurate color distance checks. Very slow, but also very accurate.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static long GetAverageColorDistance(this ColorData target, SKBitmap src)
        {
            long r = 0;
            long total = src.Width * src.Height;

            src.ToViewStream(null, (int x, int y, ColorData c) =>
            {
                int dist = target.GetColorDistance(c);
                r += dist;
            });

            r /= total;
            return (int)r;
        }

        /// <summary>
        /// Custom color matching algorithm
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetColorDistance(this ColorData c, ColorData toMatch)
        {
            int dR = (c.R - toMatch.R);
            int dG = (c.G - toMatch.G);
            int dB = (c.B - toMatch.B);
            int dHue = (int)GetDegreeDistance(c.Hue, toMatch.Hue);

            int diff = (
                (dR * dR)
                + (dG * dG)
                + (dB * dB)
                + (int)(Math.Sqrt(dHue * dHue * dHue))
                );

            return diff;
        }

        /// <summary>
        /// Use for SUPER accurate color distance checks. Very slow, but also very accurate.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static int GetAverageColorDistance(this ColorData target, List<Tuple<ColorData, int>> src)
        {
            long r = 0;
            long t = 0;

            foreach (var c in src)
            {
                int dist = target.GetColorDistance(c.Item1);
                r += dist * c.Item2;
                t += c.Item2;
            }

            r /= t;
            return (int)r;
        }

        public static ColorData Normalize(this ColorData c, int fragmentSize)
        {
            int F = fragmentSize;
            if (F < 2)
            {
                return c;
            }

            byte R = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.R) / F, 0) * F);
            byte G = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.G) / F, 0) * F);
            byte B = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.B) / F, 0) * F);

            return new ColorData(R, G, B, c.A);
        }

        public static SKColor Normalize(this SKColor c, int fragmentSize)
        => NormalizeActual(c, fragmentSize);

        [Obsolete("Stop using this one.", false)]
        public static SKColor Normalize(this SKColor c)
        => NormalizeActual(c, null);

        /// <summary>
        /// Does not normalize alpha channels
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static SKColor NormalizeActual(this SKColor c, int? fragmentSize = null)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            int F = fragmentSize ?? Options.Get.Preprocessor.RgbBucketSize;
#pragma warning restore CS0618 // Type or member is obsolete
            if (F < 2)
            {
                return c;
            }

            byte R = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.Red) / F, 0) * F);
            byte G = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.Green) / F, 0) * F);
            byte B = (byte)Math.Min(255, Math.Round(Convert.ToDecimal(c.Blue) / F, 0) * F);

            return new SKColor(R, G, B, c.Alpha);
        }


    }
}
