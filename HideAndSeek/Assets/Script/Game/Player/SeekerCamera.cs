using UnityEngine;

namespace Player
{
    /// <summary>
    /// �S���̃J��������
    /// </summary>
    public class SeekerCamera : MonoBehaviour
    {
        #region PrivateField
        /// <summary>�J������X����]�p�x</summary>
        private float xRotation = 0f;
        #endregion

        #region SerializeField
        /// <summary>�}�E�X���x<summary>
        [SerializeField] private float mouseSensitivity = 100.0f;
        /// <summary>�v���C���[��Transform</summary>
        [SerializeField] private Transform playerBody;
        /// <summary>�J������X����]�p�x�̍ŏ��l</summary>
        [SerializeField] private float minXRotation = -80f;
        /// <summary>�J������X����]�p�x�̍ő�l</summary>
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
        /// �}�E�X���͂Ɋ�Â��ăJ��������]�����鏈��
        /// </summary>
        private void LookAround()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // X���̉�]�p�x���X�V���A�͈͓��ɐ�������
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            // �v���C���[��Y������ɉ�]������
            playerBody.Rotate(Vector3.up * mouseX);
        }
        #endregion
    }
}