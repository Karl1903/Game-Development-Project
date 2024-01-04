Shader "_Orpheus/Unlit/Glowing" {
    Properties {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _Roughness ("Roughness", Range(0, 1)) = 0.5
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] [Normal] _Normals ("Normal Map", 2D) = "bump" {}
        _NormalIntensity ("Normal Map Intensity", Range(0, 1)) = 1
        _DetailNormals ("Detail Normal Map", 2D) = "bump" {}
        [Normal] _DetailNormalIntensity ("Detail Normal Map Intensity", Range(0, 1)) = 1
        _SoundWaveAmplifier ("Sound Wave Amplifier", Range(0, 10)) = 1
        _SoundWaveBorderIntensity ("Sound Wave Border Intensity", Range(0, 2)) = 1
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 1
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass {
            Tags {
                "LightMode" = "Always" // Always rendered; does not apply any lighting. This is the default value.
            }
            
            CGPROGRAM
            #pragma target 3.0 // use the best BRDF function
            #pragma vertex vert
            #pragma fragment frag

            #include "OGlowingPass.cginc"
            
            ENDCG
        }
        
        // in this pass sound waves and fog are added
        Pass {
            Name "SoundWavePass"
            Blend One One // additive blending
            ZWrite Off // don't write to depth buffer

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "OWavePass.cginc"
            
            ENDCG
        }
    }
}