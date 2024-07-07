using UnityEngine;

/// <summary>
/// 上空視点カメラの処理
/// </summary>
public class OverheadCamera : MonoBehaviour
{
    #region PrivateField
    /// <summary>カメラの移動速度</summary>
    private float moveSpeed = 10f;
    #endregion

    #region UnityEvent
    private void Update()
    {
        MoveCamera();
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
    #endregion
}
