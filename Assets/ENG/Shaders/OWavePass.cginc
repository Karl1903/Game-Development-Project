#include "UnityCG.cginc"
#include "OWaveHelpers.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;

// Align this constant with its C# script counterpart
static const int MAX_SOUNDS = 32;

// Wave data set by C# script
fixed _ActiveSoundsMask[MAX_SOUNDS]; // Unity only supports float arrays, no bool arrays
float4 _SoundWorldSpaceOrigins[MAX_SOUNDS];
float _CurrFrontWaveRadii[MAX_SOUNDS]; // Current radii, frequently updated by C# script
float _SoundRadii[MAX_SOUNDS];
float _WaveWidths[MAX_SOUNDS];
float _WaveModes[MAX_SOUNDS];
float4 _WaveColors[MAX_SOUNDS];
float _WaveFadeOutStrengths[MAX_SOUNDS];
float _WaveBorderWidths[MAX_SOUNDS];
float _SoundWaveAmplifier;
float _SoundWaveBorderIntensity;

#if defined(IS_DITHERED)
sampler2D _DitherTex;
float4 _DitherTex_ST;
#endif

struct MeshData {
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct Interpolators {
    float4 vertex : SV_POSITION;
    float2 uv1 : TEXCOORD0;
    UNITY_FOG_COORDS(1) // uses TEXCOORD1
    float3 worldPos : TEXCOORD2;
    #if defined(IS_DITHERED)
    float4 screenPos : TEXCOORD3;
    #endif
};

Interpolators vert (MeshData v) {
    Interpolators o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv1 = TRANSFORM_TEX(v.uv, _MainTex);
    UNITY_TRANSFER_FOG(o, o.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    #if defined(IS_DITHERED)
    o.screenPos = ComputeScreenPos(o.vertex);
    #endif
    return o;
}

float4 frag (Interpolators i) : SV_Target {
    float3 texColor = tex2D(_MainTex, i.uv1);
    float3 albedo = _Color * texColor;

    // calculate waves
    float3 waves = float3(0, 0, 0);
    for (int s = 0; s < MAX_SOUNDS; s++) {
        if (!_ActiveSoundsMask[s]) continue;
        
        float dist = distance(i.worldPos, _SoundWorldSpaceOrigins[s]);

        float radius = _CurrFrontWaveRadii[s];
        float width = _WaveWidths[s];
        float3 waveColor = _WaveColors[s].a * _WaveColors[s].rgb; // Alpha channel represents the wave brightness
        float borderWidth = _WaveBorderWidths[s];
        
        float waveMask = 0;
        switch (_WaveModes[s]) {
            case 1: waveMask += getHardWave(radius, dist, width); break;
            case 2: waveMask += getSoftWave(radius, dist); break;
            case 3:
                waveMask += getHardWave(radius, dist, width);
                waveMask += getSoftWave(radius, dist);
                break;
        }
        
        float fade = 1 - (radius / _SoundRadii[s]);
        fade = saturate(fade); // Clamp 0 1
        fade = pow(fade, _WaveFadeOutStrengths[s]);
        waveMask *= fade;
        
        float3 wave = albedo * waveMask * waveColor * _SoundWaveAmplifier;

        // add border
        if (isInsideBorder(radius, dist, borderWidth)) {
            wave = waveColor * 2 * _SoundWaveBorderIntensity * fade;
        }

        waves += wave;
    }

    float4 outColor = float4(waves, 1);
    
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
    
    // apply fog
    UNITY_APPLY_FOG(i.fogCoord, outColor);
    
    return outColor;
}