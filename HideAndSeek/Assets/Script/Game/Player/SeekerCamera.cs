using UnityEngine;

namespace Player
{
    /// <summary>
    /// 鬼側のカメラ処理
    /// </summary>
    public class SeekerCamera : MonoBehaviour
    {
        #region PrivateField
        /// <summary>カメラのX軸回転角度</summary>
        private float xRotation = 0f;
        #endregion

        #region SerializeField
        /// <summary>マウス感度<summary>
        [SerializeField] private float mouseSensitivity = 100.0f;
        /// <summary>プレイヤーのTransform</summary>
        [SerializeField] private Transform playerBody;
        /// <summary>カメラのX軸回転角度の最小値</summary>
        [SerializeField] private float minXRotation = -80f;
        /// <summary>カメラのX軸回転角度の最大値</summary>
        [SerializeField] private float maxXRotation = 80f;
        #endregion

        #region UnityEvent
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            LookAround();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マウス入力に基づいてカメラを回転させる処理
        /// </summary>
        private void LookAround()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // X軸の回転角度を更新し、範囲内に制限する
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            // プレイヤーをY軸周りに回転させる
            playerBody.Rotate(Vector3.up * mouseX);
        }
        #endregion
    }
}