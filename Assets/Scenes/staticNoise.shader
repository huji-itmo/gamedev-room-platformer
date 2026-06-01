Shader "Noise/WhiteNoiseFlicker"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_AspectRatio("Aspect Ratio (Width/Height)", Float) = 1.777 
		_Scale("Noise Scale", Float) = 1.0 
		_Speed("Flicker Speed", Float) = 10.0 
	}
	SubShader
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Cull Off
		ZWrite On
		ZTest LEqual

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _Color;
			float _AspectRatio;
			float _Scale;
			float _Speed;

			// Хеш-функция, принимающая 3 компонента (x, y, time)
		 float hash(float3 p)
			{
				p = frac(p * 0.1031);
				p += dot(p, p.zyx + 33.33);
				return frac((p.x + p.y) * p.z);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// 1. Получаем координаты в пикселях
				float2 pixelCoords = i.uv * _ScreenParams.xy;
				
				// 2. Применяем масштаб
				pixelCoords /= _Scale;
				
				// 3. Корректируем аспект (делим X на соотношение сторон)
				pixelCoords.x /= _AspectRatio;
				
				// 4. Генерируем шум, используя время как третье измерение
				// Это гарантирует, что шум не двигается, а именно меняет значение во времени
				float timeVal = _Time.y * _Speed;
				
				// Используем floor, чтобы весь "пиксель" шума менялся одновременно, 
				// создавая эффект четких квадратных блоков, а не размытия
				float3 noiseInput = float3(floor(pixelCoords), floor(timeVal));
				
				float f = hash(noiseInput);
				
				return float4(f, f, f, 1.0);
			}
			ENDCG
		}
	}
}