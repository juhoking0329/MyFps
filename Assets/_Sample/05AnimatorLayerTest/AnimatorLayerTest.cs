using UnityEngine;
using UnityEngine.InputSystem;

namespace MySample
{
    /// <summary>
    /// 애니메이터 레이어 테스트 예제
    /// </summary>
    public class AnimatorLayerTest : MonoBehaviour
    {
        #region Variables
        //참조
        private Animator animator;

        //[SerializeField] private string isMoving = "IsMove";

        //인풋 액션
        public InputActionReference moveActions;
        public InputActionReference AimActions;
        #endregion

        #region Property
        public bool IsMove
        {
            get { return IsMove; }
            private set 
            {
                
            }
        }
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            //인풋 처리
            Vector2 inputMove = moveActions.action.ReadValue<Vector2>();
            IsMove = inputMove != Vector2.zero;

            if(IsMove)
            {
                animator.SetLayerWeight(1, 1f);
            }
            else
            {
                animator.SetLayerWeight(1, 0f);
            }
        }
        #endregion

        #region
        #endregion
    }
}