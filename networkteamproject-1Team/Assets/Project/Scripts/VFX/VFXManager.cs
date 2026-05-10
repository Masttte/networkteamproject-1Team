using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using VolFx;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() => Instance = null;

    Volume _volume;
    CustomScreenVol _screenVFX;

    [Header("Hit VFX Settings")]
    [SerializeField] float _vfxDuration = 0.5f;
    [SerializeField] Ease _vfxEase = Ease.OutQuad;
    [Space(10)]
    [SerializeField] float _intensityInitial = 0f;
    [SerializeField] float _intensityTarget = 1f;

    [SerializeField] float _distortionInitial = 0f;
    [SerializeField] float _distortionTarget = 2f;

    [SerializeField] float _applyToGlitchInitial = 6.26f;
    [SerializeField] float _applyToGlitchTarget = 15f;

    [Header("Alert VFX Settings")]
    [SerializeField] float _alertPulseDuration = 0.08f;   // 한 번 튀는 시간
    [SerializeField] float _alertIntensityPeak = 3f;
    [SerializeField] float _alertDistortionPeak = 7f;
    [SerializeField] float _alertPulseInterval = 0.13f;   // 펄스 사이 간격
    [SerializeField] Ease _alertRiseEase = Ease.OutExpo;
    [SerializeField] Ease _alertFallEase = Ease.InQuad;

    [Header("Death VFX Settings")]
    [SerializeField] float _intensityDuration1 = 4f; // 내려가는 시간
    [SerializeField] float _intensityDuration2 = 0.2f; // 올라가는 시간
    [SerializeField] float _intensityDuration3 = 5f; // 검은색으로 가는 시간

    [SerializeField] Ease _intensityEase1 = Ease.OutSine;
    [SerializeField] Ease _intensityEase2 = Ease.OutCubic;
    [SerializeField] Ease _intensityEase3 = Ease.InQuad;
    [Space(10)]
    [SerializeField] float _intensityTarget1 = -5f;
    [SerializeField] float _intensityTarget2 = 5f;

    [SerializeField] Color _deathTint = new Color(0.14f, 0.14f, 0.14f);
    [SerializeField] Ease _deathTintEase = Ease.OutQuad;

    MotionHandle _colorHandel;
    MotionHandle _intensityHandle;
    MotionHandle _distortionHandle;
    MotionHandle _glitchHandle;

    MotionHandle _alertIntensityHandle;
    MotionHandle _alertDistortionHandle;

    MotionHandle _deathSequenceHandle;

    private void Awake()
    {
        Instance = this;
        _volume = GetComponentInChildren<Volume>();
        _volume.profile.TryGet(out _screenVFX);
    }

    public void PlayHitVFX()
    {
        PlayHitVFXAsync().Forget();
    }

    async UniTaskVoid PlayHitVFXAsync()
    {

        await UniTask.Delay(235);

        float halfDuration = _vfxDuration * 0.5f;
        _intensityHandle = LSequence.Create()
            .Append(LMotion.Create(_intensityInitial, _intensityTarget, halfDuration).WithEase(_vfxEase).Bind(x => _screenVFX._intensity.value = x))
            .Append(LMotion.Create(_intensityTarget, _intensityInitial, halfDuration).WithEase(_vfxEase).Bind(x => _screenVFX._intensity.value = x))
            .Run();

        _distortionHandle = LMotion.Create(_distortionInitial, _distortionTarget, _vfxDuration)
            .WithEase(_vfxEase)
            .Bind(x => _screenVFX._distortionScale.value = x);

        _glitchHandle = LMotion.Create(_applyToGlitchInitial, _applyToGlitchTarget, _vfxDuration)
            .WithEase(_vfxEase)
            .Bind(x => _screenVFX._applyToGlitch.value = x);

        _colorHandel = LMotion.Create(new Color(1f, 0.44f, 0.14f), Color.white, _vfxDuration)
            .WithEase(_vfxEase)
            .Bind(x => _screenVFX._glitchTint.value = x);
    }

    public void PlayAlertVFX()
    {
        PlayAlertVFXAsync().Forget();
    }

    async UniTaskVoid PlayAlertVFXAsync(int alertPulseCount = 1)
    {
        for (int i = 0; i < alertPulseCount; i++)
        {
            _alertIntensityHandle = LMotion.Create(_screenVFX._intensity.value, _alertIntensityPeak, _alertPulseDuration)
                .WithEase(_alertRiseEase)
                .Bind(x => _screenVFX._intensity.value = x);

            _alertDistortionHandle = LMotion.Create(_alertDistortionPeak, 0.14f, _alertPulseDuration)
                .WithEase(_alertRiseEase)
                .Bind(x => _screenVFX._distortionScale.value = x);

            await UniTask.Delay((int)(_alertPulseDuration * 1000));

            // 원래대로 — 내려가기
            _alertIntensityHandle = LMotion.Create(_screenVFX._intensity.value, 0, _alertPulseDuration)
                .WithEase(_alertFallEase)
                .Bind(x => _screenVFX._intensity.value = x);

            _alertDistortionHandle = LMotion.Create(0.14f, 0, _alertPulseDuration)
                .WithEase(_alertRiseEase)
                .Bind(x => _screenVFX._distortionScale.value = x);

            await UniTask.Delay((int)((_alertPulseInterval) * 1000));
        }
    }

    public void PlayDeathVFX()
    {
        var builder = LSequence.Create();

        builder.Append(LMotion.Create(0f, 0f, 0.35f).Bind(_ => { })); // 잠시 멈춤

        builder.Append(LMotion.Create(0f, _intensityTarget1, _intensityDuration1)
            .WithEase(_intensityEase1)
            .Bind(x => _screenVFX._intensity.value = x));
        builder.Join(LMotion.Create(Color.red, _deathTint, 0.5f)
            .WithEase(_deathTintEase)
            .Bind(x => _screenVFX._glitchTint.value = x));

        builder.Append(LMotion.Create(_intensityTarget1, _intensityTarget2, _intensityDuration2)
            .WithEase(_intensityEase2)
            .Bind(x => _screenVFX._intensity.value = x));


        builder.Append(LMotion.Create(_intensityTarget2, 0f, _intensityDuration3)
            .WithEase(_intensityEase3)
            .Bind(x => _screenVFX._intensity.value = x));

        builder.Join(LMotion.Create(_deathTint, Color.black, _intensityDuration3)
            .WithEase(_deathTintEase)
            .Bind(x => _screenVFX._glitchTint.value = x));

        _deathSequenceHandle = builder.Run();
    }

    private void OnDisable()
    {
        _colorHandel.Cancel();
        _intensityHandle.Cancel();
        _distortionHandle.Cancel();
        _glitchHandle.Cancel();
        _alertIntensityHandle.Cancel();
        _alertDistortionHandle.Cancel();
        _deathSequenceHandle.Cancel();
    }
}
