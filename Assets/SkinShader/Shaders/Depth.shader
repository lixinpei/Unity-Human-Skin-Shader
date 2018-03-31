// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Depth" {
SubShader {
    Tags { "RenderType"="Opaque" }
    Pass {

CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
float _Range;
struct v2f {
    float4 pos : SV_POSITION;
    float3 cmPos : TEXCOORD0;
};

v2f vert (appdata_base v) {
    v2f o;
    float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
    o.pos = mul (UNITY_MATRIX_P, viewPos);
    viewPos.xyz /= viewPos.w;
    o.cmPos = viewPos.xyz;

    return o;
}

float4 frag(v2f i) : COLOR {
    return length(i.cmPos) / _Range;

}
ENDCG
    }
}
}