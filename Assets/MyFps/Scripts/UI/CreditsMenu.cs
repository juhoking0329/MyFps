using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace MyFps
{
    /// <summary>
    /// 크레딧 화면의 텍스트 스크롤 및 나가기(Esc) 기능을 관리하는 클래스
    /// </summary>
    public class CreditsMenu : MonoBehaviour
    {
        #region Variables
        [Header("UI Panels")]
        public GameObject mainMenuUI;
        public GameObject creditsUI;

        [Header("Credits Settings")]
        public RectTransform creditsTextRect;
        public float scrollSpeed = 50f; // 텍스트가 올라가는 속도 (절반 이하로 줄임)

        private float startYPosition = -500f; // 화면 아래 아주 깊숙한 곳에서 시작
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // 더 이상 Awake에서 위치를 가져오지 않고, 강제로 -1500 부터 시작하도록 함
        }

        private void Update()
        {
            // 크레딧 화면이 활성화된 상태일 때만 동작
            if (creditsUI != null && creditsUI.activeSelf)
            {
                // 텍스트 위로 스크롤
                if (creditsTextRect != null)
                {
                    creditsTextRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
                }

                // 아무 키나 누르면 메인 메뉴로 복귀 (Esc 포함)
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CloseCredits();
                }
            }
        }
        #endregion

        #region Custom Methods
        // 크레딧 화면 열기 (위치 초기화 포함)
        public void OpenCredits()
        {
            if (creditsTextRect != null)
            {
                // 화면 아래로 위치 리셋
                Vector2 pos = creditsTextRect.anchoredPosition;
                pos.y = startYPosition;
                creditsTextRect.anchoredPosition = pos;
            }

            if (mainMenuUI != null) mainMenuUI.SetActive(false);
            if (creditsUI != null) creditsUI.SetActive(true);
        }

        // 크레딧 화면 닫기
        public void CloseCredits()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("MenuButton");
            }

            if (creditsUI != null) creditsUI.SetActive(false);
            if (mainMenuUI != null) mainMenuUI.SetActive(true);
        }
        #endregion
    }
}
