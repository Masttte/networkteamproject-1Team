using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;
using Battle;

public enum TeamType { None, A, B }

// A / B 팀 공통 로직 담당 추상 클래스
public abstract class TeamBase : NetworkBehaviour
{
    const int LAYER_A = 10;
    const int LAYER_B = 11;

    // 스폰 직후 팀 결정
    public NetworkVariable<TeamType> Team = new NetworkVariable<TeamType>(TeamType.None);

    // 플레이어 닉네임 동기화
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>("");

    public TMP_Text nameText;


    public override void OnNetworkSpawn()
    {
        Team.OnValueChanged += OnTeamChanged;
        PlayerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            BattleManager.Instance.OnGameStart += SetPlayerName;
        }
    }

    public override void OnNetworkDespawn()
    {
        Team.OnValueChanged -= OnTeamChanged;
        PlayerName.OnValueChanged -= OnPlayerNameChanged;

        if (IsOwner)
        {
            BattleManager.Instance.OnGameStart -= SetPlayerName;
        }
    }

    public void SetPlayerName()
    {
        SetPlayerNameServerRpc(LobbyManager.Instance.PlayerName);
    }
    [ServerRpc]
    void SetPlayerNameServerRpc(string newName)
    {
        if (!string.IsNullOrEmpty(newName)) PlayerName.Value = newName;
    }
    void OnPlayerNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        UpdateNameText(current.ToString());
    }
    protected virtual void UpdateNameText(string newName)
    {
        nameText.text = newName;
    }

    void OnTeamChanged(TeamType prev, TeamType next) => HandleTeamAssigned(next);
    protected abstract void OnTeamSetup();

    void HandleTeamAssigned(TeamType team)
    {
        // 공통 처리
        if (IsOwner)
        {
            ApplyCullingMask(team);
            LocalManager.Instance.IamB = (team == TeamType.B);
        }

        // 자식 처리 (IsOwner는 내부에서 판단)
        OnTeamSetup();
    }

    void ApplyCullingMask(TeamType team)
    {
        Camera cam = Camera.main;
        if (team == TeamType.B)
        {
            cam.cullingMask |= 1 << LAYER_B;
            cam.cullingMask &= ~(1 << LAYER_A);
        }
        else
        {
            cam.cullingMask |= 1 << LAYER_A;
            cam.cullingMask &= ~(1 << LAYER_B);
        }
    }

    [ClientRpc]
    public void ForceTeleportClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
    {
        if (TryGetComponent(out CharacterController cc))
            cc.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        if (TryGetComponent(out CharacterController cc2))
            cc2.enabled = true;
    }
}
