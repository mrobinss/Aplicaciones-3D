// Junta las dos imagenes del personaje de CharacterPreview: el color (con el post-procesado de los
// niveles, que en URP pierde la transparencia) y la silueta (sin post-procesado, con transparencia).
Shader "UI/CharacterPreview"
{
    Properties
    {
        [PerRendererData] _MainTex ("Color", 2D) = "black" {}
        _AlphaTex ("Silueta", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 rgb = tex2D(_MainTex, i.uv).rgb;
                fixed alpha = tex2D(_AlphaTex, i.uv).a;
                // El color se renderizo sobre negro: en los bordes se recupera dividiendo por la cobertura
                rgb = alpha > 0.01 ? saturate(rgb / alpha) : rgb;
                return fixed4(rgb * i.color.rgb, alpha * i.color.a);
            }
            ENDCG
        }
    }
}
