using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public string Name;
    public int damage;
    public float range;
    public float radius; // CircleCast 반경
    public float cooltime;

    [Header("오디오")]
    public AudioResource attackMiss;
    public AudioResource attackHit;
    public AudioResource attackBlocked;

    // 총기류 확장시 사용
    // public int maxAmmo;
    // public int magCapacity;
    // public float reloadTime;
}
