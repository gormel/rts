Shader "Custom/Crystal" {
Properties{
	[NoScaleOffset]
	_MainTex("Diffuse", 2D) = "white"{}
	[HideInInspector]
	_Tex1("Diffuse", 2D) = "white"{}
	[NoScaleOffset]
	_ReflMap("Reflect Map", CUBE) = ""{}
	_ReflAmount("Reflect Amount", Range(0.01, 2)) = 1
	_RimPower("Rim Power", Range(0.5,8.0)) = 3.0
	_Blend("Map Blend", Range(0,1)) = 0
}
SubShader{
	Tags{ "RenderType" = "Opaque" }
	//LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

	sampler2D _MainTex;
	
	samplerCUBE _ReflMap;
	fixed _ReflAmount;
	fixed _RimPower;
	fixed _Blend;

	struct Input
	{
		fixed3 worldRefl;
		fixed3 viewDir;
		fixed2 uv_MainTex:TEXCOORD0;
		fixed2 uv2_Tex1:TEXCOORD1;
	};

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
		
		fixed3 t0 = tex2D(_MainTex, IN.uv_MainTex);
		fixed3 t1 = tex2D(_MainTex, IN.uv2_Tex1);
		o.Albedo = lerp(t0, t1, _Blend);
		o.Emission = texCUBE(_ReflMap, IN.worldRefl).rgb * pow(rim, _RimPower) * _ReflAmount;
	}
	ENDCG
	}
}
