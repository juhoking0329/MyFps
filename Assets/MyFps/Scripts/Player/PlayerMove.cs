using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 플레이어의 이동을 관리하는 클래스
    /// </summary>
    public class PlayerMove : MonoBehaviour
    {
        #region Variables
        //참조
        private CharacterController controller;
        private CharacterInput input;

        //이동
        [Header("이동 속도 조절")]
        [SerializeField] private float walkSpeed = 4f;      //걷는 속도
        [SerializeField] private float sprintSpeed = 7f;    //뛰는 속도
        private float moveSpeed;                            //이동속도

        //그라운드 체크
        [Header("그라운드 체크")]
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private float groundedOffset = 0.14f;      //체크 지점 조정값
        [SerializeField] private float groundedRadius = 0.5f;       //체크 범위 영역
        public LayerMask groundLayers;                              //그라운드 레이어 체크

        //점프
        [Header("점프")]
        [SerializeField] private float gravity = -9.81f;            //중력
        [SerializeField] private float verticalVelocity = 0f;       //y축의 속도 값
        [SerializeField] private float jumpHeight = 1.2f;           //점프 높이
        [SerializeField] private float jumpTimeout = 0.1f;          //점프 키입력 처리 시간
        private float jumpTimeoutDelta = 0f;                        //점프 타임아웃 타이머
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            controller = GetComponent<CharacterController>();
            input = GetComponent<CharacterInput>();
        }

        private void Update()
        {
            //그라운드 체크
            CheckGround();

            //점프 + 중력
            GravityAndJump();

            //이동
            Move();
        }
        #endregion

        #region Custom Method
        void CheckGround()
        {
            //체크 위치 설정 (발 아래)
            Vector3 checkPosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            //체크 지점에서 그라운드 레이어가 있는지 체크
            isGrounded = Physics.CheckSphere(checkPosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        void GravityAndJump()
        {
            if (isGrounded)
            {
                //땅에 있을 때 verticalVelocity가 무한히 떨어지는 것 방지
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;

                //점프 타임아웃 타이머 감소
                if (jumpTimeoutDelta >= 0f)
                    jumpTimeoutDelta -= Time.deltaTime;

                //스페이스 입력 && 타임아웃이 끝났을 때 점프
                if (input.IsJump && jumpTimeoutDelta <= 0f)
                {
                    // 점프 공식 : v = sqrt(높이 * -2 * 중력)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    //점프 후 타임아웃 리셋 (연속 점프 방지)
                    jumpTimeoutDelta = jumpTimeout;

                    //점프 후 입력값 리셋
                    input.IsJump = false;
                }
            }
            else
            {
                //공중에 있을 때 점프 입력 무시
                jumpTimeoutDelta = jumpTimeout;

                //공중에서도 입력값 리셋
                input.IsJump = false;
            }

            //중력 적용 : 매 프레임 중력 누적
            verticalVelocity += gravity * Time.deltaTime;
        }

        void Move()
        {
            moveSpeed = input.IsSprint ? sprintSpeed : walkSpeed;

            //이동 인풋 체크
            if (input.Move == Vector2.zero)
                moveSpeed = 0f;

            //인풋에서 방향값 얻어오기
            Vector3 inputDirection = Vector3.zero;

            //플레이어의 로컬 방향 구하기
            if (input.Move != Vector2.zero)
            {
                inputDirection = transform.right * input.Move.x + transform.forward * input.Move.y;
            }

            //이동 벡터에 수직 속도(점프/중력) 합산
            Vector3 moveVector = inputDirection.normalized * moveSpeed + Vector3.up * verticalVelocity;

            //이동 : 방향 * Time.deltaTime
            controller.Move(moveVector * Time.deltaTime);
        }
        #endregion
    }
}