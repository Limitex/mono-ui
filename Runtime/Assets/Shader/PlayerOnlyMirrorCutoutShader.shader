Shader "Limitex/MonoUI/Mirror/PlayerOnlyMirrorCutoutShader"
{
    Properties
    {
        // Main textures
        [Header(Main Settings)]
        _MainTex("Base (RGB)", 2D) = "white" {}
        [HideInInspector] _ReflectionTex0("Right Eye Reflection", 2D) = "white" {}
        [HideInInspector] _ReflectionTex1("Left Eye Reflection", 2D) = "white" {}

        // Visual settings
        [Header(Visual Settings)]
        [ToggleUI(HideBackground)] _HideBackground("Hide Background", Float) = 1
        [ToggleUI(IgnoreEffects)] _IgnoreEffects("Ignore Effects", Float) = 0
        [ToggleUI(SmoothEdge)] _SmoothEdge("Smooth Edge", Float) = 1
        _AlphaTweakLevel("Alpha Tweak Level", Range(0,1)) = 0.75

        // Stencil settings
        [Header(Stencil Settings)]
        [Space(20)]
        _Stencil ("Stencil ID", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareAction ("Compare Function", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Pass Operation", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Fail Operation", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail Operation", int) = 0
        _StencilWriteMask ("Write Mask", Float) = 255
        _StencilReadMask ("Read Mask", Float) = 255
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent+1"
            "IgnoreProjector" = "True"
        }

        // Render settings
        ZWrite On
        AlphaToMask On
        LOD 100

        // Stencil state
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilCompareAction]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            // Texture inputs
            sampler2D _MainTex;
            sampler2D _ReflectionTex0;
            sampler2D _ReflectionTex1;

            // Properties
            float4 _MainTex_ST;
            float _HideBackground;
            float _IgnoreEffects;
            float _SmoothEdge;
            float _AlphaTweakLevel;

            // Vertex input structure
            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Fragment input structure
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 refl : TEXCOORD1;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Helper functions
            half CalculateLuminance(half3 color)
            {
                return dot(color, fixed3(1, 1, 1)) / 3;
            }

            half ProcessAlpha(half baseAlpha, half power)
            {
                bool applyIgnoreEffects = !_IgnoreEffects && power > 0.01;
                
                if (_SmoothEdge)
                {
                    half alpha = baseAlpha > 0 ? baseAlpha : 
                                applyIgnoreEffects ? power : 0;
                    return smoothstep(0, _AlphaTweakLevel, alpha);
                }
                
                return baseAlpha > 0 ? 1 : 
                       applyIgnoreEffects ? 1 : 0;
            }

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.refl = ComputeNonStereoScreenPos(o.pos);

                return o;
            }

            // Fragment shader
            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Sample textures
                half4 tex = tex2D(_MainTex, i.uv);
                half4 refl = unity_StereoEyeIndex == 0 
                    ? tex2Dproj(_ReflectionTex0, UNITY_PROJ_COORD(i.refl)) 
                    : tex2Dproj(_ReflectionTex1, UNITY_PROJ_COORD(i.refl));

                // Process alpha
                if (_HideBackground) 
                {
                    half power = CalculateLuminance(refl.rgb);
                    refl.a = ProcessAlpha(refl.a, power);
                    
                    // Clip for AlphaToMask compatibility
                    clip(_SmoothEdge ? refl.a - 0.01 : refl.a);
                }
                else 
                {
                    refl.a = 1;
                }

                // Apply base texture
                refl *= tex;
                return refl;
            }
            ENDCG
        }
    }
}
