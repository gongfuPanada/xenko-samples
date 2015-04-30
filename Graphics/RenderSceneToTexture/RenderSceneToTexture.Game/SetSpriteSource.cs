using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace RenderSceneToTexture
{
    /// <summary>
    /// Set the source of the sprite component.
    /// </summary>
    public class SetSpriteSource : Script
    {
        public override void Start()
        {
            base.Start();

            var spriteComponent = Entity.Get<SpriteComponent>();
            var spriteSource = Asset.Load<RenderFrame>("RenderFrame").RenderTargets[0];
            spriteComponent.SpriteProvider = new SpriteFromTexture { Texture = spriteSource };
        }
    }
}