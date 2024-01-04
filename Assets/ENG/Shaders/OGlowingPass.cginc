#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
sampler2D _RoughnessMap;
float4 _RoughnessMap_ST;
float _Roughness;
float _Metallic;
sampler2D _Normals;
float _NormalIntensity;
sampler2D _DetailNormals;
float4 _DetailNormals_ST;
float _DetailNormalIntensity;
float _GlowIntensity;
float4 _GlowColor;
float3 _GlowOriginWorldSpace; // this variable needs to be set by a script

struct MeshData {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT; // xyz = tangent direction, w = tangent sign
    float2 uv : TEXCOORD0;
};

struct Interpolators {
    float4 vertex : SV_POSITION;
    float2 uv1 : TEXCOORD0; // Main texture uvs
    float2 uv2 : TEXCOORD1; // Detail normal map uvs
    float3 normal : TEXCOORD2;
    float3 tangent : TEXCOORD3;
    float3 bitangent : TEXCOORD4;
    float3 worldPos : TEXCOORD5;
};

float3 calculateNormal(Interpolators i) {
    // normal map uses the same scaling as main texture
    float3 tangentSpaceNormal = UnpackNormal(tex2D(_Normals, i.uv1));
    // detail normal map has its own scaling
    float3 tangentSpaceDetailNormal = UnpackNormal(tex2D(_DetailNormals, i.uv2));
    
    float3 up = float3(0, 0, 1); // upwards direction = flat surface
    
    tangentSpaceNormal = lerp(up, tangentSpaceNormal, _NormalIntensity); // apply intensity
    tangentSpaceDetailNormal = lerp(up, tangentSpaceDetailNormal, _DetailNormalIntensity); // apply intensity
    float3 offset = float3(tangentSpaceDetailNormal.xy, 0); // Detail Map Offset only in x and y direction
    float3 tangentSpaceCombinedNormal = tangentSpaceNormal + offset;
    
    float3x3 mtxTangToWorld = {
        i.tangent.x, i.bitangent.x, i.normal.x,
        i.tangent.y, i.bitangent.y, i.normal.y,
        i.tangent.z, i.bitangent.z, i.normal.z,
    };
    return normalize(mul(mtxTangToWorld, tangentSpaceCombinedNormal));
}

Interpolators vert (MeshData v) {
    Interpolators o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv1 = TRANSFORM_TEX(v.uv, _MainTex);
    o.uv2 = TRANSFORM_TEX(v.uv, _DetailNormals);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.bitangent = cross(o.normal, o.tangent);
    o.bitangent *= v.tangent.w * unity_WorldTransformParams.w; // handle flipping and mirroring
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    return o;
}

float4 frag (Interpolators i) : SV_Target {
    float3 texColor = tex2D(_MainTex, i.uv1);
    float3 albedo = _Color * texColor;

    float3 specularTint;
    float oneMinusReflectivity;
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
    _RoughnessMap_ST = _MainTex_ST;
    float roughness = _Roughness * tex2D(_RoughnessMap, i.uv1).x;
    float oneMinusRoughness = 1 - roughness;
    
    float3 N = calculateNormal(i);
    float3 L = normalize(_GlowOriginWorldSpace - i.worldPos); // direction from surface to light/glow source
    float3 V = normalize(_WorldSpaceCameraPos - i.worldPos); // view direction

    UnityLight glow;
    glow.color = _GlowColor;
    glow.dir = -L * _GlowIntensity; // flip light direction to create a "lit from the inside" look
    glow.ndotl = DotClamped(N, L);
    
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0; // environmental reflections

    float3 physicallyBasedLight = UNITY_BRDF_PBS(
        albedo, specularTint, oneMinusReflectivity, oneMinusRoughness, N, V, glow, indirectLight);

    // apply lighting
    float3 outColor = physicallyBasedLight;
    
    return float4(outColor , 1);
}
