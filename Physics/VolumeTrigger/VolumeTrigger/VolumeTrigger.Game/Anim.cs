using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;

namespace VolumeTrigger
{
    public class Anim : SyncScript
    {
        public override void Start()
        {
            Entity.Get<AnimationComponent>().Play("Basic");
        }

        public override void Update()
        {
            
        }
    }
}
