using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;

namespace Raycasting
{
    /// <summary>
    /// Apply an impulse on the entity when pressing key 'Space'
    /// </summary>
    public class ImpulseOnSpaceScript : SyncScript
    {
        public override void Update()
        {
            if (Input.IsKeyDown(Keys.Space))
            {
                var rigidBody = Entity.Get<PhysicsComponent>()[0].RigidBody;

                rigidBody.Activate();
                rigidBody.ApplyImpulse(new Vector3(0, 1, 0));
            }
        }
    }
}