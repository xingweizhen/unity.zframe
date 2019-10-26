// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Stroke"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                //float4 tangent  : TANGENT;    // 切线在缩放的情况下，xy值会错误。所以放弃使用
                fixed3 normal   : NORMAL;
                fixed4 color    : COLOR;
                fixed2 texcoord : TEXCOORD0;
                fixed2 uv1      : TEXCOORD1;
                fixed2 uv2      : TEXCOORD2;
                float2 uv3      : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 tangent  : TANGENT;
                fixed4 color    : COLOR;
                fixed2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                fixed4 strokeColor   : TEXCOORD2;
                half   strokeWidth   : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;
                OUT.tangent = float4(v.uv1.x, v.uv1.y, v.uv2.x, v.uv2.y);

                OUT.color = v.color * _Color;
                OUT.strokeColor = fixed4(v.normal.x, v.normal.y, v.normal.z, v.uv3.x);
                OUT.strokeWidth = v.uv3.y;
                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed IsInRect(float2 pPos, float4 pClipRect)
            {
                pPos = step(pClipRect.xy, pPos) * step(pPos, pClipRect.zw);
                return pPos.x * pPos.y;
            }

            fixed SampleAlpha(int pIndex, v2f IN)
            {
                const fixed sinArray[12] = { 0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5 };
                const fixed cosArray[12] = { 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866 };
                half strokeWidth = IN.strokeWidth;
                fixed4 strokeColor = IN.strokeColor;
                float2 pos = IN.texcoord + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * strokeWidth;
                return IsInRect(pos, IN.tangent) * (tex2D(_MainTex, pos) + _TextureSampleAdd).a * strokeColor.a;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                fixed4 strokeColor = IN.strokeColor;

                color.a *= IsInRect(IN.texcoord, IN.tangent);
                half4 val = half4(strokeColor.r, strokeColor.g, strokeColor.b, 0);

                val.w += SampleAlpha(0, IN);
                val.w += SampleAlpha(1, IN);
                val.w += SampleAlpha(2, IN);
                val.w += SampleAlpha(3, IN);
                val.w += SampleAlpha(4, IN);
                val.w += SampleAlpha(5, IN);
                val.w += SampleAlpha(6, IN);
                val.w += SampleAlpha(7, IN);
                val.w += SampleAlpha(8, IN);
                val.w += SampleAlpha(9, IN);
                val.w += SampleAlpha(10, IN);
                val.w += SampleAlpha(11, IN);

                val.w = clamp(val.w, 0, 1);
                color = (val * (IN.color.a - color.a)) + (color * color.a);
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}


