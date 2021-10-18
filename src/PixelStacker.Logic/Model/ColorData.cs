using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelStacker.Logic.Model
{
    public class ColorData : IEquatable<ColorData>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }
        public float Hue { get; }

        private float? hue;
        private float? sat;
        private float? bri;

        public float GetHue()
        {
            if (hue == null)
            {
                new SKColor(R, G, B, A).ToHsl(out float h, out float s, out float b);
                this.hue = h;
                this.sat = s;
                this.bri = b;
            }

            return hue.Value;
        }

        public float GetSaturation()
        {
            if (sat == null)
            {
                new SKColor(R, G, B, A).ToHsl(out float h, out float s, out float b);
                this.hue = h;
                this.sat = s;
                this.bri = b;
            }

            return sat.Value;
        }
        public float GetBrightness()
        {
            if (bri == null)
            {
                new SKColor(R, G, B, A).ToHsl(out float h, out float s, out float b);
                this.hue = h;
                this.sat = s;
                this.bri = b;
            }

            return bri.Value;
        }

        public ColorData(SKColor c)
        {
            this.R = c.Red;
            this.G = c.Green;
            this.B = c.Blue;
            this.A = c.Alpha;
        }

        public ColorData(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public static ColorData FromArgb(int a, int r, int g, int b)
        {
            return new ColorData((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static bool operator ==(ColorData x, ColorData y)
        {
            if ((x is not null) ^ (y is not null)) return false;
            if ((x is null) && (y is null)) return true;
            if (x.R != y.R) return false;
            if (x.G != y.G) return false;
            if (x.B != y.B) return false;
            if (x.A != y.A) return false;
            return true;
        }

        public static bool operator !=(ColorData left, ColorData right)
        {
            return !(left == right);
        }

        public bool Equals(ColorData cd)
        {
            if (cd.R != this.R) return false;
            if (cd.G != this.G) return false;
            if (cd.B != this.B) return false;
            if (cd.A != this.A) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ColorData cd) return false;
            if (cd.R != this.R) return false;
            if (cd.G != this.G) return false;
            if (cd.B != this.B) return false;
            if (cd.A != this.A) return false;
            return true;

        }

        /// <summary>
        ///  Kind of dubious. Shouldn't I implement this?
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return $"{R}.{G}.{B}.{A}".GetHashCode(); // base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
