﻿using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SadConsole.Editor.Tools
{
    internal class Pencil : ITool
    {
        public Model.GlyphItem Glyph = new Model.GlyphItem();

        public string Name => "Pencil";

        public ToolBrush Brush { get; private set; } = new ToolBrush(1, 1);

        private System.Windows.Forms.Control toolsPanel;
        private Panels.GlyphEditPanel glyphPanel;

        public Pencil()
        {
            toolsPanel = new System.Windows.Forms.Panel();
            glyphPanel = new Panels.GlyphEditPanel();
            toolsPanel.AddArrangeControls(glyphPanel);
        }

        public System.Windows.Forms.Control UI => toolsPanel;

        public void Refresh()
        {   
            glyphPanel.DataObject = Glyph;
        }


        public void OnUpdate(MouseConsoleState mouse)
        {
            if (Global.MouseState.IsOnScreen && mouse.IsOnConsole)
            {
                Brush.Animation.Font = mouse.Console.TextSurface.Font;
                if (Global.MouseState.LeftButtonDown)
                {
                    mouse.Cell.Foreground = Glyph.Foreground;
                    mouse.Cell.Background = Glyph.Background;
                    mouse.Cell.Glyph = Glyph.Glyph;
                    mouse.Console.TextSurface.IsDirty = true;
                }
            }

            // Sync brush
            Brush.Animation.CurrentFrame[0].Foreground = Glyph.Foreground;
            Brush.Animation.CurrentFrame[0].Background = Glyph.Background;
            Brush.Animation.CurrentFrame[0].Glyph = Glyph.Glyph;
            Brush.Position = (Global.MouseState.ScreenPosition + Global.RenderRect.Location).PixelLocationToConsole(Brush.Animation.Font);
            Brush.Animation.IsDirty = true;
        }
    }
}
