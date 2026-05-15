using Battle;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class Generator : NetworkBehaviour, IInteractable
{
    private PressAction _pressAction;
    bool _isInteracting; // 로컬 플레이어가 현재 상호작용 중인지 여부

    public MaterialChanger changer;

    private NetworkVariable<bool> _isRepaired = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] AudioResource _playGenerator;

    public override void OnNetworkSpawn()
    {
        _pressAction = GetComponent<PressAction>();

        _isRepaired.OnValueChanged += OnRepairedStateChange;

        if (IsServer)
        {
            _pressAction.IsPressAction += OnPressCompleted;
        }

        // 게임 중간에 들어온거 방어로직
        if (_isRepaired.Value)
        {
            ApplyCompletedVisual();
        }
    }

    public override void OnNetworkDespawn()
    {
        _isRepaired.OnValueChanged -= OnRepairedStateChange;

        if (IsServer && _pressAction != null)
        {
            _pressAction.IsPressAction -= OnPressCompleted;
        }
    }

    // PressAction (발전기 키는 조건) 게이지가 10초 이상이 됐을 때
    private void OnPressCompleted()
    {
        if (_isRepaired.Value) return;

        _isRepaired.Value = true;
    }

    // 수정 => IsServer 플래그 변수에 걸리지 않게 변경
    private void OnRepairedStateChange(bool previousValue, bool newValue)
    {
        if (!newValue) return;

        ApplyCompletedVisual();

        // 상호작용 중이던 로컬 플레이어만 이동 복구 + 사운드
        if (_isInteracting)
            StopLocalInteract();

        if (BattleManager.Instance != null)
            BattleManager.Instance.OnGeneratorCondition();
    }

    private void ApplyCompletedVisual()
    {
        gameObject.layer = 2; // Ignore Raycast

        if (_pressAction != null)
        {
            if (_pressAction.image != null && _pressAction.image.canvas != null)
                _pressAction.image.canvas.gameObject.SetActive(false);

            _pressAction.enabled = false;
        }

        if (changer != null)
            changer.ChangeMaterial();


        AudioManager.Instance.PlaySfxWet(_playGenerator, transform.position);
    }

    public void InteractStart()
    {
        if (_isRepaired.Value) return;

        _pressAction.StartInteraction();
        _isInteracting = true;

        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        var input = playerObj.GetComponent<PlayerInputHandler>();
        input.DisableInput(InputCategory.Movement); // 발전기 상호작용 중에는 움직이지 못하게

        AudioManager.Instance.PlayUnlockLoop().Forget();
    }

    public void InteractStop()
    {
        if (_isInteracting)
            _pressAction.StopInteraction();

        StopLocalInteract();
    }

    void StopLocalInteract()
    {
        if (!_isInteracting) return;
        _isInteracting = false;

        var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObj != null)
        {
            var input = playerObj.GetComponent<PlayerInputHandler>();
            input.EnableInput(InputCategory.Movement);
        }

        AudioManager.Instance.StopUnlockLoop();
    }
}
