using UnityEngine;
using UnityEngine.Audio;

public enum AudioMixerType { Master, Music, SFX }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() => Instance = null;

    [Header("3D 사운드")]
    [SerializeField] float _spatialBlend = 0.9f; // 3D 사운드 정도
    [SerializeField] float _minDistance = 0.5f;
    [SerializeField] float _maxDistance = 49f;

    public AudioMixer audioMixer;
    bool[] isMute = new bool[3];
    float[] audioVolumes = new float[3];

    public AudioSource bgmSource;
    //public AudioSource scapeSource; // 한종류가 항상 재생중이라 주석 처리중
    public AudioSource[] wetSfxSources = new AudioSource[8]; int sfxIndex; // 배열 갯수만큼 소리 제한
    public AudioSource drySfxSource; // 리버브 없는 효과음 (예: UI 사운드)

#if UNITY_EDITOR
    private void Reset()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioMixer");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            audioMixer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
        }
    }
#endif
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject); //싱글톤

        // wetSfxSources: 각 AudioSource를 독립 자식 GameObject에 붙여서 위치를 개별 제어 가능하게 함
        // 오브젝트 풀링 안의 스피커를 hitPoint로 순간이동시켜 재사용
        var sfxMixerGroup = audioMixer.FindMatchingGroups("SFX Reverb")[0];
        for (int i = 0; i < wetSfxSources.Length; i++)
        {
            GameObject speaker = new GameObject($"SfxSpeaker_{i}");
            speaker.transform.SetParent(this.transform);

            AudioSource source = speaker.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.spatialBlend = _spatialBlend;
            source.minDistance = _minDistance;
            source.maxDistance = _maxDistance;

            wetSfxSources[i] = source;
        }
    }

    public void PlayBGM(AudioResource bgmClip)
    {
        bgmSource.resource = bgmClip;
        bgmSource.Play();
    }
    // 3D 공간음 재생 — 스피커를 position으로 이동 후 재생
    public void PlaySfxWet(AudioResource clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource speaker = wetSfxSources[sfxIndex];

        // 스피커를 타격 위치로 순간이동 → AudioListener(카메라)와의 거리가 실제 거리가 됨
        speaker.transform.position = position;
        speaker.resource = clip;
        speaker.Play();

        sfxIndex = (sfxIndex + 1) % wetSfxSources.Length;
    }
    public void PlaySfxDry(AudioResource clip)
    {
        drySfxSource.resource = clip;
        drySfxSource.Play();
    }


    void SetAudioVolume(AudioMixerType audioMixerType, float volume)
    {
        // 오디오 믹서의 값은 -80 ~ 0까지이기 때문에 0.0001 ~ 1의 Log10 * 20을 한다.
        audioMixer.SetFloat(audioMixerType.ToString(), Mathf.Log10(volume) * 20);
    }
    void SetAudioMute(AudioMixerType audioMixerType)
    {
        int type = (int)audioMixerType;
        if (!isMute[type]) // 뮤트 
        {
            isMute[type] = true;
            audioMixer.GetFloat(audioMixerType.ToString(), out float curVolume);
            audioVolumes[type] = curVolume;
            SetAudioVolume(audioMixerType, 0.001f);
        }
        else
        {
            isMute[type] = false;
            SetAudioVolume(audioMixerType, audioVolumes[type]);
        }
    }
    // 버튼 클릭 이벤트에 연결할 함수들
    public void Mute()
    {
        Instance.SetAudioMute(AudioMixerType.Music);
    }
    public void ChangeVolume(float volume)
    {
        Instance.SetAudioVolume(AudioMixerType.Music, volume);
    }
}