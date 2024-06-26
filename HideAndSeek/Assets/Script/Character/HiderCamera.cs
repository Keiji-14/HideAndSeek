using UnityEngine;

public class HiderCamera : MonoBehaviour
{
    #region PrivateField
    /// <summary>�J������X����]�p�x</summary>
    private float xRotation = 0f;
    /// <summary>�J������Y����]�p�x</summary>
    private float yRotation = 0f;
    #endregion

    #region SerializeField
    /// <summary>�}�E�X���x</summary>
    [SerializeField] private float mouseSensitivity = 100.0f;
    /// <summary>�J�����̃I�t�Z�b�g�ʒu</summary>
    [SerializeField] private Vector3 offset;
    /// <summary>�v���C���[��Transform</summary>
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
    /// �J�����̉�]�𐧌䂷�鏈��
    /// </summary>
    private void LookAround()
    {
        // �}�E�X�̓��͂��擾
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // �J�������O�̉�]��ݒ�
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// �J�����̈ʒu��ݒ肵�ăv���C���[�����鏈��
    /// </summary>
    private void FollowPlayer()
    {
        Vector3 desiredPosition = playerTransform.position + playerTransform.rotation * offset;
        transform.position = desiredPosition;
        transform.LookAt(playerTransform.position + Vector3.up * offset.y);
    }
    #endregion
}
