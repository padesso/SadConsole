﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole.Effects;
using SadConsole.Input;
using SadConsole.Surfaces;

namespace SadConsole
{
    [System.Diagnostics.DebuggerDisplay("Console")]
    public class Console: ScreenObject, IScreenObjectViewPort
    {
        /// <summary>
        /// The private virtual cursor reference.
        /// </summary>
        public Cursor Cursor { get; private set; }

        /// <summary>
        /// Toggles the VirtualCursor as visible\hidden when the console if focused\unfocused.
        /// </summary>
        public bool AutoCursorOnFocus { get; set; }

        /// <summary>
        /// Sets the viewport without triggering <see cref="SetRenderCells"/>.
        /// </summary>
        protected Rectangle ViewPortRectangle;

        /// <summary>
        /// Sets the area of the text surface that should be rendered.
        /// </summary>
        public Rectangle ViewPort
        {
            get => ViewPortRectangle;
            set
            {
                ViewPortRectangle = value;

                if (ViewPortRectangle == default)
                    ViewPortRectangle = new Rectangle(0, 0, Width, Height);
                if (ViewPortRectangle.Width > Width)
                    ViewPortRectangle.Width = Width;
                if (ViewPortRectangle.Height > Height)
                    ViewPortRectangle.Height = Height;

                if (ViewPortRectangle.X < 0)
                    ViewPortRectangle.X = 0;
                if (ViewPortRectangle.Y < 0)
                    ViewPortRectangle.Y = 0;

                if (ViewPortRectangle.X + ViewPortRectangle.Width > Width)
                    ViewPortRectangle.X = Width - ViewPortRectangle.Width;
                if (ViewPortRectangle.Y + ViewPortRectangle.Height > Height)
                    ViewPortRectangle.Y = Height - ViewPortRectangle.Height;

                IsDirty = true;
                SetRenderCells();
                OnViewPortChanged();
            }
        }


        /// <inheritdoc />
        public Console(int width, int height) : this(width, height, Global.FontDefault, new Rectangle(0, 0, width, height), null)
        {

        }
        
        /// <inheritdoc />
        public Console(int width, int height, Font font) : this(width, height, font, new Rectangle(0, 0, width, height), null)
        {

        }

        /// <summary>
        /// Creates a new text surface with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the surface.</param>
        /// <param name="height">The height of the surface.</param>
        /// <param name="font">The font used with rendering.</param>
        /// <param name="viewPort">Initial value for the viewport if this console will scroll.</param>
        public Console(int width, int height, Font font, Rectangle viewPort) : this(width, height, font, viewPort, null)
        {

        }

        /// <summary>
        /// Creates a new text surface with the specified width, height, and initial set of cell data.
        /// </summary>
        /// <param name="width">The width of the surface.</param>
        /// <param name="height">The height of the surface.</param>
        /// <param name="font">The font used with rendering.</param>
        /// <param name="viewPort">Initial value for the viewport if this console will scroll.</param>
        /// <param name="initialCells">Seeds the cells with existing values. Array size must match <paramref name="width"/> * <paramref name="height"/>.</param>
        public Console(int width, int height, Font font, Rectangle viewPort, Cell[] initialCells): base (width, height, font, initialCells)
        {
            Cursor = new Cursor(this);
            Renderer.BeforeRenderTintCallback = OnBeforeRender;

            ViewPortRectangle = viewPort;

            if (ViewPortRectangle == default)
                ViewPortRectangle = new Rectangle(0, 0, Width, Height);
            if (ViewPortRectangle.Width > Width)
                ViewPortRectangle.Width = Width;
            if (ViewPortRectangle.Height > Height)
                ViewPortRectangle.Height = Height;

            if (ViewPortRectangle.X < 0)
                ViewPortRectangle.X = 0;
            if (ViewPortRectangle.Y < 0)
                ViewPortRectangle.Y = 0;

            if (ViewPortRectangle.X + ViewPortRectangle.Width > Width)
                ViewPortRectangle.X = Width - ViewPortRectangle.Width;
            if (ViewPortRectangle.Y + ViewPortRectangle.Height > Height)
                ViewPortRectangle.Y = Height - ViewPortRectangle.Height;


            if (RenderCells.Length != ViewPortRectangle.Width * ViewPortRectangle.Height)
            {
                RenderRects = new Rectangle[ViewPortRectangle.Width * ViewPortRectangle.Height];
                RenderCells = new Cell[ViewPortRectangle.Width * ViewPortRectangle.Height];
            }

            var index = 0;

            for (var y = 0; y < ViewPortRectangle.Height; y++)
            {
                for (var x = 0; x < ViewPortRectangle.Width; x++)
                {
                    RenderRects[index] = _font.GetRenderRect(x, y);
                    RenderCells[index] = Cells[(y + ViewPortRectangle.Top) * Width + (x + ViewPortRectangle.Left)];
                    index++;
                }
            }

            AbsoluteArea = new Rectangle(0, 0, ViewPortRectangle.Width * _font.Size.X, ViewPortRectangle.Height * _font.Size.Y);

            if (LastRenderResult.Bounds.Width != AbsoluteArea.Width || LastRenderResult.Bounds.Height != AbsoluteArea.Height)
            {
                LastRenderResult.Dispose();
                LastRenderResult = new RenderTarget2D(Global.GraphicsDevice, AbsoluteArea.Width, AbsoluteArea.Height, false, Global.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            }
        }
        
        /// <inheritdoc />
        public override void Update(TimeSpan delta)
        {
            if (IsPaused) return;

            if (Cursor.IsVisible)
                Cursor.Update(delta);

            base.Update(delta);
        }

        /// <summary>
        /// Calculates which cells to draw based on <see cref="ViewPort"/>.
        /// </summary>
        public override void SetRenderCells()
        {
            if (RenderCells.Length != ViewPortRectangle.Width * ViewPortRectangle.Height)
            {
                RenderRects = new Rectangle[ViewPortRectangle.Width * ViewPortRectangle.Height];
                RenderCells = new Cell[ViewPortRectangle.Width * ViewPortRectangle.Height];
            }

            var index = 0;

            for (var y = 0; y < ViewPortRectangle.Height; y++)
            {
                for (var x = 0; x < ViewPortRectangle.Width; x++)
                {
                    RenderRects[index] = _font.GetRenderRect(x, y);
                    RenderCells[index] = Cells[(y + ViewPortRectangle.Top) * Width + (x + ViewPortRectangle.Left)];
                    index++;
                }
            }

            AbsoluteArea = new Rectangle(0, 0, ViewPortRectangle.Width * _font.Size.X, ViewPortRectangle.Height * _font.Size.Y);

            if (LastRenderResult.Bounds.Width != AbsoluteArea.Width || LastRenderResult.Bounds.Height != AbsoluteArea.Height)
            {
                LastRenderResult.Dispose();
                LastRenderResult = new RenderTarget2D(Global.GraphicsDevice, AbsoluteArea.Width, AbsoluteArea.Height, false, Global.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            }
        }

        /// <summary>
        /// Resizes the surface to the specified width and height.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="clear">When true, resets every cell to the <see cref="DefaultForeground"/>, <see cref="DefaultBackground"/> and glyph 0.</param>
        public void Resize(int width, int height, bool clear, Rectangle viewPort)
        {
            var newCells = new Cell[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (IsValidCell(x, y))
                    {
                        newCells[new Point(x, y).ToIndex(width)] = this[x, y];

                        if (clear)
                        {
                            newCells[new Point(x, y).ToIndex(width)].Foreground = DefaultForeground;
                            newCells[new Point(x, y).ToIndex(width)].Background = DefaultBackground;
                            newCells[new Point(x, y).ToIndex(width)].Glyph = 0;
                            newCells[new Point(x, y).ToIndex(width)].ClearState();
                        }
                    }
                    else
                        newCells[new Point(x, y).ToIndex(width)] = new Cell(DefaultForeground, DefaultBackground, 0);
                }
            }

            Cells = newCells;
            Width = width;
            Height = height;
            Effects = new EffectsManager(this);
            ViewPortRectangle = viewPort;
            OnCellsReset();
        }

        /// <summary>
        /// Called by the engine to process the keyboard. If assigned, invokes the <see cref="ScreenObject.KeyboardHandler"/>; otherwise invokes the <see cref="Cursor.ProcessKeyboard(Keyboard)"/> method.
        /// </summary>
        /// <param name="info">Keyboard information.</param>
        /// <returns>True when the keyboard had data and this console did something with it.</returns>
        public override bool ProcessKeyboard(Input.Keyboard info)
        {
            if (!UseKeyboard) return false;

            return KeyboardHandler?.Invoke(this, info) 
                   ?? Cursor.ProcessKeyboard(info);
        }

        /// <inheritdoc />
        protected override void OnFocusLost()
        {
            if (AutoCursorOnFocus)
                Cursor.IsVisible = false;
        }

        /// <inheritdoc />
        protected override void OnFocused()
        {
            if (AutoCursorOnFocus)
                Cursor.IsVisible = true;
        }

        /// <summary>
        /// Called when the <see cref="ViewPort"/> property changes.
        /// </summary>
        protected virtual void OnViewPortChanged() { }

        /// <summary>
        /// Called when the renderer renders the text view.
        /// </summary>
        /// <param name="batch">The batch used in rendering.</param>
        protected virtual void OnBeforeRender(SpriteBatch batch)
        {
            if (Cursor.IsVisible && ViewPort.Contains(Cursor.Position))
                Cursor.Render(batch, Font, Font.GetRenderRect(Cursor.Position.X - ViewPort.Location.X, Cursor.Position.Y - ViewPort.Location.Y));
        }
        /// <summary>
        /// Creates a new console from an existing surface.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns>A new console.</returns>
        public static Console FromSurface(ScreenObject surface)
        {
            return new Console(surface.Width, surface.Height, surface.Font, new Rectangle(0, 0, surface.Width, surface.Height), surface.Cells);
        }

        /// <summary>
        /// Creates a new console from an existing surface.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns>A new console.</returns>
        public static Console FromSurface(CellSurface surface, Font font)
        {
            return new Console(surface.Width, surface.Height, font, new Rectangle(0, 0, surface.Width, surface.Height), surface.Cells);
        }
    }
}
