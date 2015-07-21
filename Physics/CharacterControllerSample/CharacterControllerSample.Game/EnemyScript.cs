using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;

namespace CharacterControllerSample
{
    /// <summary>
    /// This simple script will start the sprite idle animation
    /// </summary>
    public class EnemyScript : StartupScript
    {
        public override void Start()
        {
            var sprite = Entity.Get<SpriteComponent>();
            SpriteAnimation.Play(sprite, 0, sprite.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, 2);
        }
    }
}