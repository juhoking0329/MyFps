using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 캐릭터의 머리위에 있는 HealthBar UI 관리
    /// 1. currentHealth값에 따른 게이지 관리
    /// 2. 월드 캔버스 UI - 게이지바가 항상 플레이어(카메라)를 바라본다
    /// 3. currentHealth값이 maxHealth이면 게이비자 UI 비활성화, maxHealth보다 작으면 활성화
    /// </summary>
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [Tooltip("적의 체력을 관리하는 Health 스크립트")]
        [SerializeField] private Health health;
        
        [Tooltip("체력 게이지를 표시할 Image UI (Fill 타입)")]
        [SerializeField] private Image healthBarImage;
        
        [Tooltip("전체 헬스바 UI를 껐다 켰다 할 부모 Canvas 오브젝트")]
        [SerializeField] private GameObject healthBarCanvas;

        [Header("Settings")]
        [Tooltip("체력 게이지바가 부드럽게 줄어드는 속도")]
        [SerializeField] private float lerpSpeed = 10f;

        // 메인 카메라 참조 (빌보딩 용도)
        private Transform mainCameraTransform;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            // 메인 카메라 찾기
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }

            // 시작할 때는 체력이 풀이므로 UI 숨기기
            if (healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(false);
            }
        }

        private void Update()
        {
            if (health == null || healthBarImage == null) return;

            // 1 & 3: currentHealth값이 maxHealth이면 게이지 UI 비활성화, 작으면 활성화
            if (health.CurrentHealth >= health.maxHealth)
            {
                if (healthBarCanvas != null && healthBarCanvas.activeSelf)
                    healthBarCanvas.SetActive(false);
            }
            else
            {
                if (healthBarCanvas != null && !healthBarCanvas.activeSelf)
                    healthBarCanvas.SetActive(true);

                // 체력 게이지(fillAmount) 부드럽게 갱신
                float targetFillAmount = health.HealthRatio;
                healthBarImage.fillAmount = Mathf.Lerp(healthBarImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
            }
        }

        private void LateUpdate()
        {
            // 2: 게이지바가 항상 플레이어(카메라)를 바라본다 (빌보드 처리)
            if (healthBarCanvas != null && healthBarCanvas.activeSelf && mainCameraTransform != null)
            {
                // UI가 항상 카메라 방향을 바라보게 함 (LookRotation 사용)
                // 캔버스가 뒤집혀 보일 수 있으므로 카메라의 forward를 바라보게 처리
                healthBarCanvas.transform.rotation = Quaternion.LookRotation(healthBarCanvas.transform.position - mainCameraTransform.position);
            }
        }
        #endregion
    }
}