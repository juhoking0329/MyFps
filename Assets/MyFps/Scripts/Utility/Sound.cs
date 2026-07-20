using UnityEngine;

namespace MyFps
{
    public enum SoundType
    {
        BGM,
        SFX
    }

    /// <summary>
    /// 사운드 데이터 속성 정의 클래스
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        #region Variables
        [SerializeField]
        public string name;              // 사운드 이름

        public SoundType soundType;      // 사운드 타입 (BGM / SFX)

        public AudioClip clip;           // 사운드 리소스 - 음원

        public float volume;             // 사운드 볼륨
        public float pitch;              // 사운드 속도

        public bool loop;                // 사운드 반복 여부
        public bool playOnAwake;         // 사운드 재생 여부

        //...

        [HideInInspector]
        public AudioSource audioSource;  // 사운드 재생용 오디오 소스
        #endregion

    }
}
