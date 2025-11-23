using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace UndergroundRaces
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private IEscena _escenaActual;
        private EscenaMenu _menu;
        private EscenaJuego _juego;
        private EscenaMenuSeleccionar _menuSeleccionar;
        private EscenaMenuJuego _menuJuego;
        private EscenaMenuAjustes _menuAjustes;

        private Stack<IEscena> _historialEscenas = new Stack<IEscena>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 576;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _menu = new EscenaMenu();
            _menu.LoadContent(this);
            _menu.OnJugarClick = CambiarAEscenaSeleccionar;
            _menu.OnAjustesClick = CambiarAEscenaAjustes;

            _menuSeleccionar = new EscenaMenuSeleccionar();
            _menuSeleccionar.LoadContent(this);

            _juego = new EscenaJuego();
            _juego.LoadContent(this);
            _juego.OnPausaSolicitada = CambiarAMenuJuego;

            // ahora que _juego existe, conectar la seleccion
            _menuSeleccionar.OnSeleccionVehiculo = (veh) => {
                _juego.SetVehiculo(veh);
                _escenaActual = _juego;
            };

            _menuJuego = new EscenaMenuJuego();
            _menuJuego.LoadContent(this);
            _menuJuego.OnReanudarClick = CambiarAEscenaJuego;
            _menuJuego.OnVolverMenuClick = CambiarAMenuPrincipal;
            _menuJuego.OnAjustesClick = CambiarAEscenaAjustes;

            _menuAjustes = new EscenaMenuAjustes();
            _menuAjustes.LoadContent(this);
            _menuAjustes.OnVolverClick = VolverDesdeAjustes;

            _escenaActual = _menu;
        }

        protected override void Update(GameTime gameTime)
        {
            _escenaActual.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _escenaActual.Draw(_spriteBatch);
            base.Draw(gameTime);
        }

        private void CambiarAEscenaJuego()
        {
            _escenaActual = _juego;
        }

        private void CambiarAEscenaSeleccionar()
        {
            _escenaActual = _menuSeleccionar;
        }

        private void CambiarAMenuJuego()
        {
            _escenaActual = _menuJuego;
        }

        private void CambiarAMenuPrincipal()
        {
            _escenaActual = _menu;
        }

        private void CambiarAEscenaAjustes()
        {
            _historialEscenas.Push(_escenaActual); // guarda escena actual
            _escenaActual = _menuAjustes;
        }

        private void VolverDesdeAjustes()
        {
            if (_historialEscenas.Count > 0)
                _escenaActual = _historialEscenas.Pop(); // vuelve a escena anterior
            else
                _escenaActual = _menu; // fallback
        }
    }
}
