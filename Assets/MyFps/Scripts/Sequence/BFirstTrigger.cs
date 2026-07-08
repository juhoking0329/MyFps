using UnityEngine;
using System.Collections;
using TMPro;

namespace MyFps
{
    /// <summary>
    /// 첫번째 복도끝 시퀀스 트리거 구현
    /// 시퀀스 내용 : 무기발견 연출 구현
    /// </summary>
    public class BFirstTrigger : SequenceTrigger
    {
        #region Variables
        //연출        
        public TextMeshProUGUI sequenceText;
        public GameObject arrow;
        public AudioSource line03;

        //페이드
        [SerializeField] private float textFadeOutDuration = 1f;    //텍스트 페이드아웃 시간
        #endregion

        #region Custom Method
        protected override IEnumerator SequencePlay(GameObject player)
        {
            //플레이어 비활성화
            player.SetActive(false);

            //대사 출력 및 음성 재생
            sequenceText.gameObject.SetActive(true);
            sequenceText.text = "Looks like a weapon on that table.";
            line03.Play();

            //음성 길이만큼 대기
            yield return new WaitForSeconds(line03.clip.length);

            //화살표 활성화
            arrow.SetActive(true);
            yield return new WaitForSeconds(1f);

            //텍스트 페이드아웃
            yield return StartCoroutine(FadeOutText());

            //플레이어 활성화
            player.SetActive(true);
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