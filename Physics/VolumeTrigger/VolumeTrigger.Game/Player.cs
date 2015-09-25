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
        private const float speed = 0.25f;
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

        private Vector3 pointerVector;

        public override void Update()
        {
            var move = new Vector3();

            if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            {
                move = -Vector3.UnitX;
            }
            if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            {
                move = Vector3.UnitX;
            }

            if (Input.PointerEvents.Any())
            {
                var last = Input.PointerEvents.Last();
                if (last != null)
                {
                    switch (last.State)
                    {
                        case PointerState.Down:
                            if (last.Position.X < 0.5)
                            {
                                pointerVector = -Vector3.UnitX;
                            }
                            else
                            {
                                pointerVector = Vector3.UnitX;
                            }
                            break;
                        case PointerState.Up:
                        case PointerState.Out:
                        case PointerState.Cancel:
                            pointerVector = Vector3.Zero;
                            break;
                    }
                }
            }

            if (pointerVector != Vector3.Zero)
            {
                move = pointerVector;
            }

            move *= speed;

            character.Move(move);
        }
    }
}
