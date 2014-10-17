using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

namespace JumpyJet
{
    /// <summary>
    /// PipeSet contains Two pipes: Top and Bottom pipes.
    /// </summary>
    public class PipeSet
    {
        public const float VerticalDistanceBetweenPipe = 230f;

        public string Name { get; set; }
        public Entity TopSpriteEntity { get; private set; }
        public Entity BottomSpriteEntity { get; private set; }

        public float ScrollSpeed { get; private set; }
        public int ScrollPos { get; private set; }
        public bool IsVisible { get; private set; }
        public int PipeWidth { get; private set; }
        public int PipeHeight { get; private set; }
        public float HalfPipeWidth { get; private set; }
        public bool IsPassedAgent { get; set; }
        public bool ShouldReset { get; private set; }

        private readonly TransformationComponent topTComp;
        private readonly TransformationComponent bottomTComp;
        private readonly Random random = new Random();
        private Rectangle topPartCollider;
        private Rectangle bodyPartCollider;
        private readonly int startScrollPos;
        private readonly float halfScrollWidth;
        private Vector3 topPipePosition;
        private Vector3 bottomPipePosition;
        private readonly Vector3 topLeftOffset;

        public PipeSet(Entity pipeEntity, Vector3 screenResolution, float scrollSpeed, float startScrollPos, string name)
        {
            Name = name;

            ScrollSpeed = scrollSpeed;
            ScrollPos = (int)startScrollPos;
            this.startScrollPos = ScrollPos;
            halfScrollWidth = screenResolution.X / 2f;

            // Store Entity and create another one for two rendering:
            // top and bottom sprite of pipe.
            // Note that: topSprite and bottomSprite entities share one pipeSpriteComp
            var spriteComp = pipeEntity.Get<SpriteComponent>();
            BottomSpriteEntity = new Entity();
            bottomTComp = new TransformationComponent();
            BottomSpriteEntity.Add(bottomTComp);
            BottomSpriteEntity.Add(spriteComp);

            TopSpriteEntity = new Entity();
            topTComp = new TransformationComponent();
            TopSpriteEntity.Add(topTComp);
            TopSpriteEntity.Add(spriteComp);

            var textureRegion = spriteComp.SpriteGroup.Images.First().Region;
            PipeHeight = textureRegion.Height;
            PipeWidth = textureRegion.Width;
            HalfPipeWidth = PipeWidth/2f;

            // Setup collider
            topPartCollider = new Rectangle(0, 0, PipeWidth, 95);
            bodyPartCollider = new Rectangle(0, 0, PipeWidth, PipeHeight - 95);

            // Setup initial position for top and bottom pipe.
            topPipePosition = new Vector3(this.startScrollPos, - 568f, 0f);

            // For top pipe, rotate a sprite by 180 degree 
            topTComp.RotationEulerXYZ = new Vector3(0, 0, (float) Math.PI);
            bottomPipePosition = new Vector3(this.startScrollPos, 312f, 0f);

            topLeftOffset = new Vector3(new Vector2(screenResolution.X/2, screenResolution.Y/2), GameModule.PipeDepth);

            ResetPipe();
        }

        public void ResetPipe()
        {
            ResetPipe(startScrollPos);
        }

        public void ResetPipe(int resetScrollPos)
        {
            ScrollPos = resetScrollPos;
            IsPassedAgent = false;
            ShouldReset = false;
            SetRandomHeight();
            UpdateSpritePos();
        }

        public void Update(float elapsedTime)
        {
            // A function that updates the scrolling state, and checks 
            // if the content is visible or not.
            if (ShouldReset)
                return;

            IsVisible = IsContentVisible();
            if (!IsVisible)
            {
                ShouldReset = true;
                return;
            }
            // Update pos according to the speed
            ScrollPos += (int)(elapsedTime * ScrollSpeed);
            UpdateSpritePos();
        }

        /// <summary>
        /// Check if both Top and Bottom pipes are collided with the agent or not.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool IsCollide(Agent agent)
        {
            return IsCollideBottomPipe(agent, bottomPipePosition) || IsCollideTopPipe(agent, topPipePosition);
        }

        /// <summary>
        /// Update position of two sprites to TransformationComponent
        /// of both sprite for drawing.
        /// </summary>
        private void UpdateSpritePos()
        {
            // Update posX by scrolling position.
            topPipePosition.X = ScrollPos;
            bottomPipePosition.X = ScrollPos;

            topTComp.Translation = topPipePosition + topLeftOffset;
            bottomTComp.Translation = bottomPipePosition + topLeftOffset;
        }

        private bool IsCollideBottomPipe(Agent agent, Vector3 position)
        {
            // Check collision of the top pipe
            topPartCollider.X = (int)position.X - topPartCollider.Width / 2;
            topPartCollider.Y = (int)position.Y - topPartCollider.Height / 2 - bodyPartCollider.Height / 2;

            if (topPartCollider.Intersects(agent.BodyCollider))
                return true;

            // Check body collider
            bodyPartCollider.X = (int)position.X - bodyPartCollider.Width / 2;
            bodyPartCollider.Y = (int)position.Y - bodyPartCollider.Height / 2;

            if (bodyPartCollider.Intersects(agent.BodyCollider))
                return true;

            return false;
        }

        private bool IsCollideTopPipe(Agent agent, Vector3 position)
        {
            // check collision of the top pipe
            topPartCollider.X = (int)position.X - topPartCollider.Width / 2;
            topPartCollider.Y = (int)position.Y + (PipeHeight / 2 - topPartCollider.Height);

            if (topPartCollider.Intersects(agent.HeadCollider))
                return true;
            if (topPartCollider.Intersects(agent.BodyCollider))
                return true;

            // check body collider
            bodyPartCollider.X = (int)position.X - bodyPartCollider.Width / 2;
            bodyPartCollider.Y = (int)position.Y - PipeHeight / 2;

            if (bodyPartCollider.Intersects(agent.BodyCollider) || bodyPartCollider.Intersects(agent.HeadCollider))
                return true;

            return false;
        }

        /// <summary>
        /// Determine if the content is visible in the screen.
        /// The content is invisible when the right side of the quad
        ///  passes the most left side of the screen.
        /// </summary>
        /// <returns></returns>
        private bool IsContentVisible()
        {
            return ScrollPos + HalfPipeWidth > -halfScrollWidth;
        }

        /// <summary>
        /// Get random height from RandomHeight() and set it to Y component
        /// of transformation for both sprites of pipe.
        /// </summary>
        private void SetRandomHeight()
        {
            // Random height of pipeset where the random value is limit
            // between [-270, 20].
            var posY = random.Next(-270, 20);

            topPipePosition.Y = posY - (VerticalDistanceBetweenPipe + PipeHeight) * 0.5f;
            bottomPipePosition.Y = posY + (VerticalDistanceBetweenPipe + PipeHeight) * 0.5f;
        }
    }
}
