using UnityEngine;

namespace Player
{
    /// <summary>
    /// 上空視点カメラの処理
    /// </summary>
    public class OverheadCamera : MonoBehaviour
    {
        #region PrivateField
        /// <summary>カメラの移動速度</summary>
        private float moveSpeed = 10f;
        /// <summary>カメラの回転速度</summary>
        private float rotateSpeed = 100f;
        /// <summary>カメラの垂直角度の制限</summary>
        private float verticalRotationLimit = 60f;
        /// <summary>垂直方向の回転角度</summary>
        private float verticalRotation = 0f;
        #endregion

        #region UnityEvent
        private void Update()
        {
            MoveCamera();
            RotateCamera();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// カメラの移動を行う処理
        /// </summary>
        private void MoveCamera()
        {
            // キーの入力を取得
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // 上下移動のためのキー入力を取得
            float upward = 0f;
            if (Input.GetKey(KeyCode.E))
            {
                upward = 1f;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                upward = -1f;
            }

            // 入力方向をベクトルとして定義
            Vector3 direction = new Vector3(horizontal, upward, vertical);

            // カメラを移動させる
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.Self);
        }

        /// <summary>
        /// カメラの回転を行う処理
        /// </summary>
        private void RotateCamera()
        {
            // マウスの入力を取得
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // カメラを水平に回転させる
            transform.Rotate(Vector3.up, mouseX * rotateSpeed * Time.deltaTime);

            // 垂直回転角度を計算
            verticalRotation -= mouseY * rotateSpeed * Time.deltaTime;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

            // 現在の回転角度を取得
            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.x = verticalRotation;
            currentRotation.z = 0;

            // カメラの回転を設定
            transform.eulerAngles = currentRotation;
        }
        #endregion
    }
}