﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelStacker.Logic.Model;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PixelStacker.IO.Formatters;
using PixelStacker.Extensions;
using PixelStacker.Resources;
using PixelStacker.Logic.Engine;
using PixelStacker.Logic.Engine.Quantizer.Enums;
using PixelStacker.Logic.Collections.ColorMapper;
using System.Linq;
using PixelStacker.IO.Config;
using System.Collections.Generic;

namespace PixelStacker.Tests
{
    [TestClass]
    public class ImportExportTests
    {
        private RenderedCanvas Canvas => Canvases["Heavy"].Value.Result;

        private Dictionary<string, AsyncLazy<RenderedCanvas>> Canvases = new Dictionary<string, AsyncLazy<RenderedCanvas>>();

        [TestInitialize]
        public async Task Setup()
        {
            MaterialPalette palette = MaterialPalette.FromResx();
            var mapper = new KdTreeMapper();
            var combos = palette.ToCombinationList().Where(x => x.Top.IsEnabled && x.Bottom.IsEnabled && x.IsMultiLayer).ToList();
            mapper.SetSeedData(combos, palette, false);

            var engine = new RenderCanvasEngine();

            this.Canvases["Fast"] = new AsyncLazy<RenderedCanvas>(async () => {
                var img = await engine.PreprocessImageAsync(null,
                    UIResources.pink_girl.To32bppBitmap(),
                    new CanvasPreprocessorSettings()
                    {
                        IsSideView = false,
                        RgbBucketSize = 15,
                        MaxHeight = 10,
                        QuantizerSettings = new QuantizerSettings()
                        {
                            Algorithm = QuantizerAlgorithm.WuColor,
                            MaxColorCount = 64,
                            IsEnabled = false,
                            DitherAlgorithm = "No dithering"
                        }
                    });


                return await engine.RenderCanvasAsync(null, ref img, mapper, palette);
            });


            this.Canvases["Quantizer"] = new AsyncLazy<RenderedCanvas>(async () => {
                var img = await engine.PreprocessImageAsync(null,
                    UIResources.pink_girl.To32bppBitmap(),
                    new CanvasPreprocessorSettings()
                    {
                        IsSideView = false,
                        RgbBucketSize = 1,
                        QuantizerSettings = new QuantizerSettings()
                        {
                            Algorithm = QuantizerAlgorithm.WuColor,
                            MaxColorCount = 32,
                            IsEnabled = true,
                            DitherAlgorithm = "No dithering"
                        }
                    });

                return await engine.RenderCanvasAsync(null, ref img, mapper, palette);
            });

            this.Canvases["Heavy"] = new AsyncLazy<RenderedCanvas>(async () => {
                var img = await engine.PreprocessImageAsync(null,
                    UIResources.yuuuuge.To32bppBitmap(),
                    new CanvasPreprocessorSettings()
                    {
                        IsSideView = false,
                        RgbBucketSize = 1,
                        MaxWidth = 1024,
                        QuantizerSettings = new QuantizerSettings()
                        {
                            Algorithm = QuantizerAlgorithm.WuColor,
                            MaxColorCount = 256,
                            IsEnabled = false,
                            DitherAlgorithm = "No dithering"
                        }
                    });

                return await engine.RenderCanvasAsync(null, ref img, mapper, palette);
            });

        }

        [TestMethod]
        [TestCategory("IO")]
        public async Task IE_PixelStackerProjectFormat()
        {
            var formatter = new PixelStackerProjectFormatter();
            await formatter.ExportAsync("io_test.zip", Canvas, null);
            var canv = await formatter.ImportAsync("io_test.zip", null);
            Assert.AreEqual(Canvas.WorldEditOrigin, canv.WorldEditOrigin);
            Assert.AreEqual(JsonConvert.SerializeObject(Canvas.MaterialPalette), JsonConvert.SerializeObject(canv.MaterialPalette));
            Assert.AreEqual(Canvas.PreprocessedImage.Height, canv.PreprocessedImage.Height);
        }

        [TestMethod]
        [TestCategory("IO")]
        public async Task IE_SvgFormat()
        {
            var formatter = new SvgFormatter();
            await formatter.ExportAsync("test.svg", Canvas, null);
        }

        [TestMethod]
        [TestCategory("IO")]
        public async Task IE_PngFormat()
        {
            var formatter = new PngFormatter();
            await formatter.ExportAsync("io_test.png", Canvas, null);
        }
    }
}