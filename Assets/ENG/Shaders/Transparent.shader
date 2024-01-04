Shader "_Orpheus/Unlit/Transparent" {
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
    }
    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        // in this pass ambient light and the first directional light are added
        Pass {
            Tags {
                "LightMode" = "ForwardBase"
            }

            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            ZWrite Off // don't write to depth buffer

            CGPROGRAM
            #pragma target 3.0 // use the best BRDF function
            #pragma vertex vert
            #pragma fragment frag

            #define IS_IN_BASE_PASS
            #define IS_TRANSPARENT

            #include "OLightingPass.cginc"
            
            ENDCG
        }
        
        // in this pass additional light sources are added
        Pass {
            Tags {
                "LightMode" = "ForwardAdd"
            }

            Blend SrcAlpha One
            ZWrite Off // don't write to depth buffer

            CGPROGRAM
            #pragma target 3.0 // use the best BRDF function
            #pragma vertex vert
            #pragma fragment frag
            
            #define IS_TRANSPARENT
            
            #pragma multi_compile DIRECTIONAL POINT SPOT // creates multiple variants of the shader

            #include "OLightingPass.cginc"
            
            ENDCG
        }
        
        // in this pass sound waves and fog are added
        Pass {
            Name "SoundWavePass"
            Blend SrcAlpha One
            ZWrite Off // don't write to depth buffer

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            #define IS_TRANSPARENT

            #include "OWavePass.cginc"
            
            ENDCG
        }
    }
}
