Shader "BitmapFont/Outline" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
		_AlphaMin ("Alpha Min", Range(0.0,1.0)) = 0.49
		_AlphaMax ("Alpha Max", Range(0.0,1.0)) = 0.54
        _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
		_ShadowAlphaMin ("Shadow Alpha Min", Range(0.0,1.0)) = 0.28
		_ShadowAlphaMax ("Shadow Alpha Max", Range(0.0,1.0)) = 0.54
		_ShadowOffsetU ("Shadow u-offset", Range(-1.0,1.0)) = 0
		_ShadowOffsetV ("Shadow v-offset", Range(-1.0,1.0)) = 0
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
	SubShader {
		Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
		}
		Lighting Off 
		Cull Off 
		ZTest Always 
		ZWrite Off 
		Fog { 
			Mode Off 
		}
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;
			float _AlphaMin;
			float _AlphaMax;
			float4 _ShadowColor;
			float _ShadowAlphaMin;
			float _ShadowAlphaMax;
			float _ShadowOffsetU;
			float _ShadowOffsetV;
			sampler2D _MainTex;

			//Unity-required vars
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float4 base = tex2D(_MainTex, i.uv);
				float alpha = smoothstep(_AlphaMin, _AlphaMax, base.w);

				float2 shadowUV = i.uv + float2(_ShadowOffsetU * _MainTex_ST.x, _ShadowOffsetV * _MainTex_ST.y);
				float4 shadowtexel = tex2D(_MainTex, shadowUV);
				float shadowAlpha = smoothstep(_ShadowAlphaMin, _ShadowAlphaMax, shadowtexel.w);
				float4 shadow = _ShadowColor * shadowAlpha;

				return mix(shadow, _Color, alpha);
				//return _Color * base.a;
			}

			ENDCG
		}
	} 
}
