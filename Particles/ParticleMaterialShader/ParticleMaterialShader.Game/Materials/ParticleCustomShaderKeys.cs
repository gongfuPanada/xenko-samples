// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

//namespace SiliconStudio.Xenko.Rendering
namespace SiliconStudio.Xenko.Rendering
{
    public partial class ParticleCustomShaderKeys
    {
        static ParticleCustomShaderKeys()
        {
            
        }

        public static readonly ParameterKey<ShaderSource> BaseColor     = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> EmissiveMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color4> EmissiveValue = ParameterKeys.New<Color4>();



        public static readonly ParameterKey<ShaderSource> BaseIntensity = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> IntensityMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> IntensityValue = ParameterKeys.New<float>();
    }
}
