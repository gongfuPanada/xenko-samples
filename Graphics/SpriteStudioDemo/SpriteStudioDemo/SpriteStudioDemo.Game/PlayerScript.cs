using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering.Sprites;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpriteStudioDemo
{
    public class PlayerScript : AsyncScript
    {
        private enum AgentAnimation
        {
            Run,
            Idle,
            Shoot
        }

        // InputState represents all command inputs from a user
        private enum InputState
        {
            None,
            RunLeft,
            RunRight,
            Shoot,
        }

        // TODO centralize
        private const float gameWidthX = 16f;       // from -8f to 8f

        private const float gameWidthHalfX = gameWidthX / 2f;

        private const int AgentMoveDistance = 10;       // virtual resolution unit/second
        private const float AgentShootDelay = 0.3f;     // second

        private static readonly Dictionary<AgentAnimation, int> AnimationFps = new Dictionary<AgentAnimation, int> { { AgentAnimation.Run, 12 }, { AgentAnimation.Idle, 7 }, { AgentAnimation.Shoot, 15 } };

        private SpriteComponent agentSpriteComponent;
        private SpriteSheet spriteSheet;

        // Touch input state
        private PointerEvent pointerState;

        private bool isPointerDown; // Cache state if a user is current touching the screen.

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private bool isAgentFacingRight;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float shootDelayCounter;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private AgentAnimation currentAgentAnimation;

        private AgentAnimation CurrentAgentAnimation { get; set; }

        public override async Task Execute()
        {
            spriteSheet = Asset.Load<SpriteSheet>("SpriteSheet");
            agentSpriteComponent = Entity.Get<SpriteComponent>();
            var animComponent = Entity.Get<AnimationComponent>();
            PlayingAnimation playingAnimation = null;

            // Calculate offset of the bullet from the Agent if he is facing left and right side // TODO improve this
            var bulletOffset = new Vector3(1.3f, 1.65f, 0f);

            // Initialize game entities
            if (!IsLiveReloading)
            {
                shootDelayCounter = 0f;
                isAgentFacingRight = true;
                currentAgentAnimation = AgentAnimation.Idle;
            }
            CurrentAgentAnimation = currentAgentAnimation;

            var normalScaleX = Entity.Transform.Scale.X;

            var bulletCS = Asset.Load<PhysicsColliderShape>("Bullet_CS");

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                var inputState = GetKeyboardInputState();

                if (inputState == InputState.None)
                    inputState = GetPointerInputState();

                // Reset the shoot delay, if state changes
                if (inputState != InputState.Shoot && CurrentAgentAnimation == AgentAnimation.Shoot)
                    shootDelayCounter = 0;

                if (inputState == InputState.RunLeft || inputState == InputState.RunRight)
                {
                    // Update Agent's position
                    var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                    Entity.Transform.Position.X += ((inputState == InputState.RunRight) ? AgentMoveDistance : -AgentMoveDistance) * dt;

                    if (Entity.Transform.Position.X < -gameWidthHalfX)
                        Entity.Transform.Position.X = -gameWidthHalfX;

                    if (Entity.Transform.Position.X > gameWidthHalfX)
                        Entity.Transform.Position.X = gameWidthHalfX;

                    isAgentFacingRight = inputState == InputState.RunRight;

                    // If agent face left, flip the sprite
                    Entity.Transform.Scale.X = isAgentFacingRight ? normalScaleX : -normalScaleX;

                    // Update the sprite animation and state
                    CurrentAgentAnimation = AgentAnimation.Run;
                    if (playingAnimation == null || playingAnimation.Name != "Run")
                    {
                        playingAnimation = animComponent.Play("Run");
                    }
                }
                else if (inputState == InputState.Shoot)
                {
                    // Update shootDelayCounter, and check whether it is time to create a new bullet
                    shootDelayCounter -= (float)Game.UpdateTime.Elapsed.TotalSeconds;

                    if (shootDelayCounter > 0)
                        continue;

                    // Reset shoot delay
                    shootDelayCounter = AgentShootDelay;

                    var rb = new RigidbodyElement { CanCollideWith = CollisionFilterGroupFlags.CustomFilter1, CollisionGroup = CollisionFilterGroups.DefaultFilter };
                    rb.ColliderShapes.Add(new ColliderShapeAssetDesc { Shape = bulletCS });

                    // Spawns a new bullet
                    var bullet = new Entity
                    {
                        new SpriteComponent { SpriteProvider = new SpriteFromSheet {Sheet = spriteSheet}, CurrentFrame = spriteSheet.FindImageIndex("bullet") },
                        new PhysicsComponent { Elements = { rb } },
                        new ScriptComponent { Scripts = { new BeamScript() }}
                    };
                    bullet.Name = "bullet";

                    bullet.Transform.Position = (isAgentFacingRight) ? Entity.Transform.Position + bulletOffset : Entity.Transform.Position + (bulletOffset * new Vector3(-1, 1, 1));
                    bullet.Transform.UpdateWorldMatrix();

                    SceneSystem.SceneInstance.Scene.Entities.Add(bullet);

                    rb.RigidBody.LinearFactor = new Vector3(1, 0, 0);
                    rb.RigidBody.AngularFactor = new Vector3(0, 0, 0);
                    rb.RigidBody.ApplyImpulse(isAgentFacingRight ? new Vector3(25, 0, 0) : new Vector3(-25, 0, 0));

                    // Start animation for shooting
                    CurrentAgentAnimation = AgentAnimation.Shoot;
                    if (playingAnimation == null || playingAnimation.Name != "Attack")
                    {
                        playingAnimation = animComponent.Play("Attack");
                    }
                }
                else
                {
                    CurrentAgentAnimation = AgentAnimation.Idle;
                    if (playingAnimation == null || playingAnimation.Name != "Stance")
                    {
                        playingAnimation = animComponent.Play("Stance");
                    }
                }
            }
        }

        /// <summary>
        /// Determine input from a user from a keyboard.
        /// Left and Right arrow for running to left and right direction, Space for shooting.
        /// </summary>
        /// <returns></returns>
        private InputState GetKeyboardInputState()
        {
            if (Input.IsKeyDown(Keys.Right))
                return InputState.RunRight;
            if (Input.IsKeyDown(Keys.Left))
                return InputState.RunLeft;

            return Input.IsKeyDown(Keys.Space) ? InputState.Shoot : InputState.None;
        }

        /// <summary>
        /// Determine input from a user from Pointer (Touch/Mouse).
        /// It analyses the input from a user, and transform it to InputState using in the game, which is then returned.
        /// </summary>
        /// <returns></returns>
        private InputState GetPointerInputState()
        {
            // Get new state of Pointer (Touch input)
            if (Input.PointerEvents.Any())
            {
                var lastPointer = Input.PointerEvents.Last();
                isPointerDown = lastPointer.State != PointerState.Up;
                pointerState = lastPointer;
            }

            // If a user does not touch the screen, there is not input
            if (!isPointerDown)
                return InputState.None;

            // Transform pointer's position from normorlize coordinate to virtual resolution coordinate
            var resolution = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            var virtualCoordinatePointerPosition = resolution * pointerState.Position;

            // Get current position of the agent, since the origin of the sprite is at the center, region needs to be shifted to top-left
            var agentSize = spriteSheet["idle0"].SizeInPixels;
            var agentSpriteRegion = new RectangleF
            {
                X = (int)VirtualCoordToPixel(Entity.Transform.Position.X) - agentSize.X / 2,
                Y = (int)VirtualCoordToPixel(Entity.Transform.Position.Y) - agentSize.Y / 2,
                Width = agentSize.X,
                Height = agentSize.Y
            };

            // Check if the touch position is in the x-axis region of the agent's sprite; if so, input is shoot
            if (agentSpriteRegion.Left <= virtualCoordinatePointerPosition.X && virtualCoordinatePointerPosition.X <= agentSpriteRegion.Right)
                return InputState.Shoot;

            // Check if a pointer falls left or right of the screen, which would correspond to Run to the left or right respectively
            return ((pointerState.Position.X) <= agentSpriteRegion.Center.X / resolution.X) ? InputState.RunLeft : InputState.RunRight;
        }

        private float VirtualCoordToPixel(float virtualCoord)
        {
            return (virtualCoord + (gameWidthHalfX)) / gameWidthX * GraphicsDevice.BackBuffer.Width;
        }
    }
}