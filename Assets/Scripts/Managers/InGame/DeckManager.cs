using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    //変数宣言
    #region Variables
    [SerializeField] private GameObject m_gameObject;   //GameManagerへの参照を追加

    //デッキの原本（インスペクターで設定する用）
    public List<Card> playerDeck;
    public Card playerLeader;
    public List<Card> enemyDeck;
    public Card enemyLeader;

    //実際にゲーム中に使う山札用のリスト
    private List<Card> playerDeckPile;
    private List<Card> enemyDeckPile;

    //public List<int> nPlayerDeck = new List<int>();
    //public List<int> nEnemyDeck = new List<int>();

    public Transform playerHandArea;
    public Transform playerFieldArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Awake()
    {
        InitDeck();

        playerDeckPile = new List<Card>(playerDeck);
        enemyDeckPile = new List<Card>(enemyDeck);

        List<Card> targetDeck;

        targetDeck = playerDeckPile;
        for (int i = 0; i < targetDeck.Count; i++)
        {
            targetDeck[i].isPlayerCard = true;
        }

        targetDeck = enemyDeckPile;
        for (int i = 0;i < targetDeck.Count; i++)
        {
            targetDeck[i].isPlayerCard = false;
        }
    }
    #endregion

    public void InitDeck()
    {
        //デッキを空にする
        playerDeck.Clear();

        //セーブデータを読み込む
        LoadDeckData();
    }

    private void LoadDeckData()
    {
        string deckJson = PlayerPrefs.GetString("PlayerDeck", "");

        if (string.IsNullOrEmpty(deckJson))
        {
            Debug.LogError("セーブデータがありません！");
            return; // テスト用データを入れる処理があっても良い
        }

        string[] idStringArray = deckJson.Split(',');

        if (idStringArray.Length == 0) return;

        if (int.TryParse(idStringArray[0], out int leaderId))
        {
            playerLeader = CardDatabase.GetCardByID(leaderId);
        }

        for (int i = 1; i < idStringArray.Length; i++)
        {
            if (int.TryParse(idStringArray[i], out int id))
            {
                // CardDatabaseを使ってカードを取得
                Card card = CardDatabase.GetCardByID(id);
                if (card != null)
                {
                    playerDeck.Add(card);
                }
            }
        }
    }

    //デッキ操作に関するメソッド
    #region Operation Deck Methods
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

    public List<Card> DrawCardsTemporary(TurnOwner owner, int count)
    {
        // どちらのデッキを操作するか決める
        List<Card> targetDeck = (owner == TurnOwner.Player) ? playerDeckPile : enemyDeckPile;

        List<Card> drawnIds = new List<Card>();

        // 枚数チェック（山札が足りない場合、あるだけ引く）
        if (targetDeck.Count < count) count = targetDeck.Count;

        for (int i = 0; i < count; i++)
        {
            drawnIds.Add(targetDeck[0]);
            targetDeck.RemoveAt(0); // 山札から消す
        }

        return drawnIds; // 抜き出したIDリストを返す
    }

    public void ReturnCardsAndShuffle(TurnOwner owner, List<Card> returnIds)
    {
        List<Card> targetDeck = (owner == TurnOwner.Player) ? playerDeckPile : enemyDeckPile;

        // 残りを山札に戻す
        targetDeck.AddRange(returnIds);

        // そのデッキをシャッフル
        DeckShuffle(owner);
    }
    #endregion
}
