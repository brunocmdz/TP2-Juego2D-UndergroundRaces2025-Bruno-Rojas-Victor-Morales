using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System;

namespace UndergroundRaces
{
    // Manejo de escena del juego: fondo, auto, sonido y carteles
    public class EscenaJuego : IEscena
    {
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
        private const float _velocidadMax = 6.0f;
        private const float _aceleracionRate = 3.5f; // unidades por segundo
        private const float _desaceleracionRate = 4.5f; // unidades por segundo
        // Avance visual del auto al acelerar
        private float _offsetForward = 0f;
        private float _offsetForwardTarget = 0f;
        private const float _offsetMax = 20f; // pixeles hacia arriba
        private const float _offsetLerpSpeed = 8f; // rapidez de la interpolacion


        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        public void LoadContent(Game game)
        {
            // Carga de recursos y generacion de frames
            // Configuracion inicial de auto, sonido y carteles
            _graphicsDevice = game.GraphicsDevice;
            _content = game.Content;

            _fondoAtlas = _content.Load<Texture2D>("images/backgroundPLANTILLA2");
            GenerarFramesFondo(_fondoAtlas, 1024, 576);

            _corsaAtlas = _content.Load<Texture2D>("images/corsaPLANTILLA");
            GenerarFramesCorsa(_corsaAtlas, _corsaAtlas.Width, _corsaAtlas.Height / 2);

            _corsaDoblandoAtlas = _content.Load<Texture2D>("images/corsaDoblandoPLANTILLA");
            GenerarFramesDoblando(_corsaDoblandoAtlas, _corsaDoblandoAtlas.Width, _corsaDoblandoAtlas.Height / 2);

            _motorSound = _content.Load<SoundEffect>("audio/motor-corsa");
            _motorInstance = _motorSound.CreateInstance();
            _motorInstance.IsLooped = true;
            _motorInstance.Volume = 0f;
            _motorInstance.Play();

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
        }

        public void Update(GameTime gameTime)
        {
            // Entradas del jugador
            var state = Keyboard.GetState();
            float lateralBase = 3.5f;

            int screenWidth = _graphicsDevice.Viewport.Width;
            float corsaAncho = _corsaAtlas.Width * 3f;
            float corsaMitad = corsaAncho / 2f;

            float rutaMargenIzquierdo = 200f;
            float rutaMargenDerecho = screenWidth - 200f;
            float limiteIzquierdo = rutaMargenIzquierdo - corsaMitad;
            float limiteDerecho = rutaMargenDerecho + corsaMitad;

            // Pausa del juego
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
            // Aceleración: objetivo cuando se presiona W
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
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

            // Calcular target del offset vertical del auto (se mueve hacia arriba al acelerar)
            _offsetForwardTarget = -_offsetMax * speedFactor;
            _offsetForward = MathHelper.Lerp(_offsetForward, _offsetForwardTarget, MathHelper.Clamp(_offsetLerpSpeed * dt, 0f, 1f));

            // Movimiento lateral escalado por velocidad actual (pero siempre usable)
            float velocidadLateral = lateralBase * (0.5f + 0.5f * speedFactor);

            if (pressingD)
            {
                if (_corsaPosition.X + velocidadLateral < limiteDerecho)
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
                if (_corsaPosition.X - velocidadLateral > limiteIzquierdo)
                    _corsaPosition.X -= velocidadLateral;

                _timerDoblando += dt * (0.5f + speedFactor);
                if (_timerDoblando >= _tiempoPorFrameDoblando)
                {
                    _timerDoblando = 0f;
                    _frameDoblandoActual = (_frameDoblandoActual + 1) % _framesDoblando.Count;
                }
            }

            // Animaciones y carteles dependen de speedFactor
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

                Vector2 direccionIzq = new Vector2(-1.5f, 1f);
                Vector2 direccionDer = new Vector2(1.5f, 1f);

                direccionIzq.Normalize();
                direccionDer.Normalize();

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

            // Ajuste de volumen y pitch en base a la velocidad actual
            float targetVol = _volumenMaximo * ( _velocidadMax > 0f ? (_velocidadActual / _velocidadMax) : 0f );
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

            float pitch = -0.2f + ((_velocidadMax > 0f) ? (_velocidadActual / _velocidadMax) * 0.8f : 0f);
            if (pitch < -1f) pitch = -1f;
            if (pitch > 1f) pitch = 1f;
            _motorInstance.Pitch = pitch;
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
                Rectangle corsaRect = _framesCorsa[_frameCorsaActual];
                Vector2 origin = new Vector2(corsaRect.Width / 2f, corsaRect.Height / 2f);
                Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                spriteBatch.Draw(_corsaAtlas, drawPos, corsaRect, Color.White, 0f, origin, 3f, _spriteEffect, 0f);
            }
            else
            {
                Rectangle corsaRect = _framesDoblando[_frameDoblandoActual];
                Vector2 origin = new Vector2(corsaRect.Width / 2f, corsaRect.Height / 2f);
                Vector2 drawPos = new Vector2(_corsaPosition.X, _corsaPosition.Y + _offsetForward);
                spriteBatch.Draw(_corsaDoblandoAtlas, drawPos, corsaRect, Color.White, 0f, origin, 3f, _spriteEffect, 0f);
            }

            spriteBatch.Draw(_cartelIzqActual, _posCartelIzq, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_cartelDerActual, _posCartelDer, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);


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
    }
}
