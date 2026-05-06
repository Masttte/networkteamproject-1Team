using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

// AnimationEvent를 받아 사운드 재생
public class AcPlayerSound : NetworkBehaviour
{
    [SerializeField] AudioResource _footStep;
    [SerializeField] AudioResource _land;

    public void OnFootstep(AnimationEvent animationEvent)
    {
        AudioManager.Instance.PlaySfxWet(_footStep, this.transform.position);
    }
    public void OnLand(AnimationEvent animationEvent)
    {
        if (!IsOwner) return; // 착지 이벤트는 오너가 아니면 가끔 안돼서, 아예 오너만 받고 ClientRpc 연결
        OnLandRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void OnLandRpc()
    {
        AudioManager.Instance.PlaySfxWet(_land, this.transform.position);
    }
}
