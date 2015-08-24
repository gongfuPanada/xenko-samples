using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;
using System.Threading.Tasks;

namespace VolumeTrigger
{
    public class Trigger : AsyncScript
    {
        public Entity TriggeredEntity;
        public Entity TestBox;
        public Vector3 StartLocation;

        public override async Task Execute()
        {
            //grab a reference to the falling sphere's rigidbody
            var rb = TriggeredEntity.Get<PhysicsComponent>()[0].RigidBody;

            //Make sure out trigger reports collisions
            var trigger = Entity.Get<PhysicsComponent>()[0].Collider;
            trigger.ContactsAlwaysValid = true;

            //start out state machine
            while (Game.IsRunning)
            {
                //wait for entities coming in
                await trigger.FirstCollision();

                //switch to dynamic and awake the rigid body
                rb.Type = RigidBodyTypes.Dynamic;
                rb.Activate(true); //need to awake to object

                //now wait for entities exiting
                await trigger.AllCollisionsEnded();

                //when out revert to kinematic and old starting position
                rb.Type = RigidBodyTypes.Kinematic;
                //reset position
                TriggeredEntity.Transform.Position = StartLocation;
                TriggeredEntity.Transform.Rotation = Quaternion.Identity;
                TriggeredEntity.Transform.UpdateWorldMatrix();
                TriggeredEntity.Get<PhysicsComponent>()[0].UpdatePhysicsTransformation();
            }
        }
    }
}