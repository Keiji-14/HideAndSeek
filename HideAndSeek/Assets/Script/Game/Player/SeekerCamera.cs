using UnityEngine;

namespace Player
{
    public class SeekerCamera : MonoBehaviour
    {
        #region PrivateField
        private float xRotation = 0f;
        #endregion

        #region SerializeField
        /// <summary>summary>
        [SerializeField] private float mouseSensitivity = 100.0f;
        /// <summary></summary>
        [SerializeField] private Transform playerBody;
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
        private void LookAround()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        #endregion
    }
}