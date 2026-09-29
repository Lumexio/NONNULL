Shader "Custom/RetroWireframe" {
    Properties {
        _Color ("Wire Color", Color) = (0, 0.502, 0.506, 1)
        _BgColor ("Background Color", Color) = (0.627, 0.627, 0.627, 1)
        _Width ("Width", Range(0, 1)) = 0.05
        _GridSize ("Grid Size", Float) = 20
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float4 _BgColor;
            float _Width;
            float _GridSize;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _GridSize;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 c = frac(i.uv);
                // Check if we are near the edge of a grid cell
                float f = (c.x < _Width || c.y < _Width) ? 1.0 : 0.0;
                return lerp(_BgColor, _Color, f);
            }
            ENDCG
        }
    }
}
