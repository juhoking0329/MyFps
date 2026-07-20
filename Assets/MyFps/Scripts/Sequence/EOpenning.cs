using UnityEngine;
using System.Collections;

namespace MyFps
{
    /// <summary>
    /// 두번째 플레이씬 오프닝 연출
    /// </summary>
    public class EOpenning : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField]
        private GameObject thePlayer;
        [SerializeField]
        private SceneFader fader;

        [Header("Opening Settings")]
        [SerializeField]
        private float delayTime = 0f;
        [SerializeField]
        private string bgmName = "BGM01";
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 게임 시작 시 오프닝 시퀀스 실행
            StartCoroutine(SequencePlay());
        }
        #endregion

        #region Custom Methods
        //01 오프닝 연출 진행 코루틴 (마우스 커서 고정, 페이드인, BGM 재생)
        IEnumerator SequencePlay()
        {
            //01-1 마우스 커서 숨김 및 락 처리
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //01-2 플레이어 일시 비활성화 (페이드 중 움직임 방지)
            if (thePlayer != null)
            {
                thePlayer.SetActive(false);
            }

            //01-3 배경음 재생 시작
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(bgmName);
            }

            //01-4 페이드인 연출 시작
            if (fader != null)
            {
                fader.FadeStart(delayTime);
            }

            //01-5 페이드 연출이 진행되는 동안 대기 (1초)
            yield return new WaitForSeconds(1f);

            //01-6 플레이어 활성화
            if (thePlayer != null)
            {
                thePlayer.SetActive(true);
            }

            // 확실하게 한 번 더 잠금 처리
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion
    }
}
