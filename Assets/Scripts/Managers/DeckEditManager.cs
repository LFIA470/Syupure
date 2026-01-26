using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum EditPhase
{ 
    LeaderSelect,   //リーダーを選ぶ
    LeaderDetail,   //リーダー拡大表示中
    DeckBuild,      //デッキを３０枚選ぶ
    Confirm,        //最終確認
}

public class DeckEditManager : MonoBehaviour
{
    //変数宣言
    #region Variables
    [Header("State")]
    public EditPhase currentPhase = EditPhase.LeaderSelect;

    [Header("Data")]
    private List<Card> allCardsList;    //ゲームに登場する全てのカードデータ
    public Card tempLeaderCandidate;
    public Card leaderCard;
    public List<Card> currentDeck = new List<Card>();  //編集中のデッキ(カードデータのリスト)

    [Header("Rules")]
    public int maxDeckSize = GameConstants.MaxDeckSize;
    public int maxSameCardCount = 4;

    [Header("UI References")]
    public GameObject cardPrefab;
    public GameObject zoomLeaderPrefab;
    public Text guideMessageText;
    public float cardScale = 3;

    [Header("Leader Choice UI")]
    public GameObject leaderChoicePanel;    //リーダーカード選択パネル
    public Transform leaderLibraryContent;  //リーダーカード一覧のスクロール画面

    [Header("Leader Detail UI")]
    public GameObject leaderDetailPanel;    //リーダー拡大パネル
    public Transform leaderDetailCardParet; //拡大画像の表示先
    public Button leaderDetailNextButton;   //このリーダーにする
    public Button leaderDetailBackButton;   //リーダー選択へ

    [Header("Deck Choice UI")]
    public GameObject deckChoicePanel;      //デッキ作成パネル
    public Transform deckLibraryContent;    //リーダーカード以外一覧のスクロール画面
    public Transform deckContent;           //デッキのスクロール画面
    public Text deckCountText;              //デッキの枚数
    public Button deckChoiceNextButton;     //次へボタン
    public Button deckChoiceBackButton;     //戻るボタン

    [Header("Confirm UI")]
    public GameObject confirmPanel;      //最終確認パネル
    public Transform confirmLeaderParent;   //リーダー表示位置
    public Transform confirmDeckContent;    //デッキ３０枚表示位置
    public Button confirmSaveButton;        //保存して完了
    public Button confirmBackButton;        //修正する

    [Header("Alert UI (Warning)")]
    public GameObject alertPanel;
    public Text alertMessageText;
    public Button alertOkButton;

    [Header("Confirm Dialog UI (Yes/No)")]
    public GameObject confirmDialogPanel;
    public Text confirmDialogMessageText;
    public Button confirmDialogYesButton;
    public Button confirmDialogNoButton;
    private System.Action onDialogYesAction;

    [Header("Save")]
    private const string SAVE_KEY = "PlayerDeck";
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Start()
    {
        //全カードリスト読み込み
        allCardsList = CardDatabase.GetAllCards();

        //全パネルを非表示にする
        leaderChoicePanel.SetActive(false);
        leaderDetailPanel.SetActive(false);
        deckChoicePanel.SetActive(false);
        confirmPanel.SetActive(false);
        alertPanel.SetActive(false);
        confirmDialogPanel.SetActive(false);

        //ボタンにイベントを登録
        SetupButtons();

        //保存されているデッキを読み込む
        LoadDeck();

        //最初のフェーズへ
        ChangePhase(EditPhase.LeaderSelect);
    }
    #endregion

    //デッキ作成画面の流れを管理するメソッド
    #region Deck Edit Flow Methods
    //デッキ作成画面：フェーズ変更
    void ChangePhase(EditPhase nextPhase)
    {
        currentPhase = nextPhase;

        leaderChoicePanel.SetActive(false);
        leaderDetailPanel.SetActive(false);
        deckChoicePanel.SetActive(false);
        confirmPanel.SetActive(false);

        switch (currentPhase)
        {
            case EditPhase.LeaderSelect:
                leaderChoicePanel.SetActive(true);
                guideMessageText.text = "リーダーカードを選択してください";
                RefreshLibraryForLeader();
                break;
            case EditPhase.LeaderDetail:
                leaderDetailPanel.SetActive(true);
                guideMessageText.text = "リーダー情報";
                ShowLeaderDetail();
                break;
            case EditPhase.DeckBuild:
                deckChoicePanel.SetActive(true);
                guideMessageText.text = "デッキのカードを選択してください";
                RefreshLibraryForDeck();
                RefreshDeckUI();
                UpdateDeckCountText();
                break;
            case EditPhase.Confirm:
                confirmPanel.SetActive(true);
                guideMessageText.text = "このデッキで保存しますか？";
                CreateConfirmList();
                break;
        }
    }
    #endregion

    //データセットやリセットのメソッド
    #region Set & Reset
    //指定箇所のオブジェクトを全部消す
    void ClearContent(Transform t)  
    {
        foreach (Transform child in t)
        {
            Destroy(child.gameObject);
        }
    }

    //ボタンを押下した時の処理をセット
    void SetupButtons()
    {
        //アラートのOK
        alertOkButton.onClick.AddListener(() => alertPanel.SetActive(false));

        //Yes/Noダイアログ
        confirmDialogYesButton.onClick.AddListener(OnConfirmDialogYesClicked);
        confirmDialogNoButton.onClick.AddListener(OnConfirmDialogNoClicked);

        //各画面のボタン
        leaderDetailNextButton.onClick.AddListener(OnLeaderDecideClicked);
        leaderDetailBackButton.onClick.AddListener(OnLeaderDetailBackClicked);

        deckChoiceNextButton.onClick.AddListener(OnDeckChoiceNextClicked);
        deckChoiceBackButton.onClick.AddListener(OnDeckChoiceBackClicked); // ←ここでダイアログを使う

        confirmSaveButton.onClick.AddListener(OnSaveButtonClicked);
        confirmBackButton.onClick.AddListener(OnConfirmBackClicked);
    }
    #endregion

    //デッキ作成画面の表示と操作に関するメソッド
    #region Deck Edit Show & Action
    //リーダーカード一覧の表示
    void RefreshLibraryForLeader()
    {
        //一旦クリア
        ClearContent(leaderLibraryContent);

        //カードID順に並べ替えて表示
        var leaders = allCardsList.Where(c => c.cardType == CardType.Leader).ToList();

        foreach (var card in leaders)
        {
            GameObject obj = Instantiate(cardPrefab, leaderLibraryContent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(cardScale, cardScale, 1.0f);

            CardView view = obj.GetComponent<CardView>();
            if (view != null )
            {
                view.SetCard(card);
                view.isDeckEditMode = true;
                view.isZoomPanel = true;
            }

            //クリックしたら詳細画面へ良くイベントを登録
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnLeaderSelected(card));
        }
    }

    //リーダーカードが選ばれた時
    void OnLeaderSelected(Card card)
    {
        tempLeaderCandidate = card;

        ChangePhase(EditPhase.LeaderDetail);
    }

    //詳細画面を表示
    void ShowLeaderDetail() 
    {
        //一旦クリア
        ClearContent(leaderDetailCardParet);

        if (tempLeaderCandidate == null) return;

        //プレハブを生成して詳細場所に置く
        GameObject obj = Instantiate(zoomLeaderPrefab, leaderDetailCardParet);

        //カード情報をセット
        CardView view = obj.GetComponent<CardView>();
        if (view != null)
        {
            view.SetCard(tempLeaderCandidate);
            view.isDeckEditMode = true;
        }

        Debug.Log(view.cardData.description);

        //カードサイズ変更
        view.transform.localScale = Vector3.one * 3.5f;

        if (obj.GetComponent<Button>()) Destroy(obj.GetComponent<Button>());
    }

    //デッキ構築画面下：カード一覧の表示(リーダー以外)
    void RefreshLibraryForDeck()
    {
        //一旦クリア
        ClearContent(deckLibraryContent);

        //カードID順に並べ替えて表示
        var cards = allCardsList.Where(c => c.cardType != CardType.Leader && c.cardType != CardType.EvolveLeader && c.cardType != CardType.EvolveCharacter).ToList();

        foreach (var card in cards)
        {
            GameObject obj = Instantiate(cardPrefab, deckLibraryContent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(cardScale, cardScale, 1.0f);

            CardView view = obj.GetComponent<CardView>();
            if (view != null)
            {
                view.SetCard(card);
                view.isDeckEditMode = true;
                view.isZoomPanel = true;
            }

            view.transform.localScale = Vector3.one * 1.3f;

            //クリックしたらデッキに追加
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnLibraryCardClicked(card));
        }
    }

    //デッキ構築画面上：デッキ内容の表示
    void RefreshDeckUI()
    {
        ClearContent(deckContent);

        //カードID順に並べ替えて表示
        var sortedDeck = currentDeck.OrderBy(c => c.cardID).ToList();

        foreach (var card in sortedDeck)
        {
            GameObject obj = Instantiate(cardPrefab, deckContent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(cardScale * 0.5f, cardScale * 0.5f, 1.0f);

            CardView view = obj.GetComponent<CardView>();
            if (view != null)
            {
                view.SetCard(card);
                view.isDeckEditMode = true;
            }

            view.transform.localScale = Vector3.one * 0.8f;

            //クリックしたらデッキから削除
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();

            btn.onClick.AddListener(() => OnDeckCardClicked(card));
        }

        //枚数更新
        UpdateDeckCountText();
    }

    //枚数表示とボタンのロック制御
    void UpdateDeckCountText()
    {
        //テキスト更新
        deckCountText.text = $"{currentDeck.Count} / {maxDeckSize}";

        //30枚ピッタリの時だけ「次へ」ボタンを押せるようにする
        bool isComlete = (currentDeck.Count == maxDeckSize);
        deckChoiceNextButton.interactable = isComlete;
    }

    //一覧のカードをクリック→デッキに追加
    void OnLibraryCardClicked(Card card)
    {
        //デッキ枚数制限
        if (currentDeck.Count >= maxDeckSize)
        {
            ShowAlert("デッキがいっぱいです！");
            return;
        }

        //同名カードの枚数制限
        if (!card.isUnlimited)
        {
            int sameCardCount = currentDeck.Count(c => c.cardID == card.cardID);

            if (sameCardCount >= maxSameCardCount)
            {
                ShowAlert($"同名カードは{maxSameCardCount}枚までです");
                return;
            }
        }

        //追加実行
        currentDeck.Add(card);

        //表示を更新
        RefreshDeckUI();
    }
 
    //デッキのカードをクリック→デッキから削除
    void OnDeckCardClicked(Card card)
    {
        //リストから削除
        currentDeck.Remove(card);

        //表示を更新
        RefreshDeckUI();
    }

    //確認画面：リスト生成
    void CreateConfirmList()
    {
        //一旦クリア
        ClearContent(confirmLeaderParent);
        ClearContent(confirmDeckContent);

        //リーダーを表示
        if (leaderCard != null)
        {
            SpawnConfirmCard(leaderCard, confirmLeaderParent);
        }

        var sortedDeck = currentDeck.OrderBy(c => c.cardID).ToList();

        //デッキ３０枚を表示
        foreach (var card in sortedDeck)
        {
            SpawnConfirmCard(card, confirmDeckContent);
        }
    }

    //確認画面：カード生成ヘルパー
    void SpawnConfirmCard(Card card, Transform parent)
    {
        GameObject obj = Instantiate(cardPrefab, parent);

        CardView view = obj.GetComponent<CardView>();
        if (view != null)
        {
            view.SetCard(card);
            //確認画面ではクリック反応させない
            view.isDeckEditMode = true;
        }

        view.transform.localScale = Vector3.one * 0.8f;

        //念のためButtonコンポーネントがついていたら削除しておく
        if (obj.GetComponent<Button>())
        {
            Destroy(obj.GetComponent<Button>());
        }
    }

    //デッキデータ読み込み
    void LoadDeck()
    {
        //保存データを文字列として取得
        string dataString = PlayerPrefs.GetString("PlayerDeck", "");

        if (string.IsNullOrEmpty(dataString)) return;

        //カンマで区切って数字の配列に戻す
        string[] idStringArray = dataString.Split(',');

        currentDeck.Clear();
        leaderCard = null;

        // 3. 順番にリストに戻していく
        for (int i = 0; i < idStringArray.Length; i++)
        {
            if (int.TryParse(idStringArray[i], out int id))
            {
                // 最初の1個目はリーダーにするルールなら
                if (i == 0)
                {
                    leaderCard = CardDatabase.GetCardByID(id);
                }
                else
                {
                    Card card = CardDatabase.GetCardByID(id);
                    if (card != null)
                    {
                        currentDeck.Add(card);
                    }
                }
            }
        }
    }
    #endregion

    //警告メッセージの表示に関するメソッド
    #region Alert & Dialog
    public void ShowAlert(string message)
    {
        alertMessageText.text = message;
        alertPanel.SetActive(true);
    }

    public void CloseAlert()
    {
        alertPanel.SetActive(false);
    }

    public void ShowConfirmDialog(string message, System.Action yesAction)
    {
        confirmDialogMessageText.text = message;
        onDialogYesAction = yesAction;
        confirmDialogPanel.SetActive(true);
    }

    void OnConfirmDialogYesClicked()    //Yesボタンが押下された時
    {
        confirmDialogPanel.SetActive(false);
        onDialogYesAction.Invoke();
    }

    void OnConfirmDialogNoClicked()     //Noボタンが押下された時
    {
        confirmDialogPanel.SetActive(false);
    }
    #endregion

    //ボタンを押した時の処理
    #region Button Effect
    //詳細画面：「決定」ボタンを押下された時
    void OnLeaderDecideClicked()
    {
        if (tempLeaderCandidate == null) return;

        //リーダーを確定
        leaderCard = tempLeaderCandidate;
        Debug.Log($"リーダー決定：{leaderCard.cardName}");

        //次のフェーズへ
        ChangePhase(EditPhase.DeckBuild);
    }

    //詳細画面：「戻る」ボタンが押下された時
    void OnLeaderDetailBackClicked()
    {
        //候補をリセットして一覧に戻る
        tempLeaderCandidate = null;
        ChangePhase(EditPhase.LeaderSelect);
    }

    //デッキ構築画面：「決定」ボタンが押下された時
    void OnDeckChoiceNextClicked()
    {
        //念のため枚数チェック
        if (currentDeck.Count != maxDeckSize) return;

        //次のフェーズへ
        ChangePhase(EditPhase.Confirm);
    }

    //デッキ構築画面：「戻る」ボタンが押下された時
    void OnDeckChoiceBackClicked()
    {
        //確認ダイアログ表示
        ShowConfirmDialog(
            "リーダー選択に戻りますか？\n作成中のデッキはリセットされます。",
            () =>
            {
                //Yesが押下された時の処理
                currentDeck.Clear();
                leaderCard = null;
                ChangePhase(EditPhase.LeaderSelect);
            }
        );
    }

    //確認画面：「保存」ボタン
    void OnSaveButtonClicked()
    {
        //最終チェック
        if (leaderCard == null || currentDeck.Count != maxDeckSize)
        {
            ShowAlert("デッキデータが不正です");
            return;
        }

        //保存用IDリストを作成
        List<int> saveIds = new List<int>();

        //先頭は必ずリーダーにする
        saveIds.Add(leaderCard.cardID);

        //続きにデッキのカードを追加
        foreach (var card in currentDeck)
        {
            saveIds.Add(card.cardID);
        }

        //JSON型で保存
        string json = string.Join(",", saveIds);
        PlayerPrefs.SetString("PlayerDeck", json);
        PlayerPrefs.Save();

        Debug.Log("★保存完了！ゲームを開始します");

        //シーン遷移
        SceneManager.LoadScene("Game");
    }

    //確認画面：「戻る」ボタン
    void OnConfirmBackClicked()
    {
        ChangePhase(EditPhase.DeckBuild);
    }
    #endregion
}
