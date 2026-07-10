using UnityEngine;
using TMPro;
using System.Collections;

namespace MyFps
{
    public enum UnlockRequirement
    {
        None,
        HeartKey1,
        HeartKey2,
        TwoEyes
    }

    /// <summary>
    /// 문 스위치 구현, 인터렉트 구현
    /// 문 스위치는 토글, 문이 열려 있으면 빨간색, 문이 닫혀 있으면 원래색
    /// </summary>
    public class DoorSwitch : Interactive
    {
        #region Variables
        [Header("Door Target")]
        [SerializeField]
        private UnlockRequirement unlockRequirement = UnlockRequirement.HeartKey1; // 기본 열쇠1 요구

        [SerializeField]
        private Door targetDoor;            // 제어할 문 컴포넌트

        [Header("Audio")]
        [SerializeField]
        private AudioSource lockSound;     // 문 잠김 사운드 (스위치 클릭 시 재생용)

        private Renderer emitterRenderer;   // 스위치 불빛 렌더러
        private Color originalColor;        // 원래 색상 저장용

        private Coroutine subtitleCoroutine; // 자막 노출용 코루틴 레퍼런스
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 스위치 불빛 오브젝트(LightEmitter)의 렌더러 찾기 및 초기 색상 저장
            Transform emitter = transform.Find("LightEmitter");
            if (emitter != null)
            {
                emitterRenderer = emitter.GetComponent<Renderer>();
                if (emitterRenderer != null && emitterRenderer.material != null)
                {
                    if (emitterRenderer.material.HasProperty("_BaseColor"))
                    {
                        originalColor = emitterRenderer.material.GetColor("_BaseColor");
                    }
                    else if (emitterRenderer.material.HasProperty("_Color"))
                    {
                        originalColor = emitterRenderer.material.GetColor("_Color");
                    }
                }
            }

            //00 초기 문 상태에 맞춰 상호작용 텍스트 설정
            if (targetDoor != null)
            {
                action = targetDoor.IsOpen ? "Close Door" : "Open Door";
            }
        }
        #endregion

        #region Custom Methods
        //01 플레이어 인터랙션 액션 처리
        protected override void DoAction()
        {
            if (targetDoor == null) return;

            //01-1 만약 문이 작동(애니메이션 재생) 중이면 상호작용 입력 무시
            if (targetDoor.IsOperating) return;

            //01-2 열쇠 보유 상태 검사
            if (targetDoor.IsLocked && !IsRequirementMet())
            {
                // 열쇠 없음 -> 실패 처리
                PlayLockedAction();
            }
            else
            {
                // 열쇠 있음 -> 잠금 해제 및 문 토글 처리
                if (targetDoor.IsLocked)
                {
                    targetDoor.IsLocked = false;
                }
                PlaySuccessAction();
            }
        }

        // 조건 검사 헬퍼 함수
        private bool IsRequirementMet()
        {
            switch (unlockRequirement)
            {
                case UnlockRequirement.HeartKey1: return PlayerStats.Instance.HasHeartKey1;
                case UnlockRequirement.HeartKey2: return PlayerStats.Instance.HasHeartKey2;
                case UnlockRequirement.TwoEyes: return PlayerStats.Instance.IsPuzzleSolved;
                default: return true;
            }
        }

        //02 문 열기 실패(잠겨있음) 액션 처리
        private void PlayLockedAction()
        {
            // 잠김 오디오 재생
            if (lockSound != null)
            {
                lockSound.Play();
            }

            // 자막 노출 처리
            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
            }

            string message = "You need Key";
            if (unlockRequirement == UnlockRequirement.TwoEyes)
            {
                message = "You must solve the puzzle";
            }

            subtitleCoroutine = StartCoroutine(ShowSubtitle(message, 2f));
        }

        //03 문 열기 성공 및 토글 액션 처리
        private void PlaySuccessAction()
        {
            targetDoor.ToggleDoor();

            // 동일한 targetDoor를 가리키는 모든 스위치 동기화
            DoorSwitch[] switches = FindObjectsOfType<DoorSwitch>();
            foreach (var s in switches)
            {
                if (s.targetDoor == this.targetDoor)
                {
                    s.SyncSwitchState(targetDoor.IsOpen);
                }
            }
        }

        // 외부에서 스위치 상태 동기화를 위해 호출되는 메서드
        public void SyncSwitchState(bool isOpen)
        {
            SetSwitchColor(isOpen);
            action = isOpen ? "Close Door" : "Open Door";
            if (actionText != null && actionUI != null && actionUI.activeSelf)
            {
                actionText.text = action;
            }
        }

        //04 스위치 램프 색상 제어 함수
        private void SetSwitchColor(bool isOpen)
        {
            if (emitterRenderer == null || emitterRenderer.material == null) return;

            Color targetColor = isOpen ? Color.red : originalColor;

            if (emitterRenderer.material.HasProperty("_BaseColor"))
            {
                emitterRenderer.material.SetColor("_BaseColor", targetColor);
            }
            else if (emitterRenderer.material.HasProperty("_Color"))
            {
                emitterRenderer.material.SetColor("_Color", targetColor);
            }

            if (emitterRenderer.material.HasProperty("_EmissionColor"))
            {
                emitterRenderer.material.SetColor("_EmissionColor", isOpen ? Color.red : Color.black);
            }
        }

        //05 화면 하단 자막 출력을 위한 코루틴 함수
        IEnumerator ShowSubtitle(string message, float duration)
        {
            GameObject seqUI = GameObject.Find("SequenceText");
            if (seqUI == null)
            {
                // Fallback search in parents
                GameObject canvas = GameObject.Find("Canvas_Sequence");
                if (canvas != null)
                {
                    Transform t = canvas.transform.Find("SequenceUI/SequenceText");
                    if (t != null) seqUI = t.gameObject;
                }
            }

            if (seqUI != null)
            {
                TextMeshProUGUI tmpText = seqUI.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    seqUI.SetActive(true);
                    tmpText.text = message;

                    yield return new WaitForSeconds(duration);

                    tmpText.text = "";
                    seqUI.SetActive(false);
                }
            }
        }
        #endregion
    }
}
