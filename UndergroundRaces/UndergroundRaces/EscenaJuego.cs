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

        // (Sistema antiguo de carteles en ruta eliminado: variables no usadas fueron limpiadas)

        // Nuevo sistema de obstáculos como rectángulos
        private class Obstaculo
        {
            public int Lane; // 0 = izquierda, 1 = centro, 2 = derecha
            public float Progress; // 0 = lejano (en el horizonte) .. 1 = cerca (jugador)
            public float Speed; // velocidad relativa de avance
            public float BaseW;
            public float BaseH;
            public bool Hit; // si ya chocó con el jugador (para evitar múltiples colisiones)
        }

        private List<Obstaculo> _obstaculos = new();
        private Random _rand = new Random();
        private float _timerSpawnObstaculos = 0f;
        private float _spawnIntervalObstaculos = 1.8f; // segundos entre spawns
        private float _vanishingY = 500; // punto de fuga aproximado
        private float _nearYOffset = 120f; // distancia vertical desde el auto hasta donde "caen" los obstaculos
        private float _obstBaseW = 90f; // aumentado (antes 60)
        private float _obstBaseH = 66f; // aumentado (antes 44)
        private float _obstMinScale = 0.30f; // un poco más grande en lejania
        private float _obstMaxScale = 2.6f; // aumentar el máximo aparente

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
        private Texture2D _obstSprite;
        private SoundEffect _crashSound;
        private Song _gameSong;

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

            // Intentar cargar sprite específico para obstáculo desde Content
            try
            {
                _obstSprite = _content.Load<Texture2D>("images/obstaculo");
            }
            catch
            {
                // fallback: usar el primer cartel si existe
                if (_carteles.Count > 0)
                    _obstSprite = _carteles[0];
                else
                    _obstSprite = null;
            }

            // posición inicial del marcador central del jugador (el sistema antiguo fue removido)
            // Pixel 1x1 para dibujado de rectángulos/debug
            _debugPixel = new Texture2D(_graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });

            // Inicializar nuevo sistema de obstáculos (rectángulos)
            _obstaculos = new List<Obstaculo>();
            _timerSpawnObstaculos = 0f;
            // ajustar el punto de fuga en función del alto de la pantalla
            int screenHeight = _graphicsDevice.Viewport.Height;
            // Ajustar punto de fuga para que los obstáculos nazcan sobre la carretera (no en el cielo)
            // Valor tunable: ~0.40..0.48 suele situarlo en la línea del horizonte/entrada de la carretera
            _vanishingY = screenHeight * 0.52f;

            // Intentar cargar sonido de choque (varios nombres posibles según Content.mgcb)
            _crashSound = null;
            try
            {
                _crashSound = _content.Load<SoundEffect>("audio/car-crash_ext-6388 (mp3cut.net).wav");
            }
            catch
            {
                try
                {
                    _crashSound = _content.Load<SoundEffect>("audio/car-crash_ext-6388 (mp3cut.net)");
                }
                catch
                {
                    try
                    {
                        _crashSound = _content.Load<SoundEffect>("audio/car-crash");
                    }
                    catch { _crashSound = null; }
                }
            }

            // Intentar cargar música para la escena de juego (usar Song preferido).
            _gameSong = null;
            try
            {
                string[] gameCandidates = new string[] {
                    "audio/retro-arcade-game-music-297305",
                    "audio/retro-arcade-game-music",
                    "audio/retro_arade",
                    "audio/retro-arade",
                    "audio/game-music",
                    "audio/game_music",
                    "audio/game"
                };

                foreach (var cand in gameCandidates)
                {
                    try
                    {
                        _gameSong = _content.Load<Song>(cand);
                        System.Diagnostics.Debug.WriteLine($"[EscenaJuego] Cargada canción de juego: {cand}");
                        break;
                    }
                    catch { }
                }
            }
            catch { _gameSong = null; }

            if (_gameSong != null)
            {
                try
                {
                    if (MediaPlayer.State != MediaState.Playing)
                    {
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = 0.5f;
                        MediaPlayer.Play(_gameSong);
                        System.Diagnostics.Debug.WriteLine("[EscenaJuego] Reproduciendo música de juego (Song).");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EscenaJuego] Error al reproducir Song: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[EscenaJuego] No se encontró Song para la música de juego.");
            }

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
            // Frenado antiguo: tecla S
            bool currentlyBraking = state.IsKeyDown(Keys.Space);

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

            // Aplicar frenado manual si se mantiene S
            if (currentlyBraking)
            {
                _velocidadActual -= _brakeRate * dt;
                if (_velocidadActual < 0f) _velocidadActual = 0f;
            }

            // Reproducir/Detener sonido de freno al empezar/parar de frenar
            if (currentlyBraking && !_isBraking)
            {
                try
                {
                    if (_brakeInstance != null)
                    {
                        _brakeInstance.IsLooped = false;
                        _brakeInstance.Volume = 1f;
                        _brakeInstance.Play();
                    }
                    else if (_brakeSong != null)
                    {
                        MediaPlayer.Play(_brakeSong);
                    }
                }
                catch { }
            }
            else if (!currentlyBraking && _isBraking)
            {
                try
                {
                    if (_brakeInstance != null)
                    {
                        _brakeInstance.Stop();
                    }
                    else if (_brakeSong != null)
                    {
                        MediaPlayer.Stop();
                    }
                }
                catch { }
            }

            _isBraking = currentlyBraking;

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

            // --- Obstáculos en la ruta (rectángulos rojos) ---
            _timerSpawnObstaculos += dt * (0.5f + speedFactor);
            if (_timerSpawnObstaculos >= _spawnIntervalObstaculos)
            {
                // intentar elegir un carril que no tenga un obstáculo cercano para evitar acumulación
                int chosenLane = -1;
                int attempts = 3;
                for (int a = 0; a < attempts; a++)
                {
                    int candidateLane = _rand.Next(0, 3);
                    bool blocked = false;
                    foreach (var exist in _obstaculos)
                    {
                        if (exist.Lane != candidateLane) continue;
                        // si hay un obstáculo en ese carril que está relativamente cerca (progress > -0.25), considerarlo bloqueado
                        if (exist.Progress > -0.25f)
                        {
                            blocked = true;
                            break;
                        }
                    }
                    if (!blocked)
                    {
                        chosenLane = candidateLane;
                        break;
                    }
                }

                if (chosenLane == -1)
                {
                    // no se encontró carril libre: reintentar más tarde
                    _timerSpawnObstaculos = _spawnIntervalObstaculos * 0.45f;
                }
                else
                {
                    Obstaculo nuevo = new Obstaculo();
                    nuevo.Lane = chosenLane;
                    nuevo.Progress = -0.18f; // empezar más lejos para separar
                    nuevo.Speed = 1.6f; // base más rápida (antes 1f)
                    nuevo.BaseW = _obstBaseW;
                    nuevo.BaseH = _obstBaseH;
                    nuevo.Hit = false;

                    // calcular rect del candidato en su posicion inicial
                    Vector2 candPos = GetObstacleScreenPos(nuevo.Lane, nuevo.Progress);
                    float candScale = MathHelper.Lerp(_obstMinScale, _obstMaxScale, MathHelper.Clamp(nuevo.Progress, 0f, 1f));
                    float candW = nuevo.BaseW * candScale;
                    float candH = nuevo.BaseH * candScale;
                    Rectangle candRect = new Rectangle((int)(candPos.X - candW / 2f), (int)(candPos.Y - candH / 2f), (int)candW, (int)candH);

                    bool overlaps = false;
                    foreach (var exist in _obstaculos)
                    {
                        Vector2 ePos = GetObstacleScreenPos(exist.Lane, exist.Progress);
                        float eScale = MathHelper.Lerp(_obstMinScale, _obstMaxScale, MathHelper.Clamp(exist.Progress, 0f, 1f));
                        float eW = exist.BaseW * eScale;
                        float eH = exist.BaseH * eScale;
                        Rectangle eRect = new Rectangle((int)(ePos.X - eW / 2f), (int)(ePos.Y - eH / 2f), (int)eW, (int)eH);
                        if (candRect.Intersects(eRect))
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        _obstaculos.Add(nuevo);
                        _timerSpawnObstaculos = 0f;
                    }
                    else
                    {
                        // si se superpone (caso raro), reintentar pronto
                        _timerSpawnObstaculos = _spawnIntervalObstaculos * 0.45f;
                    }
                }
            }

            // actualizar obstáculos
            for (int i = _obstaculos.Count - 1; i >= 0; i--)
            {
                var o = _obstaculos[i];
                // Hacer que los obstáculos frenen cuando el auto no está en velocidad.
                // Si la velocidad del coche es <= 9 km/h, detener completamente los obstáculos (opción A).
                float currentKmhObst = speedFactor * _velocidadMaxKmh;
                if (currentKmhObst <= 9f)
                {
                    // detenido
                    // no aplicar progreso
                    continue;
                }

                // Subir la velocidad mínima para evitar acumulación cuando el coche se mueve
                float obstacleSpeedMul = MathHelper.Lerp(0.35f, 1f, speedFactor);
                float laneBoost = (o.Lane == 1) ? 1.18f : 1f;
                float approachRate = 0.9f * obstacleSpeedMul * o.Speed * laneBoost;

                // boost progresivo cuando el obstáculo se acerca al tamaño máximo
                // ahora aplicado a todos los carriles (antes sólo al centro)
                {
                    float boostStart = 0.78f;
                    if (o.Progress > boostStart)
                    {
                        float nearFactor = MathHelper.Clamp((o.Progress - boostStart) / (1f - boostStart), 0f, 1f);
                        float extraBoost = MathHelper.Lerp(1f, 3.0f, nearFactor);
                        approachRate *= extraBoost;
                    }
                }

                // Garantizar un mínimo absoluto de progreso por segundo mientras el coche está en movimiento
                // aumentar mínimo para que se sientan más rápidos incluso en baja velocidad
                float minProgressPerSecond = 0.28f * o.Speed; // progreso mínimo por segundo (antes 0.18)
                approachRate = Math.Max(approachRate, minProgressPerSecond);

                o.Progress += approachRate * dt;

                Vector2 pos = GetObstacleScreenPos(o.Lane, o.Progress);
                float scale = MathHelper.Lerp(_obstMinScale, _obstMaxScale, MathHelper.Clamp(o.Progress, 0f, 1f));
                float w = o.BaseW * scale;
                float h = o.BaseH * scale;

                Rectangle jugadorRect = new Rectangle((int)_corsaPosition.X - 40, (int)_corsaPosition.Y - 40, 80, 80);
                Rectangle obstRect = new Rectangle((int)(pos.X - w / 2f), (int)(pos.Y - h / 2f), (int)w, (int)h);

                // Colisión más explícita: reducir ambas hitboxes para requerir contacto visual
                float hitboxFactor = 0.65f; // 0..1, menor = hitbox más pequeña (más estricto)
                int jW = (int)(jugadorRect.Width * hitboxFactor);
                int jH = (int)(jugadorRect.Height * hitboxFactor);
                Rectangle jugadorHit = new Rectangle(jugadorRect.Center.X - jW / 2, jugadorRect.Center.Y - jH / 2, jW, jH);

                int oW = (int)(obstRect.Width * hitboxFactor);
                int oH = (int)(obstRect.Height * hitboxFactor);
                Rectangle obstHit = new Rectangle(obstRect.Center.X - oW / 2, obstRect.Center.Y - oH / 2, oW, oH);

                if (jugadorHit.Intersects(obstHit))
                {
                    // Al chocar con un obstáculo: reducir la velocidad y eliminar el obstáculo
                    try
                    {
                        _velocidadActual *= 0.5f; // penalización de velocidad
                    }
                    catch { }

                    // reproducir sonido de choque si está disponible
                    try { _crashSound?.Play(); } catch { }

                    // marcar y eliminar el obstáculo para que no quede en pantalla
                    o.Hit = true;
                    _obstaculos.RemoveAt(i);
                    continue;
                }

                // Eliminar cuando el obstáculo haya pasado por delante del coche
                // (da la sensación de que lo pasaste rápidamente). También mantener un
                // fallback para eliminación cuando salen muy fuera de la pantalla.
                int screenH = _graphicsDevice.Viewport.Height;
                int removeBefore = (o.Lane == 1) ? 40 : 80; // eliminar antes si está en el centro
                if (pos.Y > _corsaPosition.Y + removeBefore || pos.Y > screenH + 120)
                {
                    _obstaculos.RemoveAt(i);
                }
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

            // Actualizar distancia recorrida (metros) usando la velocidad mostrada en km/h
            float currentKmhForDistance = speedFactor * _velocidadMaxKmh;
            float metersPerSecond = currentKmhForDistance / 3.6f;
            _distanciaRecorrida += metersPerSecond * dt;

            if (_distanciaRecorrida >= _distanciaObjetivo)
            {
                TimeSpan tiempoTotal = DateTime.Now - _tiempoInicio;
                string mensaje = $"Game Over - {tiempoTotal.TotalSeconds:0.0} segundos";
                OnFinCarrera?.Invoke(mensaje);
                return;
            }

            // (Bloque relacionado con sistema antiguo de carteles eliminado)
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

            // Dibujar obstáculos como rectángulos rojos siguiendo las tres trayectorias
            foreach (var o in _obstaculos)
            {
                Vector2 pos = GetObstacleScreenPos(o.Lane, o.Progress);
                float scale = MathHelper.Lerp(_obstMinScale, _obstMaxScale, MathHelper.Clamp(o.Progress, 0f, 1f));
                float w = o.BaseW * scale;
                float h = o.BaseH * scale;

                // pequeña sombra/contorno
                if (_obstSprite != null)
                {
                    // Dibujar sombra usando el propio sprite (offset y alpha) en lugar de un gran rectángulo
                    Vector2 origin = new Vector2(_obstSprite.Width / 2f, _obstSprite.Height / 2f);
                    float spriteScale = (_obstSprite.Width > 0) ? (w / _obstSprite.Width) : 1f;
                    Vector2 shadowOffset = new Vector2(3f, 3f);
                    spriteBatch.Draw(_obstSprite, pos + shadowOffset, null, Color.Black * 0.45f, 0f, origin, spriteScale * 1.02f, SpriteEffects.None, 0f);

                    // Dibujar sprite del obstáculo
                    spriteBatch.Draw(_obstSprite, pos, null, Color.White, 0f, origin, spriteScale, SpriteEffects.None, 0f);
                }
                else
                {
                    // pequeña sombra/contorno para fallback (rectángulo)
                    Rectangle shadow = new Rectangle((int)(pos.X - w / 2f) - 2, (int)(pos.Y - h / 2f) - 2, (int)w + 4, (int)h + 4);
                    spriteBatch.Draw(_debugPixel, shadow, Color.Black * 0.6f);

                    Rectangle rect = new Rectangle((int)(pos.X - w / 2f), (int)(pos.Y - h / 2f), (int)w, (int)h);
                    spriteBatch.Draw(_debugPixel, rect, Color.Red);
                }
            }


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

        // Calcula la posición en pantalla de un obstáculo dado su carril (0/1/2) y su progreso (0..1)
        private Vector2 GetObstacleScreenPos(int lane, float progress)
        {
            int screenW = _graphicsDevice.Viewport.Width;
            float centerX = screenW / 2f;

            float vanY = _vanishingY;
            float nearY = _corsaPosition.Y - _nearYOffset;

            // offsets laterales: pequeño en la distancia, grande al acercarse
            float farOffset = Math.Max(40f, screenW * 0.06f); // un poco más separacion en la distancia
            float nearOffset = Math.Min(380f, screenW * 0.44f);

            // Reducir la separación lateral para que los costados queden más cerca del medio
            float laneOuterFactor = 0.50f; // aplica a farOffset
            float laneInnerFactor = 0.50f; // aplica a nearOffset

            float laneFarX = centerX + (lane - 1) * farOffset * laneOuterFactor;
            float laneNearX = centerX + (lane - 1) * nearOffset * laneInnerFactor;

            float x = MathHelper.Lerp(laneFarX, laneNearX, progress);
            float y = MathHelper.Lerp(vanY, nearY, progress);

            return new Vector2(x, y);
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
