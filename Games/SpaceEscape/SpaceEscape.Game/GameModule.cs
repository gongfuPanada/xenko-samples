using System;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SpaceEscape.LevelSystem;

namespace SpaceEscape
{
    /// <summary>
    /// GameModule manages all entities in the game: Camera, Agent, Level and Obstacles.
    /// </summary>
    public class GameModule : ScriptContext, IScript
    {
        public event Action AgentDied;
        public event Action<float> DistanceUpdated
        {
            add { level.DistanceUpdated += value; }
            remove { level.DistanceUpdated -= value; }
        }

        private readonly Agent agent;
        private readonly Level level;

        public GameModule(IServiceRegistry registry) : base(registry)
        {
            agent = new Agent(Services);
            level = new Level(Services);

            // Adjust the color of fog effect.
            GraphicsDevice.Parameters.Set(FogEffectKeys.FogColor, Color.FromAbgr(0xFF7D02FF));

            // create the camera
            var cameraEntity = new Entity("Camera")
            {
                new CameraComponent(null, 100, 34000)
                {
                    UseViewMatrix = true,
                    AspectRatio = (float) GraphicsDevice.BackBuffer.Width/GraphicsDevice.BackBuffer.Height,
                    ViewMatrix =
                        Matrix.LookAtRH(new Vector3(0f, 1200f, -1200f), new Vector3(0f, 450f, 0f), Vector3.UnitY)
                }
            };
            cameraEntity.Add(cameraEntity.Get<CameraComponent>());

            // Setup the camera for the rendering pipeline
            RenderSystem.Pipeline.SetCamera(cameraEntity.Get<CameraComponent>());

            // Add camera to the scene.
            Entities.Add(cameraEntity);
        }

        /// <summary>
        /// Load resources in the game.
        /// </summary>
        /// <returns> Task </returns>
        public void LoadContent()
        {
            agent.LoadContent();
            level.LoadContent();

            // Execute scripts: Agent and Level.
            Script.Add(agent);
            Script.Add(level);
        }

        /// <summary>
        /// Script update loop that detect collision between agent an obstacles, 
        /// and detect if the agent falls to any hole.
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (agent.IsDead)
                    continue;

                float floorHeight;
                var agentTransformation = agent.ModelEntity.Transformation;
                var agentWorldPosition = agentTransformation.WorldMatrix.TranslationVector;

                // Calculate the agent bounding box
                var minVec = agentWorldPosition + agent.ActiveBoundingBox.Minimum;
                var maxVec = agentWorldPosition + agent.ActiveBoundingBox.Maximum;
                var agentBoundingBox = new BoundingBox(minVec, maxVec);

                // Detect collision between agents and real-world obstacles.
                if (level.DetectCollisions(ref agentBoundingBox))
                    KillAgent(0);

                // Detect if the agent falls into a hole
                if (level.DetectHoles(ref agentWorldPosition, out floorHeight))
                    KillAgent(floorHeight);
            }
        }
        private void KillAgent(float height)
        {
            agent.OnDied(height);
            if (AgentDied != null)
                AgentDied();
        }

        /// <summary>
        /// Reset game's entities: Agent and LevelBlocks.
        /// </summary>
        public void Reset()
        {
            agent.Reset();
            level.Reset();
        }

        public void StartPlayMode()
        {
            Reset();
            level.IsUpdating = true;
            agent.InputEnabled = true;
        }

        public void StartGameOverMode()
        {
            level.IsUpdating = false;
        }
    }
}
