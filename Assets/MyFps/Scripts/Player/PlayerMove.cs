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
        [Header ("이동 속도 조절")]       //헤더 특성 : 직렬화된 속성 중에 Player와 관련된 내용이다 표시
        [SerializeField] private float walkSpeed = 4f;      //걷는 속도
        [SerializeField] private float sprintSpeed = 7f;    //뛰는 속도
        private float moveSpeed;                            //이동속도

        //
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<CharacterInput>();
        }

        private void Update()
        {
            //이동
            Move();
        }
        #endregion

        #region Custom Method
        void Move()
        {
            moveSpeed = walkSpeed;

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

            //이동 : 방향 * Time.deltaTime * speed
            controller.Move(inputDirection.normalized * Time.deltaTime * moveSpeed);

        }
        #endregion
    }
}