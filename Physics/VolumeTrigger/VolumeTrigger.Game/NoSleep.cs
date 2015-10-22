using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;

namespace VolumeTrigger
{
    public class NoSleep : AsyncScript
    {
        public override Task Execute()
        {
            var pc = Entity.Get<PhysicsComponent>();
            foreach (var element in pc.Elements)
            {
                if (element.Collider != null)
                {
                    element.Collider.CanSleep = false;
                }
            }
            return Task.FromResult(0);
        }
    }
}
