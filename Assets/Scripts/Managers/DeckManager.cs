using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private GameObject m_gameObject;   //GameManagerへの参照を追加

    //デッキの原本（インスペクターで設定する用）
    public List<Card> playerDeck;
    public Card playerLeader;
    public List<Card> enemyDeck;
    public Card enemyLeader;

    //実際にゲーム中に使う山札用のリスト
    private List<Card> playerDeckPile;
    private List<Card> enemyDeckPile;

    public Transform playerHandArea;
    public Transform playerFieldArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;

    void Awake()
    {
        playerDeckPile = new List<Card>(playerDeck);
        enemyDeckPile = new List<Card>(enemyDeck);
    }

    //デッキシャッフル
    public void DeckShuffle(TurnOwner owner)
    {
        //誰がシャッフルするかに応じて、使う山札を決める
        List<Card> targetDeck;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
        }
        else
        {
            targetDeck = enemyDeckPile;
        }

        //シャッフル
        //リストの後ろから順番に、ランダムな位置のカードと交換していく
        for (int i = targetDeck.Count - 1; i > 0; i--)
        {
            //ランダムなインデックスを選ぶ
            int j = Random.Range(0 , i + 1);

            //選んだカードと現在のカードを入れ替える
            Card temp = targetDeck[i];
            targetDeck[i] = targetDeck[j];
            targetDeck[j] = temp;
        }

        Debug.Log(owner + "の山札をシャッフルしました。");
    }

    //山札の一番上から一枚引く
    public void DrawCard(TurnOwner owner)
    {
        //誰が引くかに応じて、使う山札と手札を決定する
        List<Card> targetDeck;
        Transform targetHandArea;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
            targetHandArea = playerHandArea;
        }
        else
        {
            targetDeck = enemyDeckPile;
            targetHandArea = enemyHandArea;
        }

        //山札の枚数をチェック
        if (targetDeck.Count == 0)
        {
            Debug.Log(owner + "の山札がありません。");
            return;
        }

        //山札の一番上から１枚取り出す
        Card cardData = targetDeck[0];
        targetDeck.RemoveAt(0);

        //選んだカードを手札を生成して表示する
        GameObject cardObj = Instantiate(cardPrefab, targetHandArea);
        CardView view = cardObj.GetComponent<CardView>();
        view.SetCard(cardData);

        Debug.Log(owner + "が" + cardData.cardName + "を引きました。");
    }

    //山札からランダムに一枚引く
    public void RandomDrawCard(TurnOwner owner)
    {
        //誰が引くかに応じて、使う山札と手札を決定する
        List<Card> targetDeck;
        Transform targetHandArea;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
            targetHandArea = playerHandArea;
        }
        else
        {
            targetDeck = enemyDeckPile;
            targetHandArea = enemyHandArea;
        }

        //山札の枚数をチェック
        if (targetDeck.Count == 0)
        {
            Debug.Log(owner + "の山札がありません。");
            return;
        }

        //山札からランダムに１枚選んで取り出す
        int index = Random.Range(0, targetDeck.Count);
        Card cardData = targetDeck[index];
        targetDeck.RemoveAt(index);

        //選んだカードを手札を生成して表示する
        GameObject cardObj = Instantiate(cardPrefab, targetHandArea);
        CardView view = cardObj.GetComponent<CardView>();
        view.SetCard(cardData);

        Debug.Log(owner + "が" + cardData.cardName + "を引きました。");
    }
}
