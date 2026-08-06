using UnityEngine;

namespace Unity.FPS.Utility
{
    /// <summary>
    /// 오디오 재생 관련 유틸리티 정적 클래스
    /// </summary>
    public static class AudioUtility
    {
        /// <summary>
        /// 지정된 위치에서 일회성 사운드를 재생하고, 재생이 끝나면 자동으로 파괴되는 오브젝트를 생성합니다.
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="position">사운드가 발생할 월드 위치</param>
        /// <param name="spatialBlend">공간감 (0=2D, 1=3D)</param>
        /// <param name="volume">볼륨 (0~1)</param>
        public static void CreateSFX(AudioClip clip, Vector3 position, float spatialBlend = 1f, float volume = 1f)
        {
            if (clip == null) return;

            // 임시 오디오 오브젝트 생성
            GameObject audioObject = new GameObject("AudioSFX_" + clip.name);
            audioObject.transform.position = position;

            // AudioSource 세팅
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.volume = volume;
            source.Play();

            // 클립 길이만큼 기다린 후 자동 파괴되도록 TimeSelfDestruct 컴포넌트 부착
            TimeSelfDestruct tsd = audioObject.AddComponent<TimeSelfDestruct>();
            tsd.lifeTime = clip.length;
        }
    }
}
