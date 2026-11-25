Shader "PulauKabut/UI_OctagonMinimap"
{
    Properties
    {
        _MainTex ("Map RT", 2D) = "white" {}
        _FogOfWarTex ("Fog Of War", 2D) = "black" {}
        _PlayerPos ("Player XZ + Radius + YawRad", Vector) = (0, 0, 38, 0)
        _WorldBounds ("MinX, MinZ, SizeX, SizeZ", Vector) = (-90, -140, 200, 260)
        _UnexploredColor ("Unexplored Color", Color) = (0.047, 0.133, 0.161, 0.96)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _FogOfWarTex;
            float4 _PlayerPos;
            float4 _WorldBounds;
            fixed4 _UnexploredColor;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. Potong rapi berbentuk oktagon
                float2 p = abs(i.uv - 0.5) * 2.0;
                float octD = max(max(p.x, p.y), (p.x + p.y) * 0.70710678);
                float octAlpha = saturate((0.975 - octD) * 40.0);

                // 2. Hitung posisi dunia (XZ) piksel minimap ini (sinkron rotasi kamera GTA!)
                float2 local = (i.uv - 0.5) * 2.0 * _PlayerPos.z; // (-R..+R)
                float c = cos(_PlayerPos.w);
                float s = sin(_PlayerPos.w);
                float worldX = _PlayerPos.x + (local.x * c + local.y * s);
                float worldZ = _PlayerPos.y + (-local.x * s + local.y * c);

                float2 fowUV = float2(
                    (worldX - _WorldBounds.x) / _WorldBounds.z,
                    (worldZ - _WorldBounds.y) / _WorldBounds.w);

                float explored = 0.0;
                if (fowUV.x >= 0.0 && fowUV.x <= 1.0 && fowUV.y >= 0.0 && fowUV.y <= 1.0)
                {
                    explored = tex2D(_FogOfWarTex, fowUV).a;
                }

                fixed4 mapCol = tex2D(_MainTex, i.uv);
                fixed4 col = lerp(_UnexploredColor, mapCol, explored);
                col.a = lerp(_UnexploredColor.a, 1.0, explored) * octAlpha;
                return col;
            }
            ENDCG
        }
    }
}
