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
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        SetCamera();

        // ランダムなオブジェクトに変身させる
        int randomIndex = Random.Range(0, transformObjList.Count);
        TransformIntoObject(randomIndex);
    }

    private void Update()
    {
        // 自分のキャラクターかどうかを確認
        if (!photonView.IsMine)
            return;

        TransformMove();
    }
    #endregion

    #region PublicMethod
    /// <summary>
    /// カメラの有効を切り替える処理
    /// </summary>
    public void SetCamera()
    {
        cameraTransform.gameObject.SetActive(photonView.IsMine);
    }
    #endregion

    #region PrivateMethod

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
    private void TransformIntoObject(int transformIndex)
    {
        photonView.RPC("RPC_TransformIntoObject", RpcTarget.AllBuffered, transformIndex);
    }

    /// <summary>
    /// プレイヤーを物に変身させる処理
    /// </summary>
    [PunRPC]
    private void RPC_TransformIntoObject(int transformIndex)
    {
        if (currentObject != null && currentObject != gameObject)
        {
            Destroy(currentObject);
        }

        playerModel.SetActive(false);

        var position = transform.position;
        var rotation = transform.rotation;
        currentObject = Instantiate(transformObjList[transformIndex], position, rotation);
        currentObject.transform.SetParent(this.transform);
        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;

        rigidbody.isKinematic = false;
        EnableColliders(currentObject, true);

        isTransformed = true;
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
