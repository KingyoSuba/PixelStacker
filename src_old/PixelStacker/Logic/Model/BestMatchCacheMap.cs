﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelStacker.Logic.Great
{
    /// <summary>
    /// [K, V]
    /// K = input color
    /// V = minecraft color
    /// </summary>
    public class BestMatchCacheMap : Dictionary<Color, Color>
    {
        private bool isSaved = true;
        private readonly object padlock = new { };

        public BestMatchCacheMap()
        {
        }

        public new Color this[Color key]
        {
            get => base[key];
            set
            {
                lock (padlock)
                    base[key] = value;

                isSaved = false;
            }
        }

        public new void Add(Color key, Color value)
        {
            this.isSaved = false;
            lock (padlock)
            {
                base[key] = value;
            }
        }

        public new bool Remove(Color key)
        {
            this.isSaved = false;
            lock (padlock)
                return base.Remove(key);
        }

        public new void Clear()
        {
            lock (padlock)
                base.Clear();
            Properties.Settings.Default.ColorMatchCache = "";
            Properties.Settings.Default.Save();
        }

        public BestMatchCacheMap Load()
        {
            string[] input = Properties.Settings.Default.ColorMatchCache.Split(new char[] { '\n' });

            lock (padlock)
            {

                foreach (string line in input)
                {
                    try
                    {
                        string[] parts = line.Split('\t');
                        Color key = Color.FromArgb(int.Parse(parts[0]));
                        Color val = Color.FromArgb(int.Parse(parts[1]));
                        this[key] = val;
                    }
                    catch
                    {
                    }
                }
            }

            return this;
        }

        public void Save()
        {
            if (!isSaved)
            {
                isSaved = true;
                var lines = this.ToList().Select(kvp => $"{kvp.Key.ToArgb()}\t{kvp.Value.ToArgb()}");
                Properties.Settings.Default.ColorMatchCache = string.Join("\n", lines);
                Properties.Settings.Default.Save();
            }
        }
    }
}
