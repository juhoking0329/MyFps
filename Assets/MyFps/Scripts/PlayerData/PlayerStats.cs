using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 플레이어의 속성값들을 관리하는 싱글톤 클래스
    /// </summary>
    public class PlayerStats : PersistentSingleton<PlayerStats>
    {
        #region Variables
        [Header("Player Ammunition")]
        [SerializeField]
        private int ammoCount;
        #endregion

        #region Properties
        public int AmmoCount => ammoCount;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 플레이어 탄량 초기화 (인스펙터에 설정된 초기 값을 유지하기 위해 0 이하일 때만 초기화)
            if (ammoCount <= 0)
            {
                ammoCount = 0;
            }
        }
        #endregion

        #region Custom Methods
        //01 탄환을 추가하는 함수
        public void AddAmmo(int amount)
        {
            ammoCount += amount;
        }

        //02 탄환을 소비하는 함수
        public bool UseAmmo(int amount = 1)
        {
            if(ammoCount < amount)
            {
                Debug.Log("You need to reload");
                return false;
            }

            ammoCount -= amount;
            return true;
        }
        #endregion

    }
}