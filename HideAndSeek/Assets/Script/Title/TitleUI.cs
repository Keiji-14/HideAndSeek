using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Title
{
    /// <summary>
    /// タイトル画面のUI
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        #region PrivateField
        /// <summary>マッチングボタンを押した時の処理</summary>
        public Subject<Unit> InputMatchingSubject = new Subject<Unit>();
        /// <summary>マッチング取り消しの処理</summary>
        public Subject<Unit> MatchingCancellSubject = new Subject<Unit>();
        #endregion

        #region PrivateField
        /// <summary>マッチングボタンを押した時の処理</summary>
        private IObservable<Unit> InputMatchingBtnObservable =>
            matchingBtn.OnClickAsObservable();
        /// <summary>マッチングウィンドウを閉じるボタンを押した時の処理</summary>
        private IObservable<Unit> InputCloseMatchingWindowBtnObservable =>
            closeMatchingWindowBtn.OnClickAsObservable();
        #endregion

        #region SerializeField
        /// <summary>マッチングボタン</summary>
        [SerializeField] private Button matchingBtn;
        /// <summary>マッチングウィンドウを閉じるボタン</summary>
        [SerializeField] private Button closeMatchingWindowBtn;
        /// <summary>マッチングウィンドウ</summary>
        [SerializeField] private GameObject matchingWindow;
        /// <summary>マッチングロードUI</summary>
        [SerializeField] private GameObject matchingLoadingUI;
        /// <summary>マッチング開始のテキストUI</summary>
        [SerializeField] private GameObject matchingStartTextUI;
        /// <summary>マッチング中のテキストUI</summary>
        [SerializeField] private GameObject matchingNowTextUI;
        /// <summary>マッチング完了のテキストUI</summary>
        [SerializeField] private GameObject matchedTextUI;
        /// <summary>マッチングしなかった時のテキストUI</summary>
        [SerializeField] private GameObject noMatchedTextUI;
        /// <summary>マッチング中の経過時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI timeCountText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // マッチングボタンを押した時の処理
            InputMatchingBtnObservable.Subscribe(_ =>
            {
                InputMatchingSubject.OnNext(Unit.Default);
            }).AddTo(this);

            // マッチングウィンドウの閉じるボタンを押した時の処理
            InputCloseMatchingWindowBtnObservable.Subscribe(_ =>
            {
                SwicthMatchingWindow(false);
                MatchingCancellSubject.OnNext(Unit.Default);
            }).AddTo(this);

            NoMatchingUI();
        }

        /// <summary>
        /// マッチングウィンドウの表示を切り替える処理
        /// </summary>
        public void SwicthMatchingWindow(bool isView = false)
        {
            matchingWindow.SetActive(isView);
        }

        /// <summary>
        /// マッチング中の表示を切り替える処理
        /// </summary>
        public void SwicthMatchingUI(bool isView = false)
        {
            matchingLoadingUI.SetActive(isView);
            matchingStartTextUI.SetActive(!isView);
            matchingNowTextUI.SetActive(isView);

            NoMatchingUI();
        }

        /// <summary>
        /// マッチング完了時の表示に切り替える処理
        /// </summary>
        public void SwicthMatchedUI()
        {
            // マッチング完了時にボタンを無効化にする
            matchingBtn.interactable = false;

            matchingLoadingUI.SetActive(false);
            matchingNowTextUI.SetActive(false);
            matchedTextUI.SetActive(true);
        }

        /// <summary>
        /// マッチング中の経過時間の表示処理
        /// </summary>
        /// <param name="minuteCount">マッチング中の分を計測する値</param>
        /// <param name="secondCount">マッチング中の秒を計測する値</param>
        public void MatchingTimeUI(float minuteCount, float secondCount)
        {
            timeCountText.text = minuteCount.ToString("00") + ":" + secondCount.ToString("00");
        }

        /// <summary>
        /// 対戦相手が見つからなかった時の処理
        /// </summary>
        public void NoMatchingUI(bool isView = false)
        {
            noMatchedTextUI.SetActive(isView);
        }
        #endregion
    }
}