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
bool _IsHighlighted;
float _HighlightIntensity;

#if defined(IS_DITHERED)
sampler2D _DitherTex;
float4 _DitherTex_ST;
#endif

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
    #if defined(IS_DITHERED)
    float4 screenPos : TEXCOORD6;
    #endif
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
    #if defined(IS_DITHERED)
    o.screenPos = ComputeScreenPos(o.vertex);
    #endif
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
    
    // ambient lighting
    #if defined(IS_IN_BASE_PASS)
        float3 ambientLight = unity_AmbientSky.rgb;
    #else
        float3 ambientLight = float3(0, 0, 0);
    #endif

    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos); // defines attenuation (not for directional lights)
    
    float3 N = calculateNormal(i);
    float3 L = normalize(UnityWorldSpaceLightDir(i.worldPos)); // direction from surface to light source
    float3 V = normalize(_WorldSpaceCameraPos - i.worldPos); // view direction

    UnityLight light;
    light.color = _LightColor0.rgb * attenuation;
    light.dir = L;
    light.ndotl = DotClamped(N, L);
    
    UnityIndirect indirectLight;
    indirectLight.diffuse = ambientLight;
    indirectLight.specular = 0; // environmental reflections

    float3 physicallyBasedLight = UNITY_BRDF_PBS(
        albedo, specularTint, oneMinusReflectivity, oneMinusRoughness, N, V, light, indirectLight);

    // apply lighting
    float4 outColor = float4(physicallyBasedLight, 1);

    // interactable highlight effect (fresnel)
    #if defined(IS_IN_BASE_PASS)
    if (_IsHighlighted) {
        float fresnel = 1 - dot(V, N);
        outColor += fresnel * _HighlightIntensity;
    }
    #endif
    
    #if defined(IS_DITHERED)
    // counteract the perspective correction the GPU automatically performs on interpolators
    float2 uvDither = i.screenPos.xy / i.screenPos.w;

    // aspect ratio scaling
    float aspectRatio = _ScreenParams.x / _ScreenParams.y;
    uvDither.x *= aspectRatio;

    // hardcode the tiling, there is no proper way to set the default tiling of a texture
    _DitherTex_ST = float4(19, 19, 0, 0);
    uvDither = TRANSFORM_TEX(uvDither, _DitherTex);
    
    float ditherMask = tex2D(_DitherTex, uvDither).x;
    outColor.a *= ditherMask;
    #endif
    
    #if defined(IS_TRANSPARENT)
    outColor.a *= 0.5;
    #endif
    
    return outColor;
}
