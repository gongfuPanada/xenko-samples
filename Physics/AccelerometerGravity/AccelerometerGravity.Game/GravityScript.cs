using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace AccelerometerGravity
{
    /// <summary>
    /// This script will handle keyboard inputs and set the scene gravity according to those inputs
    /// </summary>
    public class GravityScript : SyncScript
    {
        private Simulation simulation;

        public override void Start()
        {
            simulation = Entity.Get<PhysicsComponent>().Simulation;
            simulation.Gravity = new Vector3(0, 0, 0);
        }

        public override void Update()
        {
            // no keys down and default gravity
            var gravity = new Vector3(0, 0, 0);

            if (Input.IsKeyDown(Keys.Up))
            {
                gravity += new Vector3(0, 10, 0.0f);
            }
            if (Input.IsKeyDown(Keys.Left))
            {
                gravity += new Vector3(-10, 0, 0.0f);
            }
            if (Input.IsKeyDown(Keys.Down))
            {
                gravity += new Vector3(0, -10, 0.0f);
            }
            if (Input.IsKeyDown(Keys.Right))
            {
                gravity += new Vector3(10, 0, 0.0f);
            }

            simulation.Gravity = gravity;
        }
    }
}