using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 메인메뉴씬을 관리하는 클래스
    /// 메인메뉴(버튼 5개) 기능
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        #region Variables
        [Header("Scene Settings")]
        [SerializeField] 
        private string loadSceneName = "PlayScene01";

        [Header("UI Panels")]
        public GameObject mainMenuUI;
        public GameObject optionsUI;
        public CreditsMenu creditsMenu;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 메인 메뉴 진입 시 배경음악 재생
            AudioManager.Instance.PlayBGM("MenuBgm");
        }
        #endregion

        #region Custom Methods
        //01 새로운 게임 시작 (페이드 아웃 후 PlayScene01 로드)
        public void NewGame()
        {
            //01-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("New Game Button Clicked");
            AudioManager.Instance.Play("MenuButton");

            //01-2 씬 전환 수행
            if (SceneFader.instance != null)
            {
                SceneFader.instance.FadeTo(loadSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(loadSceneName);
            }
        }

        //02 저장된 게임 불러오기
        public void LoadGame()
        {
            //02-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("Load Game Button Clicked");
            AudioManager.Instance.Play("MenuButton");
        }

        //03 게임 설정 메뉴 열기 및 배경음 변경 테스트
        public void Options()
        {
            //03-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("Options Button Clicked");
            if (AudioManager.Instance != null) AudioManager.Instance.Play("MenuButton");

            //03-2 옵션 UI 표시
            if (mainMenuUI != null) mainMenuUI.SetActive(false);
            if (optionsUI != null) optionsUI.SetActive(true);
        }

        //04 크레딧 메뉴 열기
        public void Credits()
        {
            //04-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("Credits Button Clicked");
            if (AudioManager.Instance != null) AudioManager.Instance.Play("MenuButton");

            if (creditsMenu != null)
            {
                creditsMenu.OpenCredits();
            }
            else
            {
                Debug.LogWarning("CreditsMenu is not assigned in MainMenu.");
            }
        }

        //05 게임 종료
        public void QuitGame()
        {
            //05-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("Quit Game Button Clicked");
            AudioManager.Instance.Play("MenuButton");

            //05-2 게임 종료 처리
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}