using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// CCTV를 마우스 위치에 따라 회전시키는 클래스
    /// </summary>
    public class CCTVController : MonoBehaviour
    {
        #region Variables
        [Header("CCTV Settings")]
        [SerializeField]
        private Transform cctvHead;
        [SerializeField]
        private float targetDistance = 2f;

        private InputSystem_Actions _inputActions;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //00 초기 설정 진행 (현재는 없음)
        }

        private void OnEnable()
        {
            //00 입력 액션 활성화
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
            }
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            //00 입력 액션 비활성화
            if (_inputActions != null)
            {
                _inputActions.Disable();
            }
        }

        void Update()
        {
            //00 마우스 방향으로 CCTV 회전 처리
            TrackMousePointer();
        }
        #endregion

        #region Custom Methods
        //01 마우스 포인터 위치를 계산하여 CCTV 헤드가 바라보도록 회전하는 함수
        private void TrackMousePointer()
        {
            if (cctvHead == null || Camera.main == null) return;

            // 지연 초기화 검사 (스크립트 재컴파일 대비)
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
                _inputActions.Enable();
            }

            // 뉴 인풋 시스템에서 마우스 화면 좌표 읽기
            Vector2 mouseScreenPos = _inputActions.UI.Point.ReadValue<Vector2>();

            // ScreenPointToRay를 사용하여 카메라로부터 마우스 위치로 향하는 Ray 생성
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

            Vector3 worldTarget;
            // Physics.Raycast로 물리 공간 내 충돌 지점 찾기
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                worldTarget = hit.point;
            }
            else
            {
                // 충돌 지점이 없는 경우 ScreenToWorldPoint를 통해 기본 targetDistance 깊이의 좌표 획득
                Vector3 screenPosWithDepth = new Vector3(mouseScreenPos.x, mouseScreenPos.y, targetDistance);
                worldTarget = Camera.main.ScreenToWorldPoint(screenPosWithDepth);
            }

            // CCTV 헤드가 해당 월드 좌표를 조준하도록 회전
            cctvHead.LookAt(worldTarget);
        }
        #endregion
    }
}
