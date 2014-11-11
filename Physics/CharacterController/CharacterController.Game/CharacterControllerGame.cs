using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;

namespace CharacterController
{
    public class CharacterControllerGame : Game
    {
        private IPhysicsSystem physicsSystem;

        public CharacterControllerGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        }

        private Character playerController;
        private TransformationComponent playerTransformationComponent;
        private SpriteComponent playerSprite;
        private SpriteGroup playerIdle, playerRun;

        void PlayIdle()
        {
            playerSprite.SpriteGroup = playerIdle;
            SpriteAnimation.Play(playerSprite, 0, playerSprite.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, 7);
        }

        void PlayRun()
        {
            playerSprite.SpriteGroup = playerRun;
            SpriteAnimation.Play(playerSprite, 0, playerSprite.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, 12);
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            //physics is a plug-in now, needs explicit initialization
            physicsSystem = new Bullet2PhysicsSystem(this);
            physicsSystem.PhysicsEngine.Initialize();

            //sprites need a tweaked gravity
            physicsSystem.PhysicsEngine.Gravity = new Vector3(0, -1000, 0);

            VirtualResolution = new Vector3(1280, 720, 1);

            IsMouseVisible = true;

            // Load ground
            var ground = Asset.Load<Entity>("ground");
            ground.Transformation.Translation = new Vector3(0, VirtualResolution.Y, 0);
            Entities.Add(ground);

            // Load character
            var player = Asset.Load<Entity>("player");
            playerTransformationComponent = player.GetOrCreate<TransformationComponent>();
            player.Transformation.Translation = new Vector3(200, VirtualResolution.Y / 2.0f, 0);
            Entities.Add(player);
            // Load animations
            playerIdle = Asset.Load<SpriteGroup>("player_idle");
            playerRun = Asset.Load<SpriteGroup>("player_run");
            // Start idle
            playerSprite = player.GetOrCreate<SpriteComponent>();
            PlayIdle();

            // Load enemy
            var enemy = Asset.Load<Entity>("enemy");
            enemy.Transformation.Translation = new Vector3(VirtualResolution.X / 2.0f, VirtualResolution.Y / 2.0f, 0);
            Entities.Add(enemy);
            // Animate
            var enemySprite = enemy.GetOrCreate<SpriteComponent>();
            SpriteAnimation.Play(enemySprite, 0, enemySprite.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, 2);

            // Setup player controller
            playerController = player.GetOrCreate<PhysicsComponent>()[0].Character;
            playerController.Gravity = -1000.0f;
            playerController.FallSpeed = 1000.0f;
            playerController.JumpSpeed = 500.0f;
            playerController.MaxSlope*= 1.0f;

            playerController.OnFirstContactBegin += playerController_OnFirstContactBegin;

            // Add a custom script
            Script.Add(GameScript1);
        }   

        private void CreatePipeline()
        {
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new SpriteRenderer(Services));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }

        [Flags]
        enum PlayerState
        {
            Idle    = 0x0,
            Run     = 0x1,
            Jump    = 0x2
        }

        PlayerState oldState = PlayerState.Idle;
        Vector3 oldDirection = Vector3.Zero;
        bool movingToTarget;
        Vector3 autoPilotTarget = Vector3.Zero;

        void playerController_OnFirstContactBegin(object sender, CollisionArgs e)
        {
            // Stop if we collide from sides
            if (e.Contact.Normal.X < -0.5f || e.Contact.Normal.X > 0.5f)
            {
                movingToTarget = false;
            }
        }

        private async Task GameScript1()
        {
            const float speed = 5.0f;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                var playerState = PlayerState.Idle;
                var playerDirection = Vector3.Zero;

                // -- Keyboard Inputs

                // Space bar = jump
                if (Input.IsKeyDown(Keys.Space))
                {
                    playerState |= PlayerState.Jump;
                }

                // Left - right = run
                if (Input.IsKeyDown(Keys.Right))
                {
                    movingToTarget = false;
                    playerState |= PlayerState.Run;
                    playerDirection = Vector3.UnitX * speed;
                }
                else if (Input.IsKeyDown(Keys.Left))
                {
                    movingToTarget = false;
                    playerState |= PlayerState.Run;
                    playerDirection = -Vector3.UnitX * speed;
                }

                // -- Pointer (mouse/touch)
                foreach (var pointerEvent in Input.PointerEvents.Where(pointerEvent => pointerEvent.State == PointerState.Down))
                {
                    if (!movingToTarget)
                    {
                        autoPilotTarget = new Vector3(new Vector2(VirtualResolution.X, VirtualResolution.Y)*pointerEvent.Position, 0)
                        {
                            //ignore Y
                            Y = 0
                        };

                        movingToTarget = true;
                    }
                    else
                    {
                        playerState |= PlayerState.Jump;
                    }
                }

                // -- Logic

                // are we autopiloting?
                if (movingToTarget)
                {
                    var direction = autoPilotTarget - playerTransformationComponent.Translation;
                    direction.Y = 0;

                    //should we stop?
                    var length = direction.Length();
                    //stop when we are 5 pixels close
                    if (length < 5.0 && length > -5.0)
                    {
                        movingToTarget = false;

                        playerDirection = Vector3.Zero;
                        
                        playerState = PlayerState.Idle;
                    }
                    else
                    {
                        direction.Normalize();

                        playerDirection = (direction.X > 0 ? Vector3.UnitX : -Vector3.UnitX) * speed;
                        
                        playerState |= PlayerState.Run;
                    }
                }

                // did we start jumping?
                if (playerState.HasFlag(PlayerState.Jump) && !oldState.HasFlag(PlayerState.Jump))
                {
                    playerController.Jump();
                }

                // did we just land?
                if (oldState.HasFlag(PlayerState.Jump))
                {
                    if (!playerController.IsGrounded)
                    {
                        //force set jump flag
                        if (!playerState.HasFlag(PlayerState.Jump))
                        {
                            playerState |= PlayerState.Jump;
                            // Mantain motion 
                            playerDirection = oldDirection;
                        }
                    }
                    else if (playerController.IsGrounded)
                    {
                        //force clear jump flag
                        if (playerState.HasFlag(PlayerState.Jump))
                        {
                            playerState ^= PlayerState.Jump;
                        }
                    }
                }

                // did we start running?
                if (playerState.HasFlag(PlayerState.Run) && !oldState.HasFlag(PlayerState.Run))
                {
                    PlayRun();
                }
                // did we stop running?
                else if (!playerState.HasFlag(PlayerState.Run) && oldState.HasFlag(PlayerState.Run))
                {
                    PlayIdle();
                }

                // movement logic
                if (oldDirection != playerDirection)
                {
                    playerController.Move(playerDirection);

                    if (playerState.HasFlag(PlayerState.Run))
                    {
                        playerSprite.SpriteEffect = playerDirection.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    }
                }

                // Store current state for next frame
                oldState = playerState;
                oldDirection = playerDirection;
            }
        }
    }
}
