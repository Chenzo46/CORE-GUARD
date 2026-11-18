Shader "UI/FluidWobble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shape)]
        _Inset ("Inset (room for wobble, 0-0.4)", Range(0, 0.4)) = 0.12

        [Header(Edge Wobble)]
        _WobbleAmount ("Wobble Amount", Range(0, 0.2)) = 0.04
        _WobbleFrequency ("Wobble Frequency", Range(0.5, 12)) = 3
        _WobbleSpeed ("Wobble Speed", Range(0, 4)) = 0.8
        _EdgeBias ("Edge Bias (0 = whole image, 1 = edges only)", Range(0, 1)) = 0.7

        [Header(Squish And Stretch)]
        _SquishAmount ("Squish Amount", Range(0, 0.4)) = 0.08
        _SquishSpeed ("Squish Speed", Range(0, 8)) = 2
        _ShearAmount ("Sway Amount", Range(0, 0.3)) = 0.05
        _ShearSpeed ("Sway Speed", Range(0, 8)) = 1.3

        [Header(Misc)]
        _TimeOffset ("Time Offset (desync multiple images)", Float) = 0

        // Required by Unity UI (masks / stencil / clipping)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"   // we need the sprite's UVs to span 0..1
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _Inset;
            float _WobbleAmount;
            float _WobbleFrequency;
            float _WobbleSpeed;
            float _EdgeBias;
            float _SquishAmount;
            float _SquishSpeed;
            float _ShearAmount;
            float _ShearSpeed;
            float _TimeOffset;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // ---- smooth value noise ----
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Two-octave 2D flow vector in roughly -1..1
            float2 flowNoise(float2 p, float t)
            {
                float2 n1 = float2(
                    valueNoise(p + float2(t, t * 0.7)),
                    valueNoise(p + float2(31.7 - t * 0.8, 12.3 + t * 0.6)));
                float2 n2 = float2(
                    valueNoise(p * 2.1 + float2(-t * 1.3, 5.2 + t)),
                    valueNoise(p * 2.1 + float2(8.8 + t * 1.1, -t * 0.9)));
                return (n1 + 0.5 * n2) / 1.5 * 2.0 - 1.0;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t = _Time.y + _TimeOffset;

                // Centered coords (-0.5..0.5), image shrunk by inset so the
                // shape has room to bulge outward inside the rect.
                float2 c = IN.texcoord - 0.5;
                c /= max(1.0 - 2.0 * _Inset, 0.01);

                // Squash & stretch (volume preserving: x grows as y shrinks)
                float s = sin(t * _SquishSpeed) * _SquishAmount
                        + sin(t * _SquishSpeed * 0.43 + 1.7) * _SquishAmount * 0.5;
                float sx = 1.0 + s;
                float sy = 1.0 / sx;
                c /= float2(sx, sy);

                // Sway: shear that makes the shape lean side to side
                c.x += c.y * sin(t * _ShearSpeed) * _ShearAmount;
                c.y += c.x * sin(t * _ShearSpeed * 0.77 + 2.0) * _ShearAmount * 0.5;

                // Fluid domain warp; stronger toward the silhouette
                float edge = saturate(max(abs(c.x), abs(c.y)) * 2.0);
                edge = lerp(1.0, edge * edge, _EdgeBias);
                float2 warp = flowNoise(c * _WobbleFrequency, t * _WobbleSpeed);
                c += warp * _WobbleAmount * edge;

                float2 uv = c + 0.5;

                // Anything the warp pulled from outside the sprite is transparent
                float2 inside2 = step(0.0, uv) * step(uv, 1.0);
                float inside = inside2.x * inside2.y;

                half4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;
                color.a *= inside;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
