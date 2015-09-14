using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace RenderSceneToTexture
{
    /// <summary>
    /// Set the source of the sprite component.
    /// </summary>
    public class SetSpriteSource : StartupScript
    {
        public RenderFrame RenderFrame;

        public override void Start()
        {
            base.Start();

            var spriteComponent = Entity.Get<SpriteComponent>();
            var spriteSource = RenderFrame.RenderTargets[0];
			var spriteFromTexture = spriteComponent.SpriteProvider as SpriteFromTexture;
			if(spriteFromTexture != null)
				spriteFromTexture.Texture = spriteSource;
        }
    }
}