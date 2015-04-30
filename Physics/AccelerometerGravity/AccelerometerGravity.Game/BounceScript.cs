using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;

namespace AccelerometerGravity
{
    /// <summary>
    /// This script will set the restitution of each rigidbody element to 1.0f to allow the entity to bounce
    /// </summary>
    public class BounceScript : AsyncScript
    {
        public override Task Execute()
        {
            var component = Entity.Get<PhysicsComponent>();
            foreach (var physicsElement in component.Elements)
            {
                if (physicsElement.RigidBody != null)
                {
                    physicsElement.RigidBody.Restitution = 1.0f;
                }
            }

            return Task.FromResult(0);
        }
    }
}