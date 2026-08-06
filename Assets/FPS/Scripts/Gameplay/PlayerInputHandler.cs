using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 플레이어의 모든 입력을 관리하는 클래스
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        #region Variables
        private InputSystem_Actions inputActions;

        [Header("Look Settings")]
        [Tooltip("Sensitivity multiplier for moving the camera around")]
        [SerializeField] private float lookSensitivity = 1f;

        /*[Tooltip("Additional sensitivity multiplier for WebGL")]
        [SerializeField] private float webglLookSensitivityMultiplier = 0.25f;*/

        /*[Tooltip("Limit to consider an input when using a trigger on a controller")]
        [SerializeField] private float triggerAxisThreshold = 0.4f;*/

        [Tooltip("Used to flip the vertical input axis")]
        [SerializeField] private bool invertYAxis = false;

        [Tooltip("Used to flip the horizontal input axis")]
        [SerializeField] private bool invertXAxis = false;

        //외부에서 읽기 전용으로 접근 가능하도록 프로퍼티 제공
        public float LookSensitivity => lookSensitivity;
        public bool InvertYAxis => invertYAxis;
        public bool InvertXAxis => invertXAxis;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Start()
        {
            //마우스 커서 잠금 처리
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion

        #region Custom Method
        //입력을 처리할 수 있는 상태인지 체크 (커서가 잠겨있을 때만 입력 허용)
        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked;
        }

        //이동 입력값 가져오기
        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                Vector2 moveValue = inputActions.Player.Move.ReadValue<Vector2>();
                Vector3 move = new Vector3(moveValue.x, 0f, moveValue.y);

                //대각선 이동시 최대 속도를 넘지 않도록 제한
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        //마우스 좌우 회전 입력값 가져오기
        public float GetLookInputsHorizontal()
        {
            return GetMouseLookAxis(GameConstants.k_MouseAxisNameHorizontal);
        }

        //마우스 상하 회전 입력값 가져오기
        public float GetLookInputsVertical()
        {
            return GetMouseLookAxis(GameConstants.k_MouseAxisNameVertical);
        }

        //점프 버튼 눌렀는지 체크 (한번만 입력)
        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
            {
                bool actionPressed = false;
                if (inputActions != null)
                    actionPressed = inputActions.Player.Jump.WasPressedThisFrame();
                    
                return actionPressed || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
            }

            return false;
        }

        //점프 버튼 누르고 있는지 체크 (홀드)
        public bool GetJumpInputHeld()
        {
            if (CanProcessInput())
            {
                bool actionHeld = false;
                if (inputActions != null)
                    actionHeld = inputActions.Player.Jump.IsPressed();
                    
                return actionHeld || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);
            }

            return false;
        }

        //마우스 축 입력값 계산 (감도, 반전 적용)
        private float GetMouseLookAxis(string mouseInputName)
        {
            if (CanProcessInput())
            {
                Vector2 lookValue = inputActions.Player.Look.ReadValue<Vector2>();
                float i = 0f;
                if (mouseInputName == GameConstants.k_MouseAxisNameHorizontal)
                    i = lookValue.x;
                else if (mouseInputName == GameConstants.k_MouseAxisNameVertical)
                    i = lookValue.y;

                //상하 반전 처리
                if (invertYAxis && mouseInputName == GameConstants.k_MouseAxisNameVertical)
                    i *= -1f;

                //감도 적용
                i *= lookSensitivity;

                //스틱 입력값과 동일한 수준으로 감소
                i *= 0.01f;

                return i;
            }

            return 0f;
        }

        //앉기 버튼 눌렀는지 체크
        public bool GetCrouchInputDown()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Crouch.WasPressedThisFrame();
            }

            return false;
        }

        //앉기 버튼 뗐는지 체크
        public bool GetCrouchInputReleased()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Crouch.WasReleasedThisFrame();
            }

            return false;
        }

        //달리기 버튼 누르고 있는지 체크
        public bool GetSprintInputHeld()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Sprint.IsPressed();
            }

            return false;
        }

        //무기 교체 입력값 가져오기 (E, Q 키 또는 마우스 스크롤 휠)
        //반환값: 1 = 다음 무기, -1 = 이전 무기, 0 = 입력 없음
        public int GetSwitchWeaponInput()
        {
            if (CanProcessInput())
            {
                //마우스 스크롤 휠 입력 체크
                float mouseWheel = inputActions.Player.WeaponSwitch.ReadValue<Vector2>().y;
                
                if (mouseWheel > 0f)
                    return 1;
                if (mouseWheel < 0f)
                    return -1;

                //키보드 입력 체크 (1, 2키 제거 요청에 의해 Q, E만 하드코딩으로 허용)
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.eKey.wasPressedThisFrame)
                        return 1;
                    if (Keyboard.current.qKey.wasPressedThisFrame)
                        return -1;
                }
            }

            return 0;
        }

        //조준 모드
        public bool GetAimInputHeld()
        {
            if(CanProcessInput())
            {
                return inputActions.Player.Aim.IsPressed();
            }
            return false;
        }

        //조준 시작
        public bool GetAimInputDown()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Aim.WasPressedThisFrame();
            }
            return false;
        }

        //조준 끝
        public bool GetAimInputReleased()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Aim.WasReleasedThisFrame();
            }
            return false;
        }

        //발사 버튼 입력처리
        public bool GetFireInputDown()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Fire.WasPressedThisFrame();
            }
            return false;
        }

        //발사 버튼 놓았는지
        public bool GetFireInputReleased()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Fire.WasReleasedThisFrame();
            }
            return false;
        }

        //발사 버튼 누르고 있는지
        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                return inputActions.Player.Fire.IsPressed();
            }
            return false;
        }

        //재장전(Reload) 버튼 눌렀는지
        public bool GetReloadInputDown()
        {
            if (CanProcessInput())
            {
                // 'R' 키보드 입력 체크
                return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            }
            return false;
        }

        #endregion
    }
}