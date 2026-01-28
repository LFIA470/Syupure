using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandAreaInputManager1 : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
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

    [Header("調整パラメータ")]
    [SerializeField] private float cardSpacing = 100f; // カード同士の間隔（見た目に合わせて調整）
    [SerializeField] private float xOffset = 0f;       // 左端の微調整用
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
        List<CardView> cardsInHand = new List<CardView>(playerHandArea.GetComponentsInChildren<CardView>());
        if (cardsInHand.Count == 0) return null;

        // 1. クリック位置をローカル座標に変換（ここはそのまま）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        // 2. パネルの中心(0,0)基準ではなく、左端を0とする座標系に直す
        // （rect.width / 2 を足すことで、左端が 0 になる）
        float pointX = localPos.x + (rectTransform.rect.width / 2);

        // 3. 補正（パネル内の左端の余白などを調整）
        pointX -= xOffset;

        // 4. 「間隔」で割ってインデックスを出す！
        // 例：マウスが350の位置で、間隔が100なら、3.5 → インデックス3（4枚目）
        int index = Mathf.FloorToInt(pointX / cardSpacing);

        // 5. 範囲外エラーを防ぐ（重要）
        // 左端より左をクリックしたら 0、右端より右なら「最後のカード」にする
        index = Mathf.Clamp(index, 0, cardsInHand.Count - 1);

        Debug.Log($"計算X: {pointX}, 間隔: {cardSpacing} => インデックス: {index}");

        return cardsInHand[index];
    }
    #endregion
}
