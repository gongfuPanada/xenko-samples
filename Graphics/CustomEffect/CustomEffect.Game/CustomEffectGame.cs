using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Graphics;

namespace CustomEffect
{
    public class CustomEffectGame : Game
    {
        private Effect customEffect;
        private PrimitiveQuad quad;
        private Texture paradoxTexture;
        private SamplerState samplingState;

        public CustomEffectGame()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            paradoxTexture = Asset.Load<Texture2D>("LogoParadox");
            customEffect = EffectSystem.LoadEffect("CustomEffect");

            // set fixed parameters once
            customEffect.Parameters.Set(CustomEffectKeys.Center, new Vector2(0.5f, 0.5f));
            customEffect.Parameters.Set(CustomEffectKeys.Frequency, 40);
            customEffect.Parameters.Set(CustomEffectKeys.Spread, 0.5f);
            customEffect.Parameters.Set(CustomEffectKeys.Amplitude, 0.015f);
            customEffect.Parameters.Set(CustomEffectKeys.InvAspectRatio, ((float)GraphicsDevice.BackBuffer.Height) / ((float)GraphicsDevice.BackBuffer.Width));

            quad = new PrimitiveQuad(GraphicsDevice, customEffect);

            // NOTE: Linear-Wrap sampling is not available for non-square non-power-of-two textures on opengl es 2.0
            samplingState = SamplerState.New(GraphicsDevice, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Clamp));

            CreatePipeline();
        }

        private void CreatePipeline()
        {
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderQuad });
        }

        private void RenderQuad(RenderContext renderContext)
        {
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.NonPremultiplied);
            customEffect.Parameters.Set(CustomEffectKeys.Phase, -3 * (float)this.UpdateTime.Total.TotalSeconds);
            quad.Draw(paradoxTexture, samplingState, Color.White);
        }
    }
}
