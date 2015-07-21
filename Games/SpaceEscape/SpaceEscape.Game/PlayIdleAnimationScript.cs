using SiliconStudio.Paradox.Engine;

namespace SpaceEscape
{
    /// <summary>
    /// Plays the idle animation of the entity if any
    /// </summary>
    public class PlayIdleAnimationScript : StartupScript
    {
        public const string AnimationName = "Idle";

        public override void Start()
        {
            var animation = Entity.Get<AnimationComponent>();
            if (animation != null)
                animation.Play(AnimationName);
        }
    }
}