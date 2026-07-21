using System.Collections;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// PlayScene02 전용 Exit Trigger 시퀀스
    /// 임시로 MainMenu 씬을 로드합니다.
    /// </summary>
    public class FExitTrigger : SequenceTrigger
    {
        #region Variables
        // 참조
        public SceneFader fader;
        [SerializeField] private string loadToScene = "MainMenu";

        public Animator twoDoorAnimator;
        public AudioSource jumpScare;

        #endregion

        #region Unity Event Method
        private void Start()
        {
            // 커서 초기화 로직은 MainMenu로 위임함
        }

        #endregion

        #region Custom Method
        protected override IEnumerator SequencePlay(GameObject player)
        {
            // 2. 오디오 정지
            if (jumpScare != null)
            {
                jumpScare.Stop();
            }

            GameObject shGo = GameObject.Find("SHAmb");
            if (shGo != null)
            {
                AudioSource normalBgm = shGo.GetComponent<AudioSource>();
                if (normalBgm != null)
                {
                    normalBgm.Stop();
                }
            }

            // 3. 딜레이 후 씬 이동 (임시: MainMenu)
            yield return new WaitForSeconds(1f);
            
            if (fader != null)
            {
                fader.FadeTo(loadToScene);
            }
        }
        #endregion
    }
}
