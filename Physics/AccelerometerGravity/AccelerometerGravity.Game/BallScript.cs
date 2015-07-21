using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;

namespace AccelerometerGravity
{
    public class BallScript : StartupScript
    {
        public override void Start()
        {
            var sprite = Entity.Get<SpriteComponent>();
            SpriteAnimation.Play(sprite, 0, sprite.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, 2);
        }
    }
}