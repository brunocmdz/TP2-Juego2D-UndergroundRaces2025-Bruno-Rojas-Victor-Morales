using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;
using UndergroundRaces;

namespace UndergroundRaces
{
    public class EscenaMenu : IEscena
    {
        private Texture2D _fondoAtlas;
        private List<Rectangle> _framesMenu = new();
        private int _frameMenuActual = 0; // 0: normal, 1: hover Jugar, 2: hover Ajustes, 3: hover Salir
        private int _atlasCols = 2;
        private int _atlasRows = 2;
        // Mapeo lógico (hover target) -> índice del frame en el atlas
        // Por defecto usamos correspondencia directa: {normal, jugar, ajustes, salir}
        private int[] _hoverToFrame = new int[] { 0, 1, 2, 3 };
        private int _lastHovered = -1;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private Texture2D _debugPixel;

        private Rectangle _botonJugar;
        private Rectangle _botonAjustes; 
        private Rectangle _botonSalir;
        private MouseState _mouse;
        public Action OnJugarClick;
        public Action OnAjustesClick; 
        public Action OnSalirClick;
        // Herramienta de asignacion interactiva de rectangulos de botones
        private int _assignTarget = 0; // 0 = none, 1 = Jugar, 2 = Ajustes, 3 = Salir
        private Point? _assignPointA = null;
        private KeyboardState _lastKeyboardState;

        public void LoadContent(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;
            // Botones: valores por defecto (ajustados para la plantilla)
            _botonJugar = new Rectangle(390, 200, 220, 100);
            _botonAjustes = new Rectangle(340, 330, 300, 100);
            _botonSalir = new Rectangle(390, 450, 220, 100);


            // Cargar atlas de menu (plantilla con 4 variantes en 2x2)
            _fondoAtlas = _content.Load<Texture2D>("images/menu-principal-underground-races-plantilla");
            int frameW = _fondoAtlas.Width / _atlasCols;
            int frameH = _fondoAtlas.Height / _atlasRows;
            for (int r = 0; r < _atlasRows; r++)
            {
                for (int c = 0; c < _atlasCols; c++)
                {
                    _framesMenu.Add(new Rectangle(c * frameW, r * frameH, frameW, frameH));
                }
            }

            // Crear pixel para debug (dibujar rectángulos)
            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            _mouse = Mouse.GetState();
            var kb = Keyboard.GetState();

            // Cambiar target de asignacion con teclas 1/2/3
            _lastKeyboardState = kb;

            // Hover detection: cambia el frame del atlas según el botón sobre el que esté el ratón
            int hovered = 3;
            if (_botonJugar.Contains(_mouse.Position)) hovered = 0;
            else if (_botonAjustes.Contains(_mouse.Position)) hovered = 1;
            else if (_botonSalir.Contains(_mouse.Position)) hovered = 2;
            _frameMenuActual = hovered;


            // Click actions (como antes)
            if (_mouse.LeftButton == ButtonState.Pressed)
            {;

                // Si hay un target de asignacion seleccionado, usamos clicks para definir rectangulo
                if (_assignTarget != 0)
                {
                    if (_assignPointA == null)
                    {
                        _assignPointA = _mouse.Position;
                        Debug.WriteLine($"[Menu] Assign point A set at: {_assignPointA.Value.X}, {_assignPointA.Value.Y}");
                    }
                    else
                    {
                        var a = _assignPointA.Value;
                        var b = _mouse.Position;
                        int x = Math.Min(a.X, b.X);
                        int y = Math.Min(a.Y, b.Y);
                        int w = Math.Abs(a.X - b.X);
                        int h = Math.Abs(a.Y - b.Y);
                        var rect = new Rectangle(x, y, w, h);
                        switch (_assignTarget)
                        {
                            case 1: _botonJugar = rect; Debug.WriteLine($"[Menu] JUGAR assigned: {rect}"); break;
                            case 2: _botonAjustes = rect; Debug.WriteLine($"[Menu] AJUSTES assigned: {rect}"); break;
                            case 3: _botonSalir = rect; Debug.WriteLine($"[Menu] SALIR assigned: {rect}"); break;
                        }
                        // reset
                        _assignPointA = null;
                        _assignTarget = 0;
                    }
                }
                else
                {
                    // Click regular: activar botones
                    if (_botonJugar.Contains(_mouse.Position))
                    {
                        OnJugarClick?.Invoke();
                    }
                    else if (_botonAjustes.Contains(_mouse.Position))
                    {
                        OnAjustesClick?.Invoke();
                    }
                    else if (_botonSalir.Contains(_mouse.Position))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            if (_framesMenu.Count == (_atlasCols * _atlasRows) && _fondoAtlas != null)
            {
                int mapped = MathHelper.Clamp(_frameMenuActual, 0, _hoverToFrame.Length - 1);
                int frameIndex = _hoverToFrame[mapped];
                Rectangle src = _framesMenu[MathHelper.Clamp(frameIndex, 0, _framesMenu.Count - 1)];
                spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, 1024, 576), src, Color.White);
            }
            else if (_fondoAtlas != null)
            {
                spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, 1024, 576), Color.White);
            }

         
            spriteBatch.End();
        }

        private void DrawRectOutline(SpriteBatch sb, Rectangle r, Color c, int thickness)
        {
            // top
            sb.Draw(_debugPixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
            // bottom
            sb.Draw(_debugPixel, new Rectangle(r.X, r.Y + r.Height - thickness, r.Width, thickness), c);
            // left
            sb.Draw(_debugPixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
            // right
            sb.Draw(_debugPixel, new Rectangle(r.X + r.Width - thickness, r.Y, thickness, r.Height), c);
        }
    }
}
