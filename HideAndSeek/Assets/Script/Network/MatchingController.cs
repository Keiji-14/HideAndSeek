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
        /// <summary>マッチング完了時の処理</summary>
        public Subject<Unit> MatchingCompletedSubject = new Subject<Unit>();
        #endregion

        #region PrivateField
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

        private void Update()
        {
            // ゲームが開始されている場合は何もしない
            if (isGameStarted || !isMatching)
                return;

            // 3人揃ったかどうかを確認
            if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
            {
                Debug.Log($"PhotonNetwork.CurrentRoom.PlayerCount : {PhotonNetwork.CurrentRoom.PlayerCount}");

                isGameStarted = true;
                // ゲームシーンに移行
                MatchingCompletedSubject.OnNext(Unit.Default);
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
        }

        /// <summary>
        /// マッチングを終了する処理
        /// </summary>
        public void MatchingFinish()
        {
            isMatching = false;
        }
        #endregion
    }
}