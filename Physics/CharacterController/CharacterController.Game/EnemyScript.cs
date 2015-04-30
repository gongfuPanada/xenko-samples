using System.Threading.Tasks;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;

namespace CharacterController
{
    /// <summary>
    /// This simple script will start the sprite idle animation
    /// </summary>
    public class EnemyScript : AsyncScript
    {
        public override Task Execute()
        {
            var sprite = Entity.Get<SpriteComponent>();
            SpriteAnimation.Play(sprite, 0, sprite.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, 2);
            
            return Task.FromResult(0);
        }
    }
}