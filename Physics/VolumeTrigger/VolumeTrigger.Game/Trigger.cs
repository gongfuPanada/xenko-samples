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

            //start out state machine
            while (Game.IsRunning)
            {
                //wait for entities coming in
                await trigger.FirstCollision();

                SimpleMessage.OnStart();

                //now wait for entities exiting
                await trigger.AllCollisionsEnded();

                SimpleMessage.OnStop();
            }
        }
    }
}