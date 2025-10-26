Shader "UI/UIHoleShape_Fixed"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0,0,0,0.75)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.2
        _HoleSize ("Hole Size (halfWidth, halfHeight)", Vector) = (0.2, 0.3, 0, 0)
        _HoleShape ("Hole Shape (0:Circle, 1:Rect)", Int) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _HoleCenter;   // (x, y) in UV space
            float _HoleRadius;    // For circle
            float4 _HoleSize;     // (halfWidth, halfHeight) for rectangle
            int _HoleShape;       // 0 = Circle, 1 = Rectangle

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y; // width / height
                float2 uv = i.uv;

                if (_HoleShape == 0) // Circle
                {
                    float2 diff = float2((uv.x - _HoleCenter.x) * aspect, uv.y - _HoleCenter.y);
                    float dist = length(diff);

                    if (dist < _HoleRadius)
                        discard;
                }
                else if (_HoleShape == 1) // Rectangle
                {
                    float2 diff = float2((uv.x - _HoleCenter.x) * aspect, uv.y - _HoleCenter.y);

                    if (abs(diff.x) < _HoleSize.x && abs(diff.y) < _HoleSize.y)
                        discard;
                }

                return _Color;
            }
            ENDCG
        }
    }
}
