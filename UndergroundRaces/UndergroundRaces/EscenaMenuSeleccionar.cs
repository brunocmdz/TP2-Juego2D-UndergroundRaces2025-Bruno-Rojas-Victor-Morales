using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;

namespace UndergroundRaces
{
    public class EscenaMenuSeleccionar : IEscena
    {
        private Texture2D _fondo;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private MouseState _mouse;

        // Areas clicables para los dos autos (coinciden con las cajas en la imagen)
        private Rectangle _areaCorsa = new Rectangle(60, 160, 360, 300);
        private Rectangle _areaClio = new Rectangle(600, 160, 360, 300);

        public Action<EscenaJuego.VehicleType> OnSeleccionVehiculo;

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;
            _fondo = _content.Load<Texture2D>("images/menu-principal-seleccionar");
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();

            if (_mouse.LeftButton == ButtonState.Pressed)
            {
                if (_areaCorsa.Contains(_mouse.Position))
                {
                    OnSeleccionVehiculo?.Invoke(EscenaJuego.VehicleType.Corsa);
                }
                else if (_areaClio.Contains(_mouse.Position))
                {
                    OnSeleccionVehiculo?.Invoke(EscenaJuego.VehicleType.Clio);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_fondo, new Rectangle(0, 0, 1024, 576), Color.White);
            spriteBatch.End();
        }
    }
}
