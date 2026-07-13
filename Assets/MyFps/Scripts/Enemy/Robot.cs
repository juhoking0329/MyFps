using UnityEngine;
using UnityEngine.AI;

namespace MyFps
{
    /// <summary>
    /// 로봇의 애니메이션 상태 및 기본 상태 정의
    /// </summary>
    public enum RobotState
    {
        R_Idle = 0,
        R_Walk = 1,
        R_Attack = 2,
        R_Death = 3
    }

    /// <summary>
    /// AI 모드 정의 (순찰 중인지 추격 중인지 구분)
    /// </summary>
    public enum AIMode
    {
        Patrol,
        Chase
    }

    /// <summary>
    /// 로봇 적을 관리하는 클래스
    /// IDamageable 상속 받는다
    /// NavMeshAgent를 이용해 이동
    /// </summary>
    public class Robot : MonoBehaviour, IDamageable
    {
        #region Variables
        //참조
        private Animator animator;
        private Transform thePlayer;
        private Player player;
        private NavMeshAgent agent;

        //로봇의 상태 (enum)
        [SerializeField] private RobotState currentState;    //현재 상태
        private RobotState beforeState;     //현재 상태의 바로 이전 상태
        
        [SerializeField] private AIMode currentAIMode = AIMode.Patrol;

        //이동 및 순찰
        [Header("Patrol Settings")]
        [SerializeField] private float moveSpeed = 3.5f;
        public Vector3[] waypoints = new Vector3[] { 
            new Vector3(-8f, 0f, 17.5f), 
            new Vector3(-8f, 0f, 52.5f), 
            new Vector3(-27.5f, 0f, 52.5f) 
        };
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        private bool isWaiting = false;
        private const string isFiring = "IsFiring";

        //추격 및 공격
        [Header("Combat Settings")]
        [SerializeField] private float chaseRange = 15f;    // 추격 시작 거리
        [SerializeField] private float loseTargetRange = 20f; // 추격 포기 거리 (Enemy 구역 이탈 기준)
        [SerializeField] private float attakRange = 2f;     //공격 범위
        [SerializeField] private float attackDamage = 5f;   //공격력

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

        [Header("본 설정")]
        [SerializeField] private Transform spineBone;   // 척추 본 드래그 연결
        [SerializeField] private float aimSpeed = 5f;   // 조준 속도
        #endregion

        #region Unity Event Method
        private void Awake()
        {
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

            currentAIMode = AIMode.Patrol;
            ChangeState(RobotState.R_Walk); // 순찰 시작
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
            if (waypoints.Length > 0)
            {
                agent.SetDestination(waypoints[currentWaypointIndex]);
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

        private void LateUpdate()
        {
            if (isDeath) return;
            if (currentState != RobotState.R_Attack) return;
            if (thePlayer == null) return;
            if (spineBone == null) return;

            // 척추 본을 플레이어 방향으로 회전
            Vector3 direction = thePlayer.position - spineBone.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            spineBone.rotation = Quaternion.Slerp(spineBone.rotation, targetRotation, Time.deltaTime * aimSpeed);
        }
        #endregion

        #region AI Logic
        private void UpdateAI(float distanceToPlayer)
        {
            // 공격 중이라면 시선 고정
            if (currentState == RobotState.R_Attack)
            {
                if (distanceToPlayer > attakRange)
                {
                    // 거리가 벌어지면 다시 추격
                    ChangeState(RobotState.R_Walk);
                    currentAIMode = AIMode.Chase;
                }
                return;
            }

            // 순찰 모드에서 추격 모드로 전환
            if (currentAIMode == AIMode.Patrol)
            {
                if (distanceToPlayer <= chaseRange)
                {
                    currentAIMode = AIMode.Chase;
                    isWaiting = false; // 대기 중단
                    
                    // 배경음 BGM 변경 로직 (JumpScare Play)
                    if (normalBgm != null) normalBgm.Stop();
                    if (jumpScareBgm != null && !jumpScareBgm.isPlaying) jumpScareBgm.Play();

                    ChangeState(RobotState.R_Walk);
                }
            }
            // 추격 모드에서 순찰 모드로 전환 (구역 이탈)
            else if (currentAIMode == AIMode.Chase)
            {
                if (distanceToPlayer > loseTargetRange)
                {
                    currentAIMode = AIMode.Patrol;
                    
                    // 배경음 BGM 변경 로직 (Normal Play)
                    if (jumpScareBgm != null) jumpScareBgm.Stop();
                    if (normalBgm != null && !normalBgm.isPlaying) normalBgm.Play();
                    
                    // 순찰 웨이포인트로 목적지 재설정
                    if (waypoints.Length > 0)
                    {
                        agent.SetDestination(waypoints[currentWaypointIndex]);
                    }
                    ChangeState(RobotState.R_Walk);
                }
            }

            // 추격 중인데 공격 거리 안으로 들어오면
            if (currentAIMode == AIMode.Chase && distanceToPlayer <= attakRange)
            {
                ChangeState(RobotState.R_Attack);
            }
        }

        private void ExecuteStateLogic(float distanceToPlayer)
        {
            switch (currentState)
            {
                case RobotState.R_Idle:
                    if (currentAIMode == AIMode.Patrol && isWaiting)
                    {
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0f)
                        {
                            isWaiting = false;
                            
                            // 다음 웨이포인트 설정
                            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                            agent.SetDestination(waypoints[currentWaypointIndex]);
                            ChangeState(RobotState.R_Walk);
                        }
                    }
                    break;

                case RobotState.R_Walk:
                    if (currentAIMode == AIMode.Patrol)
                    {
                        agent.isStopped = false;
                        // 웨이포인트 도착 확인
                        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                        {
                            isWaiting = true;
                            waitTimer = Random.Range(2f, 3f);
                            agent.isStopped = true;
                            ChangeState(RobotState.R_Idle);
                        }
                    }
                    else if (currentAIMode == AIMode.Chase)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(thePlayer.position);
                    }
                    break;

                case RobotState.R_Attack:
                    agent.isStopped = true;
                    // 플레이어 쳐다보기
                    Vector3 lookPos = thePlayer.position;
                    lookPos.y = transform.position.y;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos - transform.position), Time.deltaTime * 5f);
                    break;
            }
        }
        #endregion

        #region Custom Method
        public void ChangeState(RobotState newState)
        {
            beforeState = currentState;
            currentState = newState;

            //애니메이터 파라미터 업데이트
            animator.SetInteger(enemyState, (int)currentState);

            // 공격 상태면 IsFiring true
            if (newState == RobotState.R_Attack)
                animator.SetBool(isFiring, true);
            else
                animator.SetBool(isFiring, false);
        }

        // 애니메이션 이벤트에서 호출할 수 있는 공격 함수
        public void Attack()
        {
            if (thePlayer == null || isDeath) return;

            float distance = Vector3.Distance(thePlayer.position, transform.position);
            if (distance <= attakRange + 0.5f) // 공격 모션 중 약간 벗어나도 맞게 보정
            {
                IDamageable damageable = thePlayer.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                }
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

            // NavMeshAgent 비활성화 (물리 충돌 정상화)
            if (agent != null) agent.enabled = false;

            // Rigidbody 추가해서 중력 적용
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;

            if (jumpScareBgm != null) jumpScareBgm.Stop();
            if (normalBgm != null && !normalBgm.isPlaying) normalBgm.Play();

            ChangeState(RobotState.R_Death);
        }
        #endregion
    }
}