﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Xenko Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Xenko.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SpaceEscape.Effects
{
    [DataContract]public partial class GameParameters : ShaderMixinParameters
    {
        public static readonly ParameterKey<bool> EnableFog = ParameterKeys.New<bool>(true);
        public static readonly ParameterKey<bool> EnableBend = ParameterKeys.New<bool>(true);
        public static readonly ParameterKey<bool> EnableOnflyTextureUVChange = ParameterKeys.New<bool>(false);
    };
    internal static partial class ShaderMixins
    {
        internal partial class SpaceEscapeEffectMain  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSource mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "XenkoForwardShadingEffect");
                if (context.GetParam(GameParameters.EnableOnflyTextureUVChange))
                    context.Mixin(mixin, "TransformationTextureUV");
                if (context.GetParam(GameParameters.EnableBend))
                    context.Mixin(mixin, "TransformationBendWorld");
                if (context.GetParam(GameParameters.EnableFog))
                    context.Mixin(mixin, "FogEffect");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("SpaceEscapeEffectMain", new SpaceEscapeEffectMain());
            }
        }
    }
}
