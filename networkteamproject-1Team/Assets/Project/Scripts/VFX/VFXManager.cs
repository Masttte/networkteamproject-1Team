using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using VolFx;

namespace VFX
{
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

        [Header("Death VFX Settings")]
        [SerializeField] float _intensityDuration1 = 3f;
        [SerializeField] float _intensityDuration2 = 0.2f;
        [SerializeField] float _intensityDuration3 = 1.0f;

        [SerializeField] Ease _intensityEase1 = Ease.OutSine;
        [SerializeField] Ease _intensityEase2 = Ease.OutCubic;
        [SerializeField] Ease _intensityEase3 = Ease.InQuad;

        [Space(10)]
        [SerializeField] float _intensityTarget1 = -5f;
        [SerializeField] float _intensityTarget2 = 5f;

        [SerializeField] Color _deathTintHit = new Color(0.11f, 0.11f, 0.11f);

        [SerializeField] float _deathTintDuration = 1.0f;
        [SerializeField] Ease _deathTintEase = Ease.OutQuad;

        MotionHandle _intensityHandle;
        MotionHandle _distortionHandle;
        MotionHandle _glitchHandle;
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

            _intensityHandle = LMotion.Create(_intensityInitial, _intensityTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._intensity.value = x);

            _distortionHandle = LMotion.Create(_distortionInitial, _distortionTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._distortionScale.value = x);

            _glitchHandle = LMotion.Create(_applyToGlitchInitial, _applyToGlitchTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._applyToGlitch.value = x);

            _ = LMotion.Create(new Color(1f, 0.5f, 0.5f), Color.white, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._glitchTint.value = x);
        }

        public void PlayDeathVFX()
        {
            var builder = LSequence.Create();

            builder.Append(LMotion.Create(0f, 0f, 0.3f).Bind(_ => { }));

            builder.Append(LMotion.Create(0f, _intensityTarget1, _intensityDuration1)
                .WithEase(_intensityEase1)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget1, _intensityTarget2, _intensityDuration2)
                .WithEase(_intensityEase2)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget2, 0f, _intensityDuration3)
                .WithEase(_intensityEase3)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(Color.white, _deathTintHit, _deathTintDuration * 0.3f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._glitchTint.value = x));

            builder.Insert(_deathTintDuration * 0.3f, LMotion.Create(_deathTintHit, Color.black, _deathTintDuration * 0.7f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._glitchTint.value = x));

            _deathSequenceHandle = builder.Run();
        }
    }
}