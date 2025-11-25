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
        private Texture2D _fondoMenuJuego;
        private Rectangle _botonReanudar;
        private Rectangle _botonAjustes;
        private Rectangle _botonVolverMenu;
        private MouseState _mouse;

        // Eventos públicos para que Game1 decida qué hacer
        public Action OnReanudarClick;
        public Action OnAjustesClick; 
        public Action OnVolverMenuClick;

        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            // Fondo del menú de pausa
            _fondoMenuJuego = _content.Load<Texture2D>("images/menu-juego-underground-races-2025");

            // Botones
            _botonReanudar   = new Rectangle(200, 220, 200, 60);   
            _botonAjustes    = new Rectangle(220, 360, 200, 60);     
            _botonVolverMenu = new Rectangle(670, 290, 200, 60);     
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();

            if (_botonReanudar.Contains(_mouse.Position) && _mouse.LeftButton == ButtonState.Pressed)
            {
                OnReanudarClick?.Invoke();
            }
            if (_botonAjustes.Contains(_mouse.Position) && _mouse.LeftButton == ButtonState.Pressed)
            {
                OnAjustesClick?.Invoke();
            }
            if (_botonVolverMenu.Contains(_mouse.Position) && _mouse.LeftButton == ButtonState.Pressed)
            {
                OnVolverMenuClick?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_fondoMenuJuego, new Rectangle(0, 0, 1024, 576), Color.White);
            spriteBatch.End();
        }
    }
}
