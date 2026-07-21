using UnityEngine;
using TMPro;

namespace MyFps
{
    /// <summary>
    /// 플레이어 스탯(AmmouCount)를 UI Text 보여주기
    /// </summary>
    public class DrawAmmoCount : MonoBehaviour
    {
        public TextMeshProUGUI ammoCountText;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Update()
        {
            if (PlayerStats.Instance != null && ammoCountText != null)
            {
                ammoCountText.text = PlayerStats.Instance.AmmoCount.ToString();
            }

            // 무기 획득 여부에 따라 UI 투명도 조절
            if (canvasGroup != null && PlayerStats.Instance != null)
            {
                bool shouldShow = PlayerStats.Instance.HasPistol || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "PlayScene02";
                canvasGroup.alpha = shouldShow ? 1f : 0f;
            }
        }
    }
}