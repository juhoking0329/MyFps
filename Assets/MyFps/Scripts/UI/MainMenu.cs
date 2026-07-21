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

        [Header("Buttons")]
        public UnityEngine.UI.Button loadGameButton;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 메인 메뉴 진입 시 커서 잠금 해제 및 표시 (어느 씬에서 오든 보장)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //00-1 메인 메뉴 진입 시 배경음악 재생
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM("MenuBgm");
            }

            //00-2 저장된 데이터 유무에 따른 Load Game 버튼 활성화/비활성화
            if (loadGameButton != null)
            {
                if (SaveLoadManager.HasFileSave())
                {
                    loadGameButton.interactable = true;
                }
                else
                {
                    loadGameButton.interactable = false;
                }
            }
        }
        #endregion

        #region Custom Methods
        //01 새로운 게임 시작 (페이드 아웃 후 PlayScene01 로드)
        public void NewGame()
        {
            //01-1 버튼 클릭 사운드 재생 및 로그 출력
            Debug.Log("New Game Button Clicked");
            AudioManager.Instance.Play("MenuButton");

            // 스탯 초기화 추가
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ResetStats();

            //01-1a 이전 플레이 기록 초기화 플래그 설정
            SaveLoadManager.isNewGame = true;
            SaveLoadManager.pendingLoadData = null;

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
            if (AudioManager.Instance != null) AudioManager.Instance.Play("MenuButton");

            //02-2 저장된 데이터 불러오기
            GameData data = SaveLoadManager.LoadGameWithFile();
            if (data != null)
            {
                // 로드할 데이터 저장 (씬이 로드된 후 PlayerStats에서 소비)
                SaveLoadManager.pendingLoadData = data;
                SaveLoadManager.isNewGame = false;

                // 씬 전환
                if (SceneFader.instance != null)
                {
                    // SceneFader는 씬 이름(string)을 받으므로, buildIndex를 이름으로 변환해야 하지만
                    // FadeTo가 int를 받도록 오버로딩되어 있는지 확인이 필요.
                    // 만약 string만 받는다면 SceneManager를 직접 사용
                    UnityEngine.SceneManagement.SceneManager.LoadScene(data.savedSceneIndex);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(data.savedSceneIndex);
                }
            }
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