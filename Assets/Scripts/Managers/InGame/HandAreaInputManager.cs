using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandAreaInputManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //変数宣言
    #region Variables
    [Header("連携するエリア")]
    [SerializeField] private Transform playerHandArea; // カードが実際に並んでいる親オブジェクト
    [SerializeField] private HandLayoutManager handLayoutManager;

    [Header("操作中の状態")]
    private CardView currenSelectedCard = null; //現在操作中(カーソルが合ってる)カード
    private CardView draggedCard = null;        //最初に押した(ドラッグを開始した)カード
    private bool isDragOutside = false;         //エリアの外にドラッグしたか
    private bool isCancelled = false;           //キャンセル操作中か
    private RectTransform rectTransform;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    #endregion

    //手札操作関連メソッド
    #region Operation Hand Methods
    public void OnPointerDown   //マウスが押された瞬間の処理
    (PointerEventData eventData)
    {
        draggedCard = GetCardAtPosition(eventData);
        currenSelectedCard = draggedCard;
        isDragOutside = false;
        isCancelled = false;

        //レイアウトマネージャに「このカードを選んだ」と伝える
        if (handLayoutManager != null)
        {
            handLayoutManager.SetSelectedCard(currenSelectedCard);
        }
    }

    public void OnDrag  //ドラッグ中の処理
    (PointerEventData eventData)
    {
        if (draggedCard == null) return;
        if (isDragOutside) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera;

        //HandInputAreaの四隅のワールド座標を取得
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector2 screenTopLeft = RectTransformUtility.WorldToScreenPoint(cam, corners[1]);
        //HandInputAreaの上辺のY座標を取得
        float topEdgeY = screenTopLeft.y;

        //マウスのY座標が、上辺より上にあるかチェック
        if (eventData.position.y > topEdgeY)
        {
            //プレイモードに移行
            Debug.Log("エリアの外に出ました。カードのプレイを開始します。");
            isDragOutside = true;

            if (handLayoutManager != null)
            {
                handLayoutManager.SetSelectedCard(null);
            }

            currenSelectedCard.ManuallyBeginDrag(eventData);
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, cam))
        {
            //手札選択モード
            //カーソル位置のカードを選択状態にする
            CardView cardOver = GetCardAtPosition(eventData);

            if (cardOver != currenSelectedCard)
            {
                currenSelectedCard = cardOver;

                if (handLayoutManager != null)
                {
                    handLayoutManager.SetSelectedCard(currenSelectedCard);
                }

                Debug.Log("選択中のカードが" + currenSelectedCard.cardData.cardName + "に変わりました。");
            }
        }
        else
        {
            //キャンセルモードに移行
            Debug.Log("エリアの横または下に出ました。キャンセルします。");

            //拡大表示を解除
            if (handLayoutManager != null)
            {
                handLayoutManager.SetSelectedCard(null);
            }

            isCancelled = true;
        }
    }

    public void OnPointerUp //マウスが離された瞬間の処理
    (PointerEventData eventData)
    {
        if (draggedCard == null) return;

        if (isDragOutside)
        {
            // エリア外でドロップされた場合 (カードプレイ)
            // CardView側がOnEndDragを処理するので、ここでは何もしない
        }
        else if (isCancelled)
        {
            Debug.Log("ドラッグがキャンセルされました。");
        }
        else
        {
            // エリア内でドロップされた（＝クリック扱い）
            if (currenSelectedCard != null)
            {
                Debug.Log(currenSelectedCard.cardData.cardName + " を拡大表示します。");
                ZoomUIPanelManager.Instance.Show(currenSelectedCard);
            }
        }

        if (handLayoutManager != null)
        {
            handLayoutManager.SetSelectedCard(null);
        }

        // 状態をリセット
        draggedCard = null;
        currenSelectedCard = null;
        isDragOutside = false;
        isCancelled = false;
    }
    #endregion

    //選択中カードを計算する
    #region Calculation Selecte Card Methods
    private CardView GetCardAtPosition  //クリック位置からカードを計算で割り出すメソッド
    (PointerEventData eventData)
    {
        //playerHandAreaの全ての子カードを取得
        List<CardView> cardsInHand = new List<CardView>(playerHandArea.GetComponentsInChildren<CardView>());
        if (cardsInHand.Count == 0) return null;

        //クリック位置をHandAreaのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        //ローカルX座標からパーセンテージを計算
        float percentage = (localPos.x + rectTransform.rect.width * rectTransform.pivot.x) / rectTransform.rect.width;

        //パーセンテージからインデックスを計算
        int index = Mathf.FloorToInt(percentage * cardsInHand.Count);
        index = Mathf.Clamp(index, 0, cardsInHand.Count - 1); //範囲内に収める

        Debug.Log($"クリック位置 {percentage * 100:F1}% , カードインデックス: {index}");

        return cardsInHand[index];
    }
    #endregion
}
