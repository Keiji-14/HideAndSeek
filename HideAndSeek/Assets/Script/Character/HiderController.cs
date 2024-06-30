using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 隠れる側の処理
/// </summary>
public class HiderController : MonoBehaviourPunCallbacks
{
    #region PrivateField
    private bool isGrounded;
    private Vector3 velocity;
    private List<GameObject> transformObjList;
    private CharacterController characterController;

    private GameObject currentObject;
    private int currentTransformIndex = 0;
    #endregion

    #region SerializeField
    [SerializeField] private string transformPath;
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private List<GameObject> transformObjLists;
    #endregion

    #region UnityEvent
    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        currentObject = gameObject;
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        Move();

        if (Input.GetMouseButtonDown(1)) // 右クリック
        {
            TransformIntoNextObject();
        }
    }
    #endregion

    #region PrivateMethod
    private void Move()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void TransformIntoNextObject()
    {
        if (transformObjLists == null || transformObjLists.Count == 0)
        {
            Debug.LogError("No transformable objects found.");
            return;
        }

        // 現在のオブジェクトを削除
        if (currentObject != null && currentObject != gameObject)
        {
            PhotonNetwork.Destroy(currentObject);
        }

        // 次のオブジェクトにインデックスを更新
        currentTransformIndex = (currentTransformIndex + 1) % transformObjLists.Count;

        // 新しいオブジェクトを生成
        var position = transform.position;
        var rotation = transform.rotation;
        currentObject = PhotonNetwork.Instantiate($"{transformPath + transformObjLists[currentTransformIndex].name}", position, rotation);

        // 新しいオブジェクトを子オブジェクトとして設定
        currentObject.transform.SetParent(this.transform);

        // プレイヤーの位置と回転を新しいオブジェクトに引き継ぐ
        currentObject.transform.position = position;
        currentObject.transform.rotation = rotation;
    }
    #endregion
}
