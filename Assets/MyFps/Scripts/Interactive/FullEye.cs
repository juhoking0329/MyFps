using System.Collections;
using TMPro;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 두 개의 눈동자를 모아 퍼즐을 푸는 상호작용 클래스
    /// </summary>
    public class FullEye : Interactive
    {
        [Header("Puzzle Visuals")]
        [SerializeField] private GameObject leftEyeVisual;
        [SerializeField] private GameObject rightEyeVisual;

        protected override void Awake()
        {
            base.Awake();
            action = "Place Eyes";
        }

        protected override void DoAction()
        {
            // 퍼즐이 이미 풀려있으면 무시
            if (PlayerStats.Instance.IsPuzzleSolved) return;

            bool placedLeft = false;
            bool placedRight = false;

            // LeftEye 배치
            if (PlayerStats.Instance.HasLeftEye && leftEyeVisual != null && !leftEyeVisual.activeSelf)
            {
                leftEyeVisual.SetActive(true);
                placedLeft = true;
            }

            // RightEye 배치
            if (PlayerStats.Instance.HasRightEye && rightEyeVisual != null && !rightEyeVisual.activeSelf)
            {
                rightEyeVisual.SetActive(true);
                placedRight = true;
            }

            // 둘 다 활성화되었는지 확인하여 퍼즐 풀림 처리
            if (leftEyeVisual != null && rightEyeVisual != null &&
                leftEyeVisual.activeSelf && rightEyeVisual.activeSelf)
            {
                PlayerStats.Instance.IsPuzzleSolved = true;
                Debug.Log("FullEye Puzzle Solved!");
                
                // 불꽃놀이 이펙트 생성
                CreateFireworks();

                // 퍼즐이 풀렸으므로 UI 더 이상 표시 안 함
                action = "";
                HideActionUI();
            }
        }

        private void CreateFireworks()
        {
            GameObject fireworks = new GameObject("FireworksEffect");
            fireworks.transform.position = transform.position + Vector3.up * 1f;
            
            ParticleSystem ps = fireworks.AddComponent<ParticleSystem>();
            
            // 시스템 정지 후 설정 변경 (오류 방지)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.duration = 2f;
            main.loop = false;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 50) });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // 분홍색 깨짐 현상 방지: 기본 파티클 머티리얼 할당
            ParticleSystemRenderer psr = fireworks.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                if (shader != null) psr.material = new Material(shader);
            }
            
            ps.Play();
            Destroy(fireworks, 3f);
        }
    }
}
