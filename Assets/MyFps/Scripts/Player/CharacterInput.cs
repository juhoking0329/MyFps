using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MyFps
{
    /// <summary>
    /// 플레이어의 인풋을 관리하는 클래스 : 뉴인풋
    /// </summary>
    public class CharacterInput : MonoBehaviour
    {
        #region Variables
        //inputSystem class 인스턴스 선언
        private InputSystem_Actions inputActions;

        //이동 입력 값 - wasd
        private Vector2 move;

        //마우스 입력값 - 마우스 위치
        private Vector2 look;
        #endregion

        #region Property
        public Vector2 Move
        {
            get { return move; }
            private set { move = value; }
        }

        public Vector2 Look
        {
            get { return look; }
            private set { look = value; }
        }
        #endregion

        #region Unity Events Method
        private void Awake()
        {
            //참조
            //inputSystem class 인스턴스 생성
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            //inputSystem class 인스턴스 활성화
            inputActions.Enable();
        }

        private void OnDisable()
        {
            //inputSystem class 인스턴스 비활성화
            inputActions.Disable();
        }

        private void Update()
        {
            //wasd 입력값 처리 : 인스턴스이름.액션맵이름.액션이름.ReadValue
            move = inputActions.Player.Move.ReadValue<Vector2>();
            Look = inputActions.Player.Look.ReadValue<Vector2>();

        }
        #endregion

    }
}