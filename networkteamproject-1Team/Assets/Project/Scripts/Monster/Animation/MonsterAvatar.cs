using Unity.Netcode;
using UnityEngine;

namespace Monster
{
    // 몬스터 아바타 시각관리 클래스 (현재는 단순히 Avatar 교체만 담당, 추후 확장 가능성있음)
    public class MonsterAvatar : NetworkBehaviour
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
        }

        protected override void OnNetworkPostSpawn()
        {
            if (LocalManager.Instance.IamB) // 몬스터 스폰은 이미 B가 정해진 이후이므로, IamB 여부로 바로 적용
                ApplyNormalAvatar();
            else
                ApplyMonsterAvatar();
        }


        void ApplyNormalAvatar()
        {
            _normalModel.SetActive(true);
            _monsterModel.SetActive(false);

            _rootAnimator.avatar = _normalAvatar;
        }
        void ApplyMonsterAvatar()
        {
            _normalModel.gameObject.SetActive(false);
            _monsterModel.gameObject.SetActive(true);

            _rootAnimator.avatar = _monsterAvatar;
        }
    }
}