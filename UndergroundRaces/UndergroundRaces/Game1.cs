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

            // Menú principal
            _menu = new EscenaMenu();
            _menu.LoadContent(this);
            _menu.OnJugarClick = CambiarAEscenaSeleccionar;
            _menu.OnAjustesClick = CambiarAEscenaAjustes;
            _menu.OnSalirClick = Exit;

            // Menú de pausa durante el juego
            _menuJuego = new EscenaMenuJuego();
            _menuJuego.LoadContent(this);
            _menuJuego.OnReanudarClick = ReanudarJuego;
            _menuJuego.OnAjustesClick = CambiarAEscenaAjustes;
            _menuJuego.OnVolverMenuClick = () =>
            {
                _juego.DetenerSonido();
                ReiniciarJuego();
                _escenaActual = _menu;
            };

            // Menú de ajustes (Usa OnVolverClick como tenías en tu EscenaMenuAjustes.cs)
            _menuAjustes = new EscenaMenuAjustes();
            _menuAjustes.LoadContent(this);
            _menuAjustes.OnVolverClick = VolverDesdeAjustes;

            // Inicializar juego y menú de selección
            ReiniciarJuego();

            _escenaActual = _menu;
        }

        private void ReiniciarJuego()
        {
            _juego = new EscenaJuego();
            _juego.LoadContent(this);
            _juego.OnPausaSolicitada = CambiarAMenuJuego;

            _juego.OnFinCarrera = (mensaje) =>
            {
                _juego.DetenerSonido();
                var gameOver = new EscenaMenuConMensaje(mensaje);
                gameOver.LoadContent(this);
                gameOver.OnVolverClick = () =>
                {
                    ReiniciarJuego();      // nueva partida limpia
                    _escenaActual = _menu;
                };
                _escenaActual = gameOver;
            };

            _menuSeleccionar = new EscenaMenuSeleccionar();
            _menuSeleccionar.LoadContent(this);
            _menuSeleccionar.OnSeleccionVehiculo = (veh) =>
            {
                _juego.SetVehiculo(veh);
                _escenaActual = _juego;
            };
        }

        private void ReanudarJuego()
        {
            _juego.ReanudarSonido();
            _escenaActual = _juego;
        }

        private void CambiarAEscenaSeleccionar()
        {
            _escenaActual = _menuSeleccionar;
        }

        private void CambiarAMenuJuego()
        {
            _juego.PausarSonido();
            _escenaActual = _menuJuego;
        }

        private void CambiarAMenuPrincipal()
        {
            _escenaActual = _menu;
        }

        private void CambiarAEscenaAjustes()
        {
            _historialEscenas.Push(_escenaActual);
            _escenaActual = _menuAjustes;
        }

        private void VolverDesdeAjustes()
        {
            if (_historialEscenas.Count > 0)
                _escenaActual = _historialEscenas.Pop();
            else
                _escenaActual = _menu;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_escenaActual != null)
                _escenaActual.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (_escenaActual != null)
                _escenaActual.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}