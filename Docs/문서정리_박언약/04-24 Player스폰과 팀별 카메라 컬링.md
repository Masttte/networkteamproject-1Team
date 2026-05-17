# 04-24 작업 노트

## TEST_NetworkTester
- Settings/Editor/Network 폴더에 프리팹 보관
- `Reset` 실행 시 씬에서 `PlayerSpawnManager` 자동 할당
### 단축키
| 키 | 동작 |
|---|---|
| F1 | StartHost |
| F2 | StartClient |
| F3 | SpawnAllPlayers (맵 씬 테스트용) |

---
## PlayerSpawnManager
- `OnLoadEventCompleted` 이벤트에 `SpawnAllPlayers` 등록 (로비→게임 씬 전환 시 자동 실행)

### 스폰 흐름
1. `clientsCompleted` 복사 후 셔플
2. `_startTeamBCount` 로 B팀 고정 인원 설정
3. 스폰 후 `PlayerRole.Team.Value`에 주입
---
## PlayerRole
- Player 프리팹에 부착
- `NetworkVariable<TeamType> Team` : 서버 Write / 모든 클라이언트 Read
- 팀이 설정되면 `OnValueChanged` 콜백이 자동으로 카메라 Culling Mask 전환
- 외부에서 팀 조회 시 `Team.Value` 사용

# 팀 시점 레이어 마스크 시스템

## 목적
- `A` 플레이어는 `Average(10)` 레이어만 본다.
- `B` 플레이어는 `Beautiful(11)` 레이어만 본다.
- `MonsterBase` 프리팹에 두 시각 모델을 겹쳐 두고 카메라 `Culling Mask` 비트만 토글한다.

## 레이어 규칙
| 레이어 번호 | 이름 | 용도 |
|---|---|---|
| 10 | Average | A팀이 보는 몬스터 외형 |
| 11 | Beautiful | B팀이 보는 몬스터 외형 |

## 프리팹 설정 방법
- Monster 프리팹 → 몬스터 자체에는 `Monster(7)` 레이어 사용
  - 자식 오브젝트 `A` → Layer: `Average(10)`
  - 자식 오브젝트 `B` → Layer: `Beautiful(11)`