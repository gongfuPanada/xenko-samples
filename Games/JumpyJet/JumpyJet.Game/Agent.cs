using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Input;

namespace JumpyJet
{
    /// <summary>
    /// Agent is controlled by a user.
    /// The control is as follow, tapping a screen/clicking a mouse will make the agent jump up.
    /// </summary>
    public class Agent
    {
        private static readonly Vector3 Gravity = new Vector3(0, 1700, 0);
        private static readonly Vector3 StartPos = new Vector3(-100, 0, 0);
        private static readonly Vector3 StartVelocity = new Vector3(0, -700, 0);

        // Collider rectangles of Agent
        private static readonly Rectangle BodyRectangle = new Rectangle(30, 30, 60, 34);
        private static readonly Rectangle HeadRectangle = new Rectangle(36, 2, 20, 20);

        private const int TopLimit = -568 + 200;
        private const int BottomLimit = 568 - 256 - 27;
        private const float NormalVelocityY = -650;
        private const float VelocityAboveTopLimit = -200;
        private const int FlyingSpriteFrameIndex = 1;
        private const int FallingSpriteFrameIndex = 0;

        public enum AgentState
        {
            Idle,
            Alive,
            Die,
        }

        public event Action DieAnimationFinished;
        public Vector3 Position;
        public Vector3 Rotation;
        public bool IsUpdating { get; set; }

        public Entity Entity { get { return agentEntity; } }
        public int AgentWidth { get; protected set; }
        public int AgentHeight { get; protected set; }
        public AgentState State { get; private set; }
        public bool IsAlive { get { return State == AgentState.Alive; } }
        public Rectangle BodyCollider { get { return bodyCollider; } }
        public Rectangle HeadCollider {get { return headCollider; }}

        private readonly InputManager inputManager;
        private readonly Entity agentEntity;
        private readonly TransformationComponent transComponent;
        private readonly SpriteComponent spriteComponent;
        private Vector3 velocity;
        private readonly Vector3 topLeftOffset;

        private Rectangle bodyCollider;
        private Rectangle headCollider;

        public Agent(Entity agentEntity, InputManager inputManager, Vector3 screenResolution)
        {
            this.inputManager = inputManager;
            this.agentEntity = agentEntity;
            spriteComponent = agentEntity.Get<SpriteComponent>();
            transComponent = agentEntity.Get<TransformationComponent>();

            // Get texture region from the sprite
            var textureRegion = spriteComponent.SpriteGroup.Images.First().Region;
            AgentWidth = textureRegion.Width;
            AgentHeight = textureRegion.Height;

            Position = StartPos;
            velocity = StartVelocity;

            bodyCollider = BodyRectangle;
            headCollider = HeadRectangle;

            topLeftOffset = new Vector3(screenResolution.X/2, screenResolution.Y/2, GameModule.AgentDepth);

            ResetAgent();
        }

        /// <summary>
        /// Reset Agent parameters: position, velocity and set state.
        /// </summary>
        /// <param name="state"></param>
        public void ResetAgent(AgentState state = AgentState.Idle)
        {
            Position.Y = 0;
            Rotation.Z = 0f;
            UpdateSpriteTransformation();

            velocity = StartVelocity;
            State = state;

            spriteComponent.CurrentFrame = FallingSpriteFrameIndex;
        }

        /// <summary>
        /// Invokes by other classes when the agent is collided.
        /// In which case, set state to die.
        /// </summary>
        public void OnCollided()
        {
            State = AgentState.Die;
        }

        /// <summary>
        /// Update the agent according to its states: {Idle, Alive, Die}
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void Update(double elapsedTime)
        {
            if (!IsUpdating)
                return;

            // Check if a sprite falling on a floor, if so, die.
            if (Position.Y > BottomLimit)
                State = AgentState.Die;

            switch (State)
            {
                case AgentState.Alive:
                    UpdateAliveState(elapsedTime, IsTouched());
                    break;
                case AgentState.Die:
                    if (DieAnimationFinished != null)
                        DieAnimationFinished();
                    break;
            }

            UpdateSpriteTransformation();
        }

        private void UpdateSpriteTransformation()
        {
            transComponent.Translation = Position + topLeftOffset;
            transComponent.RotationEulerXYZ = Rotation;
        }

        private bool IsTouched()
        {
            return inputManager.PointerEvents.Any(pointerEvent => pointerEvent.State == PointerState.Down);
        }

        private void UpdateAliveState(double elapsedTime, bool isTap)
        {
            if (inputManager.IsKeyPressed(Keys.Space) || isTap)
                velocity.Y = Position.Y < TopLimit ? VelocityAboveTopLimit : NormalVelocityY;

            velocity += Gravity * (float)elapsedTime;
            Position += velocity * (float)elapsedTime;

            UpdateAgentAnimation();

            // Update Colliders' positions
            bodyCollider.X = BodyRectangle.X + (int)Position.X - AgentWidth / 2;
            bodyCollider.Y = BodyRectangle.Y + (int)Position.Y - AgentHeight / 2;

            headCollider.X = HeadRectangle.X + (int)Position.X - AgentWidth / 2;
            headCollider.Y = HeadRectangle.Y + (int)Position.Y - AgentHeight / 2;
        }

        private void UpdateAgentAnimation()
        {
            if (velocity.Y > 0)
            // An agent is falling.
            {
                // Set falling sprite frame
                spriteComponent.CurrentFrame = FallingSpriteFrameIndex;

                // Rotate a sprite downward
                Rotation.Z += (float) Math.PI*0.01f;

                if (Rotation.Z > Math.PI/10f)
                    Rotation.Z = (float) (Math.PI/10f);
            }
            else
            // An agent is rising.
            {
                // Set rising sprite frame
                spriteComponent.CurrentFrame = FlyingSpriteFrameIndex;

                // Rotate a sprite upward
                Rotation.Z -= (float) Math.PI*0.01f;

                if (Rotation.Z < -Math.PI/10f)
                    Rotation.Z = (float) (-Math.PI/10f);
            }
        }
    }
}
