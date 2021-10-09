﻿using KdTree;
using PixelStacker.Extensions;
using PixelStacker.IO.Config;
using PixelStacker.Logic.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelStacker.Logic.Collections.ColorMapper
{
    public class KdTreeMapper : IColorMapper
    {
        public double AccuracyRating => 99.127;
        public double SpeedRating => 232.1;

        private Dictionary<Color, MaterialCombination> Cache { get; set; } = new Dictionary<Color, MaterialCombination>();

        private bool IsSideView;
        private MaterialPalette Palette;
        private KdTree<float, MaterialCombination> KdTree;

        public void SetSeedData(List<MaterialCombination> combos, MaterialPalette palette, bool isSideView)
        {
            this.IsSideView = isSideView;
            this.Palette = palette;
            this.KdTree = new KdTree<float, MaterialCombination>(3, new KdTree.Math.FloatMath());

            foreach (var cb in combos)
            {
                var c = cb.GetAverageColor(isSideView);
                float[] metrics = new float[] { c.R, c.G, c.B };
                KdTree.Add(metrics, cb);
            }
        }

        public MaterialCombination FindBestMatch(Color c)
        {
            if (Cache.TryGetValue(c, out MaterialCombination mc))
            {
                return mc;
            }

            if (c.A < 32) return Palette[Constants.MaterialCombinationIDForAir];
            var closest = KdTree.GetNearestNeighbours(new float[] { c.R, c.G, c.B }, 10);
            var found = closest.MinBy(x => c.GetAverageColorDistance(x.Value.GetColorsInImage(this.IsSideView)));
            Cache[c] = found.Value;
            return found.Value;
        }

        /// <summary>
        /// Super SLOW! Avoid if you can help it. Not at all optimized.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="maxMatches"></param>
        /// <returns></returns>
        public List<MaterialCombination> FindBestMatches(Color c, int maxMatches)
        {
            if (c.A < 32) return new List<MaterialCombination>() { Palette[Constants.MaterialCombinationIDForAir] };
            var closest = KdTree.GetNearestNeighbours(new float[] { c.R, c.G, c.B }, 10);
            var found = closest.OrderBy(x => c.GetAverageColorDistance(x.Value.GetColorsInImage(this.IsSideView)))
                .Take(maxMatches).Select(x => x.Value).ToList();

            return found;
        }
    }
}