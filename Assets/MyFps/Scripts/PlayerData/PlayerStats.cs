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

        [Header("Player Inventory")]
        [SerializeField]
        private bool hasHeartKey1 = false;
        [SerializeField]
        private bool hasHeartKey2 = false;
        [SerializeField]
        private bool hasLeftEye = false;
        [SerializeField]
        private bool hasRightEye = false;
        [SerializeField]
        private bool isPuzzleSolved = false;

        public bool HasPistol { get; set; } = false;
        #endregion

        #region Properties
        public int AmmoCount => ammoCount;
        
        public bool HasHeartKey1
        {
            get => hasHeartKey1;
            set => hasHeartKey1 = value;
        }

        public bool HasHeartKey2
        {
            get => hasHeartKey2;
            set => hasHeartKey2 = value;
        }

        public bool HasLeftEye
        {
            get => hasLeftEye;
            set => hasLeftEye = value;
        }

        public bool HasRightEye
        {
            get => hasRightEye;
            set => hasRightEye = value;
        }

        public bool IsPuzzleSolved
        {
            get => isPuzzleSolved;
            set => isPuzzleSolved = value;
        }
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            // 새 게임 또는 로드 데이터 적용
            if (SaveLoadManager.isNewGame)
            {
                ResetStats();
                SaveLoadManager.isNewGame = false;
            }
            else if (SaveLoadManager.pendingLoadData != null)
            {
                LoadAmmo(SaveLoadManager.pendingLoadData.savedAmmoCount);
                SaveLoadManager.pendingLoadData = null;
            }

            //00 탄량 초기화 (인스펙터에 설정된 초기 값을 유지하되 0 이하일때만 초기화)
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

        //03 저장된 총알 개수를 불러올 때 사용하는 함수
        public void LoadAmmo(int amount)
        {
            ammoCount = amount;
        }

        //04 새 게임 시작 시 스탯을 초기화하는 함수
        public void ResetStats()
        {
            ammoCount = 0;
            HasPistol = false;
            hasHeartKey1 = false;
            hasHeartKey2 = false;
            hasLeftEye = false;
            hasRightEye = false;
            isPuzzleSolved = false;
        }
        #endregion

    }
}