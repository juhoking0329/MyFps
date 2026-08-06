using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.Game;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    //00 AI 상태를 정의하는 열거형
    public enum EnemyState
    {
        Patrol,     // 순찰
        Follow,     // 추적
        Attack      // 공격
    }

    /// <summary>
    /// 적의 인공지능(상태 머신)을 관리하는 클래스
    /// NavMeshAgent를 이용해 이동하며, 플레이어 탐지 시 추적 및 공격을 수행합니다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        #region Variables
        [Header("State")]
        [Tooltip("현재 적의 상태")]
        [SerializeField] private EnemyState currentState = EnemyState.Patrol;

        [Header("Navigation / Patrol")]
        [Tooltip("순찰할 웨이포인트 목록")]
        [SerializeField] private Transform[] wayPoints;
        [Tooltip("다음 웨이포인트로 넘어갈 도착 판정 거리")]
        [SerializeField] private float waypointThreshold = 0.5f;
        [Tooltip("순찰 시 이동 속도")]
        [SerializeField] private float patrolSpeed = 2f;
        [Tooltip("추적 시 이동 속도")]
        [SerializeField] private float followSpeed = 5f;
        
        private NavMeshAgent agent;
        private int currentWaypointIndex = 0;

        [Header("Detection")]
        [Tooltip("플레이어를 탐지할 수 있는 반경")]
        [SerializeField] private float sightRange = 15f;
        [Tooltip("플레이어 또는 아군으로 판정할 레이어")]
        [SerializeField] private LayerMask targetLayer;

        [Header("Attack")]
        [Tooltip("공격(발사)이 가능한 사거리")]
        [SerializeField] private float attackRange = 8f;
        [Tooltip("공격 주기 (초 단위 쿨타임)")]
        [SerializeField] private float timeBetweenAttacks = 2f;
        
        [Tooltip("발사할 투사체(Projectile) 프리팹")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("투사체가 생성될 총구 위치")]
        [SerializeField] private Transform muzzleTransform;

        private Transform playerTarget;
        private bool alreadyAttacked;

        // Health 연동 (사망 시 AI 정지 용도)
        private Health health;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // 컴포넌트 참조
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        private void Start()
        {
            // 초기 속도 설정
            agent.speed = patrolSpeed;
        }

        private void Update()
        {
            // 사망 상태라면 AI 로직 정지
            if (health != null && health.CurrentHealth <= 0)
            {
                agent.isStopped = true;
                return;
            }

            // 매 프레임 상태를 체크하여 분기 처리
            CheckStateAndTransition();
            
            // 현재 상태에 따른 행동 수행
            switch (currentState)
            {
                case EnemyState.Patrol:
                    PatrolState();
                    break;
                case EnemyState.Follow:
                    FollowState();
                    break;
                case EnemyState.Attack:
                    AttackState();
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 탐지 범위와 공격 범위를 눈으로 쉽게 확인하기 위한 기즈모 그리기
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 주변에 플레이어가 있는지(거리 체크) 확인하고 상태를 전이시킵니다.
        /// </summary>
        private void CheckStateAndTransition()
        {
            // 탐지 범위 내에 대상이 있는지 확인
            Collider[] targetsInSight = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
            bool playerInSightRange = targetsInSight.Length > 0;
            
            // 공격 범위 내에 대상이 있는지 확인
            Collider[] targetsInAttackRange = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
            bool playerInAttackRange = targetsInAttackRange.Length > 0;

            if (playerInSightRange || playerInAttackRange)
            {
                // 대상이 있으면 타겟으로 설정
                Collider targetCollider = playerInAttackRange ? targetsInAttackRange[0] : targetsInSight[0];
                playerTarget = targetCollider.transform;
            }
            else
            {
                playerTarget = null;
            }

            // 조건에 따른 상태 변경
            if (!playerInSightRange && !playerInAttackRange)
            {
                currentState = EnemyState.Patrol;
                agent.speed = patrolSpeed;
            }
            else if (playerInSightRange && !playerInAttackRange)
            {
                currentState = EnemyState.Follow;
                agent.speed = followSpeed;
            }
            else if (playerInSightRange && playerInAttackRange)
            {
                currentState = EnemyState.Attack;
            }
        }

        /// <summary>
        /// 순찰(Patrol) 행동: 지정된 웨이포인트를 순회합니다.
        /// </summary>
        private void PatrolState()
        {
            if (wayPoints == null || wayPoints.Length == 0)
                return;

            agent.isStopped = false;
            
            // 현재 목적지로 이동
            Transform targetWaypoint = wayPoints[currentWaypointIndex];
            agent.SetDestination(targetWaypoint.position);

            // 목적지에 거의 도달했는지 확인 (거리 체크)
            Vector3 distanceToWaypoint = transform.position - targetWaypoint.position;
            if (distanceToWaypoint.magnitude < waypointThreshold)
            {
                // 다음 웨이포인트로 인덱스 증가 (배열 끝에 도달하면 0으로 순환)
                currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Length;
            }
        }

        /// <summary>
        /// 추적(Follow) 행동: 발견한 플레이어를 향해 쫓아갑니다.
        /// </summary>
        private void FollowState()
        {
            if (playerTarget == null) return;

            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }

        /// <summary>
        /// 공격(Attack) 행동: 제자리에 멈춰서 플레이어를 향해 발사체를 쏩니다.
        /// </summary>
        private void AttackState()
        {
            if (playerTarget == null) return;

            // 공격 중에는 이동하지 않도록 정지
            agent.isStopped = true;

            // 플레이어 방향을 바라보게 회전 (LookAt 시 y축 회전만 적용하도록 보정)
            Vector3 lookDirection = playerTarget.position - transform.position;
            lookDirection.y = 0; // 기울어지지 않게 방지
            if(lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
            }

            // 쿨타임이 지났다면 공격 실행
            if (!alreadyAttacked)
            {
                ShootProjectile();

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks); // 쿨타임 후 초기화
            }
        }

        /// <summary>
        /// 발사체(Projectile)를 생성하여 발사합니다.
        /// </summary>
        private void ShootProjectile()
        {
            if (projectilePrefab == null || muzzleTransform == null) return;

            // 총구 위치에서 투사체 생성
            GameObject projectileInstance = Instantiate(projectilePrefab, muzzleTransform.position, muzzleTransform.rotation);
            
            // 투사체의 Owner(주인)를 이 적군 오브젝트로 설정 (자신이 쏜 총알에 맞지 않기 위해)
            ProjectileStandard projectileStandard = projectileInstance.GetComponent<ProjectileStandard>();
            if (projectileStandard != null)
            {
                // 타겟의 중심부(가슴)를 향해 발사하도록 오프셋 적용
                Vector3 targetPos = playerTarget.position;
                // Actor 시스템을 이용해 조준점을 찾습니다.
                Actor targetActor = playerTarget.GetComponent<Actor>();
                if (targetActor != null && targetActor.aimPoint != null)
                {
                    targetPos = targetActor.aimPoint.position;
                }
                else
                {
                    // 예비(Fallback) 문자열 검색 및 기본 오프셋
                    Transform fallbackAimPoint = playerTarget.Find("AimPoint");
                    if (fallbackAimPoint != null)
                    {
                        targetPos = fallbackAimPoint.position;
                    }
                    else
                    {
                        targetPos += Vector3.up * 1.5f; // 기본 오프셋
                    }
                }
                
                Vector3 shootDirection = (targetPos - muzzleTransform.position).normalized;
                projectileStandard.Shoot(gameObject, shootDirection, agent.velocity, 0f);
            }
        }

        /// <summary>
        /// 쿨타임 이후 다시 공격할 수 있도록 플래그를 초기화합니다.
        /// </summary>
        private void ResetAttack()
        {
            alreadyAttacked = false;
        }
        #endregion
    }
}
