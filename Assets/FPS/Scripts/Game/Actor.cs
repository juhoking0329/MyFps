using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 전투 참가하는 캐릭터에 부착되는 클래스
    /// </summary>
    public class Actor : MonoBehaviour
    {
        #region Variables
        //소속
        public int affiliation; // 진영 구분 (0: 플레이어, 1: 적, 2: 중립 등)
        //조준점
        public Transform aimPoint; // 조준점 (총알 발사 위치 등)

        #endregion

        #region Unity Event Methods

        private void Awake()
        {
            // actor 리스트 등록 (ActorManager 싱글톤 사용)
            if (ActorManager.Instance != null)
            {
                ActorManager.Instance.RegisterActor(this);
            }
        }

        //킬
        private void OnDestroy()
        {
            // actor 리스트 삭제
            if (ActorManager.Instance != null)
            {
                ActorManager.Instance.UnregisterActor(this);
            }
        }
        #endregion
    }
}