using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace VolFx
{
	[ShaderName("Hidden/VolFx/CustomScreen")] // 에디터 시작시 VolFx 베이스가 셰이더를 이름으로 탐색해 머티리얼을 생성
    public class CustomScreenPass : VolFx.Pass
	{
		// ── 셰이더 프로퍼티 ID 캐시 ──────────────────────────────────────────
		// Shader.PropertyToID()는 한 번만 호출해서 int로 캐싱해두는 것이 성능상 유리

		// _ChannelShift (float4):
		//   xy = 채널시프트 UV 오프셋 (R채널과 B채널을 각각 +xy, -xy 방향으로 어긋나게 샘플링)
		//   zw = 왜곡 텍스처 UV 오프셋 (매 fps 갱신마다 랜덤하게 이동 → 왜곡 패턴이 바뀌는 효과)
		private static readonly int s_ChannelShift   = Shader.PropertyToID("_ChannelShift");

		// _DistortionTex: CPU에서 프로시저럴하게 생성한 256×256 노이즈 텍스처.
		//   셰이더에서 UV 왜곡 방향(rg), 강도(b), 추가 노이즈(a)로 사용된다.
		private static readonly int s_DistortionTex  = Shader.PropertyToID("_DistortionTex");

		// _DistortionData (float4):
		//   x = DistortionScale : 왜곡 텍스처 샘플링 스케일 (클수록 UV 이동 폭 ↑)
		//   y = Noise           : CbCr 노이즈 강도 → 0f로 고정 (색 노이즈 비활성)
		//   z = Quantization    : CbCr 색차 계단화 단계 수 → 15f로 고정 (JPEG 계조 뭉침 연출)
		//   w = Glitch          : 채널시프트 강도 배수 (channelShiftPow × (1 + spread) + 1)
		private static readonly int s_DistortionData = Shader.PropertyToID("_DistortionData");

		// _FxData (float4):  ★ 색에 직접 영향을 주는 핵심 파라미터 묶음
		//   x = Intensity      : lerp(YCbCr왜곡결과, 채널시프트결과, Intensity)의 t값
		//                        0이면 YCbCr 결과만, 1이면 채널시프트(글리치) 결과만 출력
		//   y = ApplyDistortY  : Y(밝기) 채널 UV 오프셋 강도 → 밝기가 물결치듯 왜곡됨
		//   z = ApplyDistortChroma : 0f 고정 (색차 왜곡 블록 비활성)
		//   w = ApplyDistortGlitch : 글리치 UV에 왜곡 오프셋 적용 강도
		private static readonly int s_FxData         = Shader.PropertyToID("_FxData");

		// ★ _GlitchTint (float4): 글리치(채널시프트) 결과에 곱해지는 색조
		private static readonly int s_GlitchTint     = Shader.PropertyToID("_GlitchTint");

		// ★ _DistortYTint (float4): _applyToY로 어긋난 영역에만 곱해지는 색조
		//   왜곡 오프셋 크기를 마스크로 사용 → 많이 어긋난 픽셀일수록 Tint가 강하게 적용됨
		private static readonly int s_DistortYTint   = Shader.PropertyToID("_DistortYTint");

		// ── 런타임 상태 ──────────────────────────────────────────────────────

		// 마지막으로 fps 갱신이 일어난 시간 (Time.time 기준)
		// _fps 주기마다, 혹은 _fpsBreak 확률로 갱신되며 왜곡 오프셋이 새로 결정된다
		private float     _fpsLastFrame;

		// 현재 프레임에서 왜곡 텍스처를 샘플링할 UV 오프셋
		// fps 갱신 시 0~37 범위의 랜덤 값으로 새로 설정된다
		// (37은 텍스처 Repeat 패턴이 눈에 안 띄게 하기 위한 임의 오프셋)
		private Vector2   _distortionOffset;

		// 프로시저럴 왜곡 텍스처. 최초 Validate 시 한 번만 생성하고 이후 재사용
		private Texture2D _distortionMap;

		// 이번 fps 갱신에서 결정된 채널시프트 랜덤 확산량
		// _channelShiftSpread × Random.value 로 계산되어 셰이더에서 UV 어긋남 폭으로 쓰인다
		private float _channelSpread;

		// 현재 셰이더 키워드 ENABLE_CHANNEL_SHIFT의 활성 상태
		// 상태가 바뀔 때만 Enable/DisableKeyword를 호출하기 위한 플래그
		private bool  _channelShiftIsOn;

        // VolFx 베이스 클래스에서 [ShaderName] 어트리뷰트를 우선 사용하므로 빈 문자열로 두어도 무방
        public override string ShaderName { get; } = string.Empty;

		// =======================================================================
		// Init() : 패스가 처음 활성화되거나 머티리얼이 재생성될 때 한 번 호출된다
		public override void Init()
		{
			// fps 타이머를 -1로 초기화 → 첫 프레임에서 반드시 갱신되도록 보장
			_fpsLastFrame    = -1f;
			// 채널시프트 키워드 상태를 꺼진 상태로 초기화
			_channelShiftIsOn = false;
		}

		// =======================================================================
		// Validate(mat) : 매 프레임 렌더 전에 호출된다
		//   - 볼륨 설정을 읽어 셰이더 파라미터를 갱신
		//   - 반환값이 false면 이 패스는 렌더를 건너뛴다
		public override bool Validate(Material mat)
		{
			var settings = Stack.GetComponent<CustomScreenVol>();

			// 볼륨 컴포넌트가 없거나 IsActive() 조건 미충족이면 렌더 스킵
			if (settings == null || !settings.IsActive())
				return false;

			// ── fps 기반 갱신 ────────────────────────────────────────────────
			// _fps(초당 갱신 횟수)에 따른 주기 경과, 또는 _fpsBreak 확률로 즉시 갱신
			// 여기서 결정된 값들이 이번 "프레임 묶음" 내내 유지된다.
			if (Time.time - _fpsLastFrame > (1f / settings._fps.value) || Random.value < settings._fpsBreak.value)
			{
				_fpsLastFrame = Time.time;

				// 왜곡 텍스처 샘플링 위치를 랜덤하게 이동 → 왜곡 패턴이 "점프"하는 느낌
				_distortionOffset = new Vector2(Random.value * 37f, Random.value * 37f);

				// ★ 채널시프트 확산량 랜덤 결정.
				// 이 값이 크면 R·G·B 채널이 더 크게 어긋나 색 분리가 두드러진다
				_channelSpread = settings._channelShiftSpread.value * Random.value;
			}

			// spread 파라미터가 0이면 채널 어긋남 없음
			if (settings._channelShiftSpread.value == 0f)
				_channelSpread = 0f;

			// 채널시프트 셰이더 키워드 ON/OFF 동기화
			_checkChannelShift();

			// ── _ChannelShift 파라미터 ───────────────────────────────────────
			// ★ xy: 채널시프트 UV 오프셋.
			//   셰이더에서 R채널은 uvG + _ChannelShift.xy, B채널은 uvG - _ChannelShift.xy 로 샘플링
			//   → R과 B가 반대 방향으로 어긋나 "색수차(Chromatic Aberration)" 느낌이 난다
			//   현재 y=0으로 고정이므로 가로 방향으로만 어긋난다
			// zw: 왜곡 텍스처 UV 오프셋 → 어느 영역의 왜곡 패턴을 쓸지 결정
			var channelShift = new Vector4(_channelSpread * .01f, 0f);
			mat.SetVector(s_ChannelShift, new Vector4(channelShift.x, channelShift.y, _distortionOffset.x, _distortionOffset.y));

			// ── 왜곡 텍스처 ─────────────────────────────────────────────────
			// 최초 1회 생성 후 재사용. 셰이더에서 UV 왜곡 방향/강도 맵으로 사용된다
			if (_distortionMap == null)
				_distortionMap = _generateDistortionMap(256);

			mat.SetTexture(s_DistortionTex, _distortionMap);

			// ── _DistortionData 파라미터 ─────────────────────────────────────
			// ★ x (DistortionScale): 왜곡 텍스처를 스케일해서 샘플링.
			//     커질수록 왜곡 방향 벡터의 변화가 빠르게 요동쳐 UV 이동이 복잡해진다.
			// y (Noise): 0f 고정 → CbCr 노이즈 비활성
			// z (Quantization): 15f 고정 → CbCr 색차를 1/15 단위로 계단화 (JPEG 색 뭉침 연출)
			// ★ w (Glitch): 채널시프트 강도.
			//     셰이더에서 lerp(채널시프트 결과, 원본, _Glitch) 에 쓰인다
			//     channelShiftPow × (1 + spread) + 1 로 계산되므로
			//     pow가 0이면 최솟값 1(lerp 비율 1 = 원본), pow가 클수록 채널시프트가 강해진다
			mat.SetVector(s_DistortionData, new Vector4(
				settings._distortionScale.value,
				0f,
				15f,
				settings._channelShiftPow.value * (1f + _channelSpread) + 1f));

			// ── _FxData 파라미터 ★ 핵심 색 조절 ────────────────────────────
			// x (Intensity + 1):
			//   셰이더에서 lerp(YCbCr왜곡결과, 채널시프트결과, mixAmt) 의 t값으로 사용
			//   intensity = -1 → mixAmt=0 이면 YCbCr 결과만 출력
			//   intensity =  0 → mixAmt=1 이면 채널시프트(글리치) 결과만 출력
			//   (볼륨의 _intensity 범위는 -5~5이므로 실제 mixAmt는 -4~6)
			//
			// ★ y (ApplyDistortY): Y(밝기) 채널 샘플링 UV에 왜곡 오프셋 적용 강도
			//   값이 클수록 밝기 정보가 뒤틀려 화면이 물결치는 것처럼 보인다.
			//   색상보다 밝기 왜곡이기 때문에 채도보다 명암 변화에 영향
			//
			// z (ApplyDistortChroma): 0f 고정 → 색차 블록 왜곡 (비활성)
			//
			// ★ w (ApplyDistortGlitch): 글리치 채널 UV에 왜곡 오프셋 적용 강도
			//   값이 크면 채널시프트가 일어나는 위치 자체도 왜곡되어
			//   색 분리 영역이 화면에서 흔들리듯 보인다.
			mat.SetVector(s_FxData, new Vector4(
				settings._intensity.value + 1f,
				settings._applyToY.value,
				0f,
				settings._applyToGlitch.value));

			// ★ 글리치 색조 전달: 채널시프트 결과에 곱해지는 색조
			mat.SetColor(s_GlitchTint, settings._glitchTint.value);

			// ★ Y왜곡 색조 전달: applyToY로 어긋난 영역에 곱해지는 색조
			mat.SetColor(s_DistortYTint, settings._distortYTint.value);

			return true;

			// ── 로컬 함수 : 채널시프트 키워드 ON/OFF ──────────────────────
			void _checkChannelShift()
			{
				// 셰이더 내 #pragma multi_compile_local _ ENABLE_CHANNEL_SHIFT 와 연동
				var isEnabled = settings._channelShiftSpread.value != 0f && settings._channelShiftPow.value != 0f;

				// 상태가 바뀐 경우에만 키워드를 토글 (불필요한 호출 방지)
				if (isEnabled != _channelShiftIsOn)
				{
					if (isEnabled)
						mat.EnableKeyword("ENABLE_CHANNEL_SHIFT");
					else
						mat.DisableKeyword("ENABLE_CHANNEL_SHIFT");
				}

				_channelShiftIsOn = isEnabled;
			}
		}

		// =======================================================================
		// Invoke() : Validate()가 true를 반환한 프레임에만 호출된다
		//   pass 0 : 화면 전체(source)를 읽어 셰이더 효과를 적용한 결과를 dest에 Blit
		public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
		{
			callApi.Blit(source, dest, _material, 0);
		}

		// =======================================================================
		// _generateDistortionMap() : 프로시저럴 왜곡 텍스처 생성 (최초 1회)
		//
		// 채널별 용도 (셰이더 참조):
		//   R (d.r) : X 방향 왜곡 벡터 기반값 → 사인 패턴 + 랜덤 혼합
		//   G (d.g) : Y 방향 왜곡 벡터 기반값 → 코사인 패턴 + 랜덤 혼합
		//   B (d.b) : 왜곡 강도(strength) → Perlin 노이즈 기반, 영역별 왜곡 세기를 제어
		//   A (d.a) : 추가 노이즈 (현재 Noise=0 고정이므로 색에 실질 영향 없음)
		//
		// ★ 색에 간접 영향:
		//   R/G 채널이 dir 벡터를 만들고, B 채널이 strength를 결정
		//   dir × strength × ApplyDistortY/Glitch 가 UV 오프셋이 되어
		//   최종적으로 어느 픽셀의 색을 읽을지가 달라진다
		public static Texture2D _generateDistortionMap(int size = 256)
		{
			var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			tex.wrapMode   = TextureWrapMode.Repeat;
			tex.filterMode = FilterMode.Point;

			for (var y = 0; y < size; y++)
			for (var x = 0; x < size; x++)
			{
				var fx = (float)x / size;
				var fy = (float)y / size;

				// R: 사인 기반 별칭(aliasing) 패턴과 랜덤을 50:50 혼합 → X 왜곡 방향
				var aliasX = Mathf.Sin(fx * 32 * Mathf.PI) * 0.5f + 0.5f;
				// G: 코사인 기반 별칭 패턴과 랜덤을 50:50 혼합 → Y 왜곡 방향
				var aliasY = Mathf.Cos(fy * 24 * Mathf.PI) * 0.5f + 0.5f;

				var r = Mathf.Lerp(Random.value, aliasX, 0.5f);
				var g = Mathf.Lerp(Random.value, aliasY, 0.5f);
				// B: Perlin 노이즈 → 영역별 왜곡 강도(strength) 맵. 부드럽게 변한다
				var b = Mathf.PerlinNoise(fx * 8, fy * 8);
				// A: 순수 랜덤 노이즈 (현재 Noise 파라미터가 0이라 색에 영향 없음)
				var a = Random.value;

				tex.SetPixel(x, y, new Color(r, g, b, a));
			}

			tex.Apply();
			return tex;
		}
	}
}