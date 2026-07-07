using System.Collections;
using TMPro;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 게임 오프닝 연출 관리 클래스
    /// 페이드인 → 시나리오 텍스트 → 플레이어 활성화 순서로 진행
    /// </summary>
    public class OpeningDirector : MonoBehaviour
    {
        #region Variables
        //참조
        [Header("참조")]
        [SerializeField] private GameObject player;                 //플레이어 오브젝트
        [SerializeField] private TextMeshProUGUI scenarioText;      //시나리오 텍스트
        [SerializeField] private MouseLook mouseLook;

        //시나리오
        [Header("시나리오")]
        [SerializeField] private string scenarioMessage = "I need to get out of here";
        [SerializeField] private float scenarioDuration = 3f;       //텍스트 표시 시간

        //페이드
        [Header("페이드")]
        [SerializeField] private float fadeInWaitTime = 1f;         //페이드인 대기 시간
        [SerializeField] private float textFadeOutDuration = 1f;    //텍스트 페이드아웃 지속 시간
        #endregion

        #region Unity Event Method
        private IEnumerator Start()
        {
            //SceneFader 초기화 대기
            yield return null;

            //0. 플레이어 및 마우스 회전 비활성화
            player.SetActive(false);
            mouseLook.enabled = false;

            //시나리오 텍스트 미리 표시 (알파 0으로 투명하게)
            scenarioText.gameObject.SetActive(true);
            SetTextAlpha(1f);

            //오프닝 연출 시작
            StartCoroutine(OpeningRoutine());
        }
        #endregion

        #region Custom Method
        IEnumerator OpeningRoutine()
        {
            //1. 시나리오 텍스트 바로 출력
            scenarioText.text = scenarioMessage;

            //2. 페이드인 연출 (검정화면 → 밝아짐, 텍스트는 이미 보임)
            SceneFader.instance.FadeStart(fadeInWaitTime);

            //3. 페이드인 + 텍스트 표시 시간 대기
            yield return new WaitForSeconds(fadeInWaitTime + scenarioDuration);

            //4. 텍스트 페이드아웃
            yield return StartCoroutine(FadeOutText());

            //5. 플레이어 및 카메라 활성화
            player.SetActive(true);
            mouseLook.enabled = true;
        }

        IEnumerator FadeOutText()
        {
            //텍스트 알파값 1 -> 0으로 서서히 숨김
            float timer = 0f;
            while (timer < textFadeOutDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / textFadeOutDuration);
                SetTextAlpha(alpha);
                yield return null;
            }

            //완전히 숨김
            scenarioText.gameObject.SetActive(false);
        }

        void SetTextAlpha(float alpha)
        {
            //텍스트 알파값 설정
            Color color = scenarioText.color;
            color.a = alpha;
            scenarioText.color = color;
        }
        #endregion
    }
}