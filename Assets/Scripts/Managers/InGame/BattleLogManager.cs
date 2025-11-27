using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    public static BattleLogManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    #endregion

    //変数宣言
    #region Variables
    [Header("UI References")]
    [SerializeField] private Text messageText; //画面に表示するテキスト

    [Header("Settings")]
    [SerializeField] private float displayTime = 3.0f; //メッセージを表示する秒数

    private Coroutine currentCoroutine; //現在実行中の表示処理

    private bool isShowingGuide = false;

    [Header("Phase UI")]
    [SerializeField] private RectTransform phasePanelRect;
    [SerializeField] private Image phaseImage;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float stayDuration = 1.0f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 入りの動き
    [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 出の動き

    [SerializeField] private List<Sprite> phaseSprites;
    #endregion

    //ログ表示に関するメソッド
    #region Add Log Methods
    public void ShowNotification(string message)    //一時的な通知を表示する（数秒で消える）
    {
        //ガイドメッセージが表示中なら、通知で上書きしないようにする
        if (isShowingGuide) return;

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ShowMessageRoutine(message, displayTime));
    }
    public void ShowGuide(string message)   //操作ガイドを表示する（ずっと消えない）
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        //時間制限なしで表示する
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        isShowingGuide = true; //ガイド表示中フラグを立てる
    }
    public void HideGuide() //ガイドを消す（操作完了時などに呼ぶ）
    {
        isShowingGuide = false;
        messageText.text = "";
        messageText.gameObject.SetActive(false);
    }
    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);

        //ガイド表示中でなければ消す
        if (!isShowingGuide)
        {
            messageText.text = "";
        }
    }
    public void ShowPhaseAnnounce(GamePhase phaseName) //フェーズ遷移演出呼び出し
    {
        phaseImage.sprite = phaseSprites[(int)phaseName];
        //switch (phaseName)
        //{
        //    case GamePhase.Start:
        //        phaseImage.sprite = phaseSprites[0];
        //        break;
        //    case GamePhase.Main:
        //        phaseImage.sprite = phaseSprites[1];
        //        break;
        //    case GamePhase.End:
        //        phaseImage.sprite = phaseSprites[2];
        //        break;
        //}
        float screenWidth = 1300.0f;

        //初期位置設定（画面右外）
        phasePanelRect.anchoredPosition = new Vector2(screenWidth, 0);

        //右から中央へスライド (In)
        SimpleTweenManager.Instance.SlideUI(
            phasePanelRect,
            new Vector2(0, 0), // 目標：中央
            slideDuration,
            0f,                // 前の待機：なし
            stayDuration,      // 後の待機：中央で止まる時間
            slideInCurve,      // カーブ：In用
            () =>              // 完了後の処理（コールバック）
            {
                //中央から左へ戻る (Out)
                //Inが終わった後に実行される
                SimpleTweenManager.Instance.SlideUI(
                    phasePanelRect,
                    new Vector2(-screenWidth, 0), // 目標：左外
                    slideDuration,
                    0f,
                    0f,
                    slideOutCurve, // カーブ：Out用
                    null
                );
            }
        );
    }
    #endregion
}