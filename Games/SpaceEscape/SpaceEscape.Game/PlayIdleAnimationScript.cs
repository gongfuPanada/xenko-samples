using SiliconStudio.Paradox.Engine;

namespace SpaceEscape
{
    /// <summary>
    /// Plays the idle animation of the entity if any
    /// </summary>
    public class PlayIdleAnimationScript : Script
    {
        public const string AnimationName = "Idle";

        public override void Start()
        {
            base.Start();

            var animation = Entity.Get<AnimationComponent>();
            if (animation != null)
                animation.Play(AnimationName);
        }
    }
}