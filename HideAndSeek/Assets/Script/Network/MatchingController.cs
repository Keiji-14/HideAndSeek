using Photon.Pun;
using UniRx;
using UnityEngine;

namespace NetWork
{
    /// <summary>
    /// オンライン対戦のマッチング管理
    /// </summary>
    public class MatchingController : MonoBehaviour
    {
        #region publicField
        /// <summary>マッチングカウント時の処理</summary>
        public Subject<float> MatchingCountSubject = new Subject<float>();
        /// <summary>マッチング完了時の処理</summary>
        public Subject<Unit> MatchingCompletedSubject = new Subject<Unit>();
        #endregion

        #region PrivateField
        /// <summary>マッチング中を計測する値</summary>
        private float matchingTimer;
        /// <summary>初期化用の値</summary>
        private const float resetCount = 0.0f;
        /// <summary>マッチング待機時間</summary>
        private const float matchingWaitTime = 60.0f;
        /// <summary>マッチング中かどうかの処理</summary>
        private bool isMatching;
        /// <summary>ゲームが開始済みかどうか</summary>
        private bool isGameStarted;
        #endregion

        #region UnityEvent
        private void Start()
        {
            isMatching = false;
            isGameStarted = false;
        }

        private void FixedUpdate()
        {
            // ゲームが開始されている場合は何もしない
            if (isGameStarted || !isMatching)
                return;

            // マッチングタイマーを更新
            MatchingTimeCount();

            // プレイヤー数を確認
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount > 0 && playerCount <= 5)
            {
                int seekerCount = 0;
                int hiderCount = 0;

                // ルーム内のプレイヤーを確認し、鬼と隠れる側の数をカウントする
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (player.CustomProperties.TryGetValue("Role", out object role))
                    {
                        if (role as string == "Seeker")
                        {
                            seekerCount++;
                        }
                        else if (role as string == "Hider")
                        {
                            hiderCount++;
                        }
                    }
                }

                // 鬼が1人以上いて、隠れる側のプレイヤー数が4未満の場合、ゲームを開始
                if (seekerCount > 0 && (matchingTimer >= matchingWaitTime || playerCount == 5))
                {
                    isGameStarted = true;
                    // ゲームシーンに移行
                    MatchingCompletedSubject.OnNext(Unit.Default);
                }
            }
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// マッチングを開始する処理
        /// </summary>
        public void MatchingStart()
        {
            isMatching = true;
            matchingTimer = resetCount;
        }

        /// <summary>
        /// マッチングを終了する処理
        /// </summary>
        public void MatchingFinish()
        {
            isMatching = false;
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチング中の時間計測の処理
        /// </summary>
        private void MatchingTimeCount()
        {
            matchingTimer += Time.deltaTime;

            MatchingCountSubject.OnNext(matchingTimer);
        }
        #endregion
    }
}