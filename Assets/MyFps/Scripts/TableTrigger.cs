using System.Collections;
using TMPro;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 테이블 트리거 작동 클래스
    /// 플레이어가 트리거에 들어오면 연출 시작
    /// </summary>
    public class TableTrigger : MonoBehaviour
    {
        #region Variables
        //참조
        [Header("참조")]
        [SerializeField] private PlayerMove playerMove;         // 추가
        [SerializeField] private MouseLook mouseLook;           // 추가
        [SerializeField] private GameObject player;             //플레이어 오브젝트
        [SerializeField] private GameObject arrow;              //화살표 오브젝트
        [SerializeField] private TextMeshProUGUI dialogueText;  //대사 텍스트

        //트리거
        [Header("트리거")]
        [SerializeField] private string dialogueMessage = "Looks like a weapon on that table.";
        [SerializeField] private float dialogueDelay = 1f;      //대사 후 딜레이
        [SerializeField] private float arrowDelay = 1f;         //화살표 후 딜레이
        [SerializeField] private float textFadeOutDuration = 1f;//대사 사라지는 시간

        //트리거 작동 여부 (한번만 작동)
        private bool isTriggered = false;
        #endregion

        #region Unity Event Method
        private void Start()
        {
            //화살표 비활성화
            arrow.SetActive(false);

            //대사 텍스트 숨김
            dialogueText.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("트리거 감지: " + other.gameObject.name); // 추가

            //플레이어가 트리거에 들어왔을 때 && 한번만 작동
            if (other.CompareTag("Player") && !isTriggered)
            {
                isTriggered = true;
                StartCoroutine(TriggerRoutine());
            }
        }
        #endregion

        #region Custom Method
        IEnumerator TriggerRoutine()
        {
            //1. 플레이어 이동/카메라 비활성화
            playerMove.enabled = false;
            mouseLook.enabled = false;

            //2. 대사 출력
            dialogueText.text = dialogueMessage;
            dialogueText.gameObject.SetActive(true);
            SetTextAlpha(1f);

            //3. 딜레이
            yield return new WaitForSeconds(dialogueDelay);

            //4. 화살표 활성화
            arrow.SetActive(true);

            //5. 딜레이
            yield return new WaitForSeconds(arrowDelay);

            //6. 대사 페이드아웃
            yield return StartCoroutine(FadeOutText());

            //7. 플레이어 이동/카메라 다시 활성화
            playerMove.enabled = true;
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
            dialogueText.gameObject.SetActive(false);
        }

        void SetTextAlpha(float alpha)
        {
            //텍스트 알파값 설정
            Color color = dialogueText.color;
            color.a = alpha;
            dialogueText.color = color;
        }
        #endregion
    }
}