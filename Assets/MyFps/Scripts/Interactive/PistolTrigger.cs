using TMPro;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 권총 획득 트리거 클래스
    /// 레이캐스트로 권총 감지, UI 표시/숨김, E키로 권총 획득
    /// </summary>
    public class PistolTrigger : MonoBehaviour
    {
        #region Variables
        //참조
        [Header("참조")]
        [SerializeField] private CharacterInput input;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private MouseLook mouseLook;
        private Camera mainCamera;

        //레이캐스트
        [Header("레이캐스트")]
        [SerializeField] private float rayDistance = 2f;        //레이 최대 거리
        [SerializeField] private LayerMask pistolLayer;         //권총 레이어 체크

        //UI
        [Header("UI")]
        [SerializeField] private GameObject actionUI;           //[ E ] + Pick Up Pistol UI
        [SerializeField] private TextMeshProUGUI actionKeyText;     //[ E ] 텍스트
        [SerializeField] private TextMeshProUGUI actionDescText;    //Pick Up Pistol 텍스트

        //크로스헤어
        [Header("크로스헤어")]
        [SerializeField] private GameObject crosshairBorder;    //크로스헤어 테두리

        //권총
        [Header("권총")]
        [SerializeField] private GameObject fakePistol;         //테이블 위의 권총
        [SerializeField] private GameObject realPistol;         //손 위의 권총
        [SerializeField] private GameObject gun;                //플레이어 팔
        [SerializeField] private GameObject arrow;              //가이드 화살표

        //획득 여부
        private bool isPicked = false;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            mainCamera = Camera.main;
        }

        private void Start()
        {
            //초기화
            SetActionUI(false);
            SetCrosshairBorder(false);
            realPistol.SetActive(false);
        }

        private void Update()
        {
            //null 체크
            if (mainCamera == null)
                mainCamera = Camera.main;

            //이미 획득했으면 실행 안 함
            if (isPicked) return;

            //레이캐스트로 권총 감지
            CheckPistol();

            //E키 입력 처리
            if (input.IsAction)
                PickUpPistol();
        }

        private void OnDrawGizmosSelected()
        {
            //레이캐스트 시각화
            Gizmos.color = Color.yellow;
            if (mainCamera != null)
                Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * rayDistance);
        }
        #endregion

        #region Custom Method
        void CheckPistol()
        {
            RaycastHit hit;

            //카메라 앞 방향으로 레이캐스트 발사
            bool isHit = Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out hit,
                rayDistance,
                pistolLayer
            );

            //Debug.Log("레이캐스트 발사중 / 감지: " + isHit);  // 추가

            if (isHit)
            {
                //Debug.Log("권총 감지됨: " + hit.collider.gameObject.name);  // 추가

                //권총 감지됨 - UI, 크로스헤어 테두리 표시
                SetActionUI(true);
                SetCrosshairBorder(true);
            }
            else
            {
                //권총 감지 안됨 - UI, 크로스헤어 테두리 숨김
                SetActionUI(false);
                SetCrosshairBorder(false);
            }
        }

        void PickUpPistol()
        {
            RaycastHit hit;

            //레이캐스트가 권총에 맞을 때만 획득
            bool isHit = Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out hit,
                rayDistance,
                pistolLayer
            );

            if (!isHit) return;

            isPicked = true;

            //UI, 크로스헤어 숨김
            SetActionUI(false);
            SetCrosshairBorder(false);

            //테이블 위 권총 비활성화
            fakePistol.SetActive(false);

            //팔 역할 Gun 비활성화
            gun.SetActive(false);

            //진짜 권총 활성화
            realPistol.SetActive(true); 

            // UI 텍스트 업데이트용 플래그 켜기
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.HasPistol = true;
            }
            
            // 만약 씬 파일에 비활성화 저장되어 있다면 켜주기 (부모인 AmmoUI도 확인)
            DrawAmmoCount ammoCountUI = Object.FindFirstObjectByType<DrawAmmoCount>(FindObjectsInactive.Include);
            if (ammoCountUI != null)
            {
                if (ammoCountUI.transform.parent != null && ammoCountUI.transform.parent.name == "AmmoUI")
                {
                    ammoCountUI.transform.parent.gameObject.SetActive(true);
                }
                ammoCountUI.gameObject.SetActive(true);
            }

            //화살표 비활성화
            if (arrow != null)
                arrow.SetActive(false);
        }

        void SetActionUI(bool isVisible)
        {
            if (actionUI != null)
                actionUI.SetActive(isVisible);
        }

        void SetCrosshairBorder(bool isVisible)
        {
            if (crosshairBorder != null)
                crosshairBorder.SetActive(isVisible);
        }
        #endregion
    }
}