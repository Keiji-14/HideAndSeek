using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekerCamera : MonoBehaviour
{
    #region PrivateField
    private float xRotation = 0f;
    #endregion

    #region SerializeField
    /// <summary>Å‘åƒJƒƒ‰‹——£</summary>
    [SerializeField] private float mouseSensitivity = 100.0f;
    /// <summary>Å¬ƒJƒƒ‰‹——£</summary>
    [SerializeField] private Transform playerBody;
    #endregion

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        LookAround();
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
