using SiliconStudio.Xenko.Engine;

namespace GravitySensor
{
    /// <summary>
    /// This script will set the restitution of each rigidbody element to 1.0f to allow the entity to bounce
    /// </summary>
    public class BounceScript : StartupScript
    {
        public override void Start()
        {
            var component = Entity.Get<PhysicsComponent>();
            foreach (var physicsElement in component.Elements)
            {
                physicsElement.Collider.Restitution = 0.9f;
            }
        }
    }
}