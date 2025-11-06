using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandAreaInputManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
{
    //変数宣言
    #region Variables
    [Header("連携するエリア")]
    [SerializeField] private Transform playerHandArea; // カードが実際に並んでいる親オブジェクト

    [Header("操作中の状態")]
    private CardView currenSelectedCard = null; //現在操作中(カーソルが合ってる)カード
    private CardView draggedCard = null;        //最初に押した(ドラッグを開始した)カード
    private bool isDragOutside = false;         //エリアの外にドラッグしたか
    private RectTransform rectTransform;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    #endregion

    #region Operation Hand Methods
    public void OnPointerDown   //マウスが押された瞬間の処理
    (PointerEventData eventData)
    {
        draggedCard = GetCardAtPosition(eventData);
        currenSelectedCard = draggedCard;
        isDragOutside = false;

        //将来エフェクトをかける
    }

    public void OnDrag  //ドラッグ中の処理
    (PointerEventData eventData)
    {
        if (currenSelectedCard == null || isDragOutside) return;
        
        //マウスカーソルがHandAreaの範囲内にまだいるかチェック
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
        {
            // ▼あなたの理想: 縦の位置がHandAreaを越えた場合▼
            Debug.Log("エリアの外に出ました。カードのプレイを開始します。");
            isDragOutside = true;

            //将来エフェクトを解除

            // CardViewに「ドラッグ操作を引き継いで！」と命令する
            //currenSelectedCard.ManuallyBeginDrag(eventData);
        }
        else
        {
            // エリア内でのドラッグ
            //現在カーソルがどのカードの上にあるか、リアルタイムで計算
            CardView cardOver = GetCardAtPosition(eventData);

            if (cardOver != currenSelectedCard)
            {
                //将来エフェクトを切り替える


                currenSelectedCard = cardOver;

                Debug.Log("選択中のカードが" + currenSelectedCard.cardData.cardName + "に変わりました。");
            }
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
        else
        {
            // エリア内でドロップされた（＝クリック扱い）
            if (currenSelectedCard != null)
            {
                Debug.Log(currenSelectedCard.cardData.cardName + " を拡大表示します。");
                ZoomUIPanelManager.Instance.Show(currenSelectedCard);
            }
        }

        //将来エフェクトを解除

        // 状態をリセット
        draggedCard = null;
        currenSelectedCard = null;
        isDragOutside = false;
    }

    public void OnPointerExit   //ドラッグ中にポインタがエリアから出た場合の処理
    (PointerEventData eventData)
    {
        // OnDragで処理しているので、基本的には不要だが、
        // 念のため、ドラッグ中にエリア外に出たらプレイモードとみなす
        if (currenSelectedCard != null && eventData.dragging && !isDragOutside)
        {
            isDragOutside = true;
            currenSelectedCard.ManuallyBeginDrag(eventData);
            ZoomUIPanelManager.Instance.Hide();
        }
    }
    #endregion

    //選択中カードを計算する
    #region Calculation Selecte Card Methods
    private CardView GetCardAtPosition  //クリック位置からカードを計算で割り出すメソッド
    (PointerEventData eventData)
    {
        // 1. playerHandAreaの全ての子カードを取得
        List<CardView> cardsInHand = new List<CardView>(playerHandArea.GetComponentsInChildren<CardView>());
        if (cardsInHand.Count == 0) return null;

        // 2. クリック位置をHandAreaのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera, // Camera (Overlay CanvasならnullでOK)
            out Vector2 localPos
        );

        // 3. ローカルX座標からパーセンテージを計算
        float percentage = (localPos.x + rectTransform.rect.width * rectTransform.pivot.x) / rectTransform.rect.width;

        // 4. パーセンテージからインデックスを計算
        int index = Mathf.FloorToInt(percentage * cardsInHand.Count);
        index = Mathf.Clamp(index, 0, cardsInHand.Count - 1); // 範囲内に収める

        Debug.Log($"クリック位置 {percentage * 100:F1}% , カードインデックス: {index}");

        return cardsInHand[index];
    }
    #endregion
}
