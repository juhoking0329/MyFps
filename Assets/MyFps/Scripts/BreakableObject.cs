using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 파괴 가능한 오브젝트를 위한 클래스
    /// 총을 맞으면(데미지를 받으면) 파괴되는 오브젝트를 구현
    /// </summary>
    public class BreakableObject : MonoBehaviour, IDamageable
    {
        #region Variables
        [Header("Object Swaps")]
        [SerializeField]
        private GameObject fakeObject;          // 부서지기 전 원본 오브젝트
        [SerializeField]
        private GameObject breakObject;         // 부서진 상태의 오브젝트 그룹

        [Header("Hidden Loot")]
        [SerializeField]
        private GameObject hiddenItem;          // 부서졌을 때 나타날 아이템 (예: HeartKey)

        [Header("Effects")]
        [SerializeField]
        private GameObject breakEffectPrefab;   // 부서질 때 재생할 파티클 프리팹
        [SerializeField]
        private AudioSource breakAudioSource;   // 부서질 때 재생할 오디오 소스

        private Collider objectCollider;        // 피격용 콜라이더
        private bool isBroken = false;          // 파괴 여부 체크
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //00 자체 콜라이더 컴포넌트 캐싱
            objectCollider = GetComponent<Collider>();
        }
        #endregion

        #region Custom Methods
        //01 외부 데미지 입력 시 호출 (IDamageable 인터페이스 구현)
        public void TakeDamage(float damage)
        {
            if (isBroken) return;

            isBroken = true;
            Break();
        }

        //02 오브젝트 파괴 및 이펙트 처리 함수
        private void Break()
        {
            // 콜라이더 비활성화
            if (objectCollider != null)
            {
                objectCollider.enabled = false;
            }

            // 원본 비활성화 및 부서진 파트 활성화
            if (fakeObject != null)
            {
                fakeObject.SetActive(false);
            }

            if (breakObject != null)
            {
                breakObject.SetActive(true);

                //02-1 플레이어 콜라이더와 파편 콜라이더 간의 충돌 무시 처리 (플레이어 이동 방해 방지)
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Collider playerCollider = player.GetComponent<Collider>();
                    if (playerCollider == null)
                    {
                        playerCollider = player.GetComponent<CharacterController>();
                    }

                    if (playerCollider != null)
                    {
                        Collider[] shardColliders = breakObject.GetComponentsInChildren<Collider>(true);
                        foreach (var shardCollider in shardColliders)
                        {
                            Physics.IgnoreCollision(playerCollider, shardCollider, true);
                        }
                    }
                }
            }

            // 숨겨진 아이템(열쇠 등) 활성화
            if (hiddenItem != null)
            {
                hiddenItem.SetActive(true);
            }

            // 부서지는 파티클 이펙트 스폰
            if (breakEffectPrefab != null)
            {
                GameObject effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // 부서지는 오디오 효과음 재생
            if (breakAudioSource != null)
            {
                breakAudioSource.Play();
            }
        }
        #endregion
    }
}
