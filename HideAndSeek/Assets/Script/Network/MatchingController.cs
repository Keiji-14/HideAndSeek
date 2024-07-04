using Photon.Pun;
using System.Collections;
using UniRx;
using UnityEngine;

namespace NetWork
{
    /// <summary>
    /// オンライン対戦のマッチング管理
    /// </summary>
    public class MatchingController : MonoBehaviour
    {
        #region PrivateField
        /// <summary>マッチング完了時の処理</summary>
        public Subject<Unit> MatchingCompletedSubject = new Subject<Unit>();
        #endregion

        #region PrivateField
        /// <summary>マッチング中かどうかの処理</summary>
        private bool isMatching = false;
        /// <summary>ゲームが開始済みかどうか</summary>
        private bool isGameStarted = false;
        #endregion

        #region UnityEvent
        private void Update()
        {
            // ゲームが開始されている場合は何もしない
            if (isGameStarted || !isMatching)
                return;

            // 4人揃ったかどうかを確認
            if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
            {
                isGameStarted = true;
                StartCoroutine(MoveGameRoom());
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

        #region PrivateMethod
        /// <summary>
        /// ゲーム画面に移動する処理
        /// </summary>
        private IEnumerator MoveGameRoom()
        {
            yield return new WaitForSeconds(2.0f);

            // ゲームシーンに移行
            PhotonNetwork.LoadLevel("GameScene");
            MatchingCompletedSubject.OnNext(Unit.Default);
        }
        #endregion
    }
}