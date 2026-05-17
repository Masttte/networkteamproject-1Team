using System;
using Player;
using UnityEngine;

// PlayerA 프리팹에 부착
// 프리팹 구조:
//   PlayerA  (Layer: Player)
//   ├─ A     (Layer: Average)    A 시점: 보통 모델 (Animator + NetworkAnimator 보유)
//   ├─ B     (Layer: Beautiful)  B 시점: 괴물 모델 (Animator + NetworkAnimator 보유)
//   └─ ...
public class TeamA : TeamBase
{
    [SerializeField] GameObject _normalModel;
    [SerializeField] GameObject _monsterModel;

    public GameObject NormalModel => _normalModel;
    public GameObject MonsterModel => _monsterModel;
    
    /// <summary>
    /// 모델 전환(SetActive 변경) 직후 발행.
    /// PlayerAnimation 등 활성 모델의 Animator를 참조하는 컴포넌트가 재캐싱용으로 구독.
    /// </summary>
    public event Action OnModelSwitched;
    
    // 두 모델의 Renderer 캐싱 (SkinnedMeshRenderer 포함)
    private ModelVisual _normalVisual;
    private ModelVisual _monsterVisual;

    private void Awake()
    {
        // 시작 시 두 모델 모두 활성 (NGO가 NetworkBehaviour 등록 가능하도록)
        _normalModel.SetActive(true);
        _monsterModel.SetActive(true);
        
        // 비주얼 요소 캐싱 (자식 포함, 비활성 포함 X — 항상 활성이므로 불필요)
        _normalVisual  = _normalModel.GetComponent<ModelVisual>();
        _monsterVisual = _monsterModel.GetComponent<ModelVisual>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // NGO 등록 완료 후 적합한 모델만 활성
        SwitchToNormalModel();
    }

    protected override void OnTeamSetup() // 팀 배정 시 모든 클라이언트에서 호출됨
    {
        // 본인이 마피아면 시민들을 괴물로 보이게 전환
        // 본인 시점 결정이 늦으면 이벤트로 대응
        if (LocalManager.Instance.IamB)
            SwitchToMonsterModel();
        else
            LocalManager.Instance.OnIamBSet += SwitchToMonsterModel;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        LocalManager.Instance.OnIamBSet -= SwitchToMonsterModel;
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
    
    void SwitchToNormalModel()
    {
        _normalVisual.SetVisible(true);
        _monsterVisual.SetVisible(false);
        OnModelSwitched?.Invoke();
    }
    
    void SwitchToMonsterModel()
    {
        _normalVisual.SetVisible(false);
        _monsterVisual.SetVisible(true);
        OnModelSwitched?.Invoke();
    }
}
