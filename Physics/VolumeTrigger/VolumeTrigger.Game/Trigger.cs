using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using System.Threading.Tasks;

namespace VolumeTrigger
{
    public class Trigger : AsyncScript
    {
        public override async Task Execute()
        {
            var trigger = Entity.Get<PhysicsComponent>();
            trigger.ProcessCollisions = true;

            //start out state machine
            while (Game.IsRunning)
            {
                //wait for entities coming in
                var firstCollision = await trigger.NewCollision();

                SimpleMessage.OnStart();

                //now wait for entities exiting
                Collision collision;
                do
                {
                    collision = await trigger.CollisionEnded();
                } while (collision != firstCollision);
               
                SimpleMessage.OnStop();
            }
        }
    }
}