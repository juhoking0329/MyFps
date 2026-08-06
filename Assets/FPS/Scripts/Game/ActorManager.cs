using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Actor를 관리하는 클래스
    /// </summary>
    public class ActorManager : MonoBehaviour
    {
        #region Singleton
        public static ActorManager Instance { get; private set; }
        #endregion

        #region Variables
        public List<Actor> Actors { get; private set; }
        public GameObject Player { get; private set; }

        [Header("Events")]
        [Tooltip("모든 적이 사망했을 때 호출되는 이벤트 (예: 웨이브 클리어)")]
        public UnityEngine.Events.UnityAction OnAllEnemiesDead;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // 싱글톤 초기화
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            //액터 리스트 생성
            Actors = new List<Actor>();
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 액터를 리스트에 등록합니다.
        /// </summary>
        public void RegisterActor(Actor actor)
        {
            if (actor != null && !Actors.Contains(actor))
            {
                Actors.Add(actor);
            }
        }

        /// <summary>
        /// 액터를 리스트에서 제거합니다.
        /// </summary>
        public void UnregisterActor(Actor actor)
        {
            if (actor != null && Actors.Contains(actor))
            {
                Actors.Remove(actor);
                
                // 적(affiliation == 1)이 삭제된 경우 남은 적 수를 체크하여 이벤트 발생
                if (actor.affiliation == 1 && GetAliveEnemyCount() == 0)
                {
                    OnAllEnemiesDead?.Invoke();
                }
            }
        }

        /// <summary>
        /// 현재 살아있는 적(affiliation == 1)의 총 수를 반환합니다.
        /// </summary>
        public int GetAliveEnemyCount()
        {
            int count = 0;
            foreach (var actor in Actors)
            {
                if (actor.affiliation == 1)
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}