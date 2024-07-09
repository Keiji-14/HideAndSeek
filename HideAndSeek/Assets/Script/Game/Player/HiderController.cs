using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 隠れる側の処理
/// </summary>
public class HiderController : MonoBehaviourPunCallbacks
{
    #region PrivateField
    /// <summary>地面についているかの判定</summary>
    private bool isGrounded;
    /// <summary>速度ベクトル</summary>
    private Vector3 velocity;
    /// <summary>キャラクターコントローラー</summary>
    private CharacterController characterController;
    [Header("Transform Object")]
    /// <summary>変身オブジェクトのインデックス</summary>
    private int currentTransformIndex = 0;
    /// <summary>変身状態</summary>
    private bool isTransformed = false;
    /// <summary>現在の変身オブジェクト</summary>
    private GameObject currentObject;
    /// <summary>Rigidbody</summary>
    private Rigidbody rigidbody;
    #endregion

    #region SerializeField
    /// <summary>変身するオブジェクトのファイルパス</summary>
    [SerializeField] private string transformPath;
    /// <summary>移動速度</summary>
    [SerializeField] private float speed;
    /// <summary>ジャンプの高さ</summary>
    [SerializeField] private float jumpHeight;
    /// <summary>重力</summary>
    [SerializeField] private float gravity;
    /// <summary>カメラのTransform</summary>
    [SerializeField] private Transform cameraTransform;
    /// <summary>プレイヤーモデル</summary>
    [SerializeField] private GameObject playerModel; 
    /// <summary>変身するオブジェクトのリスト</summary>
    [SerializeField] private List<GameObject> transformObjList;
    #endregion

    #region UnityEvent
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.isKinematic = true;

        if (photonView.IsMine)
        {
            cameraTransform.gameObject.SetActive(true);
        }
        else
        {
            cameraTransform.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 自分のキャラクターかどうかを確認
        if (!photonView.IsMine)
            return;

        if (!isTransformed)
        {
            Move();
        }
        else
        {
            TransformMove();
        }

        // 右クリック
        if (Input.GetMouseButtonDown(1))
        {
           　TransformIntoNextObject();
        }

        // 左クリック
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (isTransformed)
            {
                RevertToPlayer();
            }
        }
    }
    #endregion

    #region PrivateMethod
    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
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

    /// <summary>
    /// 変身オブジェクトの移動処理
    /// </summary>
    private void TransformMove()
    {
        if (rigidbody == null)
            return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        rigidbody.MovePosition(rigidbody.position + move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * gravity), ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// プレイヤーを物に変身させる処理
    /// </summary>
    private void TransformIntoNextObject()
    {
        if (transformObjList == null || transformObjList.Count == 0)
        {
            return;
        }

        currentTransformIndex = (currentTransformIndex + 1) % transformObjList.Count;

        photonView.RPC("RPC_TransformIntoObject", RpcTarget.AllBuffered, currentTransformIndex);
    }

    /// <summary>
    /// プレイヤーの変身を解除する処理
    /// </summary>
    private void RevertToPlayer()
    {
        photonView.RPC("RPC_RevertToPlayer", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_TransformIntoObject(int transformIndex)
    {
        if (currentObject != null && currentObject != gameObject)
        {
            PhotonNetwork.Destroy(currentObject);
        }

        playerModel.SetActive(false);

        var position = transform.position;
        var rotation = transform.rotation;
        currentObject = Instantiate(transformObjList[transformIndex], position, rotation);
        currentObject.transform.SetParent(this.transform);
        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;

        characterController.enabled = false;
        rigidbody.isKinematic = false;
        EnableColliders(currentObject, true);

        isTransformed = true;
    }

    /// <summary>
    /// プレイヤーの変身を解除する処理
    /// </summary>
    [PunRPC]
    private void RPC_RevertToPlayer()
    {
        if (currentObject != null && currentObject != gameObject)
        {
            EnableColliders(currentObject, false);
            Destroy(currentObject);
        }

        playerModel.SetActive(true);
        characterController.enabled = true;
        rigidbody.isKinematic = true;
        isTransformed = false;
    }

    /// <summary>
    /// 子オブジェクトのコライダーを有効化または無効化する
    /// </summary>
    private void EnableColliders(GameObject obj, bool enable)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = enable;
        }
    }
    #endregion
}
