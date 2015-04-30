using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SpriteEntity
{
    public class PlayerScript : AsyncScript
    {
        public LogicScript Logic;

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

        private AgentAnimation currentAgentAnimation;
        private SpriteComponent agentSpriteComponent;
        private RectangleF agentSpriteRegion;
        private SpriteGroup agentIdle;
        private SpriteGroup agentRun;
        private SpriteGroup agentShoot;

        // TODO centralize 
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;

        // Touch input state
        private PointerEvent pointerState;
        private bool isPointerDown; // Cache state if a user is current touching the screen.

        private static readonly Dictionary<AgentAnimation, int> AnimationFps = new Dictionary<AgentAnimation, int>
        {
            {AgentAnimation.Run, 12}, {AgentAnimation.Idle, 7}, {AgentAnimation.Shoot, 15}
        };

        private const int AgentMoveDistance = 10;       // virtual resolution unit/second
        private const float AgentShootDelay = 0.3f;     // second

        private AgentAnimation CurrentAgentAnimation
        {
            get
            {
                return currentAgentAnimation;
            }
            set
            {
                if (currentAgentAnimation == value)
                    return;

                currentAgentAnimation = value;
                SpriteAnimation.Play(agentSpriteComponent, 0, agentSpriteComponent.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, AnimationFps[currentAgentAnimation]);
            }
        }

        public override async Task Execute()
        {
            agentIdle = Asset.Load<SpriteGroup>("character_idle");
            agentRun = Asset.Load<SpriteGroup>("character_run");
            agentShoot = Asset.Load<SpriteGroup>("character_shoot");
            agentSpriteRegion = agentIdle.Images.First().Region;
            var bulletSpriteGroup = Asset.Load<SpriteGroup>("bullet");

            agentSpriteComponent = Entity.Get<SpriteComponent>();

            // Calculate offset of the bullet from the Agent if he is facing left and right side
            // TODO fix this
            var bulletOffset = new Vector3(1f, 0.2f, 0f); //new Vector3(agentSpriteRegion.Width * 0.5f, -14f, 0);

            // Initialize game entities
            CurrentAgentAnimation = AgentAnimation.Idle;

            var isAgentFacingRight = true;
            var shootDelayCounter = 0f;

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
                    var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds ;

                    Entity.Transform.Position.X += ((inputState == InputState.RunRight) ? AgentMoveDistance : -AgentMoveDistance) * dt ;

                    if (Entity.Transform.Position.X < -gameWidthHalfX)
                        Entity.Transform.Position.X = -gameWidthHalfX;

                    if (Entity.Transform.Position.X > gameWidthHalfX)
                        Entity.Transform.Position.X = gameWidthHalfX;

                    isAgentFacingRight = inputState == InputState.RunRight;

                    // If agent face left, flip the sprite
                    Entity.Transform.Scale.X = isAgentFacingRight ? 2f : -2f;

                    // Update the sprite animation and state
                    agentSpriteComponent.SpriteProvider = new SpriteFromSpriteGroup{SpriteGroup = agentRun};
                    CurrentAgentAnimation = AgentAnimation.Run;
                }
                else if (inputState == InputState.Shoot)
                {
                    // Update shootDelayCounter, and check whether it is time to create a new bullet
                    shootDelayCounter -= (float)Game.UpdateTime.Elapsed.TotalSeconds;

                    if (shootDelayCounter > 0)
                        continue;


                    // Reset shoot delay
                    shootDelayCounter = AgentShootDelay;

                    // Spawns a new bullet
                    var bullet = new Entity
                    {
                        new SpriteComponent { SpriteProvider = new SpriteFromSpriteGroup { SpriteGroup = bulletSpriteGroup } },

                        // Will make the beam move along a direction at each frame
                        new ScriptComponent { Scripts = { new BeamScript { DirectionX = isAgentFacingRight? 1f : -1f } } }
                    };

                    bullet.Transform.Scale = new Vector3(0.1f, 0.1f, 1.0f); // TODO fix this
                    bullet.Transform.Position = (isAgentFacingRight)
                        ? Entity.Transform.Position + bulletOffset
                        : Entity.Transform.Position + (bulletOffset * new Vector3(-1, 1, 1));

                    SceneSystem.SceneInstance.Scene.AddChild(bullet);
                    Logic.WatchBullet(bullet);
                    
                    // Start animation for shooting
                    agentSpriteComponent.SpriteProvider = new SpriteFromSpriteGroup { SpriteGroup = agentShoot };
                    CurrentAgentAnimation = AgentAnimation.Shoot; 
                }
                else
                {
                    agentSpriteComponent.SpriteProvider = new SpriteFromSpriteGroup { SpriteGroup = agentIdle };
                    CurrentAgentAnimation = AgentAnimation.Idle;
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
            var virtualCoordinatePointerPosition = resolution * (pointerState.Position - new Vector2(0.0f));

            // Get current position of the agent, since the origin of the sprite is at the center, region needs to be shifted to top-left
            agentSpriteRegion.X = (int)VirtualCoordToPixel(Entity.Transform.Position.X) - agentSpriteRegion.Width / 2;
            agentSpriteRegion.Y = (int)VirtualCoordToPixel(Entity.Transform.Position.Y) - agentSpriteRegion.Height / 2;

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
