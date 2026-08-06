using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 크로스 헤어를 관리하는 클래스
    /// </summary>
    public class CrossHairManager : MonoBehaviour
    {
        #region Variables
        public Image CrossHairImage;            //UI 이미지
        public Sprite nullCrosshairSprite;      //데이터 없을때 보여지는 스프라이트

        //참조
        private PlayerWeaponManager weaponManager;

        //무기 교체 연출
        [SerializeField] private float crosshairUpdateSharpness = 5f;   //Lerp 계수

        private RectTransform crosshairRectTransform;                   //UI RectTransform

        private CrossHairData crosshairDefault;                         //평소에 보여지는 크로스헤어
        private CrossHairData crosshairTarget;                          //타겟팅 되었을때 보여지는 크로스헤어

        private CrossHairData crosshairCurrent;                         //현재 화면에 보여지는 크로스헤어

        private bool wasPointingAtEnemy;                                //적 포착 순간 또는 적 잃어버리는 순간을 계산하기 위한 변수
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            weaponManager = GameObject.FindFirstObjectByType<PlayerWeaponManager>();
            crosshairRectTransform = CrossHairImage.GetComponent<RectTransform>();
        }

        private void Start()
        {
            //현재 액티브 무기로 크로스헤어 교체
            OnWeaponChanged(weaponManager.GetActiveWeapon());

            //무기 교체 이벤트 함수에 등록
            weaponManager.OnSwitchToWeapon += OnWeaponChanged;
        }

        private void Update()
        {
            // 스코프 켜졌을 때는 크로스헤어 비활성화
            if (weaponManager.IsScopeOn)
            {
                CrossHairImage.enabled = false;
                wasPointingAtEnemy = weaponManager.IsPointingAtEnemy;
                return;
            }
            else
            {
                // 스코프 꺼지고 현재 무기가 있으면 다시 활성화
                if (weaponManager.GetActiveWeapon() != null)
                {
                    CrossHairImage.enabled = true;
                }
            }

            //크로스헤어 보여주기
            UpdateCrosshairPointAtEnemy(false);

            //was변수 저장
            wasPointingAtEnemy = weaponManager.IsPointingAtEnemy;
        }
        #endregion

        #region Custom Method
        //크로스헤어 보여주기 : force가 true이면 강제로 보여주기
        private void UpdateCrosshairPointAtEnemy(bool force)
        {
            //크로스헤어 데이터 체크
            if (crosshairDefault.CrossHairSprite == null)
                return;

            //적을 포착하는 순간 
            if ((force == true || wasPointingAtEnemy == false ) 
                && weaponManager.IsPointingAtEnemy == true)
            {
                crosshairCurrent = crosshairTarget;
                CrossHairImage.sprite = crosshairCurrent.CrossHairSprite;
            }
            //적을 놓치는 순간
            else if ((force == true || wasPointingAtEnemy == true ) 
                && weaponManager.IsPointingAtEnemy == false)
            {
                crosshairCurrent = crosshairDefault;
                CrossHairImage.sprite = crosshairCurrent.CrossHairSprite;
            }

            //Lerp 변경
            CrossHairImage.color = Color.Lerp(CrossHairImage.color,
                crosshairCurrent.CrossHairColor, crosshairUpdateSharpness * Time.deltaTime);
            crosshairRectTransform.sizeDelta = Mathf.Lerp(crosshairRectTransform.sizeDelta.x, 
                crosshairCurrent.CrossHairSize, crosshairUpdateSharpness * Time.deltaTime
                ) * Vector2.one;

        }

        //무기 교체시 호출 되는 함수
        private void OnWeaponChanged(WeaponController newWeapon)
        {
            //newWeapon null 체크
            if(newWeapon == null)
            {
                // [수정] 이전 무기의 크로스헤어 데이터를 초기화하여 아래 UpdateCrosshairPointAtEnemy에서 덮어쓰지 않도록 방지
                crosshairDefault = default;
                crosshairTarget = default;
                crosshairCurrent = default;

                if (nullCrosshairSprite)
                {
                    CrossHairImage.sprite = nullCrosshairSprite;
                }
                else
                {
                    CrossHairImage.enabled = false;
                }
            }
            else  //새로운 무기가 들어오면
            {
                CrossHairImage.enabled = true;
                crosshairDefault = newWeapon.crossHairDefault;
                crosshairTarget = newWeapon.crossHairTargetInSight;
            }

            //강제로 크로스 헤어 보여주기
            UpdateCrosshairPointAtEnemy(true);
        }
        #endregion

    }
}