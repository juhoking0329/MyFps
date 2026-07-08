using UnityEngine;
using TMPro;
using System.Collections;

namespace MyFps
{
    /// <summary>
    /// 첫번째 플레이씬 오프닝 연출
    /// </summary>
    public class AOpenning : MonoBehaviour
    {
        #region Variables
        //참조
        public GameObject thePlayer;
        public SceneFader fader;
        public TextMeshProUGUI sequenceText;

        public AudioSource line01;
        public AudioSource line02;

        //페이드
        [Header("페이드")]
        [SerializeField] private float textFadeOutDuration = 1f;    //텍스트 페이드아웃 시간
        #endregion

        #region Unity Event Method
        private void Start()
        {
            //게임 시작과 동시에 연출 시작
            StartCoroutine(SequencePlay());
        }
        #endregion

        #region Custom Method
        IEnumerator SequencePlay()
        {
            //0.플레이 캐릭터 비 활성화
            //1.페이드인 연출(3초 대기후 페인드인 효과)
            //2.화면 하단에 시나리오 텍스트 화면 출력 및 음성 재생(2초)
            //(...Where am I?)
            //3.화면 하단에 시나리오 텍스트 화면 출력 및 음성 재생(2초)
            //(I need get out of here)
            //4. 2초후에 시나리오 텍스트 없어진다
            //5.플레이 캐릭터 활성화

            //00 마우스 커서 숨김 및 락 처리
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //0. 플레이어 비활성화
            thePlayer.SetActive(false);

            //1. 검은 화면 유지 (페이드 없이 바로 대기)
            //yield return new WaitForSeconds(1f);

            //2. 첫번째 대사 출력 및 음성 재생 (검은 화면에서)
            sequenceText.gameObject.SetActive(true);
            sequenceText.text = "...Where am I?";
            line01.Play();
            yield return new WaitForSeconds(line01.clip.length);

            //3. 페이드인 연출 (밝아지면서)
            fader.FadeStart(0f);

            //4. 두번째 대사 출력 및 음성 재생
            sequenceText.text = "I need to get out of here.";
            line02.Play();
            yield return new WaitForSeconds(line02.clip.length);

            //5. 텍스트 페이드아웃
            yield return StartCoroutine(FadeOutText());

            //6. 플레이어 활성화
            thePlayer.SetActive(true);
        }
        IEnumerator FadeOutText()
        {
            //텍스트 알파값 1 -> 0으로 서서히 숨김
            float timer = 0f;
            Color color = sequenceText.color;

            while (timer < textFadeOutDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, timer / textFadeOutDuration);
                sequenceText.color = color;
                yield return null;
            }

            //완전히 숨김
            sequenceText.gameObject.SetActive(false);
            color.a = 1f;
            sequenceText.color = color;
        }
        #endregion
    }
}