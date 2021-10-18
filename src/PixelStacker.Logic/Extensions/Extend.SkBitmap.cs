using PixelStacker.Logic.Model;
using PixelStacker.Logic.Utilities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixelStacker.Extensions
{
    public partial class Extend
    {
        [Obsolete("Make sure to convert to RGBA 32bit for this")]
        public static SKBitmap AsBitmap(this byte[] arr)
        {
            var bm = SKBitmap.Decode(arr);


            return bm;
        }


        /// <summary>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="normalize">If colors should be put into their buckets of 5 or 15 or or whatever.</param>
        /// <returns></returns>
        public static List<ColorData> GetColorsInImage(this SKBitmap src, int rgbBucketSize = 1)
        {
            List<ColorData> cs = new List<ColorData>();
            src.ToViewStream(null, (int x, int y, ColorData c) =>
            {
                cs.Add(rgbBucketSize == 1 ? c : c.Normalize(rgbBucketSize));
            });

            return cs;
        }

        /// <summary>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="normalize">If colors should be put into their buckets of 5 or 15 or or whatever.</param>
        /// <returns></returns>
        public static ColorData GetAverageColor(this SKBitmap src, int rgbBucketSize = 1)
        {
            long r = 0;
            long g = 0;
            long b = 0;
            long a = 0;
            long total = src.Width * src.Height;

            src.ToViewStream(null, (int x, int y, ColorData c) =>
            {
                r += c.R;
                g += c.G;
                b += c.B;
                a += c.A;
            });

            r /= total;
            g /= total;
            b /= total;
            a /= total;

            if (a > 128)
            {
                a = 255;
            }

            ColorData rt = ColorData.FromArgb((int)a, (int)r, (int)g, (int)b);
            return rgbBucketSize == 1 ? rt : rt.Normalize(rgbBucketSize);
        }



        /// <summary>
        /// Don't forget to dispose any unused images properly after calling this.
        /// Also CLONES the image instance to return a new image instance.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static SKBitmap To32bppBitmap(this SKBitmap src, int width, int height)
        {
            SKBitmap output = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            src.ScalePixels(output, SKFilterQuality.High);
            return output;
        }

        /// <summary>
        /// Don't forget to dispose any unused images properly after calling this.
        /// Also CLONES the image instance to return a new image instance.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static SKBitmap To32bppBitmap(this SKBitmap src)
        {
            SKBitmap cloned = src.Copy(SKColorType.Rgba8888);
            return cloned;
        }


        /// <summary>
        /// Returns a NEW image.
        /// Image MUST be 32bppARGB
        /// (int x, int y, Color cOrig, cDest) => { return newColorDest; }
        /// </summary>
        /// <param name="origImage"></param>
        /// <returns></returns>
        public static SKBitmap ToMergeStream(this SKBitmap origImage, SKBitmap dstImage, CancellationToken? worker, Func<int, int, SKColor, SKColor, SKColor> callback)
        {
            if (origImage.ColorType != dstImage.ColorType)
            {
                throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            }
            //if ((origImage.ColorType & SKColorType.Format32bppArgb) != SKColorType.Format32bppArgb)
            //{
            //    throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            //}

            //if ((dstImage.ColorType & SKColorType.Format32bppArgb) != SKColorType.Format32bppArgb)
            //{
            //    throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            //}
            //if (dstImage.ColorType != SKColorType.Format32bppArgb)
            //{
            //    throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            //}

            var pxSRC = origImage.Pixels;
            var pxDST = dstImage.Pixels;
            SKBitmap bm = new SKBitmap(dstImage.Width, dstImage.Height, dstImage.ColorType, dstImage.AlphaType);

            for (int y = 0; y < bm.Height; y++)
            {
                for (int x = 0; x < bm.Width; x++)
                {
                    var cc = callback.Invoke(x, y, pxSRC[x + y * origImage.Width], pxDST[x + y * origImage.Width]);
                    bm.SetPixel(x, y, cc);
                }

                worker?.SafeThrowIfCancellationRequested();
                if (worker != null)
                    ProgressX.Report(100 * y / origImage.Height);
            }

            return bm;
        }


        public static void ToViewStream(this SKBitmap origImage, CancellationToken? worker, Action<int, int, SKColor> callback)
        {
            if (origImage.BytesPerPixel != 4)
            {
                // throw hissy fit
                throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            }

            ////Initialize an array for all the image data
            byte[] imageBytes = origImage.Bytes;

            //Find pixelsize
            int bytesPerPixel = origImage.BytesPerPixel;
            int x = 0; int y = 0;
            var pixelData = new byte[bytesPerPixel];
            for (int i = 0; i < imageBytes.Length; i += bytesPerPixel)
            {
                //Copy the bits into a local array
                Array.Copy(imageBytes, i, pixelData, 0, bytesPerPixel);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pixelData);
                }

                //Get the color of a pixel
                // On a little-endian machine, the byte order is bb gg rr aa
                var color = new SKColor(pixelData[2], pixelData[1], pixelData[0], pixelData[3]);
                callback(x, y, color);

                x++;
                if (x > origImage.Width - 1)
                {
                    x = 0;
                    y++;
                    worker?.SafeThrowIfCancellationRequested();
                    if (worker != null)
                        ProgressX.Report(100 * y / origImage.Height);
                }
            }

            return;
        }

        public static void ToViewStream(this SKBitmap origImage, CancellationToken? worker, Action<int, int, ColorData> callback)
        {
            if (origImage.BytesPerPixel != 4)
            {
                // throw hissy fit
                throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb.");
            }

            ////Initialize an array for all the image data
            byte[] imageBytes = origImage.Bytes;

            //Find pixelsize
            int bytesPerPixel = origImage.BytesPerPixel;
            int x = 0; int y = 0;
            var pixelData = new byte[bytesPerPixel];
            for (int i = 0; i < imageBytes.Length; i += bytesPerPixel)
            {
                //Copy the bits into a local array
                Array.Copy(imageBytes, i, pixelData, 0, bytesPerPixel);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pixelData);
                }

                //Get the color of a pixel
                // On a little-endian machine, the byte order is bb gg rr aa
                ColorData color = ColorData.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]);
                callback(x, y, color);

                x++;
                if (x > origImage.Width - 1)
                {
                    x = 0;
                    y++;
                    worker?.SafeThrowIfCancellationRequested();
                    if (worker != null)
                        ProgressX.Report(100 * y / origImage.Height);
                }
            }

            return;
        }

        /// <summary>
        /// Image SHOULD be 32bppARGB
        /// </summary>
        /// <param name="origImage"></param>
        /// <returns></returns>
        public static void ToViewStreamParallel(this SKBitmap origImage, CancellationToken? worker, Action<int, int, SKColor> callback)
        {
            //if (origImage.ColorType != SKColorType.Format32bppArgb)
            //{
            //    if (!CanReadPixel(origImage.ColorType) && (origImage.ColorType & SKColorType.Indexed) != SKColorType.Indexed)
            //    {
            //        throw new ArgumentException("ColorType MUST be SKColorType.Format32bppArgb, or else format must be indexed.");
            //    }

            //    for (int x = 0; x < origImage.Width; x++)
            //    {
            //        for (int y = 0; y < origImage.Height; y++)
            //        {
            //            Color c = origImage.GetPixel(x, y);
            //            callback(x, y, c);
            //        }
            //    }

            //    return;
            //}

            ////Get the SKBitmap data
            //var srcData = origImage.LockBits(
            //    new Rectangle(0, 0, origImage.Width, origImage.Height),
            //    ImageLockMode.ReadOnly,
            //    origImage.ColorType
            //);

            ////Initialize an array for all the image data
            //byte[] imageBytes = new byte[srcData.Stride * origImage.Height];

            ////Copy the SKBitmap data to the local array
            //System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, imageBytes, 0, imageBytes.Length);

            ////Unlock the SKBitmap
            //origImage.UnlockBits(srcData);

            //int numYProcessed = 0;
            //int bitsPerPixel = Image.GetColorTypeSize(origImage.ColorType); // bits per pixel
            //int bytesPerPixel = bitsPerPixel / 8;
            //int heightInPixels = origImage.Height;
            //int widthInPixels = origImage.Width;
            //int widthInBytes = Math.Abs(srcData.Stride);

            //Parallel.For(0, heightInPixels, y =>
            //{
            //    int bYOffset = (int)(y * widthInBytes);
            //    for (int x = 0; x < widthInPixels; x++)
            //    {
            //        int bXOffset = bYOffset + (x * bytesPerPixel);

            //        Color color = BitConverter.IsLittleEndian
            //        ? color = Color.FromArgb(imageBytes[bXOffset + 3], imageBytes[bXOffset + 2], imageBytes[bXOffset + 1], imageBytes[bXOffset])
            //        : color = Color.FromArgb(imageBytes[bXOffset], imageBytes[bXOffset + 1], imageBytes[bXOffset + 2], imageBytes[bXOffset + 3]);

            //        callback(x, y, color);
            //    }

            //    worker?.SafeThrowIfCancellationRequested();
            //    Interlocked.Increment(ref numYProcessed);
            //    if (worker != null)
            //    {
            //        ProgressX.Report((int)(100 * ((float)numYProcessed / heightInPixels)));
            //    }
            //});
        }
    }
}
