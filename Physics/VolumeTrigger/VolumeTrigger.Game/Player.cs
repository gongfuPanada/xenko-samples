using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering;

namespace VolumeTrigger
{
    public class Player : SyncScript
    {
        private const float speed = 0.5f;
        private Character character;

        public override void Start()
        {
            character = Entity.Get<PhysicsComponent>()[0].Character;
            character.Gravity = -10.0f;
            if (Entity.Get<PhysicsComponent>().Count > 1)
            {
                Entity.Get<PhysicsComponent>()[1].RigidBody.CanSleep = false;
            }
        }

        public override void Update()
        {
            var move = new Vector3();
            if (Input.IsKeyDown(Keys.W))
            {
                move -= Vector3.UnitZ;
            }
            if (Input.IsKeyDown(Keys.A))
            {
                move -= Vector3.UnitX;
            }
            if (Input.IsKeyDown(Keys.S))
            {
                move = Vector3.UnitZ * 0.25f;
            }
            if (Input.IsKeyDown(Keys.D))
            {
                move = Vector3.UnitX;
            }

            move *= speed;

            character.Move(move);
        }
    }
}
