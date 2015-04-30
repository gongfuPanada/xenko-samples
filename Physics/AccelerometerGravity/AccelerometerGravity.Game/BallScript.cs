using System.Threading.Tasks;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;

namespace AccelerometerGravity
{
    public class BallScript : AsyncScript
    {
        public override Task Execute()
        {
            var sprite = Entity.Get<SpriteComponent>();
            SpriteAnimation.Play(sprite, 0, sprite.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, 2);

            return Task.FromResult(0);
        }
    }
}