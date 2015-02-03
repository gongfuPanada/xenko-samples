using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SpaceEscape.Effects;

namespace SpaceEscape.LevelSystem
{
    /// <summary>
    /// Level controls LevelBlocks in the game.
    /// </summary>
    public class Level : ScriptContext, IScript
    {
        public event Action<float> DistanceUpdated;

        private const float GameSpeed = 3000f;
        private const float RemoveBlockPosition = -1600f;
        private const float AddBlockPosition = 28000f;
        private const int NumberOfStartBlock = 5;

        private static readonly Random Random = new Random();
        private readonly List<LevelBlock> levelBlocks = new List<LevelBlock>();
        private readonly LevelPatternFactory levelFactory = new LevelPatternFactory(Random);
        private float runningDistance; // Store how far the player progressed in m.

        private const string SkyPlaneModel = "bg_00";
        private static readonly Vector3 SkyPlanePosition = new Vector3(0, -1000f, 34000f);
        private Model skyplaneModel; // Cache to scroll its UV region.
        private Vector4 skyplaneUVRegion = new Vector4(0f, 0f, 1f, 1f);

        public bool IsRunning { get; set; }
        public bool IsUpdating { get; set; }

        public float RunningDistance
        {
            get{ return runningDistance; }
            set
            {
                runningDistance = value;
                if (DistanceUpdated != null)
                    DistanceUpdated(runningDistance);
            }
        }

        public Level(IServiceRegistry registry) 
            : base(registry)
        {
        }

        public void LoadContent()
        {
            RunningDistance = 0f;

            // Load SkyPlane
            var skyplaneEntity = Asset.Load<Entity>(SkyPlaneModel);
            skyplaneModel = skyplaneEntity.Get<ModelComponent>().Model;

            skyplaneEntity.Transformation.Translation = SkyPlanePosition;
            skyplaneEntity.Get<ModelComponent>().Parameters.Set(GameParameters.EnableBend, false);
            skyplaneModel.Meshes[0].Parameters.Set(GameParameters.EnableOnflyTextureUVChange, true);

            // Load LevelBlock and Obstacle models.
            levelFactory.LoadContent(Asset);

            // Add skyPlane with LevelBlocks to EntitySystem
            Entities.Add(skyplaneEntity);
            CreateStartLevelBlocks();
        }

        /// <summary>
        /// A script for updating levelBlocks in the game.
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            IsRunning = true;
            while (IsRunning)
            {
                await Script.NextFrame();

                if (!IsUpdating)
                    continue;

                var elapsedTime = (float)Game.UpdateTime
                    .Elapsed.TotalSeconds;

                Update(elapsedTime);
            }
        }

        public void Reset()
        {
            RunningDistance = 0f;

            for (var i = levelBlocks.Count - 1; i >= 0; i--)
            {
                var levelBlock = levelBlocks[i];

                Entities.Remove(levelBlock.RootEntity);
                levelBlocks.RemoveAt(i);
            }

            CreateStartLevelBlocks();
        }

        public bool DetectCollisions(ref BoundingBox agentBB)
        {
            foreach (var levelBlock in levelBlocks)
            {
                foreach (var obst in levelBlock.CollidableObstacles)
                {
                    if (DetectCollision(ref agentBB, obst))
                        return true;
                }
            }

            return false;
        }

        private static bool DetectCollision(ref BoundingBox agentBB,
            CollidableObstacle obst)
        {
            var objTrans = obst.Entity.Transformation;
            objTrans.UpdateWorldMatrix();

            var objWorldPos = objTrans.WorldMatrix.TranslationVector;

            foreach (var boundingBox in obst.BoundingBoxes)
            {
                var minVec = objWorldPos + boundingBox.Minimum;
                var maxVec = objWorldPos + boundingBox.Maximum;
                var testBB = new BoundingBox(minVec, maxVec);

                if (Collision.BoxContainsBox(ref testBB, ref agentBB) != ContainmentType.Disjoint)
                    return true;
            }
            return false;
        }

        public bool DetectHoles(ref Vector3 agentWorldPos, out float height)
        {
            height = 0f;

            foreach (var levelBlock in levelBlocks)
            {
                levelBlock.RootEntity.Transformation.UpdateWorldMatrix();
                var worldPosZ = levelBlock.RootEntity.Transformation.Translation.Z;
                foreach (var hole in levelBlock.Holes)
                {
                    if (DetectHole(ref agentWorldPos, out height, hole,
                        worldPosZ))
                        return true;
                }
            }
            return false;
        }

        private static bool DetectHole(ref Vector3 agentWorldPos, out float height, Hole hole, float blockPosZ)
        {
            var testArea = hole.Area;
            testArea.Y += blockPosZ;
            height = hole.Height;

            var agentVec2Pos = new Vector2(-agentWorldPos.X, agentWorldPos.Z);
            return RectContains(ref testArea, ref agentVec2Pos);
        }

        private void Update(float elapsedTime)
        {
            // Check if needed to remove the first block
            var firstBlock = levelBlocks[0];
            if (firstBlock.PositionZ + firstBlock.Length * 0.5f < RemoveBlockPosition)
            {
                RemoveLevelBlock(firstBlock);
            }

            // Check if needed to add new levelblock
            var lastBlock = levelBlocks[levelBlocks.Count - 1];
            if (lastBlock.PositionZ - lastBlock.Length * 0.5f < AddBlockPosition)
            {
                AddLevelBlock(levelFactory.RandomCreateLevelBlock());
            }

            // Move levelblocks
            foreach (var levelBlock in levelBlocks)
            {
                var moveDist = GameSpeed*elapsedTime;
                levelBlock.PositionZ -= moveDist;
                RunningDistance += moveDist/100f;
            }

            if(skyplaneUVRegion.X < -1f)
            // Reset scrolling position of Skyplane's UV
                skyplaneUVRegion.X = 0f; 

            // Move Scroll position by an offset every frame.
            skyplaneUVRegion.X -= 0.0005f;

            // Update Parameters of the shader
            skyplaneModel.Meshes[0].Parameters.Set(TransformationTextureUVKeys.TextureRegion, skyplaneUVRegion);
        }

        private void CreateStartLevelBlocks()
        {
            AddLevelBlock(levelFactory.CreateSafeLevelBlock());

            for (var i = 0; i < NumberOfStartBlock; i++)
                AddLevelBlock(levelFactory.RandomCreateLevelBlock());
        }

        private void AddLevelBlock(LevelBlock newLevelBlock)
        {
            var count = levelBlocks.Count;
            levelBlocks.Add(newLevelBlock);

            if (count == 0)
            {
                Entities.Add(newLevelBlock.RootEntity);
                newLevelBlock.OnAttachedToWorld();
                return;
            }

            var prevLatestBlock = levelBlocks[count - 1];

            var originDist = 0.5f * (prevLatestBlock.Length + newLevelBlock.Length);
            newLevelBlock.PositionZ = prevLatestBlock.PositionZ + originDist;

            Entities.Add(newLevelBlock.RootEntity);
            newLevelBlock.OnAttachedToWorld();
        }

        private void RemoveLevelBlock(LevelBlock firstBlock)
        {
            levelBlocks.Remove(firstBlock);
            Entities.Remove(firstBlock.RootEntity);
        }

        private static bool RectContains(ref RectangleF rect, ref Vector2 agentPos)
        {
            return (rect.X <= agentPos.X) && (rect.X + rect.Width >= agentPos.X)
                   && (rect.Y >= agentPos.Y) && (rect.Y - rect.Height <= agentPos.Y);
        }
    }
}
