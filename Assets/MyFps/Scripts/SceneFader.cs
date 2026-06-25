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
        // ★ [요청 수정분] UIManager가 원격으로 이 스크립트를 찾을 수 있도록 싱글톤 인스턴스 선언
        public static SceneFader instance;

        public Image img;               //페이더 이미지
        public AnimationCurve curve;    //페이드 효과를 위한 커브 값 적용
        public bool isFadeIn = false;    //시작시 페이드 효과를 자동으로 적용할지 여부
        #endregion

        #region Unity Methods
        // ★ [요청 수정분] 씬이 켜질 때 자기 자신을 instance 방에 등록합니다.
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                // 간혹 씬 전환 시 중복 생성되는 페이더가 있다면 파괴하여 오작동을 막습니다.
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            //시작하자 마자 페이드 인 효과
            if (isFadeIn)
            {
                FadeStart();
            }
        }
        #endregion

        #region Custom Methods
        //페이드 시작
        public void FadeStart(float delayTime = 0f)
        {
            StartCoroutine(FadeIn(delayTime));
        }

        //페이드 인 효과 : 1초동안 a: 1 -> 0
        IEnumerator FadeIn(float delayTime)
        {
            img.color = new Color(0f, 0f, 0f, 1f);

            //딜레이 시간만큼 대기
            if (delayTime > 0f)
            {
                yield return new WaitForSeconds(delayTime);
            }

            float t = 1f;

            while (t > 0f)
            {
                t -= Time.deltaTime;
                float a = curve.Evaluate(t);    //커브를 이용해서 알파값을 계산
                img.color = new Color(0f, 0f, 0f, a);

                yield return 0;
            }
        }

        //페이드 아웃 하고 씬 이름으로 다음 씬으로 이동
        public void FadeTo(string sceneName = "")
        {
            StartCoroutine(FadeOut(sceneName));
        }

        //페이드 아웃 하고 씬 빌드번호로 다음 씬으로 이동
        public void FadeTo(int buildIndex)
        {
            StartCoroutine(FadeOut(buildIndex));
        }

        //페이드 아웃 효과 : 1초동안 a: 0 -> 1
        IEnumerator FadeOut(string sceneName)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);    //커브를 이용해서 알파값을 계산
                img.color = new Color(0f, 0f, 0f, a);
                yield return 0;
            }

            //페이드 아웃이 끝나면 다음 씬으로 이동
            if (sceneName != null && sceneName != "")
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        IEnumerator FadeOut(int buildIndex)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);    //커브를 이용해서 알파값을 계산
                img.color = new Color(0f, 0f, 0f, a);
                yield return 0;
            }

            //페이드 아웃이 끝나면 다음 씬으로 이동
            if (buildIndex >= 0)
            {
                SceneManager.LoadScene(buildIndex);
            }
        }
        #endregion
    }
}