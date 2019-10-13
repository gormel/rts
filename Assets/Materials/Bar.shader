Shader "Unlit/Bar"
{
    Properties
    {
		_Value ("Percent", Float) = 1
		_Color1 ("LeftColor", Color) = (0, 0, 0, 0)
		_Color2 ("RightColor", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			float _Value;
			fixed4 _Color1;
			fixed4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				if (i.uv.x < _Value)
					return _Color1;

				return _Color2;
            }
            ENDCG
        }
    }
}
