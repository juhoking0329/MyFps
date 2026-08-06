using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.FPS.Game;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    public enum EnemyAttackType
    {
        SingleTarget,
        AreaDamage
    }

    /// <summary>
    /// 적(Enemy)의 기본 동작(피격, 사망 연출)과 공격 판정을 담당하는 컨트롤러 스크립트
    /// Health 이벤트와 연동되어 작동합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        // 인스펙터에서 할당받지 않고 코드에서 강제로 찾아서 사용하도록 변경
        private Renderer[] meshRenderers;

        // 컴포넌 참조
        private Health health;

        [Header("Attack Settings")]
        [Tooltip("공격 방식 (단일 대상 vs 범위 공격)")]
        [SerializeField] private EnemyAttackType attackType = EnemyAttackType.SingleTarget;
        
        [Tooltip("공격 시 입히는 데미지량")]
        [SerializeField] private float attackDamage = 20f;
        
        [Tooltip("범위 공격일 경우 피해를 입히는 반경")]
        [SerializeField] private float attackRadius = 3f;
        
        [Tooltip("데미지를 입힐 대상의 레이어 (범위 공격시 사용)")]
        [SerializeField] private LayerMask attackLayerMask = -1;

        [Header("Hit Effect")]
        [Tooltip("피격 시 머티리얼이 변할 색상")]
        [SerializeField] private Color hitFlashColor = Color.red;
        [Tooltip("피격 깜빡임 지속 시간")]
        [SerializeField] private float hitFlashDuration = 0.25f;
        [Tooltip("피격 사운드")]
        [SerializeField] private AudioClip hitSfx;
        
        private Color[] originalColors; // 렌더러들의 원래 색상 기억

        [Header("Death Effect")]
        [Tooltip("사망 시 생성할 폭발/파티클 프리팹")]
        [SerializeField] private GameObject deathVfxPrefab;
        [Tooltip("사망 사운드")]
        [SerializeField] private AudioClip deathSfx;
        [Tooltip("폭발 데미지를 무시할지 여부 등을 판별하기 위한 옵션")]
        [SerializeField] private bool useIgnoredDetection = true;

        [Header("Events")]
        [Tooltip("적이 사망했을 때 발생시킬 외부 이벤트 (점수, 보상 등)")]
        public UnityAction OnEnemyDeath;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // Health 컴포넌트 참조 가져오기
            health = GetComponent<Health>();

            // 1. 코드에서 강제로 자신과 자식들의 모든 렌더러를 찾아옵니다 (인스펙터 할당 필요 없음)
            meshRenderers = GetComponentsInChildren<Renderer>();

            // 2. 찾아온 렌더러들의 원래 메테리얼(Material) 색상 백업하기
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                originalColors = new Color[meshRenderers.Length];
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_BaseColor"))
                    {
                        originalColors[i] = meshRenderers[i].material.GetColor("_BaseColor");
                    }
                    else if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = meshRenderers[i].material.color;
                    }
                }
            }
        }

        private void Start()
        {
            // Actor 스크립트가 역할을 대신하므로, EnemyManager 관련 코드를 제거합니다.
            // ...지만 강사님이 EnemyManager를 다시 만드신 관계로 추가합니다!
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.RegisterEnemy(this);
            }
        }

        private void OnEnable()
        {
            // Health 이벤트 구독
            if (health != null)
            {
                health.onDamaged += HandleHitEffect;
                health.onDeath += HandleDeathEffect;
            }
        }

        private void OnDisable()
        {
            // Health 이벤트 구독 해제
            if (health != null)
            {
                health.onDamaged -= HandleHitEffect;
                health.onDeath -= HandleDeathEffect;
            }
        }
        #endregion

        #region Custom Methods
        
        /// <summary>
        /// AI 스크립트(또는 외부)에서 공격 애니메이션 타격 시점에 호출하는 공격 판정 함수
        /// </summary>
        /// <param name="target">공격 대상 (SingleTarget 전용)</param>
        /// <param name="attackCenter">공격 판정의 중심 위치 (AreaDamage 전용)</param>
        public void DoAttack(GameObject target, Vector3 attackCenter)
        {
            if (attackType == EnemyAttackType.SingleTarget)
            {
                // 단일 대상 공격
                if (target != null)
                {
                    Damageable targetDamageable = target.GetComponentInChildren<Damageable>();
                    if (targetDamageable != null)
                    {
                        // 폭발 데미지 여부 false, 주체는 자기 자신
                        targetDamageable.InflictDamage(attackDamage, false, gameObject);
                    }
                    else
                    {
                        // Damageable 컴포넌트가 없다면 Health를 직접 찾아서 데미지 적용
                        Health targetHealth = target.GetComponentInChildren<Health>();
                        if (targetHealth != null)
                        {
                            targetHealth.TakeDamage(attackDamage, gameObject);
                        }
                    }
                }
            }
            else if (attackType == EnemyAttackType.AreaDamage)
            {
                // 범위 공격 (Physics.OverlapSphere 활용)
                Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRadius, attackLayerMask);
                foreach (Collider hitCollider in hitColliders)
                {
                    // 자신은 공격 대상에서 제외
                    if (hitCollider.transform.root == transform.root) continue;

                    // 마커가 부착된 오브젝트는 무시 (예: 방해물, 무적 기믹 등)
                    if (useIgnoredDetection)
                    {
                        IgnoredDetection ignore = hitCollider.GetComponent<IgnoredDetection>();
                        if (ignore != null) continue;
                    }

                    Damageable targetDamageable = hitCollider.GetComponent<Damageable>();
                    if (targetDamageable != null)
                    {
                        // 범위 데미지 적용
                        targetDamageable.InflictDamage(attackDamage, true, gameObject);
                    }
                    else
                    {
                         // Damageable이 없더라도 Health가 있으면 데미지 적용
                         Health health = hitCollider.GetComponentInParent<Health>();
                         if(health != null)
                         {
                              health.TakeDamage(attackDamage, gameObject);
                         }
                    }
                }
            }
        }

        /// <summary>
        /// 피격 시 연출 처리 (사운드 및 색상 깜빡임)
        /// </summary>
        private void HandleHitEffect(float damage, GameObject damageSource)
        {
            // 피격 사운드 재생
            if (hitSfx != null)
            {
                AudioUtility.CreateSFX(hitSfx, transform.position, 1f, 1f);
            }

            // 머티리얼 깜빡임 코루틴 시작
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                StopCoroutine("FlashRoutine");
                StartCoroutine("FlashRoutine");
            }
        }

        /// <summary>
        /// 색상을 붉은색으로 바꿨다가 되돌리는 코루틴
        /// </summary>
        private IEnumerator FlashRoutine()
        {
            // 피격 색상으로 변경
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                {
                    if (meshRenderers[i].material.HasProperty("_BaseColor"))
                        meshRenderers[i].material.SetColor("_BaseColor", hitFlashColor);
                    else if (meshRenderers[i].material.HasProperty("_Color"))
                        meshRenderers[i].material.color = hitFlashColor;
                }
            }

            // 대기
            yield return new WaitForSeconds(hitFlashDuration);

            // 원래 색상으로 복구
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                {
                    if (meshRenderers[i].material.HasProperty("_BaseColor"))
                        meshRenderers[i].material.SetColor("_BaseColor", originalColors[i]);
                    else if (meshRenderers[i].material.HasProperty("_Color"))
                        meshRenderers[i].material.color = originalColors[i];
                }
            }
        }

        /// <summary>
        /// 사망 시 연출 및 파괴 처리
        /// </summary>
        private void HandleDeathEffect()
        {
            // 폭발 파티클 생성
            if (deathVfxPrefab != null)
            {
                GameObject vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
                // 자동 파괴 처리는 vfx 프리팹 내부에 TimeSelfDestruct가 있거나, 아래처럼 직접 붙여줌
                TimeSelfDestruct tsd = vfx.GetComponent<TimeSelfDestruct>();
                if (tsd == null)
                {
                    tsd = vfx.AddComponent<TimeSelfDestruct>();
                    tsd.lifeTime = 3f;
                }
            }

            // 사망 사운드 재생
            if (deathSfx != null)
            {
                AudioUtility.CreateSFX(deathSfx, transform.position, 1f, 1f);
            }

            // 외부 구독자(게임 매니저, 퀘스트 매니저 등)에게 사망 소식 알림
            if (OnEnemyDeath != null)
            {
                OnEnemyDeath.Invoke();
            }

            // 매니저에서 명단 제거
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.RemoveEnemy(this);
            }

            // 적 오브젝트 파괴
            Destroy(gameObject);
        }
        #endregion
    }
}
