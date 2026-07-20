using UnityEngine;
using UnityEngine.UI;

namespace MyFps
{
    /// <summary>
    /// 옵션 메뉴 UI 및 기능 관리를 담당하는 클래스
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        #region Variables
        [Header("UI Panels")]
        public GameObject mainMenuUI;
        public GameObject optionsUI;

        [Header("Volume Sliders")]
        public Slider bgmSlider;
        public Slider sfxSlider;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            // 저장된 볼륨 값 불러와 슬라이더에 세팅 (기본값 1)
            if (bgmSlider != null)
            {
                bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
                bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            }
        }
        #endregion

        #region Custom Methods
        public void SetBgmVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBgmVolume(volume);
            }
        }

        public void SetSfxVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSfxVolume(volume);
            }
        }

        // 옵션창 닫기 (메인 메뉴로 복귀)
        public void CloseOptions()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("MenuButton");
            }

            if (optionsUI != null)
            {
                optionsUI.SetActive(false);
            }

            if (mainMenuUI != null)
            {
                mainMenuUI.SetActive(true);
            }
        }
        #endregion
    }
}
