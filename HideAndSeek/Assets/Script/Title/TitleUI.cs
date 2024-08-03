using Audio;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    /// <summary>
    /// タイトル画面のUI
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        #region PrivateField
        /// <summary>役割選択後の処理</summary>
        public Subject<string> SelectedRoleSubject = new Subject<string>();
        /// <summary>マッチングキャンセル時の処理</summary>
        public Subject<Unit> MatchingCancelSubject = new Subject<Unit>();
        #endregion

        #region PrivateField
        /// <summary>鬼側のボタンを押した時の処理 </summary>
        private IObservable<Unit> InputSeekerBtnObservable =>
             seekerBtn.OnClickAsObservable();
        /// <summary>鬼側のボタンを押した時の処理 </summary>
        private IObservable<Unit> InputHiderBtnObservable =>
             hiderBtn.OnClickAsObservable();
        /// <summary>タイトル画面に戻るボタンを押した時の処理 </summary>
        private IObservable<Unit> InputTitleBackBtnObservable =>
             titleBackBtn.OnClickAsObservable();
        /// <summary>マッチングキャンセルボタンを押した時の処理 </summary>
        private IObservable<Unit> InputMatchingCancelBtnObservable =>
             matchingCancelBtn.OnClickAsObservable();
        #endregion

        #region SerializeField
        [Header("Window Object")]
        /// <summary>タイトル画面</summary>
        [SerializeField] private GameObject titleWindow;
        /// <summary>マッチング画面</summary>
        [SerializeField] private GameObject matchWindow;
        /// <summary>マッチング中の画面</summary>
        [SerializeField] private GameObject matchingWindow;
        [Header("Matching Object")]
        /// <summary>マッチング中の経過時間UI</summary>
        [SerializeField] private GameObject timeCountUIObj;
        /// <summary>マッチング中のテキスト</summary>
        [SerializeField] private GameObject matchingUIObj;
        /// <summary>マッチング完了UI</summary>
        [SerializeField] private GameObject matchedUIObj;
        /// <summary>マッチングロードUI</summary>
        [SerializeField] private GameObject matchingLoadingUI;
        /// <summary>鬼側のUI</summary>
        [SerializeField] private GameObject seekerUIObj;
        /// <summary>隠れる側のUI</summary>
        [SerializeField] private GameObject HiderUIObj;
        [Header("Button")]
        /// <summary>鬼側の選択ボタン</summary>
        [SerializeField] private Button seekerBtn;
        /// <summary>隠れる側の選択ボタン</summary>
        [SerializeField] private Button hiderBtn;
        /// <summary>タイトル画面に戻るボタン</summary>
        [SerializeField] private Button titleBackBtn;
        /// <summary>マッチングキャンセルボタン</summary>
        [SerializeField] private Button matchingCancelBtn;
        [Header("Text")]
        /// <summary>マッチング中のテキスト</summary>
        [SerializeField] private Text matchingText;
        /// <summary>マッチング中の経過時間テキスト</summary>
        [SerializeField] private Text timeCountText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            titleWindow.SetActive(true);
            matchWindow.SetActive(false);
            matchingWindow.SetActive(false);

            InputSeekerBtnObservable.Subscribe(_ =>
            {
                SwicthMatchingWindow(true);
                ViewRoleImageUI(true);
                SE.instance.Play(SE.SEName.ButtonSE);
                SelectedRoleSubject.OnNext("Seeker");
            }).AddTo(this);

            InputHiderBtnObservable.Subscribe(_ =>
            {
                SwicthMatchingWindow(true);
                ViewRoleImageUI(false);
                SE.instance.Play(SE.SEName.ButtonSE);
                SelectedRoleSubject.OnNext("Hider");
            }).AddTo(this);

            InputTitleBackBtnObservable.Subscribe(_ =>
            {
                SE.instance.Play(SE.SEName.ButtonSE);
                SwicthMatchWindow(false);
            }).AddTo(this);

            InputMatchingCancelBtnObservable.Subscribe(_ =>
            {
                SE.instance.Play(SE.SEName.ButtonSE);
                SwicthMatchingWindow(false);
                MatchingCancelSubject.OnNext(Unit.Default);
            }).AddTo(this);
        }

        /// <summary>
        /// マッチング中のUI表示切り替えの処理
        /// </summary>
        /// <param name="isView">表示判定</param>
        public void ViewMatchingUI(bool isView)
        {
            timeCountUIObj.SetActive(isView);
            matchingLoadingUI.SetActive(isView);

            if (isView)
            {
                matchingUIObj.SetActive(true);
                StartCoroutine(UpdateMatchingText());
            }
            else
            {
                StopCoroutine(UpdateMatchingText());
            }
        }

        /// <summary>
        /// 役割選択画面の表示を切り替える処理
        /// </summary>
        /// <param name="isView">表示判定</param>
        public void SwicthMatchWindow(bool isView)
        {
            titleWindow.SetActive(!isView);
            matchWindow.SetActive(isView);
        }

        /// <summary>
        /// マッチング中の経過時間の表示処理
        /// </summary>
        /// <param name="minutes">マッチング中の分を計測する値</param>
        /// <param name="seconds">マッチング中の秒を計測する値</param>
        public void MatchingTimeUI(float minutes, float seconds)
        {
            timeCountText.text = $"{minutes:00}:{seconds:00}";
        }

        // <summary>
        /// マッチング完了UIの表示処理
        /// </summary>
        public void MatchingCompletedUI()
        {
            matchedUIObj.SetActive(true);
            matchingUIObj.SetActive(false);
            timeCountUIObj.SetActive(false);

            // マッチング完了後にキャンセルボタンを無効化
            matchingCancelBtn.interactable = false;
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチング中画面の表示を切り替える処理
        /// </summary>
        /// <param name="isView">表示判定</param>
        private void SwicthMatchingWindow(bool isView)
        {
            matchWindow.SetActive(!isView);
            matchingWindow.SetActive(isView);
        }

        /// <summary>
        /// 選択した役割の表示を切り替える処理
        /// </summary>
        /// <param name="isSeeker">鬼かどうかの判定</param>
        private void ViewRoleImageUI(bool isSeeker)
        {
            seekerUIObj.SetActive(isSeeker);
            HiderUIObj.SetActive(!isSeeker);
        }

        /// <summary>
        /// マッチング中のテキストを動的に更新するコルーチン
        /// </summary>
        private IEnumerator UpdateMatchingText()
        {
            string baseText = "マッチング中";
            int dotCount = 0;
            while (true)
            {
                matchingText.text = baseText + new string('.', dotCount);
                dotCount = (dotCount + 1) % 4;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion
    }
}