using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MyFps
{
    /// <summary>
    /// 씬 페이더 기능 구현 클래스
    /// 씬 시작할때 페이드인 효과, 씬 종료시 페이드 아웃 효과 - 페이드 아웃하면 다음 씬으로 이동
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        #region Variables
        // 싱글톤 인스턴스 선언 추가
        public static SceneFader instance;

        [Header("Fader Components")]
        [SerializeField]
        private Image img;               //페이더 이미지
        [SerializeField]
        private AnimationCurve curve;    //페이드 효과를 위한 커브 값 적용

        [Header("Fader Settings")]
        [SerializeField] 
        private bool isFadeIn = false;     //시작시 페이드 효과를 자동으로 적용할지 여부
        [SerializeField] 
        private float delayTime = 0f;      //페이더 시작전 딜레이 타임
        [SerializeField] 
        private float fadeDuration = 3f;   // 페이드 지속 시간
        #endregion

        #region Unity Event Methods
        // 싱글톤 인스턴스 등록 추가
        private void Awake()
        {
            //00 싱글톤 인스턴스 설정
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            //00 시작하자 마자 페이드 인 효과 자동 실행 여부 체크
            if (isFadeIn)
            {
                FadeStart(delayTime);
            }
        }
        #endregion

        #region Custom Methods
        //01 외부에서 호출할 수 있는 페이드 시작 기능
        public void FadeStart(float delayTime = 0f)
        {
            StartCoroutine(FadeIn(delayTime));
        }

        //02 페이드 인 연출 수행 코루틴 (1초 동안 검은 화면에서 투명화)
        IEnumerator FadeIn(float delayTime)
        {
            //02-1 페이더 이미지를 완전한 검은색으로 초기화
            img.color = new Color(0f, 0f, 0f, 1f);

            //02-2 지정된 대기시간(딜레이)이 있다면 대기
            if (delayTime > 0f)
            {
                yield return new WaitForSeconds(delayTime);
            }

            float t = 1f;

            //02-3 시간을 거꾸로 줄여가며 페이드 인 효과 연출
            while(t > 0f)
            {
                t -= Time.deltaTime / fadeDuration;  // fadeDuration으로 나눠서 속도 조절
                float a = curve.Evaluate(t);    // 커브값에 따라 알파값 점차 감소
                img.color = new Color(0f, 0f, 0f, a);

                yield return 0;
            }
        }

        //03 지정한 씬 이름으로 페이드 아웃 후 전환
        public void FadeTo(string sceneName = "")
        {
            StartCoroutine(FadeOut(sceneName));
        }

        //04 지정한 빌드 인덱스로 페이드 아웃 후 전환
        public void FadeTo(int buildIndex)
        {
            StartCoroutine(FadeOut(buildIndex));
        }

        //05 씬 이름 전환을 위한 페이드 아웃 연출 코루틴 (1초 동안 서서히 어두워짐)
        IEnumerator FadeOut(string sceneName)
        {
            float t = 0f;
            //05-1 시간을 증가시키며 페이드 아웃 효과 연출
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                float a = curve.Evaluate(t);    // 커브값에 따라 알파값 점차 증가
                img.color = new Color(0f, 0f, 0f, a);
                yield return 0;
            }

            //05-2 페이드 아웃 완료 후 씬 전환
            if(sceneName != null && sceneName != "")
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        //06 빌드 인덱스 전환을 위한 페이드 아웃 연출 코루틴
        IEnumerator FadeOut(int buildIndex)
        {
            float t = 0f;
            //06-1 시간을 증가시키며 페이드 아웃 효과 연출
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                float a = curve.Evaluate(t);    // 커브값에 따라 알파값 점차 증가
                img.color = new Color(0f, 0f, 0f, a);
                yield return 0;
            }

            //06-2 페이드 아웃 완료 후 씬 전환
            if (buildIndex >= 0)
            {
                SceneManager.LoadScene(buildIndex);
            }
        }
        #endregion
    }
}