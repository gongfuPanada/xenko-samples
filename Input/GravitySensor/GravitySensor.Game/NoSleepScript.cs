using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;

namespace GravitySensor
{
    /// <summary>
    /// This script will make sure that all the physics elements of this entity will never be set to sleep status
    /// The physics engine will sometimes set colliders to sleep state to reduce processor usage when there is no motion happening
    /// Those colliders will wake up if an external (an other collider hitting us) collision happens, but in this case we need to prevent this behavior totally,
    /// as there will be no external collision once the motion is 0.
    /// </summary>
    public class NoSleepScript : AsyncScript
    {
        public override Task Execute()
        {
            var component = Entity.Get<PhysicsComponent>();
            foreach (var physicsElement in component.Elements)
            {
                if (physicsElement.RigidBody != null)
                {
                    physicsElement.RigidBody.CanSleep = false;
                }
            }

            return Task.FromResult(0);
        }
    }
}