# 네트워크 팀프로젝트_작업 노트

**작성자**: 이성규  
**게임명**: 낯선 곳에 잡혀왔지만 괴물은 갇혀있으니 럭키비키\~\!★ (임시)  
**작성일**: 2026-04-24  
**최종 수정**: 2026-04-24  

## 프로젝트 개요

- **진행 기간**: 2026.04.24(금)~2026.05.18(목)
- **개발 환경**: Unity / C# / URP 3D / UGS + Relay
- **유니티 버전**: 6.3 LTS

# 작업 일지

## Day 1 — 2026-04-24

기초 프로젝트 세팅 및 팀 작업 방향 상담 및 회의

![alt text](Resources/Rulesets_protect-main.png)

main 브랜치 Ruleset 생성으로 휴먼 이슈로 인한 main 브랜치 커밋이나 풀리퀘 체크 과정 추가.

가이드 문서, 역할 분담 문서 등 팀 문서 양식 작성.

## Day 2 — 2026-04-25

유니티 최적화 가이드 문서 작성  
프로젝트 에셋 파일 확인  
Project Auditor 패키지를 통한 프로젝트 파일 분석

![alt text](Resources/Project%20Auditor.png)

라이트 베이킹은 에셋 자체에서 잘 설정되어 있어 별도 작업 없이 패스. 텍스쳐 목록을 확인하고 용량이 큰 순으로 해당 에셋이 적용되는 씬을 살펴보며 Max Size 조정.

![alt text](Resources/VRWorldToolkit.png)

VRWorldToolkit을 사용해 다시 텍스쳐를 상세히 확인하고 관리. 오리지널 사이즈보다 큰 MaxSize를 가진 텍스쳐의 MaxSize를 조정.  
`t:Texture`를 통해 전체 텍스쳐 사이즈 확인 완료.

> Unity의 Max Size는 임포트되는 GPU 메모리 / 빌드 크기에만 영향을 주고 원본 PNG 파일 자체는 그대로 남는다. 공유 패키지(Google Drive) 크기까지 줄이려면 원본 리사이즈가 별도로 필요.

![alt text](Resources/big4000.png)

실제 원본 용량 감소를 위해 `너비:>4000`을 에셋 임포트 폴더에 검색해 4000 사이즈 이상의 텍스쳐 이미지를 전부 선택 후 XnConvert를 통해 2048 사이즈로 변환해 덮어씌움 (스카이박스 제외 — 큐브맵 특성상 해상도 유지 필요).

![alt text](Resources/XnOutput.png)
![alt text](Resources/XnConvert.png)

![alt text](Resources/XnConvertDone.png)

수동으로 작업한 몇 개의 파일을 제외하고 77개의 파일 자동 변환 성공.

```
전체 입력 파일 크기: 731.87 MiB
전체 출력 파일 크기: 245.97 MiB
파일 크기 비: -66%
```

> Imports 폴더는 .gitignore 대상이라 Git 저장소엔 영향 없음. 다만 팀 공유 패키지(Google Drive) 크기가 줄어 신규 팀원 셋업 / 재다운로드 시 시간 단축 효과.

위 과정을 통해 3.05GB로 기존에 공유받은 Imports 폴더의 패키지 용랑을 3.05GB에서 2.32GB로 용량 감소 성공.

## Day 3 — 2026-04-26

룩뎁 샘플씬 작업 진행. 라이팅 / 컬링 / PPS 세 영역으로 나눠 진행했고, 동일 씬에서 Profiler·Stats로 실시간 검증.

### 라이팅 베이크 전략

씬 내 95개 라이트 중 대다수를 Baked로 전환하여 최적화 기반 마련. 호러 장르 특성상 정적 조명 비중이 압도적이라 Mixed보다 Baked 우선 전략이 효율적.

- 촛불 모델링의 그림자 외곽선 자연스러움을 위해 **Baked Shadow Radius** 값 조절
- 조명 Temperature **4000K** 적용 → 실내 화이트 밸런스 조정 (따뜻한 톤 ↔ 차가운 분위기 균형)
- 대량의 천장등을 Area Light로 전환하는 작업은 추후 여유 시간에 폴리싱으로 진행

### GI: APV 적용

작업 시간 효율을 고려해 **APV(Adaptive Probe Volumes)** 적용. 기존 Light Probe Group 수동 배치 대비 자동 분포 + 일관된 품질로 시간 절감.

### CPU 오클루전 컬링

| 항목 | 미적용 | 적용 | 변화 |
|------|--------|------|------|
| Batches (가장 배칭 많은 구간) | 1120 | 416 | **-63%** |

![alt text](Resources/BeforeOC.png)
![alt text](Resources/ApplyOC.png)

> **GPU 오클루전 컬링은 역효과** — 적용 시 배칭 및 다양한 수치가 2배가량 증가해 배제.  
> (CPU OC가 정적 환경에 더 적합한 결과로 보임. GPU OC는 동적 오브젝트가 많은 씬에서 효과적이라 이번 호러 씬과 매칭이 안 맞은 듯)

### Volume PPS 후처리

![alt text](Resources/BeforePPS.png)
![alt text](Resources/AfterPPS.png)

호러 톤 기본 프로파일 적용. 세부 항목은 룩뎁 가이드 별도 문서로 정리 예정.

### 다음

- Area Light 전환 (천장등) — 폴리싱 단계
- PPS 프로파일 세부 정리
- 메인 씬 적용 시 이식 가능한 자산화 (프리셋/프로파일 분리)

## Day 4 — 2026-04-27

오전 중 진행 방향 회의 진행 및 기존 설치 패키지 사용법 안내.

씬 복사 시 APV 베이킹 데이터 복사 안되는 문제 확인.
복사된 씬에서 **Baking Mode를 Baking Set으로 변경 후 Bake Probe Volumes 실행 시 정상 동작 확인.**


### 플레이어 제작
팀원의 네트워크 코어시스템 스크립트 베이스 동작 확인 후 작업 시작

#### 구조 설계 (사전 메모)

> 본격 플레이어 구현 전 사전 계획. 아직 실제 코드는 작성 전이며 본 문서에만 정리.

```
Assets/Project/Scripts/Player/
├ Core/
│  ├ PlayerController.cs           // 메인 조립자 (NetworkBehaviour)
│  ├ PlayerInputHandler.cs         // BattleInputReader 라우팅
│  └ DamageInfo.cs                 // 데미지 정보 struct
│
├ Movement/
│  ├ PlayerMovement.cs             // CharacterController.Move() 래퍼
│  └ PlayerCamera.cs               // 1인칭 + Cinemachine
│
├ Combat/
│  ├ PlayerCombat.cs               // 공격 입력 + ServerRpc
│  ├ PlayerHealth.cs               // HP NetworkVariable + IDamageable
│  └ PlayerCombatState.cs          // enum
│
├ Animation/
│  └ PlayerAnimation.cs            // Animator 파라미터 + Layer Weight
│
├ Interaction/
│  └ PlayerInteractor.cs           // IInteractor 구현
│
└ Interfaces/
   ├ IInteractor.cs
   ├ IInteractable.cs
   └ IDamageable.cs
```

draw.io를 통해 사전 구조 설계.
스크립트를 작성하기 앞서 시행착오를 줄이고 유지보수 및 협업에 편한 구조를 만들기 위함.

![alt text](Resources/PlayerController_Architecture.png)

#### Fake Shadow (URP Decal Projector)

팀원이 테스트로 제작한 플레이어 동작 테스트 중, 그림자 렌더링 관련 연출을 URP Decal Projector로 처리하기로 결정.

씬 라이팅이 전부 Static Baked로 설정되어 있어 Realtime Directional Light로 그림자만 추가하기엔 비효율 + 베이크 톤과 충돌 우려. **페이크 그림자 데칼이 호러 톤 유지 + 비용 측면 모두 적합**하다고 판단.

**작업 내용**:
- 가짜 그림자용 텍스쳐(흑백 타원 페이드) 준비
- URP Decal 머티리얼 생성 (Shader Graphs/Decal)
- URP Decal Projector 컴포넌트를 플레이어 자식으로 배치
- 점프 시 자연스러운 연출을 위해 `FakeShadowFader` 스크립트 작성
  - 아래로 Ray → Ground 레이어 충돌 거리 측정
  - 설정한 최대 높이 비례로 `fadeFactor` 조절 → 점프 시 그림자가 옅어짐
- 정상 동작 확인

## Day 5 — 2026-04-28

- 프리팹 충돌 방지를 위해 개인별 네트워크 프리팹 리스트를 활용한 독립 작업 및 추후 병합으로 합의.
- 본격적인 플레이어 개발을 위한 폴더 생성 및 임시 개인 작업용 네트워크 프리팹 리스트 생성

### 플레이어 컨트롤러 제작 시작

어제 작업한 구조 설계를 기반으로 스크립트 생성

---

#### **PlayerController 구현**

- **역할:** 플레이어 오브젝트의 최상단 에이전트(`NetworkBehaviour`)로서, 필수 컴포넌트 캐싱 및 하위 모듈 간의 **의존성 주입(DI)을 전담**하는 컨트롤 타워.
- **구현 흐름:**
    - **[컴포넌트 강제(RequireComponent)]** `CharacterController`, `Animator` 등 필수 컴포넌트를 명시하여 누락으로 인한 런타임 에러 방지.
    - **[캐싱(Awake)]** 하위 모듈이나 로직에서 사용할 핵심 컴포넌트들을 미리 참조하여 성능 최적화.
    - **[권한 제어(OnNetworkSpawn)]** `Netcode`의 `IsOwner` 체크를 통해 로컬 플레이어와 원격 플레이어의 로직을 분리.
    - **[의존성 주입(Owner-Only)]** 소유권이 확인된 경우에만 `PlayerInputHandler` 등 하위 모듈에 필요한 참조를 전달하고 초기화 및 카메라 모율의 시점(ViewPoint) 처리 호출.
- **활용:** 작업용 플레이어 프리팹의 **최상단 루트**에 부착하며, 모든 플레이어 관련 기능 모듈이 시작되는 엔트리 포인트로 사용.

---

#### **PlayerInputHandler 구현**

- **역할:** `BattleInputReader` (SO)로부터 입력 이벤트를 수신하여 이동, 카메라, 전투, 상호작용 등 개별 행동 모듈로 신호를 전달하는 **라우팅 및 관리 계층**.
- **구현 흐름:**
    - **[에디터(Reset)]** `AssetDatabase`를 이용해 필요한 `BattleInputReader` 에셋을 자동으로 찾아 할당하여 세팅 실수 방지.
    - **[초기화(Initialize)]** 하위 모듈(이동/전투 등)을 생성 및 할당하고, `BindEvents`를 호출하여 SO의 입력 액션과 실제 함수를 연결.
    - **[해제(OnDestroy)]** 오브젝트 파괴 시점에 반드시 이벤트를 구독 해제(`-=`)하여 메모리 누수 및 널 참조 에러 방지.
    - **[실행(Update)]** 입력 상태에 따라 각 모듈의 함수를 호출하거나 반응형 프로퍼티에 값을 전달하여 실제 플레이어 동작 유도.
- **활용:** `PlayerController`에 의해 제어되며, 기능 확장 시 핸들러에 모듈을 추가하고 이벤트를 연결하는 방식으로 유지보수.

---

#### **PlayerMovement 구현**

- **역할:** `CharacterController`를 기반으로 캐릭터의 물리적 이동, 점프, 중력 및 회전 로직을 직접 수행하는 **실제 행동 모듈**.
- **구현 흐름:**
    - **[캐싱(Awake)]** `CharacterController` 등 이동에 필수적인 컴포넌트를 참조하고 초기화 수행.
    - **[상태 수신(Setter Methods)]** `PlayerInputHandler`로부터 이동 벡터, 스프린트 여부, 점프 요청 등의 데이터를 전달받아 내부 변수에 갱신.
    - **[회전 및 수직 로직]** 입력 방향에 따른 부드러운 회전 보간(`SmoothDampAngle`)과 지면 체크(`isGrounded`)를 통한 중력 가속도 및 점프 속도($\sqrt{h \cdot -2g}$) 처리.
    - **[최종 이동(HandleMove)]** 현재 상태(걷기/스프린트)에 따른 속도와 입력 방향(Local to World)을 종합하여 `CharacterController.Move`로 최종 변위 적용.
- **활용:** 외부에서 직접 제어되지 않고 핸들러를 거친 신호만 처리하며, 애니메이션 시스템 등에서 참조할 수 있도록 현재 속도와 상태 값을 프로퍼티로 노출.

#### **가속도 및 점프 속도($\sqrt{h \cdot -2g}$) 공식 설명**
- 목표 높이($h$)에 도달하기 위해 필요한 수직 시작 속도($v$)를 계산.

## Day6 — 2026-04-29

### BattleInputReader 수정

팀원이 만든 인풋 액션 SO를 합의하에 수정.

- `event Action<bool> onSprintChanged` 추가
- `OnSprint` 콜백 내에서 이전 상태(`isSprint`)와 현재 입력값을 비교하여 **변화가 있을 때만** 값 갱신 + 이벤트 발행
- 기존 `isSprint` 변수는 호환성 유지를 위해 그대로 두되 프로퍼티로 외부 수정 방지.

```csharp
public void OnSprint(InputAction.CallbackContext context)
{
    bool newSprint = context.ReadValueAsButton();
    if (newSprint != isSprint)
    {
        isSprint = newSprint;
        onSprintChanged?.Invoke(isSprint);
    }
}
```

> 외부에서 sprint 입력 변화 시점에만 이벤트로 수신 가능 → 매 프레임 호출 불필요.

### PlayerInputHandler 이벤트 기반 전환

기존 Update에서 매 프레임 `_input.isSprint` 폴링하던 방식을 제거하고 `onSprintChanged` 이벤트 구독으로 변경.

- **Update 메서드 제거** — 모든 입력 처리가 이벤트 기반으로 통일
- Jump 액션 이벤트 할당 누락 발견 → 추가

---

### **PlayerMovement 보강 — 천장 충돌 처리**

테스트 중 점프 시 천장 부딪힘에서 잠깐 달라붙는 느낌 발견. `CharacterController.Move`의 반환 `CollisionFlags`로 `Above` 충돌을 감지하여, 상승 중일 때만 `_verticalVelocity`를 0으로 강제 변경. 즉시 낙하 전환되어 자연스러운 점프 종료.

---

### **PlayerCamera 구현**

- **역할**: ViewPoint 오브젝트를 활용한 1인칭 카메라 시점 모듈. VRChat 데스크톱 카메라처럼 캐릭터의 눈 위치에 뷰포인트를 두고, 시네머신 카메라가 이를 추적.

- **구현 흐름**:
    - **[ViewPoint 셋업(프리팹)]** Position Constraint를 활용해 머리 본을 Source로 잡고, 눈 중앙 쯤에 위치하도록 Offset 설정. 카메라의 **위치만** ViewPoint를 따라가고, **회전은 마우스 델타값으로 독립** 처리.
    - **[A/B 팀별 뷰포인트]** A/B 분리 구조 특성상 본 구조가 다르므로 팀별로 ViewPoint를 별도 생성(`ViewPoint_A`, `ViewPoint_B`). 본인이 어느 팀인지 체크하여 해당 ViewPoint를 카메라 타겟으로 할당.
    - **[Owner 시점 처리(SetupOwnerView)]** 카메라는 플레이어 스폰 전 대기 상태로 두었다가, Owner 캐릭터 스폰 시 팀 정보를 확인하고 해당 ViewPoint에 시네머신 카메라 부착.
    - **[회전 적용(LateUpdate)]** 마우스 델타 누적값으로 yaw/pitch 갱신. 애니메이션이 본 위치를 갱신한 뒤 ViewPoint의 회전을 강제 적용.

- **활용**: 캐릭터 본체 회전이 카메라 yaw를 따라가도록 Movement에서 처리하므로, 캐릭터 회전과 카메라 회전이 자연스럽게 일치.

#### 설계 결정 사항

**[시네머신 카메라 부착 방식: 씬 배치 + Target 갱신]**  
플레이어 자식으로 카메라를 두는 방식 대신 **씬에 시네머신 카메라를 1개 배치하고 Target만 갈아끼우는 방식** 채택.  
**이유**:
- 다중 플레이어 환경에서 카메라 인스턴스가 플레이어 수만큼 생기는 문제 회피
- 추후 관전 시점 / 결과 화면 / 컷씬 카메라 추가 시 Target 전환만으로 대응 가능
- 카메라 흔들림(Impulse) 등 외부 카메라 효과 시스템과 통합 용이

**[시네머신 Position/Rotation Control 선정]**  
- Position Control: `Hard Lock To Target` — 1인칭 시점은 보간(Damping) 없이 즉시 추적해야 흐물거림 없음
- Rotation Control: `Same As Follow Target` — Target(ViewPoint)의 회전을 그대로 사용. `Hard Look At`은 Target을 *바라보는* 회전이라 의도와 다름 (위치가 같으면 LookAt 방향 미정의로 오작동 가능)

**[CharacterController Radius 증가]**  
1인칭 시점에서 시야가 벽 안쪽으로 들어가 컬링되는 현상 방지를 위해 반경 증가. 카메라 NearClipPlane을 짧게(0.05) 두어도 캐릭터가 벽에 너무 가까이 붙으면 카메라 위치 자체가 콜라이더 밖으로 나가는 문제 발생 → 반경을 적정 수준으로 늘려 시점이 항상 콜라이더 내부에 위치하도록 보정.

**[CharacterController 채택 (vs Rigidbody)]**  
이전 OverTheSky 프로젝트에서 Rigidbody 기반 컨트롤러를 직접 구현했으나, 본 프로젝트는 호러 탐험 + 즉발 점프 위주라 물리 시뮬레이션 불필요. CharacterController의 빠른 반응성·간결한 API가 적합.

> **추후 폴리싱**: 상하 회전 시 카메라만 회전시키는 현재 방식에 캐릭터 Spine 본을 적정 수치로 IK 보정 추가하면 더 자연스러운 연출 가능 (다른 클라이언트가 봤을 때 위/아래 시선이 표현됨).

## Day 7 - 2026-04-30

### PlayerCamera 보강 — Position Constraint → LateUpdate 직접 추적

#### 이슈
이동 애니메이션 적용 후 Position Constraint가 헤드 본을 완벽히 따라가지 못하는 현상 확인. 점프 등 빠른 본 위치 변화 시 머리 메쉬가 잠깐 노출됨.

#### 원인 분석 — Unity 실행 순서
1. Update (스크립트 로직)
2. Animator (본 위치 갱신) — 헤드 본이 새 위치로
3. LateUpdate (스크립트)
4. Constraints 평가 — Position Constraint 여기서 동작
5. 렌더링

Position Constraint는 Animator 이후 평가되지만, 빠른 본 변화 시 이전 프레임 위치를 기준으로 계산하여 프레임 지연이 누적될 수 있음.

#### 해결 — LateUpdate 직접 추적
Position Constraint 컴포넌트 제거 후 PlayerCamera의 LateUpdate에서 직접 위치 갱신. 같은 프레임 내에서 위치 + 회전 동시 처리하여 지연 원천 차단.

흐름:
1. 프리팹에서 ViewPoint를 헤드 본 위치 근처(눈 중앙)에 시각적 배치
2. 시작 시점(`SetupOwnerView`)에 `InverseTransformPoint`로 헤드 본 기준 로컬 오프셋 캡처
3. 매 LateUpdate에서 `TransformPoint`로 오프셋을 다시 적용해 헤드 본 위치 추적
4. A→B 전환 시 새 헤드 본 기준으로 오프셋 재캡처

#### 헤드 본 참조 방식 — 인스펙터 할당
`Animator.GetBoneTransform` 사용 안 함. avatar 교체 시점에 본 캐싱이 갱신되는 타이밍 문제가 있어 의존성·예측 가능성 측면에서 인스펙터 할당이 우위.

> **시행착오 가치**: Constraint 사용 → 한계 발견 → LateUpdate 전환의 흐름이 프레임 지연 이슈에 대한 명확한 해결 동선이 됨.

---

### TeamA Avatar 교체 방식 개선
#### 배경
플레이어 작업 중 팀원의 TeamA 코드에서 책임 분리 측면 개선 여지 발견. 해당 팀원과 협의 후 직접 수정 진행.

#### 기존 방식의 문제
- 자식 GameObject(`_monsterModel`)에 비활성 Animator 컴포넌트가 존재하며 avatar 데이터 보관 용도로만 사용됨
- Animator 컴포넌트가 본래 목적(애니메이션 동작) 외 데이터 저장소로 우회 사용
- `_monsterModel`이 메쉬 표시 + avatar 보관 두 가지 책임을 가짐

#### 개선
- Avatar를 인스펙터에서 직접 할당
- `_monsterModel`은 메쉬 토글 전담으로 단일 책임
- 자식 Animator 컴포넌트 제거 가능 → 구조 단순화
- Avatar 자산은 다른 캐릭터에서도 재사용 가능

#### 영향
- 다른 시스템에 영향없이 동작 결과 동일

#### 안전장치 개선
- 프리팹에서 처음에 A가 꺼져있을 경우 켜지지 않는 문제 발생.
- 기존에  ApplyNormalAvatar()으로 노말 아바타로 초기화 하는 함수를 Awake 시점에 호출하여 해결

---

### PlayerAnimation 구현

- **역할**: Animator 파라미터 제어 및 블렌드 트리 연동을 통해 캐릭터의 시각적 움직임(로코모션, 점프 등)을 자연스럽게 표현하는 비주얼 전담 모듈.

- **구현 흐름**:
    - **[해시 캐싱(Awake)]** Animator 파라미터를 `static readonly int Anim___` 패턴으로 클래스 레벨 상수화하여 매 호출 해싱 비용 제거.
    - **[상태 참조(Update)]** `PlayerMovement`에서 계산된 `CurrentSpeed`, `IsGrounded`, `JustJumped` 등 상태값을 매 프레임 읽어 Animator 파라미터에 동기화.
    - **[블렌드 트리 연동]** Speed/MotionSpeed 파라미터로 Idle/Walk/Run 블렌드 트리를 자연스럽게 전환.
    - **[단발성 액션]** `JustJumped` 플래그 감지하여 점프 Bool 갱신.
    - **[Root Motion 분리]** Animator의 Root Motion 비활성화. 실제 변위는 `CharacterController.Move`가 전담, Animator는 시각적 움직임만 담당.

- **활용**: 물리 이동과 시각 표현이 완벽히 분리되어 애니메이션 클립 교체나 상체 전용 레이어 추가 시 이동 로직 코드 수정 불필요.

#### 설계 결정 사항

**[Update 폴링 방식 선택]**  
이동 관련 애니메이션 파라미터(Speed, Grounded 등)는 매 프레임 변화하는 연속값이라 이벤트 기반이 부적절. Sprint나 Attack 같은 호출성 이벤트는 이미 이벤트로 통일했지만, 연속값은 Unity 표준 패턴인 Update 폴링이 디버깅 동선도 짧고 효율적. **데이터 성격에 맞춰 처리 방식을 선택**.

**[Animator Layer 구성]**  
- Layer 0 `Locomotion`: 전신 이동 + 전신 반응 (Hit, Death)
- Layer 1 `Action`: 상체 전용 (Avatar Mask, Attack 등)

이동 중에도 손 공격이 가능해야 호러 추격 전투 분위기가 살기 때문에 Action Layer 분리. Hit/Death는 전신 정지가 자연스러우므로 Layer 0에서 처리.

**[Trigger는 이벤트 기반]**  
Attack, Hit, Death 같은 단발성 트리거는 PlayerCombat의 `NetworkVariable<PlayerCombatState>` 변경 시점에 `PlayStateAnimation(state)` 호출 방식으로 처리. 매 프레임 폴링이 아닌 호출 기반.

## Day 8 — 2026-05-04 (Week 2 Day 1)

### 플레이어 전투 애니메이션 구현

#### 모션 에셋 준비
공격(Punching), 피격(Getting Hit), 사망(Dying Backwards) 모션을 Mixamo에서 설정 후 유니티로 임포트.

#### Avatar Mask 결정
이동하며 공격하는 연출이 필요하므로 상체 마스킹 레이어 필요.
Avatar Mask는 Humanoid Rig의 본 추상화 기준으로 동작하므로 **A/B 모델 모두에 단일 마스크 적용 가능**. UpperBodyMask 한 개로 처리.

#### Animator Layer 구성

**Base Layer (Layer 0)**
- 기존 Locomotion 애니메이터를 Sub State Machine으로 정리하여 가독성 확보
- Dying Backwards (Any State 진입, 복귀 없음 — 사망은 영구 정지)

![alt text](Resources/Base_Layer.png)

**UpperBody Action Layer (Layer 1, Avatar Mask: 상체)**
- Idle (기본 상태, 마스크가 활성된 상태에서 아무 동작도 적용하지 않음)
- Punching (공격, Any State 진입 → Idle 복귀)
- Getting Hit (피격, Any State 진입 → Idle 복귀)

![alt text](Resources/UpperBodyAction_Layer.png)

#### 시행착오 — 피격 위치 변경

초기 설계: Hit/Death 모두 Base Layer (전신 반응)
**문제 발견**: 이동 중 피격 시 다리 이동 애니가 끊겨 어색함
**조정**: Hit는 UpperBody Layer로 이동 → 이동 중 피격 시 다리 애니 유지하면서 상체만 휘청거림
**Death는 Base Layer 유지**: 사망은 전신 정지가 자연스러움

#### 트랜지션 패턴
- 상체 액션은 모두 **Any State에서 트리거로 진입** → 일관성 확보
- 액션 종료 후 마스크가 활성된 상태에서 아무 동작도 적용하지 않은 Idle 상태로 트랜지션 (Layer 1만 비활성 효과)
- Death는 영구 정지라 복귀 없음

> **설계 결정 이유**: Any State 일괄 진입 패턴은 모든 상태에서 동일한 우선순위로 액션 트리거 가능하게 함. 추후 새 상체 액션(예: 도구 사용) 추가 시에도 같은 패턴으로 확장 가능.

#### 시행착오 — 마우스 연타 시 모션 끊김

연타 시 Punching 재생 도중 다시 처음부터 재생되는 현상 발견.

**원인**: Action Layer의 Any State → Punching 트랜지션에서 "Can Transition To Self"가 활성 → Punching 재생 중에 트리거 다시 들어오면 자기 자신으로 또 진입.

**해결**: "Can Transition To Self" 체크 해제. 코드 측에서도 NetworkVariable 동기화 지연 흡수 위해 RequestAttack에서 Weapon.IsReady 사전 체크.

---

### PlayerCombat 스크립트 작성

#### 책임 분리
- **PlayerCombat**: 상태 관리 / 애니메이션 호출 트리거 / 입력 차단 게이트
- **Weapon (팀원)**: 실제 공격 판정 / 데미지 전파 / 사운드
- **PlayerEntity (팀원)**: HP NetworkVariable / IDamageable 구현 / 사망 처리
- **PlayerAnimation**: 상태 받아 애니 트리거

이 책임 분리로 본인 코드는 **상태 결정만** 하면 되고, 판정/HP는 팀원 영역 활용.

#### 팀원 코드 통합
- **Weapon**: 팀원과 합의 후 onAttack 직접 구독 제거. PlayerCombat이 상태 체크 후 `_weapon.TryAttack()` 호출하는 구조로 변경. Weapon에 `IsReady` 프로퍼티 public 추가 (사전 체크용).
- **PlayerEntity**: 별도 PlayerHealth 만들지 않고 `_playerEntity.CurHp.OnValueChanged`와 `onDeath` 이벤트 구독으로 Hit/Dead 상태 전이 트리거.

#### PlayerCombatState Enum
- Normal / Attacking / Hit / Dead
- **Stunned 상태 제거** — 호러 게임 추격 분위기 위해 피격 시 입력 차단하지 않음. Hit는 애니메이션 표현 상태일 뿐, 이동/CanMove는 그대로 자유.

#### 행동 게이트
- `CanAct = (state == Normal)` — 새 공격은 Normal에서만
- `CanMove = (state != Dead)` — 사망 외엔 항상 이동 가능 (Attacking/Hit 중에도)

#### 권한 모델 — 클라 요청, 서버 승인
- Owner 클라에서 사전 검증 후 ServerRpc로 요청
- 서버가 상태 검증 + NetworkVariable 변경 + Weapon 호출
- 모든 클라는 NetworkVariable.OnValueChanged로 자동 동기화 → PlayerAnimation 트리거

#### Animation Event + UniTask 백업 하이브리드

상태 복귀 방식 선택:

**Animation Event 방식의 장점**
- 모션 끝 시점 = 상태 종료 시점 (정확히 일치)
- 모션 길이 변경 시 코드 수정 불필요
- UniTask Delay + 취소 토큰 처리 단순

**한계 — 이벤트 누락 케이스**
- 다른 트리거로 애니 도중 다른 State 전이 시 끝 이벤트 발생 안 함
- 상태가 Attacking에 영원히 머무를 위험

**해결 — 하이브리드**

### Weapon 영역 수정

#### 발견 흐름

PlayerCombat 작성 후 멀티 인스턴스 테스트 시 호스트는 공격이 정상 동작하지만 다른 클라이언트는 공격 모션만 나가고 실질적 공격(데미지/사운드)이 발생하지 않음.

#### 원인 1 — Owner 한정 Ready 초기화

Weapon 코드 분석:
Owner만 OnGameStart 구독 → Owner만 Ready() 호출 → Owner의 _state만 Ready로 변경

PlayerCombat이 ServerRpc 안에서 `_weapon.TryAttack()` 호출 → 서버 측 Weapon 인스턴스의 `_state` 확인.

서버 입장에서 본 Weapon `_state`:
- 호스트의 Weapon: 호스트가 Owner → `_state = Ready` ✅
- 다른 클라의 Weapon: 서버는 그 Weapon의 Owner가 아니라서 `OnNetworkSpawn`에서 early return → `_state = State.None` ❌

결과:
- 호스트 클라 공격 요청 → 서버가 호스트 Weapon 인스턴스에서 TryAttack → `_state == Ready` 통과 → 공격 성공
- 다른 클라 공격 요청 → 서버가 그 클라 Weapon 인스턴스에서 TryAttack → `_state == None` → 차단 → 공격 실패

**해결**: `OnGameStart` 구독을 `IsOwner` 체크 위로 이동 → 모든 인스턴스에서 Ready 처리.

#### 원인 2 — ServerRpc 권한 충돌

`_state` 문제 해결 후 새로운 에러 발생:
```
Only the owner can invoke a ServerRpc that requires ownership!
Battle.Weapon:AttackServerRpc
```
기존 Weapon은 Owner가 onAttack 직접 구독 → Owner 측에서 ServerRpc 호출 흐름.

**본인 통합 후 흐름**:
- Owner 클라 → SubmitAttackServerRpc (PlayerCombat) → 서버  
- 서버 → _weapon.TryAttack() → AttackServerRpc 호출 시도  
- 서버는 NetworkObject의 Owner가 아님 → 권한 거부  

#### 해결책 검토

**옵션 A — RequireOwnership = false**
- AttackServerRpc, BlockedServerRpc에 `RequireOwnership = false` 추가
- 두 줄 수정으로 즉시 동작
- 서버가 자기에게 ServerRpc 보내는 형태가 되어 의미 약화
- 클라 위조 가능성 (PlayerCombat이 게이트키퍼라 사실상 안전하지만)

**옵션 B — 서버 직접 처리 구조로 재구성** (선택)
- ServerRpc 제거, 서버 측 메서드 + ClientRpc 구조
- 권한 흐름 명료: 서버 권한 = 서버 처리, ServerRpc는 클라→서버 호출 전용
- 네트워크 메시지 1회 절감

옵션 A로 동작 확인 후 옵션 B로 재구성. 팀원 영역 큰 변경이라 사전 합의 후 직접 정리 및 수정.

#### 옵션 B 구현 변경 사항

- `AttackServerRpc` 제거 → `AttackOnServer` 서버 직접 메서드
- `BlockedServerRpc` 제거 → `BlockedClientRpc` 서버에서 직접 호출
- Miss 사운드를 `BroadcastMissClientRpc`로 모든 클라 동기화
- `TryAttack`에 `if (!IsServer) return` 가드 추가
- `OnGameStart` 구독 IsOwner 위로 이동 (원인 1 해결 통합)

> **시행착오 가치**: ServerRpc 권한 모델 + Owner-only 초기화 패턴이 서버 권한 통합 설계와 충돌하는 실전 케이스. 옵션 A(우회) vs 옵션 B(구조 정리) 비교를 통해 "동작 우선"과 "권한 모델 명료성"의 트레이드오프 학습.

### 사본 → 공용 영역 통합

본인 사본 환경(`Assets/WIP/LSG/`)에서 작업 진행하던 Player Proto 프리팹과 NetworkPrefabsList 항목을 공용 영역으로 이동:

- `Player_Proto.prefab` → `Assets/Project/Prefabs/Player/` 영역
- 네트워크 프리팹 등록 → 공용 DB 폴더

명명 정비 후 이동하여 다른 팀원이 동일 프리팹 사용 가능.  
기존 공용 폴더 Player 프리팹과의 통합/대체 협의는 추후 진행.

---

---
## 작업 일지 양식

## Day N — YYYY-MM-DD


