using System;
using System.Collections.Generic;
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
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SpriteEntity
{
    /// <summary>
    /// This 2D casual game consists of an agent and a background.
    /// A user could control the agent by running to the left/right and shoot.
    /// This game describes how to utilize Sprite animation in particular for the Agent and Bullet.
    /// </summary>
    public class SpriteEntityGame : Game
    {
        private static readonly Vector3 AgentStartPosition = new Vector3(100, 445, 1);
        private const int AgentMoveDistance = 10;       // virtual resolution unit/second
        private const float AgentShootDelay = 0.3f;     // second

        private const int NumberOfBullets = 10;
        private const float BulletSpeed = 700f;         // virtual resolution unit/second

        private const int NumberOfEnemies = 10;
        private const float EnemyCreationDelay = 0.8f;   // second
        private const float EnemySpeed = 500f;           // virtual resolution unit/second
        private const float EnemyTimeToLive = 2f;      // second 

        private static readonly Dictionary<AgentAnimation, int> AnimationFps = new Dictionary<AgentAnimation, int>
        {
            {AgentAnimation.Run, 12}, {AgentAnimation.Idle, 7}, {AgentAnimation.Shoot, 15}
        };

        private const int EnemyBlowupFps = 18;
        private const int EnemyActiveFps = 2;

        // AgentAnimation represents all states and animations of the agent
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

        // Agent 
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
                SpriteAnimation.Play(agentSpriteComponent, 0, agentSpriteComponent.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, AnimationFps[currentAgentAnimation]);
            }
        }

        private AgentAnimation currentAgentAnimation;
        private Entity agentEntity;
        private SpriteComponent agentSpriteComponent;
        private SpriteGroup agentIdle;
        private SpriteGroup agentRun;
        private SpriteGroup agentShoot;
        private RectangleF agentSpriteRegion;

        // Touch input state
        private PointerEvent pointerState;
        private bool isPointerDown; // Cache state if a user is current touching the screen.

        // Bullet
        private Entity bulletEntity;
        private readonly List<GameObject> bullets = new List<GameObject>();

        // Enemy
        private Entity enemyEntity;
        private SpriteGroup enemyActive;
        private SpriteGroup enemyBlowup;

        private readonly List<Enemy> enemies = new List<Enemy>();

        // Setting Virtual Resolution so that the screen has 640 and 1136 of Width and Height respectively.
        // Note that the Z component indicates the near and farplane [near, far] = [-10, 10].
        private static readonly Vector3 GameVirtualResolution = new Vector3(1136, 640, 10f);

        public SpriteEntityGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            
            // Set landscape preferred size for back buffer
            GraphicsDeviceManager.PreferredBackBufferWidth = (int)GameVirtualResolution.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int)GameVirtualResolution.Y;
        }

        /// <summary>
        /// Create rendering pipeline for the engine.
        /// Load Assets: Agent, Bullet and Background, and Initialize agent's state.
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreateRenderingPipeline();

            // Set virtual resolution for Sprite
            VirtualResolution = GameVirtualResolution;

            IsMouseVisible = true; // Enable mouse Cursor, if there is one

            // Disable multi-touch input for the game, since there is no need
            Input.MultiTouchEnabled = false;

            // Load font, initialize TextBlock, and set it to RootElement to render message of gameplay control
            var font = Asset.Load<SpriteFont>("Font");
            var textBlock = new TextBlock
            {
                Font = font,
                TextSize = 18,
                TextColor = Color.Gold,
                Text = "Shoot : Touch in a vertical section where the Agent resides\n" +
                       "Move : Touch in the screen on the corresponding side of the Agent",
            };
            textBlock.SetCanvasRelativePosition(new Vector3(0.008f, 0.9f, 0));
            UI.RootElement = new Canvas {Children = {textBlock}};

            // Load Agent
            agentEntity = Asset.Load<Entity>("character_entity");
            agentIdle = Asset.Load<SpriteGroup>("character_idle");
            agentRun = Asset.Load<SpriteGroup>("character_run");
            agentShoot = Asset.Load<SpriteGroup>("character_shoot");
            agentSpriteRegion = agentIdle.Images.First().Region;

            // Load bullet entity
            bulletEntity = Asset.Load<Entity>("bullet_entity");
            var bulletSpriteGroup = Asset.Load<SpriteGroup>("bullet");

            // Create Bullet objects in the pool
            for (var i = 0; i < NumberOfBullets; i++)
            {
                var entity = new Entity
                {
                    new TransformationComponent(),
                    new SpriteComponent
                    {
                        SpriteGroup = bulletSpriteGroup
                    },
                };

                var bullet = new GameObject {Entity = entity, Speed = new Vector3(BulletSpeed, 0, 0)};
                bullets.Add(bullet);
            }

            // Load entity of an enemy
            enemyEntity = Asset.Load<Entity>("enemy_entity");
            enemyActive = Asset.Load<SpriteGroup>("enemy_active");
            enemyBlowup = Asset.Load<SpriteGroup>("enemy_blowup");

            // Create Enemy objects in the pool
            for (var i = 0; i < NumberOfEnemies; i++)
            {
                var entity = new Entity
                {
                    new TransformationComponent(),
                    new SpriteComponent {SpriteGroup = enemyActive}
                };

                var enemy = new Enemy { Entity = entity, Speed = new Vector3(0, EnemySpeed, 0), Direction = new Vector3(0, 1, 0)};
                enemies.Add(enemy);
            }

            // Add game entities in EntitySystem for update and render
            Entities.Add(agentEntity);
            Entities.Add(Asset.Load<Entity>("background_entity"));

            // Enable UpdateInput by adding to Script System
            Script.Add(UpdateCollision);
            Script.Add(UpdateInput);
            Script.Add(UpdateEnemies);
            Script.Add(UpdateBullets);
        }

        /// <summary>
        /// Create and Set render target for the engine, and create Sprite and UI Renderer
        /// </summary>
        private void CreateRenderingPipeline()
        {
            // Create the RenderTarget setter. This clears and sets the render targets
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            // Create a Sprite Renderer
            RenderSystem.Pipeline.Renderers.Add(new SpriteRenderer(Services));
            // Create a UI Renderer
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }

        /// <summary>
        /// Process user's inputs which composed of the following actions: shooting, running left and right.
        /// Update Agent and Bullet according to the input.
        /// </summary>
        private async Task UpdateInput()
        {
            agentSpriteComponent = agentEntity.Get<SpriteComponent>();
            var agentTransformationComponent = agentEntity.Get<TransformationComponent>();

            // Calculate offset of the bullet from the Agent if he is facing left and right side
            var bulletOffset = new Vector3(agentSpriteRegion.Width * 0.5f, -14f, 0);

            // Initialize game entities
            CurrentAgentAnimation = AgentAnimation.Idle;
            agentTransformationComponent.Translation = AgentStartPosition;
            var isAgentFacingRight = true;
            var shootDelayCounter = 0f;

            while (IsRunning)
            {
                // Wait next rendering frame
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
                    var dt = (float)UpdateTime.Elapsed.TotalSeconds * 50;

                    agentTransformationComponent.Translation.X += ((inputState == InputState.RunRight) ? AgentMoveDistance : -AgentMoveDistance) * dt;

                    if (agentTransformationComponent.Translation.X < 0)
                        agentTransformationComponent.Translation.X = 0;

                    if (agentTransformationComponent.Translation.X > VirtualResolution.X)
                        agentTransformationComponent.Translation.X = VirtualResolution.X;

                    isAgentFacingRight = inputState == InputState.RunRight;

                    // If agent face left, flip the sprite
                    agentSpriteComponent.SpriteEffect = (isAgentFacingRight) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    // Update the sprite animation and state
                    agentSpriteComponent.SpriteGroup = agentRun;
                    CurrentAgentAnimation = AgentAnimation.Run;
                }
                else if (inputState == InputState.Shoot)
                {
                    // Update shootDelayCounter, and check whether it is time to create a new bullet
                    shootDelayCounter -= (float)UpdateTime.Elapsed.TotalSeconds;

                    if (shootDelayCounter > 0)
                        continue;

                    // Reset shoot delay
                    shootDelayCounter = AgentShootDelay;

                    // Get available bullet in the pool
                    var availablebullet = bullets.FirstOrDefault(bullet => !bullet.IsVisible);

                    // There is none of available bullet, skip initialization for the new bullet
                    if (availablebullet == null)
                        continue;

                    // Set origin position of the bullet
                    availablebullet.Entity.Transformation.Translation = (isAgentFacingRight)
                        ? agentTransformationComponent.Translation  + bulletOffset
                        : agentTransformationComponent.Translation  + (bulletOffset * new Vector3(-1, 1, 1));

                    availablebullet.Direction.X = (isAgentFacingRight) ? 1 : -1;

                    // Add new bullet into EntitySystem and change its visibility state
                    availablebullet.IsVisible = true;
                    Entities.Add(availablebullet.Entity);

                    // Start animation for shooting
                    agentSpriteComponent.SpriteGroup = agentShoot;
                    CurrentAgentAnimation = AgentAnimation.Shoot;
                }
                else
                {
                    agentSpriteComponent.SpriteGroup = agentIdle;
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
            var virtualCoordinatePointerPosition = new Vector2(GameVirtualResolution.X, GameVirtualResolution.Y) * pointerState.Position;

            // Get current position of the agent, since the origin of the sprite is at the center, region needs to be shifted to top-left
            agentSpriteRegion.X = (int)agentEntity.Transformation.Translation.X - agentSpriteRegion.Width / 2;
            agentSpriteRegion.Y = (int)agentEntity.Transformation.Translation.Y - agentSpriteRegion.Height / 2;

            // Check if the touch position is in the x-axis region of the agent's sprite; if so, input is shoot
            if (agentSpriteRegion.Left <= virtualCoordinatePointerPosition.X && virtualCoordinatePointerPosition.X <= agentSpriteRegion.Right)
                return InputState.Shoot;

            // Check if a pointer falls left or right of the screen, which would correspond to Run to the left or right respectively 
            return (pointerState.Position.X <= agentSpriteRegion.Center.X / GameVirtualResolution.X ) ? InputState.RunLeft : InputState.RunRight;
        }

        /// <summary>
        /// Update Position of every enemy, and check if there is a need to create a new one in the game.
        /// The need to create a new enemy is checked wether enemyCreationDelay reaches 0.
        /// By create, the game will pop a free (Not visible/ Not active) enemy from enemies list, and re-initialize its position,
        /// with random X value along the screen.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateEnemies()
        {
            var enemyCreationDelay = 0f;
            var random = new Random();

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();
                var elapsedTime = (float)UpdateTime.Elapsed.TotalSeconds;

                // Update position for each enemy
                foreach (var enemy in enemies.Where(enemy => enemy.IsVisible))
                {
                    // Update an age of this enemy, and check to see if it needs to be removed
                    enemy.Age += elapsedTime;
                    var enemySpriteComponent = enemy.Entity.Get<SpriteComponent>();

                    if (enemy.Age >= EnemyTimeToLive || (enemy.IsDied && enemySpriteComponent.CurrentFrame == enemySpriteComponent.SpriteGroup.Images.Count - 1))
                    {
                        Entities.Remove(enemy.Entity);
                        enemy.Reset();
                        continue;
                    }

                    // If an enemy should not be updated (reached the ground), Skip updating
                    if(!enemy.ShouldUpdate)
                        continue;

                    // Update position of the enemy
                    var enemyTransComp = enemy.Entity.Get<TransformationComponent>();
                    enemyTransComp.Translation += enemy.Direction * enemy.Speed * elapsedTime;

                    // Check if the enemy has already reached the ground; if so, stops updating it
                    if (enemyTransComp.Translation.Y < 0 || enemyTransComp.Translation.Y > GameVirtualResolution.Y - 200)
                        enemy.ShouldUpdate = false;
                }

                // Check if we need to create a new enemy. It is when enemyCreationDelay <= 0
                enemyCreationDelay -= (float)UpdateTime.Elapsed.TotalSeconds;

                if(enemyCreationDelay > 0)
                    continue;

                enemyCreationDelay = EnemyCreationDelay;

                // Try to create an enemy, by finding the first available enemy in the list
                var availableEnemy = enemies.FirstOrDefault(enemy => !enemy.IsVisible);

                // Could not find an available enemy, do nothing
                if(availableEnemy == null)
                    continue;

                // Found an available enemy, initialize it with random position in X axis along the screen and add to the entity system
                availableEnemy.Entity.Transformation.Translation = new Vector3((float)(random.NextDouble() * GameVirtualResolution.X), 0, 0);

                var availableEnemySpriteComponent = availableEnemy.Entity.Get<SpriteComponent>();
                availableEnemySpriteComponent.SpriteGroup = enemyActive;
                SpriteAnimation.Play(availableEnemySpriteComponent, 0, availableEnemySpriteComponent.SpriteGroup.Images.Count - 1, AnimationRepeatMode.LoopInfinite, EnemyActiveFps);

                availableEnemy.IsVisible = true;
                availableEnemy.ShouldUpdate = true;
                Entities.Add(availableEnemy.Entity);
            }
        }

        /// <summary>
        /// Update every bullet for its position, and visibility
        /// </summary>
        /// <returns></returns>
        private async Task UpdateBullets()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();
                var elapsedTime = (float)UpdateTime.Elapsed.TotalSeconds;

                // Updating bullets
                foreach (var bullet in bullets.Where(bullet => bullet.IsVisible))
                {
                    var bulletTransComp = bullet.Entity.Get<TransformationComponent>();

                    // Check if a bullet is visible. If not, remove it from Entity system and mark state as invisible
                    if (bulletTransComp.Translation.X < 0 || bulletTransComp.Translation.X > GameVirtualResolution.X)
                    {
                        Entities.Remove(bullet.Entity);
                        bullet.IsVisible = false;
                    }

                    // Update position of a bullet
                    bulletTransComp.Translation += bullet.Direction * bullet.Speed * elapsedTime;
                }
            }
        }

        /// <summary>
        /// Check collision between enemies and bullets O(n^2).
        /// Colliders of an enemy and a bullet are represented by rectangle which size are equal to their TextureRegion.
        /// Given that, Collision between an enemy and a bullet is done by testing for containing and overlapping.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateCollision()
        {
            // Collider of both a bullet and an enemy is represented by Rectangle which can be obtained from Sprite's texture region
            var bulletRectangleCollider = bulletEntity.Get<SpriteComponent>().SpriteGroup.Images.First().Region;
            var enemyRectangleCollider = enemyEntity.Get<SpriteComponent>().SpriteGroup.Images.First().Region;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                foreach (var bullet in bullets.Where(bullet => bullet.IsVisible))
                {
                    // Retrieve the current position of this bullet, and update its collider.
                    // Note that origin of a sprite bullet is at the center 
                    // so its position needs to shift by offset to be at the top-left corner of sprite.
                    bulletRectangleCollider.X = (int)bullet.Entity.Transformation.Translation.X - bulletRectangleCollider.Width / 2;
                    bulletRectangleCollider.Y = (int)bullet.Entity.Transformation.Translation.Y - bulletRectangleCollider.Height / 2;

                    foreach (var enemy in enemies.Where(enemy => enemy.IsVisible && !enemy.IsDied))
                    {
                        // Retrieve the current position of this enemy, and update its collider
                        enemyRectangleCollider.X = (int)enemy.Entity.Transformation.Translation.X - enemyRectangleCollider.Width / 2;
                        enemyRectangleCollider.Y = (int)enemy.Entity.Transformation.Translation.Y - enemyRectangleCollider.Height / 2;

                        // Check collision between colliders
                        if (!bulletRectangleCollider.Intersects(enemyRectangleCollider))
                            continue;

                        // Found a collision between the current enemy and bullet.
                        // Resolving the collision by removing both enemy and bullet from Entities, making them disappeared from the screen.
                        var enemySpriteComponent = enemy.Entity.Get<SpriteComponent>();
                        enemySpriteComponent.SpriteGroup = enemyBlowup;
                        SpriteAnimation.Play(enemySpriteComponent, 0, enemySpriteComponent.SpriteGroup.Images.Count - 1, AnimationRepeatMode.PlayOnce, EnemyBlowupFps);
                        enemy.IsDied = true;

                        Entities.Remove(bullet.Entity);
                        bullet.IsVisible = false;
                    }
                }
            }
        }

        /// <summary>
        /// GameObject represents a sprite entity which has direction, and speed. It represents, in this sample, enemies and bullets.
        /// </summary>
        private class GameObject
        {
            public Entity Entity;
            public Vector3 Direction;
            public Vector3 Speed;
            public bool IsVisible;
        }

        /// <summary>
        /// Enemy is one of Game object used in the sample.
        /// Age of an enemy indicates how long it has been since it is created.
        /// </summary>
        private class Enemy : GameObject
        {
            public float Age;
            public bool ShouldUpdate;
            public bool IsDied;

            public void Reset()
            {
                Age = 0;
                ShouldUpdate = false;
                IsVisible = false;
                IsDied = false;
            }
        }
    }
}
