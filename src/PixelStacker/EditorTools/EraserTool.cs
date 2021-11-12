﻿using PixelStacker.Logic.IO.Config;
using PixelStacker.Logic.Model;
using PixelStacker.UI.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace PixelStacker.EditorTools
{
    public class EraserTool : AbstractCanvasEditorTool
    {
        private MaterialCombination Air { get; }
        private MaterialPalette Palette => this.CanvasEditor.Canvas.MaterialPalette ?? MaterialPalette.FromResx();

        public EraserTool(CanvasEditor editor) : base(editor)
        {
            var palette = editor.Canvas.MaterialPalette ?? MaterialPalette.FromResx();
            this.Air = palette[Constants.MaterialCombinationIDForAir];
        }

        public override void OnClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Point loc = CanvasEditor.GetPointOnImage(e.Location, this.CanvasEditor.PanZoomSettings, EstimateProp.Floor);
            var cd = this.CanvasEditor.Canvas.CanvasData[loc.X, loc.Y];
            var painter = this.CanvasEditor.Painter;
            var buffer = painter.HistoryBuffer;
            buffer.AppendChange(Palette[cd], Palette[this.Air], new PxPoint(loc.X, loc.Y));
        }

        private bool IsDragging = false;

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            IsDragging = true;

            Point loc = CanvasEditor.GetPointOnImage(e.Location, this.CanvasEditor.PanZoomSettings, EstimateProp.Floor);
            var cd = this.CanvasEditor.Canvas.CanvasData[loc.X, loc.Y];
            var painter = this.CanvasEditor.Painter;
            var buffer = painter.HistoryBuffer;
            buffer.AppendChange(Palette[cd], Palette[this.Air], new PxPoint(loc.X, loc.Y));
        }
        public override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            IsDragging = false;

            var record = this.CanvasEditor.Painter.HistoryBuffer.ToHistoryRecord(true);
            this.CanvasEditor.Painter.History.AddChange(record);
            this.CanvasEditor.RepaintRequested = true;
            // TODO: Send it to the history buffer

        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (!IsDragging) return;

            Point loc = CanvasEditor.GetPointOnImage(e.Location, this.CanvasEditor.PanZoomSettings, EstimateProp.Floor);
            var cd = this.CanvasEditor.Canvas.CanvasData[loc.X, loc.Y];
            var painter = this.CanvasEditor.Painter;
            var buffer = painter.HistoryBuffer;
            buffer.AppendChange(Palette[cd], Palette[this.Air], new PxPoint(loc.X, loc.Y));
        }
    }
}