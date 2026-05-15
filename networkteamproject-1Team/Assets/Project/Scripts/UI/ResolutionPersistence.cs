using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Michsky.UI.Dark;

/// <summary>
/// QualityManager에 누락된 해상도(주사율 포함) PlayerPrefs 영속화 보완 컴포넌트.
/// QualityManager가 붙은 GameObject에 함께 부착한다.
/// </summary>
public class ResolutionPersistence : MonoBehaviour
{
    // RefreshRate는 numerator/denominator 분수형이라 두 키로 분할 저장
    const string PREF_W      = "ResolutionWidth";
    const string PREF_H      = "ResolutionHeight";
    const string PREF_RR_NUM = "ResolutionRefreshNumerator";
    const string PREF_RR_DEN = "ResolutionRefreshDenominator";

    [SerializeField] QualityManager qualityManager;
    
    private FullScreenMode _originalMode;
    private FullScreenMode _appliedMode;

    private void Awake()
    {
        _originalMode = Screen.fullScreenMode;
        _appliedMode = _originalMode;
        if (qualityManager == null) qualityManager = GetComponent<QualityManager>();
    }

    private void Start()
    {
        InitAsync(destroyCancellationToken).Forget();
    }
    
    // 모드 변경 감지 + 저장된 해상도 재적용
    // 예: 사용자가 UI에서 Borderless → Windowed 전환 시
    private void Update()
    {
        if (Screen.fullScreenMode != _appliedMode)
        {
            _appliedMode = Screen.fullScreenMode;
            ApplyResolutionFromPrefs();
        }
    }
    
    // ===== Initialization =====

    private async UniTaskVoid InitAsync(System.Threading.CancellationToken ct)
    {
        // HorizontalSelector(WindowMode)의 invokeAtStart가 fullScreenMode를 복원할 시간 확보
        await UniTask.NextFrame(ct);
        
        // QualityManager.Start가 망친 fullScreenMode 복원
        if (Screen.fullScreenMode != _originalMode)
        {
            Debug.Log("Restore Screen.fullScreenMode");
            Screen.fullScreenMode = _originalMode;
        }
        
        // 첫 적용 후 _appliedMode 갱신 (Update 폴링 기준)
        _appliedMode = Screen.fullScreenMode;
        
        // 저장된 해상도 적용 + 드롭다운 동기화
        if (PlayerPrefs.HasKey(PREF_W) && PlayerPrefs.HasKey(PREF_H))
        {
            ApplyResolutionFromPrefs();
            SyncDropdownIndexFromPrefs();
        }
        
        // 드롭다운 변경 시 저장 리스너 등록 (적용은 QualityManager.SetResolution 담당)
        if (qualityManager.defaultDropdown != null)
            qualityManager.defaultDropdown.onValueChanged.AddListener(SaveResolution);
    }
    
    // ===== Apply / Sync =====
    
    /// <summary>
    /// PlayerPrefs에서 저장된 해상도/주사율 읽어 Screen.SetResolution 호출.
    /// 현재 fullScreenMode 기준으로 적용.
    /// </summary>
    private void ApplyResolutionFromPrefs()
    {
        if (!PlayerPrefs.HasKey(PREF_W) || !PlayerPrefs.HasKey(PREF_H)) return;
        
        int w = PlayerPrefs.GetInt(PREF_W);
        int h = PlayerPrefs.GetInt(PREF_H);
        uint rrNum = (uint)PlayerPrefs.GetInt(PREF_RR_NUM, 0);
        uint rrDen = (uint)PlayerPrefs.GetInt(PREF_RR_DEN, 0);
        
        if (rrNum > 0 && rrDen > 0)
        {
            var ratio = new RefreshRate { numerator = rrNum, denominator = rrDen };
            Screen.SetResolution(w, h, Screen.fullScreenMode, ratio);
        }
        else
        {
            // 주사율 키 없는 기존 저장값 마이그레이션 경로
            Screen.SetResolution(w, h, Screen.fullScreenMode);
        }
    }
    
    /// <summary>
    /// PlayerPrefs의 해상도와 매칭되는 드롭다운 인덱스를 찾아 UI 표시만 동기화.
    /// SetValueWithoutNotify로 onValueChanged 재발동 방지.
    /// </summary>
    private void SyncDropdownIndexFromPrefs()
    {
        if (qualityManager.defaultDropdown == null) return;
        if (!PlayerPrefs.HasKey(PREF_W) || !PlayerPrefs.HasKey(PREF_H)) return;
        
        int w = PlayerPrefs.GetInt(PREF_W);
        int h = PlayerPrefs.GetInt(PREF_H);
        uint rrNum = (uint)PlayerPrefs.GetInt(PREF_RR_NUM, 0);
        uint rrDen = (uint)PlayerPrefs.GetInt(PREF_RR_DEN, 0);
        
        int matched = FindMatchingResolutionIndex(w, h, rrNum, rrDen);
        
        if (matched >= 0)
        {
            qualityManager.defaultDropdown.SetValueWithoutNotify(matched);
            qualityManager.defaultDropdown.RefreshShownValue();
        }
    }
    
    /// <summary>
    /// Screen.resolutions에서 주어진 w/h/주사율과 매칭되는 인덱스 검색.
    /// 주사율 정보 없으면 w/h 매칭 폴백.
    /// </summary>
    private int FindMatchingResolutionIndex(int w, int h, uint rrNum, uint rrDen)
    {
        var resolutions = Screen.resolutions;
        int matched = -1;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width != w || resolutions[i].height != h) continue;
            
            if (rrNum > 0 && rrDen > 0)
            {
                var r = resolutions[i].refreshRateRatio;
                if (r.numerator == rrNum && r.denominator == rrDen)
                {
                    return i;
                }
            }
            else if (matched < 0)
            {
                matched = i; // 주사율 정보 없을 때 폴백
            }
        }
        
        return matched;
    }
    
    // ===== Save =====
    
    /// <summary>
    /// 드롭다운 변경 시 호출. 새 해상도/주사율을 PlayerPrefs에 저장.
    /// QualityManager.SetResolution(적용)과 독립 동작 (적용/저장 책임 분리).
    /// </summary>
    private void SaveResolution(int index)
    {
        Debug.Log($"[ResolutionPersistence] SaveResolution(index={index}) 호출");
        
        var resolutions = Screen.resolutions;
        if (index < 0 || index >= resolutions.Length) return;
        
        var r = resolutions[index];
        PlayerPrefs.SetInt(PREF_W, r.width);
        PlayerPrefs.SetInt(PREF_H, r.height);
        PlayerPrefs.SetInt(PREF_RR_NUM, (int)r.refreshRateRatio.numerator);
        PlayerPrefs.SetInt(PREF_RR_DEN, (int)r.refreshRateRatio.denominator);
        PlayerPrefs.Save();
    }
}