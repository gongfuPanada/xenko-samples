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
            foreach (var element in Entity.GetAll<PhysicsComponent>())
            {
                if (element != null)
                {
                    element.CanSleep = false;
                }
            }
            return Task.FromResult(0);
        }
    }
}
