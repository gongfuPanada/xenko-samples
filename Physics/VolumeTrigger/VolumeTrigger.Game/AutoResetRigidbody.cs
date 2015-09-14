using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace VolumeTrigger
{
    public class AutoResetRigidbody : AsyncScript
    {
        private Vector3 startLocation;
        private Quaternion startRotation;

        private RigidBody rigidBody;

        public override Task Execute()
        {
            startLocation = Entity.Transform.Position;
            startRotation = Entity.Transform.Rotation;

            //grab a reference to the falling sphere's rigidbody
            rigidBody = Entity.Get<PhysicsComponent>()[0].RigidBody;

            SimpleMessage.Start += SimpleMessage_Start;
            SimpleMessage.Stop += SimpleMessage_Stop;

            return Task.FromResult(0);
        }

        private void SimpleMessage_Stop(object sender, EventArgs e)
        {
            //when out revert to kinematic and old starting position
            rigidBody.Type = RigidBodyTypes.Kinematic;
            //reset position
            Entity.Transform.Position = startLocation;
            Entity.Transform.Rotation = startRotation;
            Entity.Transform.UpdateWorldMatrix();
            Entity.Get<PhysicsComponent>()[0].UpdatePhysicsTransformation();
        }

        private void SimpleMessage_Start(object sender, EventArgs e)
        {
            //switch to dynamic and awake the rigid body
            rigidBody.Type = RigidBodyTypes.Dynamic;
            rigidBody.Activate(true); //need to awake to object
        }
    }
}
