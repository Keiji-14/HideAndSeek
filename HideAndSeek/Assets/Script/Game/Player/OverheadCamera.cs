using UnityEngine;

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
    /// カメラの移動を処理するメソッド
    /// </summary>
    private void MoveCamera()
    {
        // 入力から水平および垂直方向の移動量を取得
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 入力方向をベクトルとして定義
        Vector3 direction = new Vector3(horizontal, 0, vertical);

        // カメラを移動させる
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// カメラの回転を処理するメソッド
    /// </summary>
    private void RotateCamera()
    {
        // マウスのX軸の移動量を取得
        float mouseX = Input.GetAxis("Mouse X");

        // カメラを回転させる
        transform.Rotate(Vector3.up, mouseX * rotateSpeed * Time.deltaTime);
    }
    #endregion
}
