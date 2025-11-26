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
        private Texture2D _fondoAjustes;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private Rectangle _botonAtras;
        private MouseState _mouse;
        private Texture2D _debugPixel;
        private MouseState _prevMouse;

        // UI rects
        private Rectangle _sfxMinus, _sfxPlus, _musicMinus, _musicPlus, _masterMinus, _masterPlus, _brightMinus, _brightPlus;
        private Rectangle _sfxBar, _musicBar, _masterBar, _brightBar;

        public Action OnVolverClick;


        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            _fondoAjustes = _content.Load<Texture2D>("images/menu-ajustes-underground-races-2025");
            // fondo único cargado arriba

            _botonAtras = new Rectangle(20, 20, 60, 60); // ajustá tamaño si querés más grande

            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });

            // Layout values (we only need button sizes here)
            int minusW = 36;
            int plusW = 36;

            // The background image itself is the selectable surface. Assign clickable areas
            // for - and + using coordinates captured f rom your clicks (do not draw extra UI).
            // Use sizes similar to previous buttons (minusW x 28 / plusW x 28).

            // Coordinates provided by user clicks (mapped to rows):
            // SFX: minus=(266,141)  plus=(642,141)
            // MUSIC: minus=(389,267) plus=(770,261)
            // VOLUMEN: minus=(438,390) plus=(499,395)
            // BRILLO: minus=(378,514) plus=(763,514)
            _sfxMinus = new Rectangle(266 - minusW/2, 141 - 14, minusW, 28);
            _sfxPlus  = new Rectangle(642 - plusW/2, 141 - 14, plusW, 28);
            _musicMinus = new Rectangle(389 - minusW/2, 267 - 14, minusW, 28);
            _musicPlus  = new Rectangle(770 - plusW/2, 261 - 14, plusW, 28);
            _masterMinus = new Rectangle(438 - minusW/2, 390 - 14, minusW, 28);
            _masterPlus  = new Rectangle(825 - plusW/2, 395 - 14, plusW, 28);
            _brightMinus = new Rectangle(378 - minusW/2, 514 - 14, minusW, 28);
            _brightPlus  = new Rectangle(763 - plusW/2, 514 - 14, plusW, 28);

            // Keep bar rects empty (we won't draw them) but set a reasonable hit area near center
            _sfxBar = new Rectangle(_sfxMinus.Right + 8, _sfxMinus.Y, 120, 28);
            _musicBar = new Rectangle(_musicMinus.Right + 8, _musicMinus.Y, 120, 28);
            _masterBar = new Rectangle(_masterMinus.Right + 8, _masterMinus.Y, 120, 28);
            _brightBar = new Rectangle(_brightMinus.Right + 8, _brightMinus.Y, 120, 28);

        }

        public void Update(GameTime gameTime)
        { 
            _mouse = Mouse.GetState();

            // Volver
            if (_botonAtras.Contains(_mouse.Position) && _mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
            {
                OnVolverClick?.Invoke();
            }

            // Detect clicks on +/- (only on press edge)
            if (_mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
            {
                // click position logging removed per user request

                // SFX
                if (_sfxMinus.Contains(_mouse.Position)) { Settings.DecreaseSfx(); }
                else if (_sfxPlus.Contains(_mouse.Position)) { Settings.IncreaseSfx(); }

                // Music
                if (_musicMinus.Contains(_mouse.Position)) { Settings.DecreaseMusic(); MediaPlayer.Volume = Settings.MusicVolume * Settings.MasterVolume; }
                else if (_musicPlus.Contains(_mouse.Position)) { Settings.IncreaseMusic(); MediaPlayer.Volume = Settings.MusicVolume * Settings.MasterVolume; }

                // Master
                if (_masterMinus.Contains(_mouse.Position)) { Settings.DecreaseMaster(); MediaPlayer.Volume = Settings.MusicVolume * Settings.MasterVolume; }
                else if (_masterPlus.Contains(_mouse.Position)) { Settings.IncreaseMaster(); MediaPlayer.Volume = Settings.MusicVolume * Settings.MasterVolume; }

                // Brightness
                if (_brightMinus.Contains(_mouse.Position)) { Settings.DecreaseBrightness(); }
                else if (_brightPlus.Contains(_mouse.Position)) { Settings.IncreaseBrightness(); }
            }

            _prevMouse = _mouse;
        }

        // click logging removed

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            // Draw only the background; the background image itself is the selectable surface.
            spriteBatch.Draw(_fondoAjustes, new Rectangle(0, 0, 1024, 576), Color.White);

            // Apply overlay for brightness (still keep this behavior)
            float overlayAlpha = 1f - Settings.Brightness;
            if (_debugPixel != null && overlayAlpha > 0f)
            {
                spriteBatch.Draw(_debugPixel, new Rectangle(0, 0, 1024, 576), Color.Black * overlayAlpha);
            }
            // (temporary pixel markers removed)

            spriteBatch.End();
        }
    }
}
