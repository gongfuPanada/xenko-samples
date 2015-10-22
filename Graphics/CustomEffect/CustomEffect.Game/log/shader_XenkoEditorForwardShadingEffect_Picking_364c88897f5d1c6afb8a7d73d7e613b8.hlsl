struct PS_STREAMS 
{

    #line 11 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Graphics/Shaders/ShaderBaseStream.pdxsl"
    float4 ColorTarget_id1;

    #line 12
    float4 ColorTarget1_id2;

    #line 13
    float4 ColorTarget2_id3;
};
struct PS_OUTPUT 
{

    #line 11
    float4 ColorTarget_id1 : SV_Target0;

    #line 12
    float4 ColorTarget1_id2 : SV_Target1;

    #line 13
    float4 ColorTarget2_id3 : SV_Target2;
};
struct VS_STREAMS 
{

    #line 9 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/PositionStream4.pdxsl"
    float4 Position_id18;

    #line 9 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/NormalStream.pdxsl"
    float3 meshNormal_id14;

    #line 12 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/PositionStream4.pdxsl"
    float4 PositionWS_id19;

    #line 8 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/PositionHStream4.pdxsl"
    float4 PositionH_id21;

    #line 15 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/PositionStream4.pdxsl"
    float DepthVS_id20;

    #line 15 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/NormalStream.pdxsl"
    float3 normalWS_id16;

    #line 8 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Graphics/Shaders/ShaderBaseStream.pdxsl"
    float4 ShadingPosition_id0;
};
struct VS_OUTPUT 
{

    #line 8
    float4 ShadingPosition_id0 : SV_Position;
};
struct VS_INPUT 
{

    #line 9 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/PositionStream4.pdxsl"
    float4 Position_id18 : POSITION;

    #line 9 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/NormalStream.pdxsl"
    float3 meshNormal_id14 : NORMAL;
};
cbuffer PerFrame 
{

    #line 15 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Shaders/Transformation.pdxsl"
    float4x4 ViewProjection_id26;
};
cbuffer PerObject 
{

    #line 26
    float4x4 World_id30;

    #line 28
    float4x4 WorldInverseTranspose_id31;
};
cbuffer Globals 
{

    #line 11 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Utils/ModelComponentPickingShader.pdxsl"
    float4 ModelComponentId_id37;

    #line 14
    float4 MeshId_id38;

    #line 17
    float4 MaterialId_id39;
};

#line 18 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationBase.pdxsl"
void PostTransformPosition_id6()
{
}

#line 12
void PreTransformPosition_id4()
{
}

#line 14 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationWAndVP.pdxsl"
void PostTransformPosition_id14(inout VS_STREAMS streams)
{

    #line 16
    PostTransformPosition_id6();

    #line 17
    streams.ShadingPosition_id0 = mul(streams.PositionWS_id19, ViewProjection_id26);

    #line 18
    streams.PositionH_id21 = streams.ShadingPosition_id0;

    #line 19
    streams.DepthVS_id20 = streams.ShadingPosition_id0.w;
}

#line 15 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationBase.pdxsl"
void TransformPosition_id5()
{
}

#line 8 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationWAndVP.pdxsl"
void PreTransformPosition_id13(inout VS_STREAMS streams)
{

    #line 10
    PreTransformPosition_id4();

    #line 11
    streams.PositionWS_id19 = mul(streams.Position_id18, World_id30);
}

#line 20 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationBase.pdxsl"
void BaseTransformVS_id7(inout VS_STREAMS streams)
{

    #line 22
    PreTransformPosition_id13(streams);

    #line 23
    TransformPosition_id5();

    #line 24
    PostTransformPosition_id14(streams);
}

#line 15 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Graphics/Shaders/ShaderBase.pdxsl"
void VSMain_id0()
{
}

#line 8 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/NormalFromMesh.pdxsl"
void GenerateNormal_VS_id18(inout VS_STREAMS streams)
{

    #line 11
    streams.normalWS_id16 = mul(streams.meshNormal_id14, (float3x3)WorldInverseTranspose_id31);
}

#line 27 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Transformation/TransformationBase.pdxsl"
void VSMain_id8(inout VS_STREAMS streams)
{

    #line 29
    VSMain_id0();

    #line 30
    BaseTransformVS_id7(streams);
}

#line 19 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Utils/ModelComponentPickingShader.pdxsl"
PS_OUTPUT PSMain(VS_OUTPUT __input__)
{
    PS_STREAMS streams = (PS_STREAMS)0;

    #line 21
    streams.ColorTarget_id1 = ModelComponentId_id37;

    #line 22
    streams.ColorTarget1_id2 = MeshId_id38;

    #line 23
    streams.ColorTarget2_id3 = MaterialId_id39;
    PS_OUTPUT __output__ = (PS_OUTPUT)0;
    __output__.ColorTarget_id1 = streams.ColorTarget_id1;
    __output__.ColorTarget1_id2 = streams.ColorTarget1_id2;
    __output__.ColorTarget2_id3 = streams.ColorTarget2_id3;
    return __output__;
}

#line 8 "C:/Dev/Xenko/sources/engine/SiliconStudio.Xenko.Engine/Rendering/Core/NormalBase.pdxsl"
VS_OUTPUT VSMain(VS_INPUT __input__)
{
    VS_STREAMS streams = (VS_STREAMS)0;
    streams.Position_id18 = __input__.Position_id18;
    streams.meshNormal_id14 = __input__.meshNormal_id14;

    #line 10
    VSMain_id8(streams);

    #line 14
    GenerateNormal_VS_id18(streams);
    VS_OUTPUT __output__ = (VS_OUTPUT)0;
    __output__.ShadingPosition_id0 = streams.ShadingPosition_id0;
    return __output__;
}
