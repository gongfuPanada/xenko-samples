using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Audio;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace SimpleAudio
{
    public class SimpleAudioGame : Game
    {
        // Audio
        private SoundMusic music;
        private SoundEffect effect;

        // Graphic
        private SpriteBatch spriteBatch;
        private Texture djTexture;
        private Texture waveTexture;

        private readonly Vector2 djOffset = new Vector2(0, 50);
        private readonly Vector2 textOffset = new Vector2(0, 380);
        private readonly Vector2 waveInitialOffset = new Vector2(150, 25);

        private Vector2 waveCurrentOffset;
        private float waveCurrentAlpha;
        private SpriteFont font;

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Load audio
            music = Asset.Load<SoundMusic>("AmbientMusic");
            effect = Asset.Load<SoundEffect>("SoundEffect");

            // Load and create graphic objects
            spriteBatch = new SpriteBatch(GraphicsDevice) {VirtualResolution = VirtualResolution};
            djTexture = Asset.Load<Texture>("dj");
            waveTexture = Asset.Load<Texture>("wave");
            font = Asset.Load<SpriteFont>("Font");

            // Create the rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = Render });
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));

            // Add the script in charge of playing the audio
            Script.Add(PlayAudio);
        }

        private void Render(RenderContext renderContext)
        {
            const float textSize = 60f;
            const string text = "Tap on the screen!";
            var screenCenter = new Vector2(VirtualResolution.X, VirtualResolution.Y) / 2;
            var screenSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            
            spriteBatch.Begin();
            spriteBatch.Draw(djTexture, screenCenter + djOffset, Color.White, 0, new Vector2(djTexture.Width, djTexture.Height) / 2); // dj
            spriteBatch.Draw(waveTexture, screenCenter + new Vector2(-waveCurrentOffset.X, waveCurrentOffset.Y), waveCurrentAlpha * Color.White, 0, new Vector2(waveTexture.Width, waveTexture.Height) / 2); // left wave
            spriteBatch.Draw(waveTexture, screenCenter + new Vector2(+waveCurrentOffset.X, waveCurrentOffset.Y), waveCurrentAlpha * Color.White, 0, new Vector2(waveTexture.Width, waveTexture.Height) / 2); // right wave
            spriteBatch.DrawString(font, text, textSize, screenCenter - spriteBatch.MeasureString(font, text, textSize, screenSize) / 2 + textOffset, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0, TextAlignment.Left);
            spriteBatch.End();
        }

        private async Task PlayAudio()
        {
            // the background music in loop
            music.IsLooped = true;
            music.Play();

            var timeSinceLastSound = 1f;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                timeSinceLastSound += (float)UpdateTime.Elapsed.TotalSeconds;

                // play the sound effect on each touch on the screen
                if (Input.PointerEvents.Any(x => x.State == PointerState.Down))
                {
                    effect.Stop();
                    effect.Play();

                    timeSinceLastSound = 0;
                }

                var ratioFactor = Math.Max(0, 0.75f - timeSinceLastSound);
                waveCurrentOffset = waveInitialOffset*ratioFactor + (1 - ratioFactor)*(waveInitialOffset + new Vector2(350, 0));
                waveCurrentAlpha = ratioFactor;
            }
        }
    }
}
