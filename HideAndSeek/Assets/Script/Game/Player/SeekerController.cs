using Game;
using Audio;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 鬼側のプレイヤー処理
    /// </summary>
    public class SeekerController : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region PrivateField
        /// <summary>地面についているかの判定</summary>
        private bool isGrounded;
        /// <summary>走るアニメーションの状態</summary>
        private bool isRunning;
        /// <summary>ジャンプアニメーションの状態</summary>
        private bool isJumping;
        /// <summary>攻撃モーション中の状態</summary>
        private bool isAttacking;
        /// <summary>前回のisAttackingの状態</summary>
        private bool previousIsAttacking;
        /// <summary>速度ベクトル</summary>
        private Vector3 velocity;
        [Header("Component")]
        /// <summary>カメラ</summary>
        private Camera camera;
        /// <summary>キャラクターコントローラー</summary>
        private CharacterController characterController;
        /// <summary>AudioListener</summary>
        private AudioListener audioListener;
        /// <summary>ゲームUI</summary>
        private GameUI gameUI;
        #endregion

        #region SerializeField
        /// <summary>ライフ</summary>
        [SerializeField] private int life;
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
        [Header("Component")]
        /// <summary>名前用をキャンバス</summary>
        [SerializeField] private Canvas nameCanvas;
        /// <summary>アニメーター</summary>
        [SerializeField] private Animator animator;
        /// <summary>攻撃モーションのクリップ</summary>
        [SerializeField] private AnimationClip attackAnimationClip;
        /// <summary>プレイヤー名の表示</summary>
        [SerializeField] private PlayerNameDisplay playerNameDisplay;
        #endregion

        #region UnityEvent
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            audioListener = GetComponentInChildren<AudioListener>();
            gameUI = FindObjectOfType<GameUI>();
            camera = Camera.main;

            SetCamera();
            gameUI.UpdateLife(life);

            playerNameDisplay.Init(false);
        }

        private void Update()
        {
            RotationCanvas();

            // 自分のキャラクターかどうかを確認
            if (!photonView.IsMine)
                return;

            if (!isAttacking)
            {
                Move();
            }

            HandleAttack();
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// AudioListenerの有効を切り替える処理
        /// </summary>
        /// <param name="isActive">有効判定</param>
        public void SwitchAudioListener(bool isActive)
        {
            if (audioListener == null)
            {
                audioListener = GetComponentInChildren<AudioListener>();
            }
            audioListener.enabled = isActive;
        }

        /// <summary>
        /// Photonのシリアライズ処理
        /// </summary>
        /// <param name="stream">データストリーム</param>
        /// <param name="info">メッセージ情報</param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isRunning);
                stream.SendNext(isGrounded);
                stream.SendNext(isJumping);
                stream.SendNext(isAttacking);
            }
            else
            {
                isRunning = (bool)stream.ReceiveNext();
                isGrounded = (bool)stream.ReceiveNext();
                isJumping = (bool)stream.ReceiveNext();
                bool newIsAttacking = (bool)stream.ReceiveNext();
                animator.SetBool("isRunning", isRunning);
                animator.SetBool("isGrounded", isGrounded);

                if (isJumping)
                {
                    // ジャンプフラグが立っていたらジャンプアニメーションをトリガー
                    animator.SetTrigger("Jump");
                }

                if (newIsAttacking && !previousIsAttacking)
                {
                    animator.SetTrigger("Attack");
                }

                isAttacking = newIsAttacking;
                previousIsAttacking = isAttacking;
            }
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// カメラの有効を切り替える処理
        /// </summary>
        private void SetCamera()
        {
            cameraTransform.gameObject.SetActive(photonView.IsMine);
        }

        /// <summary>
        /// キャンバスをカメラに見えるように回転させる処理
        /// </summary>
        private void RotationCanvas()
        {
            if (Camera.main == null)
                return;

            Vector3 cameraDirection = Camera.main.transform.forward;

            // HPバーの方向をカメラの方向に向ける
            nameCanvas.transform.LookAt(nameCanvas.transform.position + cameraDirection);

        }

        /// <summary>
        /// 移動処理
        /// </summary>
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

            // ジャンプ処理
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

        /// <summary>
        /// 攻撃処理
        /// </summary>
        private void HandleAttack()
        {
            // 左クリックで攻撃
            if (Input.GetMouseButtonDown(0) && !isAttacking && isGrounded)
            {
                StartCoroutine(PerformAttack());
            }
        }

        /// <summary>
        /// 攻撃のコルーチン処理
        /// </summary>
        private IEnumerator PerformAttack()
        {
            // 攻撃が既に行われている場合は終了
            if (isAttacking)
                yield break;

            isAttacking = true;

            yield return new WaitForSeconds(0.1f);

            photonView.RPC("RPC_SetAttackAnimation", RpcTarget.All);

            yield return StartCoroutine(AttackCoroutine());

            isAttacking = false;
        }

        /// <summary>
        /// 攻撃コルーチン
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            animator.SetTrigger("Attack");
            SE.instance.Play(SE.SEName.AttackSE);

            RaycastHit hit;
            Vector3 forward = camera.transform.TransformDirection(Vector3.forward);

            // 攻撃が当たったかどうか
            if (Physics.Raycast(camera.transform.position, forward, out hit, attackRange))
            {
                if (hit.collider.CompareTag("Hider"))
                {
                    CaptureHider(hit.collider.gameObject);
                    SE.instance.Play(SE.SEName.AttackSE);
                }
                else
                {
                    HandleWrongAttack(hit.collider.gameObject);
                }
            }

            // 攻撃モーションの長さ分待機
            yield return new WaitForSeconds(attackAnimationClip.length);
            isAttacking = false;
        }

        /// <summary>
        /// 隠れる側のプレイヤーを捕まえる処理
        /// </summary>
        /// <param name="hider">隠れる側のプレイヤーオブジェクト</param>
        private void CaptureHider(GameObject hider)
        {
            // 最親オブジェクトを取得
            GameObject rootHider = GetRootParent(hider);

            // Hiderを捕まえた時の処理
            PhotonView seekderView = gameObject.GetComponent<PhotonView>();
            PhotonView hiderView = rootHider.GetComponent<PhotonView>();

            if (hiderView != null)
            {
                // GameControllerのインスタンスを取得
                GameController gameController = FindObjectOfType<GameController>();
                if (gameController != null)
                {
                    gameController.OnPlayerCaught(seekderView.ViewID, hiderView.ViewID);
                }
            }
        }

        /// <summary>
        /// オブジェクトの最親オブジェクトを取得する処理
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>最親オブジェクト</returns>
        private GameObject GetRootParent(GameObject obj)
        {
            Transform current = obj.transform;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current.gameObject;
        }

        /// <summary>
        /// 間違ったターゲットに攻撃した場合の処理
        /// </summary>
        /// <param name="target">攻撃対象のオブジェクト</param>
        private void HandleWrongAttack(GameObject target)
        {
            Debug.Log("Wrong target hit: " + target.name);
            life--;
            gameUI.UpdateLife(life);

            // 鬼側のライフが尽きた場合の処理
            if (life <= 0)
            {
                PhotonView seekerView = gameObject.GetComponent<PhotonView>();
                if (seekerView != null)
                {
                    // GameControllerのインスタンスを取得
                    GameController gameController = FindObjectOfType<GameController>();
                    if (gameController != null)
                    {
                        gameController.SeekerFailed(seekerView.ViewID);
                    }
                }
            }
        }

        /// <summary>
        /// 走るアニメーションを同期するRPC
        /// </summary>
        /// <param name="isRunning">走っているかどうかの状態</param>
        [PunRPC]
        private void RPC_SetRunningAnimation(bool isRunning)
        {
            animator.SetBool("isRunning", isRunning);
        }

        /// <summary>
        /// ジャンプアニメーションを同期するRPC
        /// </summary>
        [PunRPC]
        private void RPC_SetJumpAnimation()
        {
            animator.SetTrigger("Jump");
        }

        /// <summary>
        /// 攻撃アニメーションを同期するRPC
        /// </summary>
        [PunRPC]
        private void RPC_SetAttackAnimation()
        {
            animator.SetTrigger("Attack");
        }

        /// <summary>
        /// 地面にいるかどうかの状態を同期するRPC
        /// </summary>
        /// <param name="groundedState">地面にいるかどうかの状態</param>
        [PunRPC]
        private void RPC_UpdateGroundedState(bool groundedState)
        {
            isGrounded = groundedState;
            animator.SetBool("isGrounded", isGrounded);
        }
        #endregion
    }
}