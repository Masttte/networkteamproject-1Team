using UnityEngine;

// PlayerA 프리팹에 부착
// 프리팹 구조:
//   PlayerA  (Layer: Player)
//   ├─ A     (Layer: Average)    A 시점: 보통 모델
//   ├─ B     (Layer: Beautiful)  B 시점: 괴물 모델
//   └─ ...
public class TeamA : TeamBase
{
    [SerializeField] GameObject _normalModel;
    [SerializeField] GameObject _monsterModel;
    
    [Header("Avatars")]
    [SerializeField] Avatar _normalAvatar;
    [SerializeField] Avatar _monsterAvatar;
    
    Animator _rootAnimator;

    private void Awake()
    {
        _rootAnimator = GetComponent<Animator>();
        
        // 시작 시점에 normal avatar 명시적 설정 및 꺼져있을지 모를 모델 활성화 (안전장치)
        ApplyNormalAvatar(); 
    }

    protected override void OnTeamSetup() // 팀 배정 시 모든 클라이언트에서 호출됨
    {
        // B팀 클라이언트에서 PlayerA를 괴물로 보이게 처리
        // 앞 순서로 생성된 PlayerA 프리팹은 델리게이트로 처리 (플레이어 B 설정이 늦었을때 적용)
        if (LocalManager.Instance.IamB)
            ApplyMonsterAvatar();
        else
            LocalManager.Instance.OnIamBSet += ApplyMonsterAvatar;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        LocalManager.Instance.OnIamBSet -= ApplyMonsterAvatar;
    }

    // 노말 아바타로 Reset/되돌리기
    void ApplyNormalAvatar()
    {
        _normalModel.SetActive(true);
        _monsterModel.SetActive(false);
        _rootAnimator.avatar = _normalAvatar;
    }
    
    // 루트 Animator의 Avatar를 괴물 Avatar 자산으로 교체
    void ApplyMonsterAvatar()
    {
        _normalModel.gameObject.SetActive(false);
        _monsterModel.gameObject.SetActive(true);

        _rootAnimator.avatar = _monsterAvatar;
    }

    protected override void UpdateNameText(string newName)
    {
        nameText.text = newName;

        // 내가B면 A팀(괴물)을 빨간색으로 표시
        if (LocalManager.Instance.IamB)
        {
            nameText.color = Color.red;
        }
        else
        {
            nameText.color = Color.white;
        }
    }
}
