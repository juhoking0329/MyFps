using UnityEngine;

namespace MySample
{
    /// <summary>
    /// 플레이어 쫓아다니는 카메라 컨트롤러
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public Transform thePlayer;
        public Vector3 offset;

        private void LateUpdate()
        {
            transform.position = thePlayer.position + offset;
        }

    }
}