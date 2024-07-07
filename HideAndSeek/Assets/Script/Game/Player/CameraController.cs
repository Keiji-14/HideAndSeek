using UnityEngine;


    /// <summary>
    /// プレイヤーに追従するカメラの処理
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region PrivateField
        /// <summary>対象の高さ</summary>
        private float targetHeight = 2.0f;

        private float currentDistance; // 現在のカメラ距離
        private float desiredDistance; // 目標とするカメラ距離
        private float correctedDistance; // 矯正後のカメラ距離

        private float CameraFollowDelay; // カメラ回転後のカメラフォローまでの遅延
        #endregion

        #region SerializeField
        /// <summary>ターゲット</summary>
        [SerializeField] private Transform target;
        /// <summary></summary>
        [SerializeField] float distance = 5.0f;
        /// <summary>最大カメラ距離</summary>
        [SerializeField] private float horizontalAngle = 0.0f;
        /// <summary>最小カメラ距離</summary>
        [SerializeField] private float verticalAngle = 10.0f;

        // カメラの移動限界
        /// <summary>見上げ限界角度</summary>
        [SerializeField] private float verticalAngleMinLimit = -30f;
        /// <summary>見下ろし限界角度 </summary>
        [SerializeField] private float verticalAngleMaxLimit = 80f;
        /// <summary>最大ズーム距離 </summary>
        [SerializeField] private float maxDistance = 20f; 
        /// <summary>最小ズーム距離 </summary>
        [SerializeField] private float minDistance = 0.6f;
        /// <summary>画面の横幅分カーソルを移動させたとき何度回転するか</summary>
        [SerializeField] private float rotationSpeed = 180.0f;
        /// <summary>回転の減衰速度 (higher = faster) </summary>
        [SerializeField] private float rotationDampening = 0.5f;
        /// <summary>Auto Zoom speed (Higher = faster) </summary>
        [SerializeField] private float zoomDampening = 5.0f;

        // 衝突検知用
        /// <summary>What the camera will collide with</summary>
        [SerializeField] private LayerMask collisionLayers = -1;
        /// <summary>衝突する物体からカメラを遠ざけるときのオフセット </summary>
        [SerializeField] private float offsetFromWall = 0.1f;

        /// <summary>マウスでカメラのコントロールを許可するかどうか</summary>
        [SerializeField] private bool allowMouseInput = true;
        #endregion

        #region UnityEvent
        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            horizontalAngle = angles.x;
            verticalAngle = angles.y;

            currentDistance = distance;
            desiredDistance = distance;
            correctedDistance = distance;

            CameraFollowDelay = 0f;
        }

        void LateUpdate()
        {
            // ターゲットが定義されていない場合は何もしない
            if (target == null)
                return;

            Vector3 vTargetOffset; // ターゲットからのオフセット

            if (GUIUtility.hotControl == 0)
            {
                // マウス入力が許可されているかどうかを確認する
                if (allowMouseInput)
                {
                    horizontalAngle += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
                    verticalAngle -= Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;
                    CameraFollowDelay = 1.0f;
                }

                if (CameraFollowDelay > 0f)
                {
                    CameraFollowDelay -= Time.deltaTime;
                }
                else
                {
                    // マウスによる回転が無効の場合、カメラ視線をターゲットの視線にじわじわあわせる
                    RotateBehindTarget();
                }

                verticalAngle = ClampAngle(verticalAngle, verticalAngleMinLimit, verticalAngleMaxLimit);

                // カメラの向きを設定
                Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

                // 希望のカメラ位置を計算
                vTargetOffset = new Vector3(0, -targetHeight, 0);
                Vector3 position = target.transform.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

                // 高さを使ってユーザーが設定した真のターゲットの希望の登録点を使って衝突をチェック
                RaycastHit collisionHit;
                Vector3 trueTargetPosition = new Vector3(target.transform.position.x,
                    target.transform.position.y + targetHeight, target.transform.position.z);

                // 衝突があった場合は、カメラ位置を補正し、補正後の距離を計算
                var isCorrected = false;
                if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers))
                {
                    // 元の推定位置から衝突位置までの距離を計算し、衝突した物体から安全な「オフセット」距離を差し引く
                    // このオフセットは、カメラがヒットした面の真上にいないよう逃がす距離
                    correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                    isCorrected = true;
                }

                // スムージングのために、距離が補正されていないか、または補正された距離が現在の距離より
                // も大きい場合にのみ、距離を返す。
                currentDistance = !isCorrected || correctedDistance > currentDistance
                    ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening)
                    : correctedDistance;

                // 限界を超えないようにする
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

                // 新しい currentDistance に基づいて位置を再計算する。
                position = target.transform.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

                // 最後にカメラの回転と位置を設定。
                transform.rotation = rotation;
                transform.position = position;

            }

        }
        #endregion

        #region PrivateMethod
        // カメラを背後にまわす。
        private void RotateBehindTarget()
        {
            float targetRotationAngle = target.transform.eulerAngles.y;
            float currentRotationAngle = transform.eulerAngles.y;
            horizontalAngle = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
        }

        // 角度クリッピング
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
        #endregion
    }
