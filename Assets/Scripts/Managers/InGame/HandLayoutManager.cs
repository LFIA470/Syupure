using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLayoutManager : MonoBehaviour
{
    //変数宣言
    #region Variables
    [Header("レイアウト設定")]
    [SerializeField]
    [Tooltip("カードの大きさ")]
    private float cardScale = 1.25f;

    [SerializeField]
    [Tooltip("選択中のカードをどれだけ大きくするか")]
    private float selectedCardScale = 1.25f;

    [SerializeField]
    [Tooltip("カード同士が重なる角度（大きいほど広がる）")]
    private float cardSpacing = 10f;

    [SerializeField]
    [Tooltip("扇形の半径（大きいほど緩やかなカーブ）")]
    private float curveRadius = 800f;

    [SerializeField]
    [Tooltip("カードがY軸（縦）にどれだけ上がるか")]
    private float cardElevation = 10f;

    [SerializeField]
    [Tooltip("手札が広がれる最大の角度")]
    private float maxCurveAngle = 90f;

    [SerializeField]
    [Tooltip("当たり判定エリアのRectTransform")]
    private RectTransform handInputAreaRect;

    [SerializeField]
    [Tooltip("カード1枚のおおよその横幅（当たり判定の調整用）")]
    private float cardWidth = 100f;

    //現在選択されているカード
    private CardView currenlySelectedCard = null;

    // カードのRectTransformを保持するリスト
    private List<RectTransform> cardRects = new List<RectTransform>();
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Update()
    {
        // 毎フレーム、カードのレイアウトを更新する
        UpdateLayout();
    }
    #endregion

    //Set,Getメソッド
    #region Set Get Methods
    public void SetSelectedCard(CardView card)  //現在選択中のカード
    {
        currenlySelectedCard = card;
    }
    #endregion

    //レイアウト計算メソッド
    #region Calculation Layout Methods
    private void UpdateLayout() //レイアウトを再計算して配置する
    {
        // 子オブジェクト（カード）のリストを更新
        cardRects.Clear();
        foreach (Transform child in transform)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
            {
                cardRects.Add(rect);
            }
        }

        int childCount = cardRects.Count;
        if (childCount == 0)
        {
            //カードが0枚になったら、当たり判定エリアの幅も0にする
            if (handInputAreaRect != null)
            {
                handInputAreaRect.sizeDelta = new Vector2(0, handInputAreaRect.sizeDelta.y);
            }
            return;
        }

        // --- ここからが扇形レイアウトの計算 ---

        //手札全体の広がり角度を計算
        //(カード枚数 * 間隔) が、最大角度を超えないようにする
        float totalAngle = Mathf.Min(childCount * cardSpacing, maxCurveAngle);

        //扇の中心が0度になるよう、開始角度を計算
        float startAngle = -totalAngle / 2f;

        //各カードを配置
        for (int i = 0; i < childCount; i++)
        {
            RectTransform card = cardRects[i];
            CardView cardView = card.GetComponent<CardView>();

            //このカードの中心角度を計算
            //(カードが1枚なら0度、3枚なら -10度, 0度, 10度 のようになる)
            float targetAngle = startAngle + i * cardSpacing + (cardSpacing / 2f);

            //角度から、円弧上の位置(X, Y)を計算 (三角関数)
            float x = Mathf.Sin(targetAngle * Mathf.Deg2Rad) * curveRadius;
            float y = (1f - Mathf.Cos(targetAngle * Mathf.Deg2Rad)) * -curveRadius;

            //カードを少し持ち上げる (Y座標を足す)
            y += i * cardElevation;

            //計算した位置と回転を適用
            card.anchoredPosition = new Vector2(x, y);
            card.localRotation = Quaternion.Euler(0, 0, -targetAngle);

            if (cardView != null && cardView == currenlySelectedCard)
            {
                //もし、このカードが現在選択中のカードなら
                card.localScale = Vector3.one * cardScale * selectedCardScale;
                card.localPosition += new Vector3(0, 60, 0);
            }
            else
            {
                //それ以外のカードなら、通常のスケールに戻す
                card.localScale = Vector3.one * cardScale;
            }

                //重なり順を調整（真ん中のカードが一番手前になるように）
                //（ここでは単純に、リストの順番で奥から手前に並べています）
                card.SetSiblingIndex(i);
        }

        //当たり判定エリアの横幅を調整
        if (handInputAreaRect != null)
        {
            // 1枚の時
            if (childCount == 1)
            {
                handInputAreaRect.sizeDelta = new Vector2(cardWidth, handInputAreaRect.sizeDelta.y);
            }
            // 2枚以上ある時
            else
            {
                // 一番左のカードの中心角度と、一番右のカードの中心角度を計算
                float firstCardAngle = startAngle + (cardSpacing / 2f);
                float lastCardAngle = startAngle + (childCount - 1) * cardSpacing + (cardSpacing / 2f);

                // その角度から、一番左のカードのX座標と、一番右のカードのX座標を計算
                float x_left = Mathf.Sin(firstCardAngle * Mathf.Deg2Rad) * curveRadius;
                float x_right = Mathf.Sin(lastCardAngle * Mathf.Deg2Rad) * curveRadius;

                // 全体の幅 ＝ (右端のX座標 - 左端のX座標) + カード1枚分の幅
                float totalVisualWidth = (x_right - x_left) + cardWidth;

                // HandInputAreaの横幅を、計算した全体の幅に設定
                handInputAreaRect.sizeDelta = new Vector2(totalVisualWidth, handInputAreaRect.sizeDelta.y);
            }
        }
    }
    #endregion
}