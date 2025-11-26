using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using UndergroundRaces;

namespace UndergroundRaces
{
    public class EscenaMenuAjustes : IEscena
    {
        private Texture2D _fondoAtlas;
        private List<Rectangle> _framesAjustes = new();
        private int _frameActual = 9; // 0–8 = hover botones, 9 = normal
        private int _atlasCols = 2;
        private int _atlasRows = 5;
        private int[] _hoverToFrame = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 1, 0 };
        private int _lastHovered = -1;

        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private Texture2D _debugPixel;
        private MouseState _mouse;
        private MouseState _prevMouse;

        private Rectangle _botonAtras;
        private Rectangle _sfxMinus, _sfxPlus, _musicMinus, _musicPlus, _masterMinus, _masterPlus, _brightMinus, _brightPlus;
        private Rectangle _sfxBar, _musicBar, _masterBar, _brightBar;

        public Action OnVolverClick;

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            _fondoAtlas = _content.Load<Texture2D>("images/menu-ajustes-plantillas-underground-races-2025");

            int frameW = _fondoAtlas.Width / _atlasCols;
            int frameH = _fondoAtlas.Height / _atlasRows;
            _framesAjustes.Clear();
            for (int r = 0; r < _atlasRows; r++)
            {
                for (int c = 0; c < _atlasCols; c++)
                {
                    _framesAjustes.Add(new Rectangle(c * frameW, r * frameH, frameW, frameH));
                }
            }

            _botonAtras = new Rectangle(20, 20, 60, 60);

            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });

            int minusW = 36;
            int plusW = 36;

            _sfxMinus    = new Rectangle(266 - minusW / 2, 141 - 14, minusW, 28);
            _sfxPlus     = new Rectangle(642 - plusW / 2, 141 - 14, plusW, 28);
            _musicMinus  = new Rectangle(389 - minusW / 2, 267 - 14, minusW, 28);
            _musicPlus   = new Rectangle(770 - plusW / 2, 261 - 14, plusW, 28);
            _masterMinus = new Rectangle(438 - minusW / 2, 390 - 14, minusW, 28);
            _masterPlus  = new Rectangle(825 - plusW / 2, 395 - 14, plusW, 28);
            _brightMinus = new Rectangle(378 - minusW / 2, 514 - 14, minusW, 28);
            _brightPlus  = new Rectangle(763 - plusW / 2, 514 - 14, plusW, 28);

            int barraW = 160;
            int barraH = 28;
            int barraYoffset = 0;

            _sfxBar    = CentrarBarra(_sfxMinus, _sfxPlus, barraW, barraH, barraYoffset);
            _musicBar  = CentrarBarra(_musicMinus, _musicPlus, barraW, barraH, barraYoffset);
            _masterBar = CentrarBarra(_masterMinus, _masterPlus, barraW, barraH, barraYoffset);
            _brightBar = CentrarBarra(_brightMinus, _brightPlus, barraW, barraH, barraYoffset);
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();

            int hovered = 9; // default: normal

            if (_sfxMinus.Contains(_mouse.Position)) hovered = 0;
            else if (_sfxPlus.Contains(_mouse.Position)) hovered = 1;
            else if (_musicMinus.Contains(_mouse.Position)) hovered = 2;
            else if (_musicPlus.Contains(_mouse.Position)) hovered = 3;
            else if (_masterMinus.Contains(_mouse.Position)) hovered = 4;
            else if (_masterPlus.Contains(_mouse.Position)) hovered = 5;
            else if (_brightMinus.Contains(_mouse.Position)) hovered = 6;
            else if (_brightPlus.Contains(_mouse.Position)) hovered = 7;
            else if (_botonAtras.Contains(_mouse.Position)) hovered = 8;

            if (hovered != _lastHovered)
            {
                _frameActual = hovered;
                _lastHovered = hovered;
            }

            if (_mouse.LeftButton == ButtonState.Pressed &&
                _prevMouse.LeftButton == ButtonState.Released)
            {
                if (_sfxMinus.Contains(_mouse.Position)) Settings.DecreaseSfx();
                else if (_sfxPlus.Contains(_mouse.Position)) Settings.IncreaseSfx();

                if (_musicMinus.Contains(_mouse.Position)) Settings.DecreaseMusic();
                else if (_musicPlus.Contains(_mouse.Position)) Settings.IncreaseMusic();

                if (_masterMinus.Contains(_mouse.Position)) Settings.DecreaseMaster();
                else if (_masterPlus.Contains(_mouse.Position)) Settings.IncreaseMaster();

                if (_brightMinus.Contains(_mouse.Position)) Settings.DecreaseBrightness();
                else if (_brightPlus.Contains(_mouse.Position)) Settings.IncreaseBrightness();

                if (_botonAtras.Contains(_mouse.Position)) OnVolverClick?.Invoke();

                MediaPlayer.Volume = Settings.MusicVolume * Settings.MasterVolume;
            }

            _prevMouse = _mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            if (_framesAjustes.Count == (_atlasCols * _atlasRows) && _fondoAtlas != null)
            {
                int frameIndex = _hoverToFrame[MathHelper.Clamp(_frameActual, 0, _hoverToFrame.Length - 1)];
                Rectangle src = _framesAjustes[MathHelper.Clamp(frameIndex, 0, _framesAjustes.Count - 1)];
                spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, 1024, 576), src, Color.White);
            }

            float overlayAlpha = 1f - Settings.Brightness;
            if (_debugPixel != null && overlayAlpha > 0f)
            {
                spriteBatch.Draw(_debugPixel, new Rectangle(0, 0, 1024, 576), Color.Black * overlayAlpha);
            }

            DrawBar(spriteBatch, _sfxBar, Settings.SfxVolume, Color.Orange);
            DrawBar(spriteBatch, _musicBar, Settings.MusicVolume, Color.Yellow);
            DrawBar(spriteBatch, _masterBar, Settings.MasterVolume, Color.Red);
            DrawBar(spriteBatch, _brightBar, Settings.Brightness, Color.White);

            spriteBatch.End();
        }

        private void DrawBar(SpriteBatch spriteBatch, Rectangle area, float value, Color color)
        {
            int segments = 10;
            int spacing = 4;
            int segmentWidth = (area.Width - (segments - 1) * spacing) / segments;
            int segmentHeight = area.Height;

            for (int i = 0; i < segments; i++)
            {
                int x = area.X + i * (segmentWidth + spacing);
                Rectangle rect = new Rectangle(x, area.Y, segmentWidth, segmentHeight);
                Color fill = (i < (int)(value * segments)) ? color : Color.Gray;
                spriteBatch.Draw(_debugPixel, rect, fill);
            }
        }

        private Rectangle CentrarBarra(Rectangle minus, Rectangle plus, int width, int height, int yOffset)
        {
            int centerX = (minus.X + plus.X + plus.Width) / 2;
            int x = centerX - width / 2;
            int y = minus.Y + yOffset;
            return new Rectangle(x, y, width, height);
        }
    }
}