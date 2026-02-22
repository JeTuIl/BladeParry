Shader "UI/LifeBarOrganic"
{
	Properties
	{
		_MainTex            ("Sprite Texture", 2D) = "white" {}
		_Color              ("Tint", Color) = (1,1,1,1)

		_StencilComp        ("Stencil Comparison", Float) = 8
		_Stencil            ("Stencil ID", Float) = 0
		_StencilOp          ("Stencil Operation", Float) = 0
		_StencilWriteMask   ("Stencil Write Mask", Float) = 255
		_StencilReadMask    ("Stencil Read Mask", Float) = 255

		_CullMode           ("Cull Mode", Float) = 0
		_ColorMask          ("Color Mask", Float) = 15
		_ClipRect           ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		[Header(Organic Pattern)]
		_PatternSpeed       ("Pattern Speed", Range(0, 2)) = 0.3
		_PatternStrength    ("Pattern Strength", Range(0, 1)) = 0.15

		[Header(Border Glow)]
		_BorderGlowColor    ("Border Glow Color", Color) = (1, 1, 1, 0.4)
		_BorderGlowWidth    ("Border Glow Width", Range(0, 0.5)) = 0.12

		[Header(Highlight Edge)]
		_HighlightSide       ("Highlight Side (0=None 1=L 2=R 3=T 4=B)", Range(0, 4)) = 0
		_HighlightGlowColor  ("Highlight Glow Color", Color) = (1, 1, 1, 0.7)
		_HighlightGlowWidth  ("Highlight Width", Range(0, 0.5)) = 0.08
		_HighlightWaveSpeed  ("Wave Speed", Range(0, 5)) = 1.5
		_HighlightWaveScale  ("Wave Frequency", Range(0.5, 20)) = 6

		[Header(Bubble Overlay)]
		_BubbleCount         ("Bubble Count", Range(1, 32)) = 5
		_BubbleColor         ("Bubble Color (darkens behind)", Color) = (0.5, 0.5, 0.5, 0.5)
		_BubbleSpeed         ("Bubble Speed", Range(0.2, 2)) = 0.5
		_BubbleSizeMin       ("Bubble Size Min", Range(0.02, 0.3)) = 0.04
		_BubbleSizeMax       ("Bubble Size Max", Range(0.02, 0.3)) = 0.12
		_BubbleWaveAmplitude ("Bubble Wave Amplitude", Range(0, 0.25)) = 0.08
		_BubbleWaveFreqMin   ("Bubble Wave Freq Min", Range(0.3, 8)) = 0.8
		_BubbleWaveFreqMax   ("Bubble Wave Freq Max", Range(0.3, 8)) = 3

		[Header(Particle Overlay)]
		_ParticleGradient    ("Particle Gradient (1D: left=spawn, right=trail end)", 2D) = "white" {}
		_ParticleIntensity   ("Particle Intensity", Range(0, 2)) = 1
		_ParticleCount       ("Particle Count", Range(1, 128)) = 12
		_ParticleSpeed       ("Particle Speed", Range(0.2, 3)) = 1
		_ParticleSize        ("Particle Size", Range(0.01, 0.5)) = 0.04
		_ParticleFadeLength ("Particle Fade Length", Range(0.05, 1)) = 0.35
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull [_CullMode]
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
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex        : SV_POSITION;
				fixed4 color        : COLOR;
				float2 texcoord     : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float4 mask         : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
			float _UIMaskSoftnessX;
			float _UIMaskSoftnessY;
			int _UIVertexColorAlwaysGammaSpace;

			float _PatternSpeed;
			float _PatternStrength;
			fixed4 _BorderGlowColor;
			float _BorderGlowWidth;
			float _HighlightSide;
			fixed4 _HighlightGlowColor;
			float _HighlightGlowWidth;
			float _HighlightWaveSpeed;
			float _HighlightWaveScale;
			float _BubbleCount;
			fixed4 _BubbleColor;
			float _BubbleSpeed;
			float _BubbleSizeMin;
			float _BubbleSizeMax;
			float _BubbleWaveAmplitude;
			float _BubbleWaveFreqMin;
			float _BubbleWaveFreqMax;
			sampler2D _ParticleGradient;
			float4 _ParticleGradient_ST;
			float _ParticleIntensity;
			float _ParticleCount;
			float _ParticleSpeed;
			float _ParticleSize;
			float _ParticleFadeLength;

			// Value noise helper for organic pattern (no extra texture)
			float hash(float2 p)
			{
				float3 p3 = frac(float3(p.xyx) * 0.1031);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.x + p3.y) * p3.z);
			}

			float valueNoise(float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac(uv);
				f = f * f * (3.0 - 2.0 * f);

				float a = hash(i);
				float b = hash(i + float2(1, 0));
				float c = hash(i + float2(0, 1));
				float d = hash(i + float2(1, 1));

				return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
			}

			float organicPattern(float2 uv)
			{
				float t = _Time.y * _PatternSpeed;
				// Multiple layers in different directions so they overlap and appear more random
				float n1 = valueNoise(uv * 4.0 + float2(t * 0.5, 0));
				float n2 = valueNoise(uv * 6.0 + float2(0, t * 0.3));
				float n3 = valueNoise(uv * 5.0 + float2(t * -0.4, t * 0.35));
				float n4 = valueNoise(uv * 7.0 + float2(t * 0.25, t * -0.2));
				return (n1 + n2 + n3 + n4) * 0.25;
			}

			void borderGradients(float2 uv, out float gradU, out float gradV, out float shortSidePixels)
			{
				gradU = length(float2(ddx(uv.x), ddy(uv.x)));
				gradV = length(float2(ddx(uv.y), ddy(uv.y)));
				gradU = max(gradU, 1e-5);
				gradV = max(gradV, 1e-5);
				shortSidePixels = min(1.0 / gradU, 1.0 / gradV);
			}

			float edgeFactorForSide(float2 uv, float side, float gradU, float gradV, float shortSidePixels, float width)
			{
				float edgeDistPixels = 0.0;
				if (side <= 0.5) return 0.0;
				if (side <= 1.5) edgeDistPixels = uv.x / gradU;
				else if (side <= 2.5) edgeDistPixels = (1.0 - uv.x) / gradU;
				else if (side <= 3.5) edgeDistPixels = uv.y / gradV;
				else edgeDistPixels = (1.0 - uv.y) / gradV;
				float edgeDistNorm = edgeDistPixels / max(shortSidePixels, 1e-5);
				return 1.0 - saturate(edgeDistNorm / max(width, 0.001));
			}

			float highlightWaveAlpha(float2 uv, float side)
			{
				float along = 0.0, perp = 0.0;
				if (side <= 1.5) { along = uv.y; perp = uv.x; }       // Left/Right
				else if (side <= 2.5) { along = uv.y; perp = 1.0 - uv.x; }
				else { along = uv.x; perp = uv.y; }                   // Top/Bottom (along=x, perp=y)
				float t = _Time.y * _HighlightWaveSpeed;
				float k = _HighlightWaveScale * 6.28318;
				// Several waves in different directions and speeds so the result is less regular
				float w1 = sin(along * k - t);
				float w2 = sin(along * k * 1.37 + perp * 4.0 - t * 0.73);
				float w3 = sin(along * k * 0.61 - perp * 3.5 + t * 1.2);
				float w4 = sin(along * k * 1.9 + perp * 2.5 + t * 0.5);
				float combined = (w1 + w2 + w3 + w4) * 0.25;
				return 0.5 + 0.5 * combined;
			}

			float borderGlowFactor(float2 uv)
			{
				float gradU, gradV, shortSidePixels;
				borderGradients(uv, gradU, gradV, shortSidePixels);
				float dLeft = uv.x / gradU;
				float dRight = (1.0 - uv.x) / gradU;
				float dTop = uv.y / gradV;
				float dBottom = (1.0 - uv.y) / gradV;
				float edgeDistPixels = min(min(dLeft, dRight), min(dTop, dBottom));
				float edgeDistNorm = edgeDistPixels / max(shortSidePixels, 1e-5);
				return 1.0 - saturate(edgeDistNorm / max(_BorderGlowWidth, 0.001));
			}

			float bubbleOverlay(float2 uv, float side, float gradU, float gradV, float shortSidePixels)
			{
				if (side <= 0.5) return 0.0;
				float t = _Time.y * _BubbleSpeed;
				float bubbleSum = 0.0;
				float countF = _BubbleCount - 0.5;
				float shortPix = max(shortSidePixels, 1.0);
				for (int i = 0; i < 32; i++)
				{
					float fi = (float)i;
					float r = hash(float2(fi * 0.03125, 0.1));
					float s = 0.85 + 0.3 * hash(float2(fi * 0.03125 + 0.05, 0.15));
					float sizeRand = hash(float2(fi * 0.03125 + 0.02, 0.2));
					float sizeFrac = lerp(_BubbleSizeMin, _BubbleSizeMax, sizeRand);
					float radiusPixels = sizeFrac * shortPix;
					float innerR = radiusPixels * 0.35;
					float outerR = radiusPixels;
					float midR = (innerR + outerR) * 0.55;
					float phase = t * s + fi * 0.1 + r * 0.5;
					float centerAlong = (side <= 1.5) ? frac(phase) : (side <= 2.5) ? (1.0 - frac(phase)) : (side <= 3.5) ? (1.0 - frac(phase)) : frac(phase);
					float waveFreq = lerp(_BubbleWaveFreqMin, _BubbleWaveFreqMax, hash(float2(fi * 0.03125 + 0.03, 0.25)));
					float waveOffset = _BubbleWaveAmplitude * sin(centerAlong * 6.28318 * waveFreq);
					float centerAcross = saturate(r + waveOffset);
					float2 center = (side <= 2.5) ? float2(centerAlong, centerAcross) : float2(centerAcross, centerAlong);
					float dPixels = length(float2((uv.x - center.x) / gradU, (uv.y - center.y) / gradV));
					float noiseVal = valueNoise(uv * 10.0 + center);
					float dEff = dPixels * (1.0 + 0.12 * (noiseVal - 0.5));
					float blob = smoothstep(innerR, midR, dEff) * (1.0 - smoothstep(midR, outerR, dEff));
					float fade = smoothstep(0.0, 0.15, centerAlong) * smoothstep(1.0, 0.85, centerAlong);
					bubbleSum = 1.0 - (1.0 - bubbleSum) * (1.0 - blob * fade * step(fi, countF));
				}
				return saturate(bubbleSum);
			}

			float4 particleOverlay(float2 uv, float side, float gradU, float gradV, float shortSidePixels)
			{
				if (side <= 0.5) return float4(0, 0, 0, 0);
				float t = _Time.y * _ParticleSpeed;
				float4 particleSum = float4(0, 0, 0, 0);
				float countF = _ParticleCount - 0.5;
				float shortPix = max(shortSidePixels, 1.0);
				float radiusPixels = _ParticleSize * shortPix;
				for (int i = 0; i < 128; i++)
				{
					float fi = (float)i;
					float r = hash(float2(fi * 0.0078125 + 0.4, 0.1));
					float s = 0.9 + 0.2 * hash(float2(fi * 0.0078125 + 0.45, 0.15));
					float phase = t * s + fi * 0.08 + r * 0.3;
					float fadeLen = max(0.05, _ParticleFadeLength);
					float centerAlong = (side <= 1.5 || side > 3.5) ? (frac(phase) * fadeLen) : (1.0 - frac(phase) * fadeLen);
					float distFromSpawn = (side <= 1.5 || side > 3.5) ? centerAlong : (1.0 - centerAlong);
					float2 gradUV = float2(distFromSpawn, 0.5);
					gradUV = gradUV * _ParticleGradient_ST.xy + _ParticleGradient_ST.zw;
					float4 gradColor = tex2D(_ParticleGradient, gradUV);
					float2 center = (side <= 2.5) ? float2(centerAlong, r) : float2(r, centerAlong);
					float dPixels = length(float2((uv.x - center.x) / gradU, (uv.y - center.y) / gradV));
					float blob = 1.0 - smoothstep(0.0, radiusPixels, dPixels);
					float distFade = 1.0 - smoothstep(0.0, max(0.05, _ParticleFadeLength), distFromSpawn);
					float w = blob * distFade * step(fi, countF);
					particleSum += gradColor * w;
				}
				particleSum = saturate(particleSum);
				particleSum.rgb *= particleSum.a;
				particleSum.a = 1.0;
				return particleSum;
			}

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				float4 vPosition = UnityObjectToClipPos(v.vertex);
				OUT.worldPosition = v.vertex;
				OUT.vertex = vPosition;

				float2 pixelSize = vPosition.w;
				pixelSize /= abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

				float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

				if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
					v.color.rgb = UIGammaToLinear(v.color.rgb);
				OUT.color = v.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				float pattern = organicPattern(IN.texcoord);
				float variation = 1.0 + _PatternStrength * (pattern - 0.5) * 2.0;
				color.rgb *= variation;

				float glowF = borderGlowFactor(IN.texcoord);
				float glowPattern = 0.6 + 0.4 * organicPattern(IN.texcoord);
				color.rgb += _BorderGlowColor.rgb * _BorderGlowColor.a * glowF * glowPattern;

				float gradU, gradV, shortSidePixels;
				borderGradients(IN.texcoord, gradU, gradV, shortSidePixels);
				float highlightF = edgeFactorForSide(IN.texcoord, _HighlightSide, gradU, gradV, shortSidePixels, _HighlightGlowWidth);
				float waveAlpha = highlightWaveAlpha(IN.texcoord, _HighlightSide);
				color.rgb += _HighlightGlowColor.rgb * _HighlightGlowColor.a * highlightF * waveAlpha;

				float bubbleSum = bubbleOverlay(IN.texcoord, _HighlightSide, gradU, gradV, shortSidePixels);
				color.rgb *= lerp(1.0, _BubbleColor.rgb, _BubbleColor.a * bubbleSum);

				float4 particleSum = particleOverlay(IN.texcoord, _HighlightSide, gradU, gradV, shortSidePixels);
				color.rgb += particleSum.rgb * _ParticleIntensity;

				#if UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color *= m.x * m.y;
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
