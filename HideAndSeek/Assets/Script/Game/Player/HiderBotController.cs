using GameData;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    /// <summary>
    /// 隠れる側のボット処理
    /// </summary>
    public class HiderBotController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>ボットの名前</summary>
        private string botName;
        /// <summary>現在の変身オブジェクト</summary>
        private GameObject currentObject;
        /// <summary>NavMeshAgentコンポーネント</summary>
        private NavMeshAgent navMeshAgent;
        /// <summary>変身するオブジェクトのリスト</summary>
        private List<GameObject> transformationObjList;
        /// <summary>移動先のリスト</summary>
        private List<Transform> targetPositionList;
        /// <summary>鬼に見えないようにするためのRendererリスト</summary>
        private List<Renderer> rendererList;
        #endregion

        #region SerializeField
        /// <summary>移動速度</summary>
        [SerializeField] private float speed = 3f;
        /// <summary>名前用をキャンバス</summary>
        [SerializeField] private Canvas nameCanvas;
        /// <summary>プレイヤー名の表示</summary>
        [SerializeField] private PlayerNameDisplay playerNameDisplay;
        #endregion

        #region UnityEvent
        void Start()
        {
            var stageData = GameDataManager.Instance().GetStageData();
            if (stageData != null)
            {
                transformationObjList = stageData.transformationObjList;
                targetPositionList = stageData.botTargetPositionList;
            }

            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.speed = speed;
            navMeshAgent.baseOffset = 0;

            // ランダムなオブジェクトに変身させる
            int randomIndex = Random.Range(0, transformationObjList.Count);
            TransformIntoObject(randomIndex);

            // 初期移動先を設定
            MoveToRandomPosition();
        }

        private void Update()
        {
            RotationCanvas();

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                navMeshAgent.isStopped = true;
            }
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="name">ボットの名前</param>
        public void Init(string name)
        {
            botName = name;
            playerNameDisplay.Init(true, botName);
        }

        /// <summary>
        /// 隠れる側のボットプレイヤーを非表示にする
        /// </summary>
        public void HideBotPlayer()
        {
            foreach (var renderer in rendererList)
            {
                if (renderer != null && renderer.gameObject != null)
                {
                    renderer.enabled = false;
                }
                else
                {
                    Debug.LogWarning("Renderer is missing or has been destroyed.");
                }
            }
        }

        /// <summary>
        /// 隠れる側のボットプレイヤーを表示する
        /// </summary>
        public void ShowBotPlayer()
        {
            foreach (var renderer in rendererList)
            {
                renderer.enabled = true;
            }
        }

        /// <summary>
        /// ボットのプレイヤー名を返す
        /// </summary>
        /// <returns>ボットの名前</returns>
        public string GetBotName()
        {
            return botName;
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
        /// プレイヤーを物に変身させる処理
        /// </summary>
        /// <param name="transformIndex">変身するオブジェクトのインデックス</param>
        private void TransformIntoObject(int transformIndex)
        {
            photonView.RPC("RPC_TransformIntoObject", RpcTarget.AllBuffered, transformIndex);

            rendererList = new List<Renderer>(GetComponentsInChildren<Renderer>());
        }

        /// <summary>
        /// RPCでプレイヤーを物に変身させる処理
        /// </summary>
        /// <param name="transformIndex">変身するオブジェクトのインデックス</param>
        [PunRPC]
        private void RPC_TransformIntoObject(int transformIndex)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (currentObject != null && currentObject != gameObject)
            {
                Destroy(currentObject);
            }

            var position = transform.position;
            var rotation = transform.rotation;
            currentObject = PhotonNetwork.Instantiate($"Prefabs/Transform/{GameDataManager.Instance().GetStageData().name}/{transformationObjList[transformIndex].name}", position, rotation);
            currentObject.transform.SetParent(this.transform);
            currentObject.transform.localPosition = Vector3.zero;
            currentObject.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// ランダムに移動させる処理
        /// </summary>
        private void MoveToRandomPosition()
        {
            if (targetPositionList.Count == 0) return;

            int randomIndex = Random.Range(0, targetPositionList.Count);
            Vector3 targetPosition = targetPositionList[randomIndex].position;
            navMeshAgent.SetDestination(targetPosition);
        }
        #endregion
    }
}