# AudioManager
## 04-29 작업 노트
오디오를 관리하는 싱글톤 매니저 작성

## 믹서와 오디오 소스 구조

```
Master
├─ BGM  : bgmSource (BGM), scapeSource (환경음)
└─ Sfx  : drySfxSource (UI등)
     └─ Sfx Reverb  : wetSfxSources[8] (3D 공간 효과음 풀)
```

## 사용법

### 1. BGM 재생
```csharp
AudioManager.Instance.PlayBGM(bgmClip);
```

### 2. 3D 공간 효과음 — `PlaySfxWet(clip, position)`
```csharp
// hitPoint: Raycast 등으로 얻은 월드 좌표
AudioManager.Instance.PlaySfxWet(weaponSO.attackHit, hitPoint);
```
- **거리 감쇠 적용**: 소리가 발생한 `position`과 `AudioListener`(카메라) 사이의 거리 기준으로 볼륨 감소
- 리버브 효과 적용

### 3. 2D 효과음 — `PlaySfxDry(clip)`
```csharp
AudioManager.Instance.PlaySfxDry(countdownClip);
```
- 거리와 무관하게 항상 같은 볼륨으로 재생
- UI나 이벤트 알림등 **3D 효과가 없는 소리**에 사용

### 4. 볼륨 조절 / 뮤트 (TODD: 추후 UI 슬라이더와 연동)
```csharp
// 오디오 믹서 슬라이더의 Min Value를 0.0001로 설정해줘야한다
    private void Mute()
    {
        AudioManager.Instance.SetAudioMute(AudioMixerType.BGM);
    }
    private void ChangeVolume(float volume)
    {
        AudioManager.Instance.SetAudioVolume(AudioMixerType.BGM, volume);
    }
```

---

## 기술 구현: 3D 공간음 풀링 시스템

### 문제 배경
멀티플레이어 환경에서 3D 거리 감쇠를 구현하려면 AudioSource가 소리가 발생한 **월드 좌표**에 실제로 존재해야 한다.

단순히 `AudioManager` 하나가 소리를 틀면, 리스너(카메라)와의 거리가 실제 전투 거리를 반영하지 못한다.

### 해결 방법: 오브젝트 풀 + 위치 이동

매번 `Instantiate/Destroy`하면 GC(가비지 컬렉션)를 유발한다.
→ 게임 시작 시 스피커 오브젝트 8개를 미리 만들어두고, 소리를 재생할 때마다 해당 스피커를 `hitPoint`로 순간이동시켜 재사용한다.

```
Awake() 시점
  SfxSpeaker_0 ~ 7 생성 (자식 오브젝트, AudioSource 부착)

PlaySfxWet(clip, hitPoint) 호출 시
  1. 풀에서 다음 스피커(sfxIndex) 선택
  2. speaker.transform.position = hitPoint  ← 순간이동
  3. speaker.Play()                          ← 재생
  4. sfxIndex = (sfxIndex + 1) % 8          ← 다음 스피커로 넘김
```

### 3D AudioSource 설정값
| 항목 | 의미 |
|---|---|
| `spatialBlend`| 완전한 3D 공간음 |
| `rolloffMode` | 거리에 따른 현실적인 로그 감쇠 (기본값) |
| `minDistance` | 이 거리 이내는 최대 볼륨 유지 (기본값) |
| `maxDistance` | 이 거리 밖에서는 감쇠 완료 |

---

## 네트워크 연동 (Weapon.cs 기준)

3D 공간음이 의미있으려면 **소리의 기준 위치(hitPoint)** 가 모든 클라이언트에 동기화되어야 한다.

```
[오너 클라이언트] Attack()
  Raycast → hit.point 획득

  Miss  → PlaySfxWet(Miss, _attackPoint.position)   로컬만 재생 (본인 위치에서 헛스윙)
  Blocked → BlockedServerRpc(hit.point)
                └─ BlockedClientRpc(hitPoint)         모든 클라이언트에서 재생 (벽에 맞는 소리등)
  Hit   → AttackServerRpc(targetId, damage, hit.point)
              └─ AttackClientRpc(..., hitPoint)        모든 클라이언트에서 재생 (플레이어 때리는 소리)

[각 클라이언트]
  PlaySfxWet(clip, hitPoint)
    → SfxSpeaker가 hitPoint로 이동 → 내 카메라(AudioListener)와의 거리 기반 감쇠
```

- 오디오 편집 및 최적화 작업 진행
- 발자국 소리 추가 구현
- 오디오 랜덤 컨테이너 활용하여 효과음 다양화
  - 소스를 랜덤이나 셔플로 돌리는것 뿐아니라 
  - 피치와 볼륨을 랜덤으로 조절하여 같은 소리도 다양하게 들리도록 함