using UnityEngine;
using UnityEngine.Audio;

namespace MyFps
{
    /// <summary>
    /// 사운드 데이터를 관리하는 싱글톤 클래스
    /// 사운드(배경음, 효과음) 플레이
    /// </summary>
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        #region Variables
        [Header("Audio List")]
        [SerializeField]
        public Sound[] sounds;

        [Header("BGM State")]
        [SerializeField] 
        private string bgmSound = ""; 
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            //00 부모 클래스 싱글톤 초기화 실행
            base.Awake();

            //00 각 사운드 데이터에 오디오 소스 컴포넌트 동적 할당 및 기본 셋팅
            foreach (var sound in sounds)
            {
                if (sound == null) continue;
                sound.audioSource = gameObject.AddComponent<AudioSource>();
                sound.audioSource.clip = sound.clip;
                sound.audioSource.volume = sound.volume;
                sound.audioSource.pitch = sound.pitch;
                sound.audioSource.loop = sound.loop;
                sound.audioSource.playOnAwake = sound.playOnAwake;
            }

            LoadVolumeSettings();
        }
        #endregion

        #region Custom Methods
        private void LoadVolumeSettings()
        {
            float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

            SetBgmVolume(bgmVol);
            SetSfxVolume(sfxVol);
        }

        public void SetBgmVolume(float volume)
        {
            PlayerPrefs.SetFloat("BGMVolume", volume);
            foreach (var sound in sounds)
            {
                if (sound != null && sound.audioSource != null && sound.soundType == SoundType.BGM)
                {
                    sound.audioSource.volume = sound.volume * volume;
                }
            }
        }

        public void SetSfxVolume(float volume)
        {
            PlayerPrefs.SetFloat("SFXVolume", volume);
            foreach (var sound in sounds)
            {
                if (sound != null && sound.audioSource != null && sound.soundType == SoundType.SFX)
                {
                    sound.audioSource.volume = sound.volume * volume;
                }
            }
        }

        //01 이름으로 원하는 사운드를 재생하는 함수
        public void Play(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            //이름으로 재생할 사운드 찾기
            Sound sound = null;
            foreach(var s in sounds)
            {
                if (s.name == name)
                {
                    sound = s;
                    break;
                }
            }
            //찾지 못했다면 경고 로그 출력 후 종료
            if (sound == null)
            {
                Debug.Log($"Sound: {name} not found!");
                return;
            }
            //찾았다면 사운드 재생
            sound.audioSource.Play();
        }

        //02 이름으로 재생 중인 사운드를 정지하는 함수
        public void Stop(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            //이름으로 정지할 사운드 찾기
            Sound sound = null;
            foreach (var s in sounds)
            {
                if (s.name == name)
                {
                    sound = s;
                    break;
                }
            }
            //찾지 못했다면 경고 로그 출력 후 종료
            if (sound == null)
            {
                Debug.Log($"Sound: {name} not found!");
                return;
            }
            //찾았다면 사운드 정지
            sound.audioSource.Stop();
        }

        //03 배경 음악(BGM)을 재생 및 전환하는 함수
        public void PlayBGM(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            //현재 플레이 되는 배경음 체크 - 재생 중이라면 정지
            if(bgmSound == name)    //같은 배경음을 플레이하면 변경 없이 계속 플레이
            {
                return;
            }

            //현재 재생 중인 배경음 정지
            Stop(bgmSound);  

            //이름으로 재생할 사운드 찾기
            Sound sound = null;
            foreach (var s in sounds)
            {
                if (s.name == name)
                {
                    sound = s;
                    bgmSound = s.name;  //배경 사운드 이름 저장
                    break;
                }
            }
            //찾지 못했다면 경고 로그 출력 후 종료
            if (sound == null)
            {
                Debug.Log($"BGMSound: {name} not found!");
                return;
            }
            //찾았다면 사운드 재생
            sound.audioSource.Play();
        }
        #endregion
    }
}