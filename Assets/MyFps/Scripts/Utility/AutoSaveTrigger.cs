using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyFps
{
    /// <summary>
    /// 씬이 시작될 때 자동으로 게임을 저장하는 스크립트
    /// </summary>
    public class AutoSaveTrigger : MonoBehaviour
    {
        private void Start()
        {
            // 약간의 딜레이를 주어 PlayerStats 등이 완전히 초기화된 후 저장하도록 할 수도 있지만,
            // Start 시점에 바로 저장해도 무방함.
            Invoke(nameof(PerformSave), 0.5f);
        }

        private void PerformSave()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int currentAmmo = 0;

            if (PlayerStats.Instance != null)
            {
                currentAmmo = PlayerStats.Instance.AmmoCount;
            }

            // 요구사항 1번과 2번을 모두 수행 (하지만 로드 시에는 2번을 사용)
            SaveLoadManager.SaveSceneWithPlayerPrefs(currentSceneIndex);
            SaveLoadManager.SaveGameWithFile(currentSceneIndex, currentAmmo);
        }
    }
}
