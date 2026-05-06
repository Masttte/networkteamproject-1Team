using Battle;
using UnityEngine;

// PlayerB 프리팹에 부착
// B는 모두에게 사람으로 보임
public class TeamB : TeamBase
{
    protected override void OnTeamSetup()
    {
        BattleManager.Instance.OnGameStart += UpdateNameText;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        BattleManager.Instance.OnGameStart -= UpdateNameText;
    }
    private void UpdateNameText()
    {
        UpdateNameText(PlayerName.Value.ToString());
    }
    protected override void UpdateNameText(string newName)
    {
        nameText.text = newName;
    }
}
