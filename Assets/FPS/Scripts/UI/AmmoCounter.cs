using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 개별 무기의 총알 잔탄량, 인덱스 번호, 100% 충전 상태 등을 UI에 표시하는 스크립트입니다.
    /// </summary>
    public class AmmoCounter : MonoBehaviour
    {
        #region Variables
        [Header("UI References")]
        [Tooltip("게이지 바 뒤에 깔리는 배경 이미지")]
        public Image BackgroundImage;
        [Tooltip("실제 총알 잔여량을 보여주는 원형 게이지 (전경 이미지)")]
        public Image FillImage;
        [Tooltip("이 무기가 몇 번째 슬롯인지 나타내는 텍스트 (예: 1, 2, 3)")]
        public TextMeshProUGUI WeaponIndexText;
        [Tooltip("현재 남은 총알 수를 표시하는 텍스트")]
        public TextMeshProUGUI AmmoCountText;
        
        [Header("Color Change Component")]
        [Tooltip("게이지 바의 색상을 관리하는 외부 스크립트 참조")]
        public FillBarColorChange fillBarColorChange;

        [Header("Animation Settings")]
        [Tooltip("UI 크기 및 투명도 전환이 얼마나 부드럽게 일어날지 결정하는 Lerp 계수")]
        public float lerpSpeed = 10f;

        // 부드러운 애니메이션을 위해 목표로 하는 크기와 투명도를 저장하는 변수
        private float targetScale = 0.8f;
        private float targetAlpha = 0.5f;

        // 이 UI가 추적하고 있는 무기 스크립트 참조
        private WeaponController weapon;
        // 투명도(알파) 조절을 위한 컴포넌트
        private CanvasGroup canvasGroup;
        // 부여받은 슬롯 번호
        private int weaponIndex;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // 게임 시작 시 CanvasGroup 컴포넌트를 찾거나 없으면 강제로 붙여줍니다.
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // FillBarColorChange 컴포넌트 자동 할당
            if (fillBarColorChange == null)
                fillBarColorChange = GetComponent<FillBarColorChange>();
        }

        private void Update()
        {
            if (weapon == null) return;

            // 총알(Ammo) 게이지 비율을 계산합니다 (현재 탄창 수 / 최대 탄창 수)
            float targetFillRatio = 0f;
            if (weapon.MaxAmmo > 0)
            {
                // 차지 중일 때는 충전된 비율만큼 탄환 게이지를 미리 깎아서 보여줍니다.
                float effectiveAmmo = weapon.CurrentAmmo - weapon.CurrentChargeRatio;
                targetFillRatio = Mathf.Clamp01(effectiveAmmo / weapon.MaxAmmo);
            }

            // 구한 목표 비율로 UI 이미지의 fillAmount(게이지 채워짐 정도)를 부드럽게 감소시킵니다.
            if (FillImage != null) 
            {
                FillImage.fillAmount = Mathf.Lerp(FillImage.fillAmount, targetFillRatio, Time.deltaTime * lerpSpeed);
            }

            // 텍스트에 현재 남은 탄환 수를 갱신합니다.
            if (AmmoCountText != null)
            {
                // 실시간으로 변하는 소수점 아래는 무시하고 정수 형태로 보여주도록 "F0" 또는 그냥 캐스팅 활용
                AmmoCountText.text = Mathf.CeilToInt(weapon.CurrentAmmo).ToString();
            }

            // 컬러 변경 로직은 FillBarColorChange에게 위임합니다.
            if (fillBarColorChange != null)
            {
                // 색상 판단용 수치 계산
                float colorRatio = 0.5f; // 기본(정상) 상태

                if (weapon.CurrentAmmo <= 0)
                {
                    colorRatio = 0f; // 탄환 없음 (배경 빨간색 점멸 트리거)
                }
                else if (weapon.CurrentAmmo >= weapon.MaxAmmo)
                {
                    colorRatio = 1f; // 탄창 가득 참 (전경 점멸 트리거)
                }

                // 외부 스크립트에 색상 업데이트 요청
                fillBarColorChange.UpdateVisual(colorRatio);
            }

            // UI 부드러운 전환 (Lerp) 연출 적용
            if (canvasGroup != null)
            {
                // 현재 알파값에서 목표 알파값으로 자연스럽게 이동합니다.
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * lerpSpeed);
                // 현재 크기에서 목표 크기로 자연스럽게 이동합니다.
                float currentScale = Mathf.Lerp(transform.localScale.x, targetScale, Time.deltaTime * lerpSpeed);
                transform.localScale = Vector3.one * currentScale;
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 무기를 획득했을 때 초기 설정을 위해 호출됩니다.
        /// </summary>
        public void Initialize(WeaponController newWeapon, int index)
        {
            weapon = newWeapon;
            weaponIndex = index;
            // 유저가 보기 편하도록 내부 인덱스(0,1,2..)에 1을 더해 (1,2,3..)로 표기합니다.
            if (WeaponIndexText != null) WeaponIndexText.text = (index + 1).ToString();

            // FillBarColorChange가 있다면 초기화
            if (fillBarColorChange != null)
            {
                // 100% 수치는 1, 0% 수치는 0으로 초기화
                fillBarColorChange.Initialize(1f, 0f);
            }
        }

        /// <summary>
        /// 들고 있는 무기가 교체되었을 때 활성화 연출을 담당합니다.
        /// </summary>
        public void SetActiveWeapon(bool isActive)
        {
            // 현재 유저가 손에 쥐고 있는 무기(Active)라면 목표값을 크고 선명하게 설정합니다.
            if (isActive)
            {
                targetScale = 1.1f;
                targetAlpha = 1f;
            }
            // 인벤토리에 넣어둔 무기(Inactive)라면 목표값을 작고 흐릿하게 설정합니다.
            else
            {
                targetScale = 0.8f;
                targetAlpha = 0.5f;
            }
        }
        #endregion
    }
}
