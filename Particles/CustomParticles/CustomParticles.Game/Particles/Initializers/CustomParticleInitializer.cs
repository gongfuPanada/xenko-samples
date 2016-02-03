// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles;
using SiliconStudio.Xenko.Particles.DebugDraw;
using SiliconStudio.Xenko.Particles.Initializers;

namespace CustomParticles.Particles.Initializers
{
    [DataContract("CustomParticleInitializer")]
    [Display("Custom Cone Initializer")]
    public class CustomParticleInitializer : ParticleInitializer
    {
        [DataMember(100)]
        [DataMemberRange(0, 120, 0.01, 0.1)]
        [Display("Arc")]
        public float Angle = 20f;

        [DataMember(200)]
        [Display("Velocity")]
        public float Strength = 1f;

        public CustomParticleInitializer()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var range = (float) (Angle*Math.PI/180f);
            var magnitude = WorldScale.X;

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var x = (randSeed.GetFloat(RandomOffset.Offset2A + SeedOffset) - 0.5f)*range;
                var z = (randSeed.GetFloat(RandomOffset.Offset2B + SeedOffset) - 0.5f) * range;

                var u = (randSeed.GetFloat(RandomOffset.Offset2A + SeedOffset) - 0.5f) * range;
                var v = (randSeed.GetFloat(RandomOffset.Offset2B + SeedOffset) - 0.5f) * Math.PI;

//                var particleRandPos = new Vector3(x, 1, z);

                var xz = (float) Math.Sin(u);
                var particleRandPos = new Vector3((float) Math.Cos(v) * xz, (float)Math.Sqrt(1 - u*u), (float)Math.Sin(v) * xz);
                particleRandPos.Normalize();


                particleRandPos *= magnitude;
                WorldRotation.Rotate(ref particleRandPos);

                (*((Vector3*) particle[posField])) = particleRandPos + WorldPosition;

                (*((Vector3*) particle[velField])) = particleRandPos * Strength;

                i = (i + 1) % maxCapacity;
            }
        }

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        public override bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            debugDrawShape = DebugDrawShape.Cone;

            rotation = WorldRotation;

//            scale = (PositionMax - PositionMin);
//            translation = (PositionMax + PositionMin) * 0.5f * WorldScale;

            scale = new Vector3(1, -1 , 1);
            translation = new Vector3(0, Strength + 1, 0);

            var radiusToStrength = (float) Math.Tan((Angle * Math.PI / 180f))*(Strength + 1);
            var coneScale = new Vector3(radiusToStrength, Strength + 1, radiusToStrength);

            scale *= WorldScale*coneScale;
            rotation.Rotate(ref translation);
            translation += WorldPosition;

            return true;
        }
    }
}
