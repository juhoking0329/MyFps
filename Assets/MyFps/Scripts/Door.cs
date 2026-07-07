using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 문 열기/닫기
    /// 문 열릴때 등록된 함수 호출하는 이벤트 구현
    /// 문 닫힐때 등록된 함수 호출하는 이벤트 구현
    /// </summary>
    public class Door : MonoBehaviour
    {
        #region Variables
        [Header("Door Settings")]
        [SerializeField]
        private bool isLocked = true;
        [SerializeField]
        private bool isOpen = false;

        [Header("Audio")]
        [SerializeField]
        private AudioSource doorAudioSource; // 문 작동 사운드

        private Animator animator;
        private bool isOperating = false;    // 현재 문 작동(애니메이션) 중인지 체크
        #endregion

        #region Properties
        public bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }

        public bool IsOpen => isOpen;
        public bool IsOperating => isOperating; // 외부에서 작동 여부를 읽을 수 있는 프로퍼티
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //00 애니메이터 컴포넌트 자동 찾기
            animator = GetComponent<Animator>();
        }
        #endregion

        #region Custom Methods
        //01 문을 여는 함수
        public void OpenDoor()
        {
            if (isOpen || isOperating) return;

            isOpen = true;
            StartCoroutine(OperationDelay());

            if (animator != null)
            {
                animator.SetBool("IsOpen", true);
            }

            if (doorAudioSource != null)
            {
                doorAudioSource.Play();
            }
        }

        //02 문을 닫는 함수
        public void CloseDoor()
        {
            if (!isOpen || isOperating) return;

            isOpen = false;
            StartCoroutine(OperationDelay());

            if (animator != null)
            {
                animator.SetBool("IsOpen", false);
            }

            if (doorAudioSource != null)
            {
                doorAudioSource.Play();
            }
        }

        //03 문을 토글(열기/닫기)하는 함수
        public void ToggleDoor()
        {
            if (isOperating) return;

            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        //04 문 애니메이션이 재생 완료될 때까지 작동을 잠그는 지연 코루틴
        private System.Collections.IEnumerator OperationDelay()
        {
            isOperating = true;

            // 애니메이터 전이(Transition) 완료 대기를 위해 0.1초 지연
            yield return new WaitForSeconds(0.1f);

            if (animator != null)
            {
                // 현재 진행 중인 상태 정보 추출하여 해당 재생 길이(length) 만큼 대기
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                float waitTime = Mathf.Max(0f, stateInfo.length - 0.1f);
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                // 애니메이터가 없는 경우 기본 대기시간 설정
                yield return new WaitForSeconds(1.0f);
            }

            isOperating = false;
        }
        #endregion
    }
}
