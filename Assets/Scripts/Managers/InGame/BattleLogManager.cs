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
    #endregion
}