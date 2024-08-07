using GameData;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 隠れる側のプレイヤー処理
    /// </summary>
    public class HiderController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>現在の変身オブジェクト</summary>
        private GameObject currentObject;
        /// <summary>Rigidbody</summary>
        private Rigidbody rigidbody;
        /// <summary>自身のカメラ処理のコンポーネント</summary>
        private HiderCamera hiderCamera;
        /// <summary>鬼に見えないようにするためのRendererリスト</summary>
        private List<Renderer> rendererList;
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
        /// <summary>変身するオブジェクトのリスト</summary>
        [SerializeField] private List<GameObject> transformationObjList;
        [Header("Component")]
        /// <summary>名前用をキャンバス</summary>
        [SerializeField] private Canvas nameCanvas;
        /// <summary>プレイヤー名の表示</summary>
        [SerializeField] private PlayerNameDisplay playerNameDisplay;
        #endregion

        #region UnityEvent
        private void Start()
        {
            var stageData = GameDataManager.Instance().GetStageData();
            if (stageData != null)
            {
                transformationObjList = stageData.transformationObjList;
            }

            rigidbody = GetComponent<Rigidbody>();
            hiderCamera = GetComponentInChildren<HiderCamera>();
            rendererList = new List<Renderer>(GetComponentsInChildren<Renderer>());

            // ランダムなオブジェクトに変身させる
            int randomIndex = Random.Range(0, transformationObjList.Count);
            TransformIntoObject(randomIndex);

            SetCamera();

            playerNameDisplay.Init(false);
        }

        private void Update()
        {
            RotationCanvas();

            // 自分のキャラクターかどうかを確認
            if (!photonView.IsMine)
                return;

            TransformMove();
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// 隠れる側のプレイヤーを非表示にする
        /// </summary>
        public void HidePlayer()
        {
            foreach (var renderer in rendererList)
            {
                renderer.enabled = false;
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤーを表示する
        /// </summary>
        public void ShowPlayer()
        {
            foreach (var renderer in rendererList)
            {
                renderer.enabled = true;
            }
        }

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
        /// 変身オブジェクトの移動処理
        /// </summary>
        private void TransformMove()
        {
            if (rigidbody == null || (hiderCamera != null && hiderCamera.IsCameraLocked()))
                return;

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = hiderCamera.transform.right * moveX + hiderCamera.transform.forward * moveZ;

            // 水平移動用
            move.y = 0;
            rigidbody.MovePosition(rigidbody.position + move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * gravity), ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// レイキャストで地面に設置しているかどうかの判定
        /// </summary>
        private bool IsGrounded()
        {
            // キャラクターの中心より少し上からRayを飛ばす
            float rayLength = 0.2f; // Rayの長さを少し余裕を持たせる
            Vector3 origin = transform.position + Vector3.up * 0.1f; // キャラクターの位置から少し上
            return Physics.Raycast(origin, Vector3.down, rayLength);
        }

        /// <summary>
        /// プレイヤーを物に変身させる処理
        /// </summary>
        /// <param name="transformIndex">変身するオブジェクトのインデックス</param>
        private void TransformIntoObject(int transformIndex)
        {
            photonView.RPC("RPC_TransformIntoObject", RpcTarget.AllBuffered, transformIndex);
        }

        /// <summary>
        /// プレイヤーを物に変身させる処理
        /// </summary>
        /// <param name="transformIndex">変身するオブジェクトのインデックス</param>
        [PunRPC]
        private void RPC_TransformIntoObject(int transformIndex)
        {
            if (currentObject != null && currentObject != gameObject)
            {
                Destroy(currentObject);
            }

            var position = transform.position;
            var rotation = transform.rotation;
            currentObject = Instantiate(transformationObjList[transformIndex], position, rotation);
            currentObject.transform.SetParent(this.transform);
            currentObject.transform.localPosition = Vector3.zero;
            currentObject.transform.localRotation = Quaternion.identity;
        }
        #endregion
    }
}