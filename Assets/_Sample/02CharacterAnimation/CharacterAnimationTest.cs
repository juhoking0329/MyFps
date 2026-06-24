using UnityEngine;
using UnityEngine.InputSystem;

namespace MySample
{
    /// <summary>
    /// 캐릭터 애니메이션을 제어하는 예제 클래스
    /// 뉴 인풋 시스템
    /// 기본이 대기 상태
    /// W키가 들어오면 걷기 상태
    /// + Shift 키를 누르면 뛰는 상태 
    /// </summary>
    public class CharacterAnimationTest : MonoBehaviour
    {
        #region Variables
        // 00 컴포넌트 참조 및 캐싱용 변수들
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerInput playerInput;
        // 00 인게임 조작 및 속도 조절 관련 설정값 변수들
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float rotationSpeed = 10.0f;
        // 00 블렌드 트리 애니메이션 보간 관련 설정값 변수들
        [Header("Animation Settings")]
        [SerializeField] private float speedDampTime = 0.1f; // 걷기/뛰기 전환 시 부드러운 전환을 보정하는 속도 소요 시간
                                                             // 00 입력 제어를 위한 내부 리드 전용 변수들
        private InputAction moveAction;
        private InputAction sprintAction;
        private Vector2 inputVector;
        private bool isSprintPressed;

        // 00 블렌드 트리 파라미터 제어용 보간 연산 변수들
        private float currentSpeedParam = 0.0f;
        #endregion

        #region Unity Events
        // 00 게임이 실행될 때 PlayerInput에서 키 입력 액션을 초기화하는 함수
        private void Awake()
        {
            InitializeInputs();
        }
        // 00 매 프레임마다 키 입력을 감지하고 이동 및 애니메이션 처리를 수행하는 함수
        private void Update()
        {
            HandleMovementInput();
        }
        #endregion
        #region Custom Methods
        // 00 PlayerInput 컴포넌트로부터 Move와 Sprint 액션을 가져와 바인딩하는 함수
        private void InitializeInputs()
        {
            // PlayerInput 컴포넌트 자동 참조
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            // Input Actions 바인딩
            if (playerInput != null)
            {
                moveAction = playerInput.actions["Move"];
                sprintAction = playerInput.actions["Sprint"];
            }
            else
            {
                Debug.LogError("PlayerInput 컴포넌트를 캐릭터에서 찾을 수 없습니다!");
            }
        }
        // 00 키 입력을 매 프레임 확인하고 이동 여부를 판별하여 처리하는 메인 핸들러 함수
        private void HandleMovementInput()
        {
            if (moveAction == null || sprintAction == null) return;

            // WASD 및 Shift 입력값 캐싱
            inputVector = moveAction.ReadValue<Vector2>();
            isSprintPressed = sprintAction.IsPressed();

            // 현재 캐릭터가 입력 중(이동 중)인지 판별
            bool isMoving = inputVector.sqrMagnitude > 0.01f;
            bool isRunning = isMoving && isSprintPressed;

            // 애니메이터 및 이동 기능 처리
            UpdateAnimator(isMoving, isRunning);
            MoveCharacter(isMoving, isRunning);
        }
        // 00 캐릭터의 이동 속도(Speed) 수치를 계산하여 Blend Tree 파라미터를 부드럽게 갱신하는 함수
        private void UpdateAnimator(bool isMoving, bool isRunning)
        {
            if (animator == null) return;

            // 목표 파라미터 값 결정 (Idle = 0, Walk = 1, Run = 2)
            float targetSpeedParam = 0.0f;
            if (isMoving)
            {
                targetSpeedParam = isRunning ? 2.0f : 1.0f;
            }

            // 현재 파라미터 값을 목표값으로 부드럽게 보간 (Time.deltaTime / speedDampTime 초 단위 계산)
            currentSpeedParam = Mathf.MoveTowards(currentSpeedParam, targetSpeedParam, Time.deltaTime / speedDampTime);

            // 애니메이터의 Float 파라미터 "Speed"에 보간된 속도 전달
            animator.SetFloat("Speed", currentSpeedParam);
        }
        // 00 2D 입력 벡터를 3D 절대 방향으로 전환하여 실제 물리적 위치와 회전을 업데이트하는 함수
        private void MoveCharacter(bool isMoving, bool isRunning)
        {
            if (!isMoving) return;
            // 이동 벡터 및 이동 속도 결정
            Vector3 movement = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
            float speed = isRunning ? runSpeed : walkSpeed;

            // 월드 좌표 공간 기준 위치 변경
            transform.Translate(movement * speed * Time.deltaTime, Space.World);

            // 진행 방향을 향해 캐릭터 점진적 회전 처리
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        #endregion
    }
}