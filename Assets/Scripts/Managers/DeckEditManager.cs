using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class DeckEditManager : MonoBehaviour
{
    [Header("Data")]
    //ゲームに登場する全てのカードデータ
    private List<Card> allCardsList;

    //編集中のデッキ(カードデータのリスト)
    private List<Card> currentDeck = new List<Card>();

    [Header("Rules")]
    public int maxDeckSize = GameConstants.MaxDeckSize;
    public int maxSameCardCount = 4;

    [Header("UI References")]
    public Transform libraryContent;
    public Transform deckContent;
    public GameObject cardPrefab;
    public Text deckCountText;
    public float cardScale = 0.6f;

    public GameObject alertPanel;
    public Text alertMessageText;

    private const string SAVE_KEY = "PlayerDeck";

    void Start()
    {
        allCardsList = CardDatabase.GetAllCards();

        alertPanel.SetActive(false);

        //保存されているデッキを読み込む
        LoadDeck();

        //UIを表示する
        RefreshLibraryUI();
        RefreshDeckUI();
    }

    //全カード一覧の表示
    void RefreshLibraryUI()
    {
        //一旦クリア
        foreach (Transform child in libraryContent) Destroy(child.gameObject);

        //カードID順に並べ替えて表示
        var sortedList = allCardsList.OrderBy(c => c.cardID).ToList();

        foreach (Card card in sortedList)
        {
            GameObject obj = Instantiate(cardPrefab, libraryContent);

            obj.transform.localPosition = Vector3.zero;

            obj.transform.localScale = new Vector3(cardScale, cardScale, 1.0f);

            CardView view = obj.GetComponent<CardView>();
            view.SetCard(card);

            view.isDeckEditMode = true;

            view.isZoomPanel = true;

            //クリックしたらデッキに追加
            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnLibraryCardClicked(card));
        }
    }

    //デッキの表示
    void RefreshDeckUI()
    {
        foreach (Transform child in deckContent) Destroy(child.gameObject);

        //カードID順に並べ替えて表示
        var sortedDeck = currentDeck.OrderBy(c => c.cardID).ToList();

        foreach (Card card in sortedDeck)
        {
            GameObject obj = Instantiate(cardPrefab, deckContent);

            obj.transform.localPosition = Vector3.zero;

            obj.transform.localScale = new Vector3(cardScale * 0.5f, cardScale * 0.5f, 1.0f);

            CardView view = obj.GetComponent<CardView>();
            view.SetCard(card);

            view.isDeckEditMode = true;

            //クリックしたらデッキから削除
            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnDeckCardClicked(card));
        }

        //枚数更新
        deckCountText.text = $"{currentDeck.Count} / {maxDeckSize}";
    }

    //一覧のカードをクリック→デッキに追加
    void OnLibraryCardClicked(Card card)
    {
        //デッキ枚数制限
        if (currentDeck.Count >= maxDeckSize)
        {
            Debug.Log("デッキがいっぱいです！");
            return;
        }

        //リーダー制限
        if (currentDeck.Count == 0)
        {
            if (card.cardType != CardType.Leader)
            {
                Debug.Log("デッキの一枚目は「リーダーカード」を選んでください！");
                return;
            }
        }
        else
        {
            if (card.cardType == CardType.Leader)
            {
                Debug.Log("「リーダーカード」はデッキに一枚しか入れられません！");
                return;
            }
        }

        //同名カードの枚数制限
        if (!card.isUnlimited)
        {
            int sameCardCount = currentDeck.Count(c => c.cardID == card.cardID);

            if (sameCardCount >= maxSameCardCount)
            {
                Debug.Log($"同名カードはデッキに{maxSameCardCount}枚までしか入れられません！");
                return;
            }
        }

        currentDeck.Add(card);
        RefreshDeckUI();    //デッキ側を更新
    }

    //デッキのカードをクリック→デッキから削除
    void OnDeckCardClicked(Card card)
    {
        currentDeck.Remove(card);
        RefreshDeckUI();
    }

    //デッキデータ保存
    public void OnSaveButtonClicked()
    {
        //枚数チェック
        if (currentDeck.Count != 30)
        {
            ShowAlert($"デッキは30枚である必要があります！\n(現在: {currentDeck.Count}枚)");
            return; // 処理をここで中断
        }

        //リーダーチェック
        if (currentDeck.Count > 0 && currentDeck[0].cardType != CardType.Leader)
        {
            ShowAlert("デッキの1枚目はリーダーカードにしてください！");
            return;
        }

        //カードデータからIDのリストに変換
        DeckSaveData data = new DeckSaveData();
        foreach (Card card in currentDeck)
        {
            data.cardIds.Add(card.cardID);
        }

        //JSONにして保存
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("デッキを保存しました！");

        //タイトルへ戻る
        SceneManager.LoadScene("Game");
    }

    //デッキデータ読み込み
    void LoadDeck()
    {
        currentDeck.Clear();

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            DeckSaveData data = JsonUtility.FromJson<DeckSaveData>(json);

            //IDからカードデータを復元
            foreach (int id in data.cardIds)
            {
                //allCardsListの中からIDが一致するものを探す
                Card foundCard = allCardsList.Find(c => c.cardID == id);
                if (foundCard != null)
                {
                    currentDeck.Add(foundCard);
                }
            }
        }
    }

    public void ShowAlert(string message)
    {
        alertMessageText.text = message;
        alertPanel.SetActive(true);
    }

    public void CloseAlert()
    {
        alertPanel.SetActive(false);
    }
}
