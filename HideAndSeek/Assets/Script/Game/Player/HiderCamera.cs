using UnityEngine;

namespace Player
{
    /// <summary>
    /// 隠れる側のカメラ処理
    /// </summary>
    public class HiderCamera : MonoBehaviour
    {
        #region PrivateField
        /// <summary>カメラロック状態</summary>
        private bool isCameraLocked = false;
        /// <summary>カメラのX軸回転角度</summary>
        private float xRotation = 0f;
        /// <summary>カメラのY軸回転角度</summary>
        private float yRotation = 0f;
        #endregion

        #region SerializeField
        /// <summary>マウス感度</summary>
        [SerializeField] private float mouseSensitivity = 100.0f;
        /// <summary>カメラのオフセット位置</summary>
        [SerializeField] private Vector3 offset;
        /// <summary>プレイヤーのTransform</summary>
        [SerializeField] private Transform playerTransform;
        /// <summary>カメラの補間速度</summary>
        [SerializeField] private float smoothSpeed = 0.125f;
        /// <summary>カメラのX軸回転角度の最小値</summary>
        [SerializeField] private float minXRotation = -18f;
        /// <summary>カメラのX軸回転角度の最大値</summary>
        [SerializeField] private float maxXRotation = 90f;
        #endregion

        #region UnityEvent
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                SwitchLockCamera();
            }

            LookAround();
        }

        void LateUpdate()
        {
            FollowPlayer();
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// カメラロック状態を取得する処理
        /// </summary>
        public bool IsCameraLocked()
        {
            return isCameraLocked;
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 自プレイヤーのロックを切り替える処理
        /// </summary>
        private void SwitchLockCamera()
        {
            isCameraLocked = !isCameraLocked;
        }

        /// <summary>
        /// カメラの回転を制御する処理
        /// </summary>
        private void LookAround()
        {
            // マウスの入力を取得
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

            // カメラリグの回転を設定
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            if (!isCameraLocked)
            {
                playerTransform.Rotate(Vector3.up * mouseX);
            }
        }

        /// <summary>
        /// カメラの位置を設定してプレイヤーを見る処理
        /// </summary>
        private void FollowPlayer()
        {
            Vector3 desiredPosition;
            // 縦方向の回転を考慮してカメラの位置を設定
            Quaternion cameraRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            desiredPosition = playerTransform.position + cameraRotation * offset;

            // カメラの位置をスムーズに補間
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // プレイヤーを注視
            transform.LookAt(playerTransform.position + Vector3.up * offset.y);
        }
        #endregion
    }
}