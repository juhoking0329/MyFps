using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

namespace MySample
{
    /// <summary>
    /// NavMeshAgent를 이용한 에이전트 컨트롤러
    /// 마우스로 맵을 클릭하면 클릭한 지점으로 Agent가 이동하도록 구현
    /// </summary>
    public class AgentController : MonoBehaviour
    {
        #region Variables
        //참조
        private NavMeshAgent m_agent;

        //입력 처리
        public InputActionReference ClickAcion;
        #endregion

        #region Unity Event Methods
        void Awake()
        {
            //참조
            m_agent = GetComponent<NavMeshAgent>();
        }

        void OnEnable()
        {
            if (ClickAcion != null && ClickAcion.action != null)
                ClickAcion.action.Enable();
        }

        void OnDisable()
        {
            if (ClickAcion != null && ClickAcion.action != null)
                ClickAcion.action.Disable();
        }

        void Update()
        {
            Vector2? screenPos = null;

            // 1. 터치스크린 입력 처리
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // 2. 기존 마우스 또는 Input Action 유지
            else if (ClickAcion != null && ClickAcion.action != null && ClickAcion.action.WasPressedThisFrame())
            {
                if (Pointer.current != null)
                {
                    screenPos = Pointer.current.position.ReadValue();
                }
            }

            if (screenPos.HasValue)
            {
                Vector3 worldPos = ScreenToRay(screenPos.Value);
                if (worldPos != Vector3.zero)
                {
                    m_agent.SetDestination(worldPos);
                }
            }
        }
        #endregion

        #region Custom Methods
        private Vector3 ScreenToRay(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
            return Vector3.zero;
        }

        #endregion
    }
}