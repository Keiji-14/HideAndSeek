using Game;
using Photon.Pun;
using UnityEngine;

public class SeekerController : MonoBehaviourPunCallbacks
{
    #region PrivateField
    /// <summary>地面についているかの判定</summary>
    private bool isGrounded;
    /// <summary>速度ベクトル</summary>
    private Vector3 velocity;
    /// <summary>カメラ</summary>
    private Camera camera;
    /// <summary>キャラクターコントローラー</summary>
    private CharacterController characterController;
    #endregion

    #region SerializeField
    /// <summary>移動速度</summary>
    [SerializeField] private float speed;
    /// <summary>ジャンプの高さ</summary>
    [SerializeField] private float jumpHeight;
    /// <summary>重力</summary>
    [SerializeField] private float gravity;
    /// <summary>攻撃の射程距離</summary>
    [SerializeField] private float attackRange;
    /// <summary>カメラのTransform</summary>
    [SerializeField] private Transform cameraTransform;
    #endregion

    #region UnityEvent
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;

        SetCamera();
    }

    private void Update()
    {
        // 自分のキャラクターかどうかを確認
        if (!photonView.IsMine)
            return;

        Move();
        HandleAttack();
    }
    #endregion

    #region PublicMethod
    /// <summary>
    /// カメラの有効を切り替える処理
    /// </summary>
    public void SetCamera()
    {
        if (photonView.IsMine)
        {
            cameraTransform.gameObject.SetActive(true);
        }
        else
        {
            cameraTransform.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void NotifyCapture(int hiderViewID)
    {
        PhotonView hiderPhotonView = PhotonView.Find(hiderViewID);
        if (hiderPhotonView != null && hiderPhotonView.CompareTag("Hider"))
        {
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.OnPlayerCaught();
            }
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

    private void HandleAttack()
    {
        // 左クリックで攻撃
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Vector3 forward = camera.transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(camera.transform.position, forward, out hit, attackRange))
            {
                if (hit.collider.CompareTag("Hider"))
                {
                    Debug.Log("Hider hit: " + hit.collider.name);
                    // Hiderを捕まえた時の処理を追加
                    CaptureHider(hit.collider.gameObject);
                }
                else
                {
                    Debug.Log("Hit something else: " + hit.collider.name);
                }
            }
        }
    }

    private void CaptureHider(GameObject hider)
    {
        // Hiderを捕まえた時の処理
        PhotonView hiderPhotonView = hider.GetComponent<PhotonView>();
        if (hiderPhotonView != null)
        {
            photonView.RPC("NotifyCapture", RpcTarget.All, hiderPhotonView.ViewID);
        }
    }
    #endregion
}