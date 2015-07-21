using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace CharacterControllerSample
{
    /// <summary>
    /// This script will interface the Physics character controller of the entity to move the character around
    /// Will also animate the sprite of the entity, between run and idle.
    /// </summary>
    public class CharacterScript : SyncScript
    {
        [Flags]
        enum PlayerState
        {
            Idle = 0x0,
            Run = 0x1,
            Jump = 0x2
        }

        private bool movingToTarget;
        private SpriteSheet idleGroup;
        private SpriteSheet runGroup;
        private SpriteComponent playerSprite;

        private const float speed = 0.05f;
        private PlayerState oldState = PlayerState.Idle;
        private Vector3 oldDirection = Vector3.Zero;
        private Vector3 autoPilotTarget = Vector3.Zero;

        private Character playerController;

        void PlayIdle()
        {
            var provider = (SpriteFromSheet) playerSprite.SpriteProvider;
            provider.Sheet = idleGroup;
            SpriteAnimation.Play(playerSprite, 0, provider.Sheet.Sprites.Count - 1, AnimationRepeatMode.LoopInfinite, 7);
        }

        void PlayRun()
        {
            var provider = (SpriteFromSheet)playerSprite.SpriteProvider;
            provider.Sheet = runGroup;
            SpriteAnimation.Play(playerSprite, 0, provider.Sheet.Sprites.Count - 1, AnimationRepeatMode.LoopInfinite, 12);
        }

        void playerController_OnFirstContactBegin(object sender, CollisionArgs e)
        {
            // Stop if we collide from sides
            if (e.Contact.Normal.X < -0.5f || e.Contact.Normal.X > 0.5f)
            {
                movingToTarget = false;
            }
        }

        public override void Start()
        {
            playerController = Entity.Get<PhysicsComponent>().Elements[0].Character;

            //Please remember that in the GameStudio element the parameter Step Height is extremely important, it not set properly it will cause the entity to snap fast to the ground
            playerController.JumpSpeed = 5.0f;
            playerController.Gravity = -10.0f;
            playerController.FallSpeed = 10.0f;

            playerController.FirstContactStart += playerController_OnFirstContactBegin;

            idleGroup = Asset.Load<SpriteSheet>("player_idle");
            runGroup = Asset.Load<SpriteSheet>("player_run");
            playerSprite = Entity.Get<SpriteComponent>();
            PlayIdle();
        }

        public override void Cancel()
        {
            playerController.FirstContactStart -= playerController_OnFirstContactBegin;

            // Unload graphic resources.
            Asset.Unload(idleGroup);
            Asset.Unload(runGroup);
        }

        public override void Update()
        {
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
                    var screenX = (pointerEvent.Position.X - 0.5f) * 2.0f;
                    screenX *= 8.75f;

                    autoPilotTarget = new Vector3(screenX, 0, 0);

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
                var direction = autoPilotTarget - Entity.Transform.Position;
                direction.Y = 0;

                //should we stop?
                var length = direction.Length();
                //stop when we are 5 pixels close
                if (length < 0.05f && length > -0.05f)
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
                    if ((playerDirection.X > 0 && Entity.Transform.Scale.X < 0) ||
                        (playerDirection.X < 0 && Entity.Transform.Scale.X > 0))
                    {
                        Entity.Transform.Scale.X *= -1.0f;
                    }
                }
            }

            // Store current state for next frame
            oldState = playerState;
            oldDirection = playerDirection;
        }
    }
}