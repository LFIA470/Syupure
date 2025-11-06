using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderManager : MonoBehaviour
{
    //変数宣言
    #region Variables
    [SerializeField] private DeckManager _deckManager;

    [SerializeField] private Transform playerLeaderArea;
    [SerializeField] private Transform enemyLeaderArea;
    [SerializeField] private GameObject cardPrefab;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    private void Start()
    {
        DrawLeader();
    }
    #endregion

    //リーダーカードの表示に関するメソッド
    #region Draw Methods
    public void DrawLeader()    //リーダー表示
    {
        //リーダーカードを取得
        Card playerLeader = _deckManager.playerLeader;
        Card enemyLeader = _deckManager.enemyLeader;

        CreateLeaderCard(playerLeader, playerLeaderArea);
        CreateLeaderCard(enemyLeader, enemyLeaderArea);
    }
    #endregion

    //リーダーカードの生成に関するメソッド
    #region Create Methods
    private void CreateLeaderCard(Card card, Transform parentArea)  //リーダーカード生成
    {
        //カードを生成
        GameObject cardObj = Instantiate(cardPrefab, parentArea);   //カードのプレハブをインスタンス化

        //カードサイズ変更
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(160, 216); // 適切なサイズに調整

        CardView view = cardObj.GetComponent<CardView>();   //CardViewコンポーネント追加
        view.SetCard(card); //カード情報をUIに反映
    }
    #endregion
}
