using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace MyFps
{
    /// <summary>
    /// 게임 시작 시 인트로 오프닝 시퀀스 처리 클래스
    /// 인트로 연출, ...
    /// </summary>
    public class IntroOpenning : MonoBehaviour
    {
        #region Variables
        //참조
        public SceneFader fader;
        [SerializeField] private string loadToScene = "PlayScene01"; // 로드할 씬 이름

        // 스킵 액션 참조
        public InputActionReference skipActionReference; 
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            // 씬 시작 시 페이드 인
            fader.FadeStart();
        }

        private void Update()
        {
            // 스킵 액션이 활성화되어 있고, 스킵 버튼이 눌렸을 때
            if (skipActionReference.action.WasPressedThisFrame())
            {
                Exit(); // 씬 전환
            }
        }
        #endregion

        #region Custom Methods
        //오프닝 시퀀스

        //
        public void Exit()
        {
            fader.FadeTo(loadToScene);
            
            // 페이드 아웃 중 카메라가 돌아가는 현상을 막기 위해 시네머신 브레인 비활성화 (현재 방향 고정)
            var brain = FindObjectOfType<Camera>()?.GetComponent("CinemachineBrain") as MonoBehaviour;
            if (brain != null)
            {
                brain.enabled = false;
            }
        }
        #endregion 
    }
}