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

        // 現在のオブジェクトを削除
        if (currentObject != null && currentObject != gameObject)
        {
            PhotonNetwork.Destroy(currentObject);
        }

        // プレイヤーモデルを非表示
        playerModel.SetActive(false);

        // 次のオブジェクトにインデックスを更新
        currentTransformIndex = (currentTransformIndex + 1) % transformObjList.Count;

        // 新しいオブジェクトを生成
        var position = transform.position;
        var rotation = transform.rotation;
        currentObject = PhotonNetwork.Instantiate($"{transformPath + transformObjList[currentTransformIndex].name}", position, rotation);

        // 新しいオブジェクトを子オブジェクトとして設定
        currentObject.transform.SetParent(this.transform);

        // プレイヤーの位置と回転を新しいオブジェクトに引き継ぐ
        currentObject.transform.position = position;
        currentObject.transform.rotation = rotation;

        // CharacterControllerを無効化
        characterController.enabled = false;

        // Rigidbodyを有効化
        rigidbody.isKinematic = false;

        // 子オブジェクトのコライダーを有効化
        EnableColliders(currentObject, true);

        isTransformed = true;
    }

    /// <summary>
    /// プレイヤーの変身を解除する処理
    /// </summary>
    private void RevertToPlayer()
    {
        if (currentObject != null && currentObject != gameObject)
        {
            // 子オブジェクトのコライダーを無効化
            EnableColliders(currentObject, false);

            PhotonNetwork.Destroy(currentObject);
        }

        // プレイヤーモデルを表示
        playerModel.SetActive(true);

        // CharacterControllerを再度有効化
        characterController.enabled = true;

        // Rigidbodyを無効化
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
