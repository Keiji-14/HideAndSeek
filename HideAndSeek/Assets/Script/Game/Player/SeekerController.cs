using Game;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class SeekerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region PrivateField
    /// <summary>地面についているかの判定</summary>
    private bool isGrounded;
    /// <summary>走るアニメーションの状態</summary>
    private bool isRunning;
    /// <summary>ジャンプアニメーションの状態</summary>
    private bool isJumping;
    /// <summary>攻撃アニメーションの状態</summary>
    private bool isAttack;
    /// <summary>攻撃モーション中の状態</summary>
    private bool isAttacking;
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
    /// <summary>アニメーター</summary>
    [SerializeField] private Animator animator;
    /// <summary>攻撃モーションのクリップ</summary>
    [SerializeField] private AnimationClip attackAnimationClip;
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
        cameraTransform.gameObject.SetActive(photonView.IsMine);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRunning);
            stream.SendNext(isGrounded);
            stream.SendNext(isJumping);
            stream.SendNext(isAttack);
        }
        else
        {
            isRunning = (bool)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
            isJumping = (bool)stream.ReceiveNext();
            isAttack = (bool)stream.ReceiveNext();
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isGrounded", isGrounded);

            if (isJumping)
            {
                animator.SetTrigger("Jump"); // ジャンプフラグが立っていたらジャンプアニメーションをトリガー
            }

            if (isAttack)
            {
                animator.SetTrigger("Attack");
                isAttack = false; // 攻撃フラグをリセット
            }
        }
    }
    #endregion

    #region PrivateMethod
    private void Move()
    {
        isGrounded = characterController.isGrounded;
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * speed * Time.deltaTime);

        // アニメーションの設定
        bool newIsRunning = move.magnitude > 0;
        if (newIsRunning != isRunning)
        {
            isRunning = newIsRunning;
            photonView.RPC("RPC_SetRunningAnimation", RpcTarget.All, isRunning);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            photonView.RPC("RPC_SetJumpAnimation", RpcTarget.All);
        }

        if (isGrounded)
        {
            isJumping = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // isGroundedの状態を定期的に同期する
        photonView.RPC("RPC_UpdateGroundedState", RpcTarget.All, isGrounded);
    }

    private void HandleAttack()
    {
        // 左クリックで攻撃
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            StartCoroutine(AttackCoroutine());

            RaycastHit hit;
            Vector3 forward = camera.transform.TransformDirection(Vector3.forward);

            isAttack = true;
            photonView.RPC("RPC_SetAttackAnimation", RpcTarget.All);

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

    private IEnumerator AttackCoroutine()
    {
        isAttack = true;
        photonView.RPC("RPC_SetAttackAnimation", RpcTarget.All);

        RaycastHit hit;
        Vector3 forward = camera.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(camera.transform.position, forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Hider"))
            {
                Debug.Log("Hider hit: " + hit.collider.name);
                CaptureHider(hit.collider.gameObject);
            }
            else
            {
                Debug.Log("Hit something else: " + hit.collider.name);
            }
        }

        // 攻撃モーションの長さ分待機
        yield return new WaitForSeconds(attackAnimationClip.length);
        isAttacking = false;
        isAttack = false;
    }

    private void CaptureHider(GameObject hider)
    {
        // 親オブジェクトが存在する場合、親オブジェクトを取得
        if (hider.transform.parent != null)
        {
            hider = hider.transform.parent.gameObject;
        }

        // Hiderを捕まえた時の処理
        PhotonView hiderView = hider.GetComponent<PhotonView>();
        if (hiderView != null)
        {
            // GameControllerのインスタンスを取得
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.OnPlayerCaught(hiderView.ViewID);
            }
        }
    }

    [PunRPC]
    private void RPC_SetRunningAnimation(bool isRunning)
    {
        animator.SetBool("isRunning", isRunning);
    }

    [PunRPC]
    private void RPC_SetJumpAnimation()
    {
        animator.SetTrigger("Jump");
    }

    [PunRPC]
    private void RPC_SetAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    [PunRPC]
    private void RPC_UpdateGroundedState(bool groundedState)
    {
        isGrounded = groundedState;
        animator.SetBool("isGrounded", isGrounded);
    }
    #endregion
}