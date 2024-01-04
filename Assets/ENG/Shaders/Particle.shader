Shader "_Orpheus/Particles/Standard Unlit" {
    Properties {
        _MainTex("Albedo", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _SoundWaveAmplifier ("Sound Wave Amplifier", Range(0, 30)) = 1
        _SoundWaveColorRelevance ("Sound Wave Color Relevance", Range(0, 1)) = 0 // if set to 1, e.g. red particles are only visible from waves with a color that has red in it
    }
    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        
        Blend SrcAlpha One
        ZWrite Off // don't write to depth buffer

        Pass {
            Tags {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "OWaveHelpers.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            // Sound Wave Parameters
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
            // Material properties
            float _SoundWaveAmplifier;
            float _SoundWaveColorRelevance;

            struct MeshData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1) // uses TEXCOORD1
                float3 worldPos : TEXCOORD2;
            };

            Interpolators vert (MeshData v) {
                Interpolators o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }

            float4 frag (Interpolators i) : SV_Target {
                float3 albedo = tex2D(_MainTex, i.uv);
                albedo *= _Color;

                float4 outColor = float4(0, 0, 0, 1);
                
                // apply waves
                float3 waves = float3(0, 0, 0);
                float3 waveMasks;
                for (int s = 0; s < MAX_SOUNDS; s++) {
                    if (!_ActiveSoundsMask[s]) continue;
                    
                    float dist = distance(i.worldPos, _SoundWorldSpaceOrigins[s]);

                    float radius = _CurrFrontWaveRadii[s];
                    float width = _WaveWidths[s];
                    float3 waveColor = _WaveColors[s].a * _WaveColors[s].rgb; // Alpha channel represents the wave brightness
                    
                    float waveMask = 0;

                    waveMask += getHardWave(radius, dist, width);
                    waveMask += getSoftWave(radius, dist);
                    /*switch (_WaveModes[s]) {
                    case 1: waveMask += getHardWave(radius, dist, width); break;
                    case 2: waveMask += getSoftWave(radius, dist); break;
                    case 3:
                        waveMask += getHardWave(radius, dist, width);
                        waveMask += getSoftWave(radius, dist);
                        break;
                    }*/
                    
                    float fade = 1 - (radius / _SoundRadii[s]);
                    fade = saturate(fade); // Clamp 0 1
                    fade = pow(fade, _WaveFadeOutStrengths[s] * 0.3);
                    
                    waveMask *= fade;

                    float3 wave = albedo * waveMask * _SoundWaveAmplifier;
                    float3 coloredWave = wave * waveColor;
                    wave = lerp(wave, coloredWave, _SoundWaveColorRelevance);
                    
                    waves += wave;
                    waveMasks += waveMask;
                }

                outColor.rgb = waves;
                outColor.a *= waveMasks;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, outColor);

                return outColor;
            }
            ENDCG
        }
    }
}
