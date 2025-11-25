using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;

namespace UndergroundRaces
{
    // Manejo de escena del juego: fondo, auto, sonido y carteles
    public class EscenaJuego : IEscena
    {
        public enum VehicleType { Corsa, Clio }

        private VehicleType _vehiculoSeleccionado = VehicleType.Corsa;

        // Clio atlases/frames
        private Texture2D _clioAtlas;
        private List<Rectangle> _framesClio = new();
        private Texture2D _clioDoblandoAtlas;
        private List<Rectangle> _framesClioDoblando = new();
        // Escalas por vehículo
        private float _corsaScale = 3f;
        private float _clioScale = 2.2f; // más pequeño para Clio
        // Fondo animado
        private Texture2D _fondoAtlas;
        private List<Rectangle> _framesFondo = new();
        private int _frameActual = 0;
        private int _totalFrames = 14;
        private float _timerFrame = 0f;
        private float _tiempoPorFrameBase = 0.02f;
        private bool _avanzando = false;

        // Textura auto recto
        private Texture2D _corsaAtlas;
        private List<Rectangle> _framesCorsa = new();
        private int _frameCorsaActual = 0;
        private float _timerCorsa = 0f;
        private float _tiempoPorFrameCorsa = 0.08f;
        private bool _usandoAtlas = true;
        private Vector2 _corsaPosition;
        private SpriteEffects _spriteEffect = SpriteEffects.None;

        // Textura auto doblando
        private Texture2D _corsaDoblandoAtlas;
        private List<Rectangle> _framesDoblando = new();
        private int _frameDoblandoActual = 0;
        private float _timerDoblando = 0f;
        private float _tiempoPorFrameDoblando = 0.1f;


        // Efectos de sonido del auto
        private SoundEffect _motorSound;
        private SoundEffectInstance _motorInstance;
        // Sonido de frenado (puede cargarse como SoundEffect o Song segun el pipeline)
        private SoundEffect _brakeSound;
        private SoundEffectInstance _brakeInstance;
        private Song _brakeSong;
        private bool _isBraking = false;
        private float _motorVolume = 0f;
        private const float _volumenMaximo = 0.5f;
        private const float _velocidadCambioVolumen = 0.01f;

        // Evento de pausa
        public Action OnPausaSolicitada;

        // Texturas de carteles
        private List<Texture2D> _carteles = new();
        private int _indiceCartelIzq = 0;
        private int _indiceCartelDer = 0;
        private Texture2D _cartelIzqActual;
        private Texture2D _cartelDerActual;
        private Vector2 _posCartelIzq;
        private Vector2 _posCartelDer;
        private Vector2 _posCartelIzqInicial;
        private Vector2 _posCartelDerInicial;
        private float _velocidadCartel = 3.5f;
        private float _tiempoCartel = 3f;
        private float _timerCartelIzq = 0f;
        private float _timerCartelDer = 0f;

        // Aceleración / velocidad
        private float _velocidadActual = 0f;
        private float _velocidadObjetivo = 0f;
        private const float _velocidadMax = 18.0f;
        private const float _aceleracionRate = 6.0f; // unidades por segundo (aumentada)
        private const float _desaceleracionRate = 6.0f; // unidades por segundo (aumentada)
        // Valor de la frenada al mantener S (unidades por segundo)
        private const float _brakeRate = 12.0f;
        // Ritmo de desaceleración por inercia (coasting) cuando se suelta el acelerador
        private const float _coastRate = 1.5f;
        // Valor máximo mostrado al usuario en el medidor (km/h)
        private const float _velocidadMaxKmh = 240f;
        // Avance visual del auto al acelerar
        private float _offsetForward = 0f;
        private float _offsetForwardTarget = 0f;
        private const float _offsetMax = 20f; // pixeles hacia arriba
        private const float _offsetLerpSpeed = 8f; // rapidez de la interpolacion

        // Carteles en la ruta (obstáculos)
        private Texture2D _cartelObsActual;
        private Vector2 _posCartelObs;
        private Vector2 _posCartelObsInicial;
        private float _timerCartelObs = 0f;
        private float _tiempoCartelObs = 4f; // cada 4 segundos cambia
        private int _indiceCartelObs = 0;
        // Obstáculos en la ruta
        private List<Vector2> _cartelesRuta = new();
        private Texture2D _cartelRutaTexture;
        private float _timerSpawnCartel = 0f;
        private float _tiempoSpawnCartel = 5f; // cada 5 segundos aparece uno

        // Distancia recorrida
        private float _distanciaRecorrida = 0f;
        private float _distanciaObjetivo = 2000f; // metros para terminar
        private DateTime _tiempoInicio;

        // Evento de fin de carrera
        public Action<string> OnFinCarrera;

        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private Texture2D _debugPixel;
        private SpriteFont _afaFont;

        public void LoadContent(Game game)
        {
            // Carga de recursos y generacion de frames
            // Configuracion inicial de auto, sonido y carteles
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            _fondoAtlas = _content.Load<Texture2D>("images/backgroundPLANTILLA2");
            GenerarFramesFondo(_fondoAtlas, 1024, 576);

            // Corsa (plantilla)
            _corsaAtlas = _content.Load<Texture2D>("images/corsaPLANTILLA");
            GenerarFramesCorsa(_corsaAtlas, _corsaAtlas.Width, _corsaAtlas.Height / 2);

            _corsaDoblandoAtlas = _content.Load<Texture2D>("images/corsaDoblandoPLANTILLA");
            GenerarFramesDoblando(_corsaDoblandoAtlas, _corsaDoblandoAtlas.Width, _corsaDoblandoAtlas.Height / 2);

            // Clio (otros assets)
            _clioAtlas = _content.Load<Texture2D>("images/clio-underground-races-2025-avanzando-plantilla");
            GenerarFramesEnLista(_clioAtlas, _clioAtlas.Width, _clioAtlas.Height / 2, _framesClio);

            _clioDoblandoAtlas = _content.Load<Texture2D>("images/clio-underground-races-2025-doblando-plantilla");
            GenerarFramesEnLista(_clioDoblandoAtlas, _clioDoblandoAtlas.Width, _clioDoblandoAtlas.Height / 2, _framesClioDoblando);

            _motorSound = _content.Load<SoundEffect>("audio/motor-corsa");
            _motorInstance = _motorSound.CreateInstance();
            _motorInstance.IsLooped = true;
            _motorInstance.Volume = 0f;
            _motorInstance.Play();

            // Intentar cargar sonido de freno: primero como SoundEffect (wav/ogg), si falla intentar cargar como Song (mp3)
            try
            {
                // intenta una ruta simple (sin espacios)
                _brakeSound = _content.Load<SoundEffect>("audio/Car Braking - Sound Effect [HQ] [XBR8NuELc4w] (mp3cut.net)");
                _brakeInstance = _brakeSound.CreateInstance();
            }
            catch
            {
                // intentar con el nombre completo presente en Content.mgcb
                _brakeSound = _content.Load<SoundEffect>("audio/Car Braking - Sound Effect [HQ] [XBR8NuELc4w] (mp3cut.net).wav");
                _brakeInstance = _brakeSound.CreateInstance();

            }

            for (int i = 1; i <= 8; i++)
            {
                _carteles.Add(_content.Load<Texture2D>($"images/cartel{i}"));
            }

            _cartelIzqActual = _carteles[_indiceCartelIzq];
            _cartelDerActual = _carteles[_indiceCartelDer];

            int screenWidth = _graphicsDevice.Viewport.Width;
            _posCartelIzq = new Vector2(30, 180);
            _posCartelDer = new Vector2(screenWidth - 130, 180);

            // Guardar posiciones iniciales para respawn consistente
            _posCartelIzqInicial = _posCartelIzq;
            _posCartelDerInicial = _posCartelDer;

            _corsaPosition = new Vector2(screenWidth / 2f, 500);
            _cartelObsActual = _carteles[_indiceCartelObs];

            // posición inicial en el centro de la pista
            _posCartelObs = new Vector2(_graphicsDevice.Viewport.Width / 2f, -100);
            _posCartelObsInicial = _posCartelObs;
            // Pixel 1x1 para dibujado de rectángulos/debug
            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });

            // Cargar la fuente 'afa' para el medidor de velocidad (si existe)
            try
            {
                _afaFont = _content.Load<SpriteFont>("font/afa");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EscenaJuego] No se pudo cargar la fuente 'font/afa': {ex.Message}");
                _afaFont = null;
            }

        }

        public void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state.IsKeyDown(Keys.Escape))
                OnPausaSolicitada?.Invoke();

            bool pressingD = state.IsKeyDown(Keys.D);
            bool pressingA = state.IsKeyDown(Keys.A);

            if (pressingD)
            {
                _usandoAtlas = false;
                _spriteEffect = SpriteEffects.None;
            }
            else if (pressingA)
            {
                _usandoAtlas = false;
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _usandoAtlas = true;
                _spriteEffect = SpriteEffects.None;
                _frameDoblandoActual = 0;
            }

            _avanzando = state.IsKeyDown(Keys.W);
            _velocidadObjetivo = _avanzando ? _velocidadMax : 0f;

            if (_velocidadActual < _velocidadObjetivo)
            {
                _velocidadActual += _aceleracionRate * dt;
                if (_velocidadActual > _velocidadObjetivo) _velocidadActual = _velocidadObjetivo;
            }
            else if (_velocidadActual > _velocidadObjetivo)
            {
                _velocidadActual -= _desaceleracionRate * dt;
                if (_velocidadActual < _velocidadObjetivo) _velocidadActual = _velocidadObjetivo;
            }

            float speedFactor = _velocidadMax > 0f ? _velocidadActual / _velocidadMax : 0f;

            _offsetForwardTarget = -_offsetMax * speedFactor;
            _offsetForward = MathHelper.Lerp(_offsetForward, _offsetForwardTarget, MathHelper.Clamp(_offsetLerpSpeed * dt, 0f, 1f));

            float velocidadLateral = 3.5f * (0.5f + 0.5f * speedFactor);

            if (pressingD)
            {
                if (_corsaPosition.X + velocidadLateral < _graphicsDevice.Viewport.Width - 200)
                    _corsaPosition.X += velocidadLateral;

                _timerDoblando += dt * (0.5f + speedFactor);
                if (_timerDoblando >= _tiempoPorFrameDoblando)
                {
                    _timerDoblando = 0f;
                    _frameDoblandoActual = (_frameDoblandoActual + 1) % _framesDoblando.Count;
                }
            }
            else if (pressingA)
            {
                if (_corsaPosition.X - velocidadLateral > 200)
                    _corsaPosition.X -= velocidadLateral;

                _timerDoblando += dt * (0.5f + speedFactor);
                if (_timerDoblando >= _tiempoPorFrameDoblando)
                {
                    _timerDoblando = 0f;
                    _frameDoblandoActual = (_frameDoblandoActual + 1) % _framesDoblando.Count;
                }
            }

            // --- Obstáculo en la ruta ---
            _posCartelObs += new Vector2(0, _velocidadCartel * (0.5f + speedFactor));

            _timerCartelObs += dt;
            if (_timerCartelObs >= _tiempoCartelObs)
            {
                _indiceCartelObs++;

                if (_indiceCartelObs >= _carteles.Count)
                {
                    // Llegó al último cartel → fin de carrera
                    TimeSpan tiempoTotal = DateTime.Now - _tiempoInicio;
                    string mensaje = $"El jugador terminó en {tiempoTotal.TotalSeconds:0.0} segundos";
                    OnFinCarrera?.Invoke(mensaje); // tu manejador debe cambiar a MenuPrincipal
                    return;
                }

                _cartelObsActual = _carteles[_indiceCartelObs];

                // Posición aleatoria dentro de la ruta
                float rutaMargenIzquierdo = 200f;
                float rutaMargenDerecho = _graphicsDevice.Viewport.Width - 200f;
                float xPos = new Random().Next((int)rutaMargenIzquierdo, (int)rutaMargenDerecho);

                _posCartelObs = new Vector2(xPos, -100); // respawn desde arriba
                _timerCartelObs = 0f;
            }

            // Colisión con obstáculo
            Rectangle jugadorRect = new Rectangle((int)_corsaPosition.X - 40, (int)_corsaPosition.Y - 40, 80, 80);
            Rectangle cartelRect = new Rectangle((int)_posCartelObs.X, (int)_posCartelObs.Y, 100, 100);

            if (jugadorRect.Intersects(cartelRect))
            {
                _velocidadActual *= 0.5f; // ralentiza al jugador
                _posCartelObs = new Vector2(_graphicsDevice.Viewport.Width / 2f, -100); // reinicia obstáculo
            }

            // --- Fondo y carteles laterales ---
            if (speedFactor > 0.01f)
            {
                _timerFrame += dt * (0.5f + speedFactor * 1.5f);
                if (_timerFrame >= _tiempoPorFrameBase)
                {
                    _timerFrame = 0f;
                    _frameActual = (_frameActual + 1) % _framesFondo.Count;
                }

                _timerCorsa += dt * (0.5f + speedFactor * 1.5f);
                if (_timerCorsa >= _tiempoPorFrameCorsa)
                {
                    _timerCorsa = 0f;
                    _frameCorsaActual = (_frameCorsaActual + 1) % _framesCorsa.Count;
                }

                Vector2 direccionIzq = Vector2.Normalize(new Vector2(-1.5f, 1f));
                Vector2 direccionDer = Vector2.Normalize(new Vector2(1.5f, 1f));

                _posCartelIzq += direccionIzq * _velocidadCartel * (0.5f + speedFactor);
                _posCartelDer += direccionDer * _velocidadCartel * (0.5f + speedFactor);

                _timerCartelIzq += dt;
                _timerCartelDer += dt;

                if (_timerCartelIzq >= _tiempoCartel)
                {
                    _indiceCartelIzq = (_indiceCartelIzq + 1) % _carteles.Count;
                    _cartelIzqActual = _carteles[_indiceCartelIzq];
                    _posCartelIzq = _posCartelIzqInicial;
                    _timerCartelIzq = 0f;
                }

                if (_timerCartelDer >= _tiempoCartel)
                {
                    _indiceCartelDer = (_indiceCartelDer + 1) % _carteles.Count;
                    _cartelDerActual = _carteles[_indiceCartelDer];
                    _posCartelDer = _posCartelDerInicial;
                    _timerCartelDer = 0f;
                }
            }
            else
            {
                _frameCorsaActual = 0;
            }

            // --- Volumen y pitch del motor ---
            float targetVol = _volumenMaximo * speedFactor;
            if (_motorVolume < targetVol)
            {
                _motorVolume += _velocidadCambioVolumen;
                if (_motorVolume > targetVol) _motorVolume = targetVol;
            }
            else
            {
                _motorVolume -= _velocidadCambioVolumen;
                if (_motorVolume < targetVol) _motorVolume = targetVol;
            }
            _motorInstance.Volume = _motorVolume;

            float pitch = -0.2f + speedFactor * 0.8f;
            _motorInstance.Pitch = MathHelper.Clamp(pitch, -1f, 1f);

            if (_indiceCartelObs >= _carteles.Count)
            {
                // Llegó al último cartel → fin de carrera
                TimeSpan tiempoTotal = DateTime.Now - _tiempoInicio;
                string mensaje = $"El jugador terminó en {tiempoTotal.TotalSeconds:0.0} segundos";
                OnFinCarrera?.Invoke(mensaje); // el manejador en Game1 cambia a MenuPrincipal
                return;
            }
        }






        public void Draw(SpriteBatch spriteBatch)
        {
            //Dibujar todos los elementos
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            Rectangle frameRect = _framesFondo[_frameActual];
            spriteBatch.Draw(_fondoAtlas, new Rectangle(0, 0, screenWidth, screenHeight), frameRect, Color.White);

            if (_usandoAtlas)
            {
                if (_vehiculoSeleccionado == VehicleType.Corsa)
                {
                    Rectangle corsaRect = _framesCorsa[_frameCorsaActual];
                    Vector2 origin = new Vector2(corsaRect.Width / 2f, corsaRect.Height / 2f);
                    Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                    spriteBatch.Draw(_corsaAtlas, drawPos, corsaRect, Color.White, 0f, origin, _corsaScale, _spriteEffect, 0f);
                }
                else
                {
                    Rectangle clioRect = _framesClio[_frameCorsaActual];
                    Vector2 origin = new Vector2(clioRect.Width / 2f, clioRect.Height / 2f);
                    Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                    spriteBatch.Draw(_clioAtlas, drawPos, clioRect, Color.White, 0f, origin, _clioScale, _spriteEffect, 0f);
                }
            }
            else
            {
                if (_vehiculoSeleccionado == VehicleType.Corsa)
                {
                    Rectangle corsaRect = _framesDoblando[_frameDoblandoActual];
                    Vector2 origin = new Vector2(corsaRect.Width / 2f, corsaRect.Height / 2f);
                    Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                    spriteBatch.Draw(_corsaDoblandoAtlas, drawPos, corsaRect, Color.White, 0f, origin, _corsaScale, _spriteEffect, 0f);
                }
                else
                {
                    Rectangle clioRect = _framesClioDoblando[_frameDoblandoActual];
                    Vector2 origin = new Vector2(clioRect.Width / 2f, clioRect.Height / 2f);
                    Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                    spriteBatch.Draw(_clioDoblandoAtlas, drawPos, clioRect, Color.White, 0f, origin, _clioScale, _spriteEffect, 0f);
                }
            }

            spriteBatch.Draw(_cartelIzqActual, _posCartelIzq, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_cartelDerActual, _posCartelDer, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            // Dibujar cartel obstáculo en la ruta
            spriteBatch.Draw(_cartelObsActual, _posCartelObs, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);


            // Dibujar un rectángulo rojo en la esquina superior derecha (margen 10px)
            int rectW = 180;
            int rectH = 100;
            Rectangle topRightRect = new Rectangle(screenWidth - rectW - 10, 10, rectW, rectH);
            if (_debugPixel != null)
            {
                // fondo rojo del contenedor
                spriteBatch.Draw(_debugPixel, topRightRect, Color.Red);

                // contenido interior con padding
                int pad = 8;
                Rectangle inner = new Rectangle(topRightRect.X + pad, topRightRect.Y + pad, topRightRect.Width - pad * 2, topRightRect.Height - pad * 2);

                // fondo oscuro del medidor
                spriteBatch.Draw(_debugPixel, inner, new Color(0, 0, 0, 180));

                // calcular factor de velocidad (0..1)
                float speedFactor = (_velocidadMax > 0f) ? (_velocidadActual / _velocidadMax) : 0f;
                speedFactor = MathHelper.Clamp(speedFactor, 0f, 1f);

                // dibujar barra de fondo y barra llenada
                int barHeight = 20;
                Rectangle barBg = new Rectangle(inner.X + 6, inner.Y + inner.Height / 2 - barHeight / 2, inner.Width - 12, barHeight);
                spriteBatch.Draw(_debugPixel, barBg, Color.DarkGray);
                Rectangle barFill = new Rectangle(barBg.X, barBg.Y, (int)(barBg.Width * speedFactor), barBg.Height);
                spriteBatch.Draw(_debugPixel, barFill, Color.LimeGreen);

                // dibujar texto con la velocidad numérica (usa la fuente 'afa' si está disponible)
                if (_afaFont != null)
                {
                    // Mostrar solo la velocidad actual en km/h (sin el máximo)
                    float displayedSpeed = MathHelper.Clamp(speedFactor * _velocidadMaxKmh, 0f, _velocidadMaxKmh);
                    string texto = string.Format("{0:0} km/h", displayedSpeed);
                    Vector2 medidas = _afaFont.MeasureString(texto);
                    Vector2 txtPos = new Vector2(inner.X + (inner.Width - medidas.X) / 2f, inner.Y + 6);
                    spriteBatch.DrawString(_afaFont, texto, txtPos, Color.White);
                }
            }

            spriteBatch.End();
        }


        private void GenerarFramesFondo(Texture2D atlas, int anchoFrame, int altoFrame)
        {
            // Crea frames recortando el atlas de fondo
            int columnas = atlas.Width / anchoFrame;
            int filas = atlas.Height / altoFrame;

            for (int y = 0; y < filas; y++)
            {
                for (int x = 0; x < columnas; x++)
                {
                    _framesFondo.Add(new Rectangle(x * anchoFrame, y * altoFrame, anchoFrame, altoFrame));
                }
            }

            if (_framesFondo.Count > _totalFrames)
                _framesFondo = _framesFondo.GetRange(0, _totalFrames);
        }

        private void GenerarFramesCorsa(Texture2D atlas, int anchoFrame, int altoFrame)
        {
            // Crea frames recortando el atlas de el auto
            int filas = atlas.Height / altoFrame;
            for (int y = 0; y < filas; y++)
            {
                _framesCorsa.Add(new Rectangle(0, y * altoFrame, anchoFrame, altoFrame));
            }
        }

        private void GenerarFramesDoblando(Texture2D atlas, int anchoFrame, int altoFrame)
        {
            // Crea frames del auto doblando
            int filas = atlas.Height / altoFrame;
            for (int y = 0; y < filas; y++)
            {
                _framesDoblando.Add(new Rectangle(0, y * altoFrame, anchoFrame, altoFrame));
            }
        }

        private void GenerarFramesEnLista(Texture2D atlas, int anchoFrame, int altoFrame, List<Rectangle> lista)
        {
            int filas = atlas.Height / altoFrame;
            for (int y = 0; y < filas; y++)
            {
                lista.Add(new Rectangle(0, y * altoFrame, anchoFrame, altoFrame));
            }
        }
        public void SetVehiculo(VehicleType veh)
        {
            // Asigna el vehículo seleccionado por el jugador
            _vehiculoSeleccionado = veh;

            // Reinicia posición del jugador en el centro de la pantalla
            int screenWidth = _graphicsDevice.Viewport.Width;
            _corsaPosition = new Vector2(screenWidth / 2f, 500);

            // Reinicia variables de animación
            _frameCorsaActual = 0;
            _frameDoblandoActual = 0;
            _offsetForward = 0f;
            _velocidadActual = 0f;
            _velocidadObjetivo = 0f;

            // Reinicia efectos visuales
            _spriteEffect = SpriteEffects.None;
            _usandoAtlas = true;
            if (_motorInstance != null)
            _motorInstance.Play();
            _tiempoInicio = DateTime.Now;
        }
        public void PausarSonido()
        {
            if (_motorInstance != null)
                _motorInstance.Pause();
        }
        public void DetenerSonido()
        {
            if (_motorInstance != null)
            {
                _motorInstance.Stop();
            }
        }
        public void ReanudarSonido()
        {
            if (_motorInstance != null)
            {
                _motorInstance.Resume();
            }
        }
    }
}
