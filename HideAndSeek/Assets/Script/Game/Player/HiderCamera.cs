using UnityEngine;

public class HiderCamera : MonoBehaviour
{
    #region PrivateField
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
    #endregion

    #region UnityEvent
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        LookAround();
    }

    void LateUpdate()
    {
        FollowPlayer();
    }
    #endregion

    #region PrivateMethod
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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // カメラリグの回転を設定
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// カメラの位置を設定してプレイヤーを見る処理
    /// </summary>
    private void FollowPlayer()
    {
        Vector3 desiredPosition = playerTransform.position + playerTransform.rotation * offset;
        transform.position = desiredPosition;
        transform.LookAt(playerTransform.position + Vector3.up * offset.y);
    }
    #endregion
}
