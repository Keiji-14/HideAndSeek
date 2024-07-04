using NetWork;
using Scene;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace Title
{
    /// <summary>
    /// タイトル画面の処理管理
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        #region PrivateField
        /// <summary>マッチング中の分を計測する値</summary>
        private float minuteCount;
        /// <summary>マッチング中の秒を計測する値</summary>
        private float secondCount;
        /// <summary>初期化用の値</summary>
        private const float resetCount = 0.0f;
        /// <summary>マッチング完了時の待機時間</summary>
        private const float matchedWaitSeconds = 2.0f;
        /// <summary>マッチング中かどうかの処理</summary>
        private bool isMatching = false;
        /// <summary>マッチング開始ボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputMatchingObservable =>
            matchingBtn.OnClickAsObservable();
        #endregion

        #region SerializeField
        /// <summary>マッチング開始ボタン</summary>
        [SerializeField] private Button matchingBtn;
        /// <summary>初回起動時の処理</summary>
        [SerializeField] private FirstStartup firstStartup;
        /// <summary>タイトル画面のUI</summary>
        [SerializeField] private TitleUI titleUI;
        #endregion

        #region UnityEvent
        public void FixedUpdate()
        {
            if (!isMatching)
                return;

            MatchingTimeCount();
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // 初回起動かどうかの判定
            if (!PlayerPrefs.HasKey("FirstTime"))
            {
                firstStartup.Init();
            }
            else
            {
                firstStartup.AlreadyStartUp();
            }

            InputMatchingObservable.Subscribe(_ =>
            {
                NetworkManager.instance.ConnectUsingSettings();
            }).AddTo(this);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチング中の時間計測の処理
        /// </summary>
        private void MatchingTimeCount()
        {
            /*secondCount += Time.deltaTime;

            if (secondCount >= 59.5f)
            {
                minuteCount++;
                secondCount -= 59.5f;
            }

            titleUI.MatchingTimeUI(minuteCount, secondCount);

            // 3分経過した場合は対戦相手が見つからなかったと表示する
            if (minuteCount >= 3.0f)
            {
                IsMatching(false);
                titleUI.NoMatchingUI(true);
            }*/
        }

        /// <summary>
        /// マッチング中かどうかの処理
        /// </summary>
        private void IsMatching(bool isMatchingStart)
        {
            /*isMatching = isMatchingStart;

            titleUI.SwicthMatchingUI(isMatching);

            if (isMatching)
            {
                NetworkManager.instance.ConnectUsingSettings();
                minuteCount = resetCount;
                secondCount = resetCount;
            }
            else
            {
                NetworkManager.instance.LeaveRoom();
            }*/
        }
        #endregion
    }
}