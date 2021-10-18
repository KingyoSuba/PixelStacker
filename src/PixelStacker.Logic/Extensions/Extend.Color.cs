using PixelStacker.IO.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace PixelStacker.Extensions
{
    // Sat: 0...1
    // Hue: 0...360
    // Brightness: 0...1
    public static class ExtendColor
    {
        /// <summary>
        /// Overlay the TOP color ontop of the BOTTOM color
        /// </summary>
        /// <param name="RGBA2_Bottom"></param>
        /// <param name="RGBA1_Top"></param>
        /// <returns></returns>
        public static Color OverlayColor(this Color RGBA2_Bottom, Color RGBA1_Top)
        {
            double alpha = Convert.ToDouble(RGBA1_Top.A) / 255;
            int R = (int)((RGBA1_Top.R * alpha) + (RGBA2_Bottom.R * (1.0 - alpha)));
            int G = (int)((RGBA1_Top.G * alpha) + (RGBA2_Bottom.G * (1.0 - alpha)));
            int B = (int)((RGBA1_Top.B * alpha) + (RGBA2_Bottom.B * (1.0 - alpha)));
            return Color.FromArgb(255, R, G, B);
        }

        public static Color AverageColors(this IEnumerable<Color> colors)
        {
            long r = 0;
            long g = 0;
            long b = 0;
            long a = 0;
            long total = colors.Count();
            foreach (var c in colors)
            {
                r += c.R;
                g += c.G;
                b += c.B;
                a += c.A;
            }

            r /= total;
            g /= total;
            b /= total;
            a /= total;

            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }




        public static IEnumerable<Color> OrderByColor(this IEnumerable<Color> source)
        {
            return source.OrderByColor(c => c);
        }

        public static IEnumerable<TSource> OrderByColor<TSource>(this IEnumerable<TSource> source, Func<TSource, Color> colorSelector)
        {
            var grayscale = source.Where(x => colorSelector(x).GetSaturation() <= 0.20
            || colorSelector(x).GetBrightness() <= 0.15
            || colorSelector(x).GetBrightness() >= 0.85)
            .OrderByDescending(x => colorSelector(x).GetBrightness());
            const int numHueFragments = 18;

            var colorsInOrder = grayscale.ToList();

            //// Sat: 0...1
            //// Hue: 0...360
            //// Brightness: 0...1
            bool isAscendingBrightness = false;
            source.Except(grayscale)
                .GroupBy(x => (int)Math.Round(colorSelector(x).GetHue()) / numHueFragments)
                .OrderBy(x => x.Key)
                .ToList().ForEach(grouping =>
                {
                    isAscendingBrightness = !isAscendingBrightness;

                    if (isAscendingBrightness)
                    {
                        colorsInOrder.AddRange(grouping.OrderBy(g => colorSelector(g).GetBrightness()));
                    }
                    else
                    {
                        colorsInOrder.AddRange(grouping.OrderByDescending(g => colorSelector(g).GetBrightness()));
                    }
                });

            return colorsInOrder;
        }
    }
}
