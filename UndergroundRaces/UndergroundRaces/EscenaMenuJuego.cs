using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;
using UndergroundRaces;

namespace UndergroundRaces
{
    public class EscenaMenuJuego : IEscena
    {
        // Atlas y frames (misma lógica que EscenaMenu)
        private Texture2D _fondoAtlas;
        private List<Rectangle> _framesMenuJuego = new();
        private int _frameActual = 3; // 0: hover Reanudar, 1: hover Ajustes, 2: hover Volver, 3: normal
        private int _atlasCols = 2;
        private int _atlasRows = 2;
        private int[] _hoverToFrame = new int[] { 1, 3, 2, 0 }; // mapeo directo
        private int _lastHovered = -1;

        // Botones
        private Rectangle _botonReanudar;
        private Rectangle _botonAjustes;
        private Rectangle _botonVolverMenu;
        private MouseState _mouse;

        // Eventos
        public Action OnReanudarClick;
        public Action OnAjustesClick;
        public Action OnVolverMenuClick;

        // Infra
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            // Cargar atlas de menú de pausa (4 variantes en 2x2)
            _fondoAtlas = _content.Load<Texture2D>("images/menu-juego-plantillas-underground-races-2025");
            int frameW = _fondoAtlas.Width / _atlasCols;
            int frameH = _fondoAtlas.Height / _atlasRows;

            _framesMenuJuego.Clear();
            for (int r = 0; r < _atlasRows; r++)
            {
                for (int c = 0; c < _atlasCols; c++)
                {
                    _framesMenuJuego.Add(new Rectangle(c * frameW, r * frameH, frameW, frameH));
                }
            }

            // Coordenadas de botones (ajustá si necesitás fine-tune con el fondo)
            _botonReanudar   = new Rectangle(200, 220, 200, 60);
            _botonAjustes    = new Rectangle(220, 360, 200, 60);
            _botonVolverMenu = new Rectangle(670, 290, 200, 60);
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();

            // Hover detection (misma idea que en EscenaMenu)
            // 0 = Reanudar, 1 = Ajustes, 2 = Volver, 3 = normal
            int hovered = 3;
            if (_botonReanudar.Contains(_mouse.Position)) hovered = 0;
            else if (_botonAjustes.Contains(_mouse.Position)) hovered = 1;
            else if (_botonVolverMenu.Contains(_mouse.Position)) hovered = 2;

            if (hovered != _lastHovered)
            {
                _frameActual = hovered;
                _lastHovered = hovered;
            }

            // Click actions (edge o hold, según tu preferencia)
            if (_mouse.LeftButton == ButtonState.Pressed)
            {
                if (_botonReanudar.Contains(_mouse.Position)) OnReanudarClick?.Invoke();
                else if (_botonAjustes.Contains(_mouse.Position)) OnAjustesClick?.Invoke();
                else if (_botonVolverMenu.Contains(_mouse.Position)) OnVolverMenuClick?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            if (_framesMenuJuego.Count == (_atlasCols * _atlasRows) && _fondoAtlas != null)
            {
                int mapped = MathHelper.Clamp(_frameActual, 0, _hoverToFrame.Length - 1);
                int frameIndex = _hoverToFrame[mapped];
                Rectangle src = _framesMenuJuego[MathHelper.Clamp(frameIndex, 0, _framesMenuJuego.Count - 1)];
                spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, 1024, 576), src, Color.White);
            }
            else if (_fondoAtlas != null)
            {
                // Fallback: dibujar textura completa si algo falla
                spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, 1024, 576), Color.White);
            }

            spriteBatch.End();
        }
    }
}