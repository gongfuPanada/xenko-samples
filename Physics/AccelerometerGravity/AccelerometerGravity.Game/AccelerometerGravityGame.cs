using System;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace AccelerometerGravity
{
    public class AccelerometerGravityGame : Game
    {
        private readonly bool simulateGravity = Platform.Type == PlatformType.Windows;
        private TextBlock textBlock;

        private IPhysicsSystem physicsSystem;

        public AccelerometerGravityGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            //physics is a plug-in now, needs explicit initialization
            physicsSystem = new Bullet2PhysicsSystem(this);
            physicsSystem.PhysicsEngine.Initialize();

            //Set a fixed resolution
            VirtualResolution = new Vector3(1280, 720, 1);

            var ground0 = Asset.Load<Entity>("screen_limit_1");
            ground0.Transformation.Translation = new Vector3(0, VirtualResolution.Y, 0); // DOWN
            var ground1 = Asset.Load<Entity>("screen_limit_2");
            ground1.Transformation.Translation = new Vector3(0, 0, 0); // LEFT
            var ground2 = Asset.Load<Entity>("screen_limit_3");
            ground2.Transformation.Translation = new Vector3(0, 0, 0); // UP
            var ground3 = Asset.Load<Entity>("screen_limit_4");
            ground3.Transformation.Translation = new Vector3(VirtualResolution.X, 0, 0); // RIGHT

            Entities.Add(ground0);
            Entities.Add(ground1);
            Entities.Add(ground2);
            Entities.Add(ground3);

            //this is to allow objects colliding this collider to "bounce"
            ground0.GetOrCreate<PhysicsComponent>()[0].Collider.Restitution = 1.0f;
            ground1.GetOrCreate<PhysicsComponent>()[0].Collider.Restitution = 1.0f;
            ground2.GetOrCreate<PhysicsComponent>()[0].Collider.Restitution = 1.0f;
            ground3.GetOrCreate<PhysicsComponent>()[0].Collider.Restitution = 1.0f;
            
            var ball0 = Asset.Load<Entity>("ball");
            ball0.Transformation.Translation = new Vector3((VirtualResolution.X / 2.0f) + 100, VirtualResolution.Y / 2.0f, 0.0f);

            var ball1 = ball0.Clone();
            ball1.Transformation.Translation = new Vector3((VirtualResolution.X / 2.0f) + -100, VirtualResolution.Y / 2.0f, 0.0f);

            Entities.Add(ball0);
            //after the entities are added we can manipulate the physics further
            var ball0RigidBody = ball0.GetOrCreate<PhysicsComponent>()[0].RigidBody;
            ball0RigidBody.CanSleep = false; //disable sleeping so to never get stuck in case of no motion
            ball0RigidBody.Restitution = 1.0f;
            //also turn on sprite animation
            var ball0Sprite = ball0.GetOrCreate<SpriteComponent>();
            SpriteAnimation.Play(ball0Sprite, 0, ball0Sprite.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, 2);
            

            Entities.Add(ball1);
            //after the entities are added we can manipulate the physics further          
            var ball1RigidBody = ball1.GetOrCreate<PhysicsComponent>()[0].RigidBody;
            ball1RigidBody.CanSleep = false; //disable sleeping so to never get stuck in case of no motion
            ball1RigidBody.Restitution = 1.0f;
            //also turn on sprite animation
            var ball1Sprite = ball1.GetOrCreate<SpriteComponent>();
            SpriteAnimation.Play(ball1Sprite, 0, ball1Sprite.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, 2);

            //Set gravity to zero as default
            physicsSystem.PhysicsEngine.Gravity = new Vector3(0, 0, 0);

            if (simulateGravity)
            {
                textBlock = new TextBlock { Text = "Use arrows to play with gravity!", Font = Asset.Load<SpriteFont>("Font"), TextColor = Color.White, TextSize = 40 };
                textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
                textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.9f, 0f));
                UI.RootElement = new Canvas { Children = { textBlock } };
            }

            Script.Add(InputsRoutine);
        }

        private void CreatePipeline()
        {
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new SpriteRenderer(Services));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }

        public void SetAccelerometerVector(Vector3 readings)
        {
            if (physicsSystem != null && physicsSystem.PhysicsEngine.Initialized)
            {
                physicsSystem.PhysicsEngine.Gravity = new Vector3(readings.X, readings.Y, 0.0f) * 100.0f;
            }
        }

        private async Task InputsRoutine()
        {
            var hasTouchedArrows = false;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                if (simulateGravity)
                {
                    // no keys down and default gravity
                    var gravity = new Vector3(0, 0, 0);

                    if (Input.IsKeyDown(Keys.Up))
                    {
                        gravity += new Vector3(0, 500.0f, 0.0f);
                        hasTouchedArrows = true;
                    }
                    if (Input.IsKeyDown(Keys.Left))
                    {
                        gravity += new Vector3(-500, 0, 0.0f);
                        hasTouchedArrows = true;
                    }
                    if (Input.IsKeyDown(Keys.Down))
                    {
                        gravity += new Vector3(0, -500, 0.0f);
                        hasTouchedArrows = true;
                    }
                    if (Input.IsKeyDown(Keys.Right))
                    {
                        gravity += new Vector3(500, 0, 0.0f);
                        hasTouchedArrows = true;
                    }

                    physicsSystem.PhysicsEngine.Gravity = gravity;

                    if (hasTouchedArrows)
                        textBlock.TextColor *= 0.965f;
                }
            }
        }
    }
}
