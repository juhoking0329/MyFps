using UnityEngine;
using UnityEngine.AI;

namespace MyFps
{
    /// <summary>
    /// 로봇의 애니메이션 상태 및 기본 상태 정의
    /// </summary>
    public enum GunManState
    {
        G_Idle = 0,
        G_Walk = 1,     // 추격용 Walk (하반신만 걷기)
        G_Attack = 2,
        G_Death = 3,
        G_Patrol = 4    // 순찰용 Walk (전신 Rifle Walk)
    }

    /// <summary>
    /// AI 모드 정의 (순찰 중인지 추격 중인지 구분)
    /// </summary>
    public enum GunManAIMode
    {
        Patrol,
        Chase
    }

    /// <summary>
    /// 건맨 적을 관리하는 클래스
    /// IDamageable 상속 받는다
    /// NavMeshAgent를 이용해 이동
    /// </summary>
    public class GunMan : MonoBehaviour, IDamageable
    {
        #region Variables
        //참조
        private Animator animator;
        private Transform thePlayer;
        private Player player;
        private NavMeshAgent agent;

        //로봇의 상태 (enum)
        [SerializeField] private GunManState currentState;    //현재 상태
        private GunManState beforeState;     //현재 상태의 바로 이전 상태
        
        [SerializeField] private GunManAIMode currentGunManAIMode = GunManAIMode.Patrol;

        //이동 및 순찰
        [Header("Patrol Settings")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private Transform[] waypoints; // 인스펙터창에서 빈 오브젝트 할당
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        private bool isWaiting = false;
        private const string isFiring = "IsFiring";

        //추격 및 공격
        [Header("Combat Settings")]
        [SerializeField] private float chaseRange = 15f;    // 추격 시작 거리
        [SerializeField] private float loseTargetRange = 20f; // 추격 포기 거리 (Enemy 구역 이탈 기준)
        [SerializeField] private float attakRange = 10f;     // 사격 사거리
        [SerializeField] private float attackDamage = 5f;   // 공격력
        [SerializeField] private float attackDelay = 2.0f;  // 사격 주기 (2초마다 발사)
        private float attackTimer = 0f;
        private const string fireTrigger = "FireTrigger";

        //체력
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 20f;
        private float currentHealth = 0f;
        private bool isDeath = false;       //죽음 체크

        //애니메이션 파라미터
        private const string enemyState = "EnemyState";

        [Header("Audio")]
        [SerializeField] private AudioSource jumpScareBgm;
        [SerializeField] private AudioSource normalBgm;

        // 다중 적 BGM 관리를 위한 정적 변수
        private static int activeChaseCount = 0;
        private bool isCurrentlyChasing = false;

        [Header("회전 보정")]
        [SerializeField] private float aimRotationOffsetY = 0f; // 조준 방향 오프셋 (인스펙터 조절용)
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            activeChaseCount = 0;
            isCurrentlyChasing = false;

            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            
            // NavMeshAgent가 없으면 동적으로 추가
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }

            player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                thePlayer = player.transform;
            }
        }

        private void Start()
        {
            //초기화
            agent.speed = moveSpeed;
            agent.stoppingDistance = attakRange - 0.5f;
            agent.updateRotation = true; // 이동 시 자연스러운 회전을 위해 켬

            currentGunManAIMode = GunManAIMode.Patrol;
            ChangeState(GunManState.G_Patrol); // 순찰 시작
            currentHealth = maxHealth;

            // 오디오 소스 자동 매핑 (미할당 시)
            if (jumpScareBgm == null)
            {
                GameObject jsGo = GameObject.Find("JumpScare");
                if (jsGo != null) jumpScareBgm = jsGo.GetComponent<AudioSource>();
            }
            if (normalBgm == null)
            {
                GameObject shGo = GameObject.Find("SHAmb");
                if (shGo != null) normalBgm = shGo.GetComponent<AudioSource>();
            }
            
            // 첫 웨이포인트 목적지 설정
            if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
            {
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }

        private void Update()
        {
            if (isDeath) return;

            if (thePlayer == null)
            {
                player = FindFirstObjectByType<Player>();
                if (player != null) thePlayer = player.transform;
                return;
            }

            if (player.IsDeath) return;

            float distanceToPlayer = Vector3.Distance(thePlayer.position, transform.position);

            // 상태 및 AI 모드 업데이트 로직
            UpdateAI(distanceToPlayer);

            // 현재 상태별 로직 실행
            ExecuteStateLogic(distanceToPlayer);
        }
        #endregion

        #region AI Logic
        // 시야각 내에 있고 장애물이 없는지 확인하는 함수
        private bool CanSeePlayer()
        {
            if (thePlayer == null) return false;
            
            // 총구/가슴 높이에서 플레이어 가슴 높이로 레이캐스트
            Vector3 origin = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
            Vector3 target = thePlayer.position + Vector3.up * 1.5f;
            Vector3 direction = (target - origin).normalized;

            // loseTargetRange만큼 멀리까지 레이를 쏴서 플레이어인지 확인
            if (Physics.Raycast(origin, direction, out RaycastHit hit, loseTargetRange))
            {
                if (hit.collider.CompareTag("Player") || hit.collider.transform.root == thePlayer.root)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateAI(float distanceToPlayer)
        {
            // 공격 중이라면 시선 고정
            if (currentState == GunManState.G_Attack)
            {
                // 거리가 벌어지거나 시야가 막히면(벽, 문) 다시 추격
                if (distanceToPlayer > attakRange || !CanSeePlayer())
                {
                    ChangeState(GunManState.G_Walk);
                    currentGunManAIMode = GunManAIMode.Chase;
                }
                return;
            }

            // 순찰 모드에서 추격 모드로 전환
            if (currentGunManAIMode == GunManAIMode.Patrol)
            {
                // 거리 안에 있고 시야가 뚫려있어야 인식(Chase)
                if (distanceToPlayer <= chaseRange && CanSeePlayer())
                {
                    currentGunManAIMode = GunManAIMode.Chase;
                    isWaiting = false; // 대기 중단
                    
                    // 다중 적 대비 배경음 BGM 변경 로직
                    if (!isCurrentlyChasing)
                    {
                        isCurrentlyChasing = true;
                        activeChaseCount++;
                        if (activeChaseCount == 1)
                        {
                            if (normalBgm != null) normalBgm.Stop();
                            if (jumpScareBgm != null && !jumpScareBgm.isPlaying) jumpScareBgm.Play();
                        }
                    }

                    ChangeState(GunManState.G_Walk); // 추격용 Walk 상태로 변경
                }
            }
            // 추격 모드에서 순찰 모드로 전환 (구역 이탈 또는 시야 차단)
            else if (currentGunManAIMode == GunManAIMode.Chase)
            {
                if (distanceToPlayer > loseTargetRange || !CanSeePlayer())
                {
                    currentGunManAIMode = GunManAIMode.Patrol;
                    
                    // 다중 적 대비 배경음 BGM 변경 로직
                    if (isCurrentlyChasing)
                    {
                        isCurrentlyChasing = false;
                        activeChaseCount--;
                        if (activeChaseCount <= 0)
                        {
                            activeChaseCount = 0;
                            if (jumpScareBgm != null) jumpScareBgm.Stop();
                            if (normalBgm != null && !normalBgm.isPlaying) normalBgm.Play();
                        }
                    }
                    
                    // 순찰 웨이포인트로 목적지 재설정
                    if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
                    {
                        agent.SetDestination(waypoints[currentWaypointIndex].position);
                    }
                    ChangeState(GunManState.G_Patrol); // 다시 순찰용 Walk 상태로 변경
                }
            }

            // 추격 중인데 공격 거리 안으로 들어오고 시야가 확보되면
            if (currentGunManAIMode == GunManAIMode.Chase && distanceToPlayer <= attakRange && CanSeePlayer())
            {
                ChangeState(GunManState.G_Attack);
            }
        }

        private void ExecuteStateLogic(float distanceToPlayer)
        {
            switch (currentState)
            {
                case GunManState.G_Idle:
                    if (currentGunManAIMode == GunManAIMode.Patrol && isWaiting)
                    {
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0f)
                        {
                            isWaiting = false;
                            
                            // 다음 웨이포인트 설정
                            if (waypoints != null && waypoints.Length > 0)
                            {
                                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                                if (waypoints[currentWaypointIndex] != null)
                                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                            }
                            ChangeState(GunManState.G_Patrol);
                        }
                    }
                    break;

                case GunManState.G_Patrol:
                    if (currentGunManAIMode == GunManAIMode.Patrol)
                    {
                        agent.isStopped = false;

                        // 웨이포인트 도착 확인
                        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                        {
                            isWaiting = true;
                            waitTimer = Random.Range(2f, 3f);
                            agent.isStopped = true;
                            ChangeState(GunManState.G_Idle);
                        }
                    }
                    break;

                case GunManState.G_Walk:
                    if (currentGunManAIMode == GunManAIMode.Chase)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(thePlayer.position);
                    }
                    break;

                case GunManState.G_Attack:
                    agent.isStopped = true;
                    // 플레이어 쳐다보기 + 오프셋 적용
                    Vector3 attackLookPos = thePlayer.position;
                    attackLookPos.y = transform.position.y;
                    Quaternion attackTargetRot = Quaternion.LookRotation(attackLookPos - transform.position);
                    attackTargetRot *= Quaternion.Euler(0, aimRotationOffsetY, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, attackTargetRot, Time.deltaTime * 8f);

                    // 2초마다 주기적 사격 트리거
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                    {
                        Attack();
                        attackTimer = attackDelay; // 주기 리셋
                    }
                    break;
            }
        }
        #endregion

        #region Custom Method
        public void ChangeState(GunManState newState)
        {
            beforeState = currentState;
            currentState = newState;

            //애니메이터 파라미터 업데이트
            animator.SetInteger(enemyState, (int)currentState);

            // 상태에 따른 NavMeshAgent 회전 제어
            if (newState == GunManState.G_Attack)
            {
                agent.updateRotation = false; // 공격 시 수동으로 회전 보정
                attackTimer = attackDelay; // 상태 진입 시 바로 사격하지 않고 딜레이 적용
            }
            else
            {
                agent.updateRotation = true; // 그 외에는 자연스럽게 이동 방향 주시
            }
        }

        // 타이머에 의해 주기적으로 호출되는 공격 함수
        public void Attack()
        {
            if (thePlayer == null || isDeath) return;

            // 사격 애니메이션 재생 (트리거 발동 -> 1회 재생 후 자동 복귀)
            animator.SetTrigger(fireTrigger);
            Debug.Log("Attack 호출됨 - FireTrigger 발동");

            float distance = Vector3.Distance(thePlayer.position, transform.position);
            // 사거리에 있고 시야(벽/문)가 막히지 않았을 때만 데미지 판정
            if (distance <= attakRange + 2.0f && CanSeePlayer()) 
            {
                IDamageable damageable = thePlayer.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                }
            }
            else if (!CanSeePlayer())
            {
                Debug.Log("사격이 벽이나 문에 막혔습니다.");
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDeath) return;

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} currentHealth: {currentHealth}");

            // 데미지 효과 처리 (생략)

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        void Die()
        {
            isDeath = true;
            if (agent != null) agent.isStopped = true;

            // NavMeshAgent 비활성화 (물리 충돌 방지)
            if (agent != null) agent.enabled = false;

            // 승천(충돌 버그) 방지: 물리력 차단 및 충돌체 제거
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            CapsuleCollider col = gameObject.GetComponent<CapsuleCollider>();
            if (col != null) col.enabled = false;

            // 사망 애니메이션 중 공중 부양 버그를 방지하기 위해 루트 모션 다시 활성화
            if (animator != null) animator.applyRootMotion = true;

            if (isCurrentlyChasing)
            {
                isCurrentlyChasing = false;
                activeChaseCount--;
                if (activeChaseCount <= 0)
                {
                    activeChaseCount = 0;
                    if (jumpScareBgm != null) jumpScareBgm.Stop();
                    if (normalBgm != null && !normalBgm.isPlaying) normalBgm.Play();
                }
            }

            ChangeState(GunManState.G_Death);
        }
        #endregion
    }
}