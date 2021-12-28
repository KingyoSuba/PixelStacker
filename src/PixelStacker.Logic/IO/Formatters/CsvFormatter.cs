using PixelStacker.Logic.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixelStacker.Logic.IO.Formatters
{
    public class CsvFormatter : IExportFormatter
    {
        public Task<byte[]> ExportAsync(PixelStackerProjectData canvas, CancellationToken? worker = null)
        {
            Dictionary<Material, int> materialCounts = new Dictionary<Material, int>();
            bool isv = canvas.IsSideView;
            var data = canvas.CanvasData;
            var Palette = canvas.MaterialPalette;
            bool isMultilayer = data.Any(x => Palette[x.PaletteID].IsMultiLayer);
            int addTop = 0; if (isMultilayer) addTop = 1; if (isv) addTop = 2;
            int addBottom = 1;

            StringBuilder sb = new StringBuilder();
            // sb.AppendLine("Width,Height");
            // sb.AppendLine(data.Width + "," + data.Height);

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();


            for (int x = 0; x < data.Width; x++)
            {
                var list1 = new List<string>();
                var list2 = new List<string>();
                for (int y = 0; y < data.Height; y++)
                {
                    var mc = data[x, y];
                    list1.Add(mc.Top.GetBlockNameAndData(isv));
                    list2.Add(mc.Bottom.GetBlockNameAndData(isv));
                    materialCounts[mc.Top] = materialCounts.TryGetValue(mc.Top, out int existingT) ? existingT + addTop : addTop;
                    materialCounts[mc.Bottom] = materialCounts.TryGetValue(mc.Bottom, out int existingB) ? existingB + addBottom : addBottom;
                }
                sb1.AppendLine(String.Join(";", list1));
                sb2.AppendLine(String.Join(";", list2));
            }
            sb.AppendLine("\"sep=;\"");
            sb.AppendLine("Top");
            sb.Append(sb1);

            sb.AppendLine("Bottom");
            sb.Append(sb2);

            sb.AppendLine("Count");
            sb.AppendLine("\"Material\";\"Block Count\";\"Full Stacks needed\"");
            sb.AppendLine("\"Total\"," + materialCounts.Values.Sum());

            foreach (var kvp in materialCounts.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"\"{kvp.Key.GetBlockNameAndData(isv).Replace("\"", "\"\"")}\";{kvp.Value};{kvp.Value / 64} stacks and {kvp.Value % 64} remaining blocks");
            }

            byte[] result = Encoding.UTF8.GetBytes(sb.ToString());
            return Task.FromResult(result);
        }

        public async Task ExportAsync(string filePath, PixelStackerProjectData canvas, CancellationToken? worker = null)
            => File.WriteAllBytes(filePath, await this.ExportAsync(canvas, worker));
    }
}
