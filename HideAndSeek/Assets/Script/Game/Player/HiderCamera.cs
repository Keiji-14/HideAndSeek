using UnityEngine;

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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

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
        if (!isCameraLocked)
        {
            Vector3 desiredPosition = playerTransform.position + playerTransform.rotation * offset;
            transform.position = desiredPosition;
        }
        else
        {
            Vector3 direction = new Vector3(Mathf.Sin(yRotation * Mathf.Deg2Rad), 0, Mathf.Cos(yRotation * Mathf.Deg2Rad));
            Vector3 desiredPosition = playerTransform.position - direction * offset.z + Vector3.up * offset.y;
            transform.position = desiredPosition;
        }
        transform.LookAt(playerTransform.position + Vector3.up * offset.y);
    }
    #endregion
}