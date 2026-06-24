using TMPro;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 플레이어와 인터렉티브 기능 구현
    /// 가까이 가서 마우스 가져가면 액션 UI 보여준다
    /// 액션 : 문을 연다
    /// </summary>
    public class DoorCellOpen : MonoBehaviour
    {
        #region Variables
        //참조
        private CharacterInput input;
        private Camera mainCamera;

        //레이캐스트
        [Header("레이캐스트")]
        [SerializeField] private float rayDistance = 2f;        //레이 최대 거리
        [SerializeField] private LayerMask doorLayer;           //문 레이어 체크

        //UI
        [Header("UI")]
        [SerializeField] private GameObject actionUI;           //[ E ] + Open The Door UI 오브젝트
        [SerializeField] private TextMeshProUGUI actionKeyText;     //[ E ] 텍스트
        [SerializeField] private TextMeshProUGUI actionDescText;    //Open The Door 텍스트

        //문
        private GameObject currentDoor;                         //현재 감지된 문 오브젝트
        private Animator doorAnimator;                          //문 애니메이터
        private AudioSource doorAudio;                          //문 사운드
        private bool isDoorOpen = false;                        //문 열림 여부
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            input = GetComponent<CharacterInput>();
            mainCamera = Camera.main;
        }

        private void Start()
        {
            //UI 초기화 - 숨김
            SetActionUI(false);
        }

        private void Update()
        {
            //레이캐스트로 문 감지
            CheckDoor();

            //E키 입력 처리
            if (input.IsAction && currentDoor != null && !isDoorOpen)
            {
                OpenDoor();
            }
        }

        private void OnDrawGizmosSelected()
        {
            //레이캐스트 시각화 (씬 뷰에서 확인용)
            Gizmos.color = Color.red;
            if (mainCamera != null)
                Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * rayDistance);
        }
        #endregion

        #region Custom Method
        void CheckDoor()
        {
            RaycastHit hit;

            //카메라 앞 방향으로 레이캐스트 발사
            bool isHit = Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out hit,
                rayDistance,
                doorLayer
            );

            if (isHit)
            {
                //문 감지됨 - UI 표시
                currentDoor = hit.collider.gameObject;
                doorAnimator = currentDoor.GetComponentInParent<Animator>();
                doorAudio = currentDoor.GetComponentInParent<AudioSource>();
                SetActionUI(true);
            }
            else
            {
                //문 감지 안됨 - UI 숨김
                currentDoor = null;
                SetActionUI(false);
            }
        }

        void OpenDoor()
        {
            isDoorOpen = true;

            //UI 숨김
            SetActionUI(false);

            //문 애니메이션 실행
            if (doorAnimator != null)
                doorAnimator.SetTrigger("Open");

            //문 사운드 출력
            if (doorAudio != null)
                doorAudio.Play();

            //Door Trigger 콜라이더 제거 (문 통과 가능하게)
            Collider doorCollider = currentDoor.GetComponent<Collider>();
            if (doorCollider != null)
                doorCollider.enabled = false;
        }

        void SetActionUI(bool isVisible)
        {
            //UI 오브젝트 전체 활성/비활성
            if (actionUI != null)
                actionUI.SetActive(isVisible);
        }
        #endregion
    }
}