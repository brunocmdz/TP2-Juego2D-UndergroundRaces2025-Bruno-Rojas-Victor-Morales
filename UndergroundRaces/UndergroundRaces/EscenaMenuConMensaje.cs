using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using UndergroundRaces;

namespace UndergroundRaces
{
    public class EscenaMenuConMensaje : IEscena
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteFont _font;
        private string _mensaje;
        private ContentManager _content;

        // Botón para volver al menú principal
        private Rectangle _botonVolver;
        private Texture2D _debugPixel;
        private MouseState _mouse;

        public Action OnVolverClick;

        public EscenaMenuConMensaje(string mensaje)
        {
            _mensaje = mensaje;
        }

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            // Cargar fuente
            _font = _content.Load<SpriteFont>("font/afa");

            // Botón volver
            _botonVolver = new Rectangle(400, 400, 220, 80);

            // Pixel para debug/dibujo
            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();

            if (_mouse.LeftButton == ButtonState.Pressed)
            {
                if (_botonVolver.Contains(_mouse.Position))
                {
                    OnVolverClick?.Invoke();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Fondo negro
            spriteBatch.Draw(_debugPixel, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), Color.Black);

            // Mostrar mensaje en el centro
            if (_font != null)
            {
                // Normalizar texto para quitar acentos y caracteres no soportados
                string safeMensaje = new string(
                    _mensaje.Normalize(NormalizationForm.FormD)
                            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                            .ToArray()
                );

                Vector2 size = _font.MeasureString(safeMensaje);
                Vector2 pos = new Vector2((_graphicsDevice.Viewport.Width - size.X) / 2f, 200);
                spriteBatch.DrawString(_font, safeMensaje, pos, Color.White);
            }

            // Dibujar botón volver
            spriteBatch.Draw(_debugPixel, _botonVolver, Color.DarkRed);
            if (_font != null)
            {
                string texto = "Volver al menu"; // sin acento para evitar error
                Vector2 size = _font.MeasureString(texto);
                Vector2 pos = new Vector2(_botonVolver.X + (_botonVolver.Width - size.X) / 2f,
                                          _botonVolver.Y + (_botonVolver.Height - size.Y) / 2f);
                spriteBatch.DrawString(_font, texto, pos, Color.White);
            }

            spriteBatch.End();
        }
    }
}
