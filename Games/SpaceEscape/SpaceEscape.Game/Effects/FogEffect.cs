// AUTO-GENERATED, DO NOT MODIFY!
using System;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects
{
    public static partial class FogEffectKeys
    {
        public static readonly ParameterKey<Color4> FogColor = ParameterKeys.New<Color4>(new Color4(1,1,1,1));
        public static readonly ParameterKey<float> fogNearPlaneZ = ParameterKeys.New<float>(8000.0f);
        public static readonly ParameterKey<float> fogFarPlaneZ = ParameterKeys.New<float>(25000.0f);
        public static readonly ParameterKey<float> fogNearPlaneY = ParameterKeys.New<float>(0.0f);
        public static readonly ParameterKey<float> fogFarPlaneY = ParameterKeys.New<float>(12000.0f);
    }
}
