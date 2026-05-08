Shader "Hidden/VolFx/CustomScreen"
{
    SubShader
    {
        name "CustomScreen"
        Tags { "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass // 0
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _DistortionTex;

            #pragma multi_compile_local _ ENABLE_CHANNEL_SHIFT

            float4  _ChannelShift;
            #define _DistortionOffset _ChannelShift.zw

            float4  _DistortionData;
            #define _DistortionScale _DistortionData.x
            #define _Noise           _DistortionData.y   // 현재 0f 고정
            #define _Quantization    _DistortionData.z   // 현재 15f 고정
            #define _Glitch          _DistortionData.w   // 채널시프트 강도 배수

            float4  _FxData;
            #define _Intensity           _FxData.x   // YCbCr결과 ↔ 글리치결과 혼합 비율
            #define _ApplyDistortY       _FxData.y   // 밝기 채널 UV 왜곡 강도
            #define _ApplyDistortChroma  _FxData.z   // 0f 고정
            #define _ApplyDistortGlitch  _FxData.w   // 글리치 UV 왜곡 강도

            // ★ 글리치(채널시프트) 색조: 채널시프트가 일어나는 픽셀에 곱해지는 색상
            // (1,1,1,1) = 원본 색 그대로, (1,0,0,1) = 빨간 색조
            float4 _GlitchTint;

            // ★ Y왜곡 색조: applyToY로 어긋난 픽셀에 곱해지는 색상
            // 왜곡 오프셋 크기가 마스크 역할 → 많이 어긋날수록 Tint가 강하게 적용됨
            float4 _DistortYTint;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o; o.vertex = v.vertex; o.uv = v.uv;
                return o;
            }

            // ── RGB ↔ YCbCr 변환 ──────────────────────────────────────────
            float3 RGBtoYCbCr(float3 col)
            {
                float3x3 m = float3x3(
                     0.299,     0.587,     0.114,
                    -0.168736, -0.331264,  0.5,
                     0.5,     -0.418688, -0.081312
                );
                return mul(m, col);
            }

            float3 YCbCrtoRGB(float3 col)
            {
                float3x3 m = float3x3(
                    1.0,  0.0,      1.402,
                    1.0, -0.34414, -0.71414,
                    1.0,  1.772,    0.0
                );
                return mul(m, col);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;

                // 왜곡 텍스처 샘플링
                // d.rg: UV 왜곡 방향 벡터 기반값 (-0.5 ~ 0.5)
                // d.b : 왜곡 강도(strength)
                // d.a : 추가 노이즈 (Noise=0 고정이라 현재 미사용)
                float4 d = tex2D(_DistortionTex, uv * _DistortionScale + _DistortionOffset + 0.5) - float4(.5, .5, 0, 0);
                float2 dir      = d.rg * 2.0 * 0.02;
                float  strength = d.b;

                // ── YCbCr 경로 ───────────────────────────────────────────
                // Y(밝기) 채널만 UV 왜곡 적용 (ApplyDistortChroma=0 이므로 색차 왜곡 없음)
                float2 yOffset = dir * strength * _ApplyDistortY;
                float2 uvY     = uv + yOffset + 0.5;

                // blockUV: ApplyDistortChroma=0이므로 원본 UV 그대로
                float2 uvChroma  = uv + 0.5;
                float2 blockUV   = uvChroma;

                float4 rgbaY = tex2D(_MainTex, uvY);
                float4 rgbaC = tex2D(_MainTex, blockUV);
                // 원본 위치(어긋나지 않은 기준 픽셀)의 Y 정보 샘플링
                float4 rgbaY_original = tex2D(_MainTex, uv + 0.5);

                float  alphaY = (rgbaY.a + rgbaC.a) * .5;

                float3 yccY = RGBtoYCbCr(rgbaY.rgb);
                float3 yccC = RGBtoYCbCr(rgbaC.rgb);
                float3 yccY_orig = RGBtoYCbCr(rgbaY_original.rgb);

                // CbCr 색차 계단화 (JPEG 색 뭉침 연출, Quantization=15 고정)
                yccC.yz = floor(yccC.yz * _Quantization) / _Quantization;

                float3 yccMix = (yccY.xyz + yccC.xyz) * .5;
                float3 col    = YCbCrtoRGB(yccMix);

                // ★ Y왜곡 실선 색조 적용
                // 어긋난 Y값과 원본 Y값의 차이를 구해서, 바뀐 부분(실선)만 마스크로 잡는다.
                // 차이가 큰 곳은 왜곡으로 인해 튀어나온 실선 부분이다.
                float diffY = abs(yccY.x - yccY_orig.x);
                float distortMask = saturate(diffY * 5.0); // 계수는 실선 부각 강도 (필요시 조절 가능)

                // 마스크 영역에만 _DistortYTint 색조 곱하기
                col = lerp(col, col * _DistortYTint.rgb, distortMask);

                // ── 글리치(채널시프트) 경로 ──────────────────────────────
                // ApplyDistortGlitch 값만큼 글리치 UV도 왜곡됨
                float4 glitchCol;
                {
                    float2 uvG = uv + dir * strength * _ApplyDistortGlitch + 0.5;

                #if ENABLE_CHANNEL_SHIFT
                    // R채널: uvG + ChannelShift.xy 방향으로 샘플링
                    // G채널: uvG 그대로 샘플링
                    // B채널: uvG - ChannelShift.xy 방향으로 샘플링
                    // → R과 B가 반대 방향으로 어긋나 색수차(chromatic aberration) 효과 발생
                    float4 r = tex2D(_MainTex, uvG + _ChannelShift.xy);
                    float4 g = tex2D(_MainTex, uvG);
                    float4 b = tex2D(_MainTex, uvG - _ChannelShift.xy);

                    // _Glitch가 0에 가까울수록 채널 분리가 강하고,
                    // 1에 가까울수록 원본 g 픽셀로 수렴
                    glitchCol = lerp(float4(r.r, g.g, b.b, dot(float3(r.a, g.a, b.a), float3(.333, .334, .333))), g, _Glitch);
                #else
                    glitchCol = tex2D(_MainTex, uvG);
                #endif

                    // ★ 글리치 결과에 색조(Tint) 곱하기
                    // _GlitchTint = (1,1,1,1)이면 원본, (1,0,0,1)이면 빨간 색수차
                    // 채널시프트가 꺼진(ENABLE_CHANNEL_SHIFT 없는) 경우에도 동일하게 적용됨
                    glitchCol *= _GlitchTint;
                }

                // ── 최종 혼합 ────────────────────────────────────────────
                // _Intensity(mixAmt)에 따라 YCbCr결과와 글리치결과를 lerp
                // 0 → YCbCr 왜곡 결과만 출력
                // 1 → 글리치(채널시프트 + 색조) 결과만 출력
                float  mixAmt = _Intensity;
                float4 result = lerp(float4(col, alphaY), glitchCol, mixAmt);
                result.a = (alphaY + glitchCol.a) * .5;

                return result;
            }

            ENDHLSL
        }
    }
}
