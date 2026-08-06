using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// HUD에 있는 플레이어 HealthBar UI 관리하는 클래스
    /// HealthBar 게이지 관리
    /// 플레이어의 health는 Find Object로 가져와서 참조
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [Tooltip("플레이어의 Health 스크립트 참조")]
        private Health playerHealth;
        
        [Tooltip("체력을 표시할 UI Image (Fill)")]
        [SerializeField] private Image healthFillImage;
        
        [Tooltip("피격 시 화면을 붉게 깜빡일 UI Image")]
        [SerializeField] private Image flashImage;

        [Header("Settings")]
        [Tooltip("체력 게이지가 부드럽게 깎이는 속도")]
        [SerializeField] private float healthLerpSpeed = 10f;
        
        [Tooltip("화면 깜빡임 최대 투명도 (0~1)")]
        [SerializeField] private float maxFlashAlpha = 0.5f;
        
        [Tooltip("화면 깜빡임 속도")]
        [SerializeField] private float flashSpeed = 2f;
        
        [Tooltip("화면 깜빡임 색상")]
        [SerializeField] private Color flashColor = Color.red;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //참조
            PlayerCharacterController playerCharacterController 
                = GameObject.FindAnyObjectByType<PlayerCharacterController>();
            playerHealth = playerCharacterController.GetComponent<Health>();
        }

        private void Update()
        {
            if (playerHealth == null) return;

            UpdateHealthUI();
            UpdateFlashFeedback();
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 체력바 게이지를 부드럽게(Lerp) 갱신합니다.
        /// </summary>
        private void UpdateHealthUI()
        {
            if (healthFillImage == null) return;
            
            float targetFillAmount = playerHealth.HealthRatio;
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFillAmount, Time.deltaTime * healthLerpSpeed);
        }

        /// <summary>
        /// 체력이 위험 수치일 때 화면을 붉게 깜빡입니다.
        /// </summary>
        private void UpdateFlashFeedback()
        {
            if (flashImage == null) return;

            // CanvasGroup이 붙어있다면 강제로 1로 설정하여 Image.color의 알파값이 무시되지 않도록 함
            CanvasGroup cg = flashImage.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha < 1f)
            {
                cg.alpha = 1f;
            }

            Color currentColor = flashImage.color;

            if (playerHealth.IsCritical)
            {
                // 체력이 낮을 때 핑퐁(PingPong) 효과로 알파 값을 부드럽게 오르내림
                float alpha = Mathf.PingPong(Time.time * flashSpeed, maxFlashAlpha);
                currentColor = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            }
            else
            {
                // 안전할 때는 투명도를 0으로 부드럽게 내림
                float alpha = Mathf.Lerp(currentColor.a, 0f, Time.deltaTime * flashSpeed);
                currentColor = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            }

            flashImage.color = currentColor;
        }
        #endregion
    }
}