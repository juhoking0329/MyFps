using UnityEngine;

namespace MySample
{
    /// <summary>
    /// 사운드 플레이 테스트 클래스
    /// </summary>
    public class SoundTest : MonoBehaviour
    {
        #region Variables
        //재생할 오디오 소스 속성
        public AudioClip clip;

        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private bool isLoop = false;
        [SerializeField] private bool isPlayOnAwake = false;

        //오디오 소스 컴포넌트
        private AudioSource audioSource;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //오디오 소스 참조
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            //오디오 소스 속성 설정
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = isLoop;
            audioSource.playOnAwake = isPlayOnAwake;
        }

        void Start()
        {
            //재생 여부 설정
            audioSource.Play();
        }
        #endregion

        #region Custom Methods

        #endregion
    }
}