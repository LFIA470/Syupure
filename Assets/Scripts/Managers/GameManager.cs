using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnOwner
{
    Player,
    Enemy,
}

public enum FieldOwner
{
    Player,
    Enemy,
}

public enum GamePhase
{
    Start,
    Main,
    End
}

public enum MainPhaseState
{
    Idle,       // 何もしていない、入力待ち
    Appealing,  // アピール演出中・処理中
    Selection   // 対象選択中など
}

public enum TargetingState
{
    None,           // 選択していない
    SelectAlly,     // 味方を選択中
    SelectEnemy,    // 敵を選択中
    SelectHandCard, // 手札を選択中
}

public class GameManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    //シングルトン設定(どこからでもアクセス出来る司令塔にする)
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    //変数宣言
    #region Variables
    [Header("Component References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Transform playerHandArea;  //手札エリアのTransformを設定
    public Transform PlayerHandArea { get { return playerHandArea; } }
    [SerializeField] private Transform playerLeaderArea;    //リーダーエリアのTransformを設定
    public Transform PlayerLeaderArea { get { return playerLeaderArea; } }
    [SerializeField] private Transform playerCharacterSlotsParent;  //キャラクタースロットのTrasnformを設定
    public Transform PlayerCharacterSlotsParent { get { return playerCharacterSlotsParent; } }
    [SerializeField] private SpellArea playerSpellArea; //スペルエリア
    [SerializeField] private Transform PlayerReserveArea;   //トラッシュのTransformを設定
    [SerializeField] private Transform enemyLeaderArea; //リーダーエリアのTransformを設定※相手用
    public Transform EnemyLeaderArea { get {return enemyLeaderArea; } }
    [SerializeField] private Transform enemyCharacterSlotsParent;   //キャラクタースロットのTransformを設定※相手用
    public Transform EnemyCharacterSlotsParent { get {return enemyCharacterSlotsParent; } }
    [SerializeField] private Transform enemySpellArea;  //スペルエリア※相手用
    [SerializeField] private Transform EnemyReserveArea;    //トラッシュのTransformを設定※相手用

    [Header("Game State")]
    public bool isPlayerTurn = true;        //ターン確認
    public GamePhase currentPhase = GamePhase.Start;
    public MainPhaseState currentMainPhaseState = MainPhaseState.Idle;
    public TargetingState targetingState = TargetingState.None;
    private CardView cardToPlay;            //プレイしようとしているカード
    private CardView appealSourceCard;
    private List<CardView> buffedCardsThisTurn = new List<CardView>();
    public List<Card> playerFieldList = new List<Card>();
    public List<Card> allCardDataList;
    public GameObject cardPrefab;
    public SearchPanel searchPanel;
    private List<Card> tempDrawnIds = new List<Card>();
    private TurnOwner _currentSearchOwner;
    private float defaultCardSize = 1.04f;

    [Header("Player & Enemy Stats")]
    public int playerMana = 0;              //マナ(プレイヤー)
    public int playerAppealPoints = 0;      //プレイヤーアピールポイント(勝利条件)
    public int enemyMana = 0;               //マナ(相手)
    public int enemyAppealPoints = 0;       //相手アピールポイント(相手の勝利条件)

    public CardView selectedCard;           //選択されているカードを一時的に保存する

    [Header("AI")]
    [SerializeField] private EnemyAI enemyAI;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    private void Start()
    {
        //ゲームスタート
        StartGame();
    }
    #endregion

    //ゲームの流れを管理するメソッド
    #region Game Flow Methods
    public void ChangePhase(GamePhase newPhase) //フェーズを切り替え、そのフェーズ開始時の処理を行う
    {
        currentPhase = newPhase;
        Debug.Log($"フェーズ移行：{newPhase}");

        switch (newPhase)
        {
            case GamePhase.Start:
                StartCoroutine(OnStartPhase());
                break;
            case GamePhase.Main:
                StartCoroutine(OnMainPhase());
                break;
            case GamePhase.End:
                StartCoroutine(OnEndPhase());
                break;
        }
    }
    private IEnumerator OnStartPhase()
    {
        //スタートフェイズ開始演出
        BattleLogManager.Instance.ShowPhaseAnnounce(GamePhase.Start);
        
        yield return new WaitForSeconds(1.0f);

        //マナ回復
        BattleLogManager.Instance.ShowNotification("マナを回復");
        if (isPlayerTurn)
        {
            playerMana = GameConstants.DefaultMaxMana;
            UIManager.Instance.UpdateCheerPowertUI(playerMana, GameConstants.DefaultMaxMana, TurnOwner.Player);
        }
        else
        {
            enemyMana = GameConstants.DefaultMaxMana;
            UIManager.Instance.UpdateCheerPowertUI(enemyMana, GameConstants.DefaultMaxMana, TurnOwner.Enemy);
        }

            yield return new WaitForSeconds(1.0f);

        //カードを引く
        deckManager.DrawCard(isPlayerTurn ? TurnOwner.Player : TurnOwner.Enemy);

        //スタートフェイズの効果処理

        //自動的にメインフェイズへ移行
        ChangePhase(GamePhase.Main);
    }
    private IEnumerator OnMainPhase()
    {
        //メインフェイズ開始演出
        BattleLogManager.Instance.ShowPhaseAnnounce(GamePhase.Main);

        yield return new WaitForSeconds(1.0f);

        //メインフェイズ開始時効果

        if (isPlayerTurn)
        {
            UIManager.Instance.SetTurnEndButtonActive(true);
        }
        else
        {
            enemyAI.StartEnemyTurn();
        }
    }
    private IEnumerator OnEndPhase()
    {
        UIManager.Instance.SetTurnEndButtonActive(false);
        BattleLogManager.Instance.ShowPhaseAnnounce(GamePhase.End);

        yield return new WaitForSeconds(1.0f);

        //エンドフェイズの効果処理

        //バフの解除
        ClearAllBuffs();

        //相手にターンを渡す
        if (isPlayerTurn) StartTurn(TurnOwner.Enemy);
        else StartTurn(TurnOwner.Player);
    }
    public void StartGame() //ゲームの準備と開始を行う
    {
        UIManager.Instance.SetTurnEndButtonActive(false);

        UIManager.Instance.UpdateCheerPowertUI(playerMana, GameConstants.DefaultMaxMana, TurnOwner.Player);
        UIManager.Instance.UpdateCheerPowertUI(playerMana, GameConstants.DefaultMaxMana, TurnOwner.Enemy);

        BattleLogManager.Instance.ShowNotification("ゲームスタート");

        //両プレイヤーのデッキシャッフル
        deckManager.DeckShuffle(TurnOwner.Player);
        deckManager.DeckShuffle(TurnOwner.Enemy);

        //初期手札を配る
        for (int i = 0; i < GameConstants.StartingHandSize; i++)
        {
            deckManager.DrawCard(TurnOwner.Player);
            deckManager.DrawCard(TurnOwner.Enemy);
        }

        //先攻・後攻を決める

        //UIを0で初期化
        UIManager.Instance.UppdateAppealPointUI(playerAppealPoints, enemyAppealPoints);

        //最初のターンを開始する
        StartTurn(TurnOwner.Player);
    }
    public void StartTurn(TurnOwner owner)  //ターン移行(開始)
    {
        BattleLogManager.Instance.ShowNotification(owner + "のターン");
        isPlayerTurn = (owner == TurnOwner.Player);

        //いきなり処理せず、スタートフェイズに移行するだけにする
        ChangePhase(GamePhase.Start);
    }
    public void EndTurn(TurnOwner owner)//ターン移行(終了)
    {
        //いきなりターンを交代せず、エンドフェイズに移行する
        if (currentPhase == GamePhase.Main)
        {
            ChangePhase(GamePhase.End);
        }
        else
        {
            BattleLogManager.Instance.ShowNotification("メインフェイズ以外ではターンを終了出来ません");
        }
    }
    private void CheckGameEnd() //勝敗がついたかチェック
    {
        //プレイヤーが勝ったか?
        if (playerAppealPoints >= GameConstants.WinAppealPoint)
        {
            Debug.Log("プレイヤーの勝利です！");

            GameResultData.IsPlayerWin = true;

            SceneManager.LoadScene("Result");
        }
        else if (enemyAppealPoints >= GameConstants.WinAppealPoint)
        {
            Debug.Log("あなたの敗北です。");

            GameResultData.IsPlayerWin = false;

            SceneManager.LoadScene("Result");
        }
    }
    private void ClearAllBuffs()
    {
        Debug.Log("ターン終了。全ての一時効果をリセットします。");
        foreach (CardView card in buffedCardsThisTurn)
        {
            if (card != null)   //カードが退場などで消えていないチェック
            {
                card.appealBuff = 0;
            }
        }
        //リストを空にする
        buffedCardsThisTurn.Clear();
    }
    #endregion

    //カードのプレイに関するメソッド
    #region Card Playing Methods
    public void OnCardClicked(CardView cardView)//カードがクリックされた時にCardViewから呼ばれる
    {
        //すでに選択中のカードがあれば、それを一旦リセット(選択解除)
        if (selectedCard != null)
        {
            //選択中のカードの見た目を元に戻す(追加予定)
        }

        //新しくクリックされたカードを選択中にする
        selectedCard = cardView;
        Debug.Log(selectedCard.cardData.cardName + "を選択しました");

        //選択されたカードの見た目を変える処理
    }
    public void CardDroppedOnSlot(CardView card, CharacterSlot slot)//キャラクタースロットにドロップされた時の処理
    {
        //ルールチェック
        //カードタイプがキャラクターかどうか
        if (card.cardData.cardType != CardType.Character)
        {
            BattleLogManager.Instance.ShowNotification("キャラクターカード以外はフィールドに出せません");
            card.ReturnToOriginalParent();
            return;
        }

        //キャラクターエリアが使用済みかどうか
        if (slot.occupiedCard != null)
        {
            BattleLogManager.Instance.ShowNotification("このスロットは既に使用されています");
            card.ReturnToOriginalParent();
            return;
        }

        //共通ルールチェック
        bool canPlay = PlayCard(card.cardData, TurnOwner.Player);

        //実行処理
        if (canPlay)
        {
            //成功した場合のみ、カードをスロットに配置し、スロットに使用中であることを記憶させる
            Debug.Log(card.cardData.cardName + "をスロットに配置しました。");
            card.transform.SetParent(slot.transform, false);
            card.transform.localScale = Vector3.one * defaultCardSize;
            card.transform.localRotation = Quaternion.identity;
            slot.occupiedCard = card;

            if (card.cardData.timingID == "ENTRY")
            {
                Debug.Log("登場時効果発動！");
                EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Player);
            }
        }
        else
        {
            //共通ルールチェックで失敗した場合
            card.ReturnToOriginalParent();
        }
    }
    public void CardDroppedOnCharacter(CardView droppedCard, CardView targetCharacter)
    {
        if (droppedCard.cardData.cardType != CardType.Accessory)
        {
            Debug.Log("カードタイプ違い");
            droppedCard.ReturnToOriginalParent();
            return;
        }

        bool canPlay = PlayCard(droppedCard.cardData, TurnOwner.Player);
        if (!canPlay)
        {
            Debug.Log("ルール違反");
            droppedCard.ReturnToOriginalParent();
            return;
        }

        EffectManager.Instance.ExecuteEffect(droppedCard.cardData, TurnOwner.Player, targetCharacter);

        MoveCardToDiscard(droppedCard, TurnOwner.Player);
    }
    public void CardDroppedOnSpellArea(CardView card, SpellArea spellArea)//アピール・イベントエリアにドロップされた時の処理
    {
        //カードタイプチェック
        if (card.cardData.cardType != CardType.Event && card.cardData.cardType != CardType.Appeal)
        {
            Debug.Log("このエリアにはイベントかアピールしか出せません。");
            card.ReturnToOriginalParent();
            return;
        }

        //共通ルールチェック
        bool canPlay = PlayCard(card.cardData, TurnOwner.Player);

        //実行処理
        if (!canPlay)
        {
            // 失敗したら手札に戻す
            card.ReturnToOriginalParent();
            return;
        }

        switch (card.cardData.cardType)
        {
            case CardType.Appeal:
                Debug.Log(card.cardData.cardName + " をプレイするため、ターゲット選択に移行します。");
                card.transform.SetParent(spellArea.transform, false);
                card.transform.localRotation = Quaternion.identity;
                card.transform.localScale = Vector3.one * defaultCardSize;
                EnterTargetingMode(card);
                break;
            case CardType.Event:
                Debug.Log(card.cardData.cardName + " の効果を発動します。");
                EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Player);
                MoveCardToDiscard(card, TurnOwner.Player);
                break;
        }
    }
    #endregion

    //ルールの判定に関するメソッド
    #region Rule Check Methods
    public bool IsPlayable(CardView cardView)//カードがプレイ可能か判断する
    {
        Card cardData = cardView.cardData;

        int cardCost = 0;   //チェックに使うため、カードのコストを一時的に保存する

        switch (cardData.cardType)
        {
            case CardType.Character:
                //cardDataをCharacter型にキャストしてコストを取得
                cardCost = (cardData as CharacterCard).cost;
                break;
            case CardType.Event:
                //cardDataをEvent型にキャストしてコストを取得
                cardCost = (cardData as EventCard).cost;
                break;
            case CardType.Appeal:
                //cardDataをAppeal型にキャストしてコストを取得
                cardCost = (cardData as AppealCard).cost;
                break;
            case CardType.Leader:
                Debug.Log("リーダーカードを手札からプレイできません。");
                return false;
        }

        //条件１：プレイヤーのターンか？
        bool condition1 = isPlayerTurn;

        //条件２：カードは手札にあるか？
        bool condition2 = cardView.transform.parent == playerHandArea;

        //条件３：マナは足りてるか？
        bool condition3 = playerMana >= cardCost;

        //３つの条件を全て満たしていれば true (プレイ可能)を返す
        return condition1 && condition2 && condition3;
    }
    // クリックされたカードが「自分の場のキャラ/リーダー」か？
    private bool IsMyFieldCard(CardView card)
    {
        if (card.transform.parent == playerLeaderArea) return true;
        CharacterSlot slot = card.GetComponentInParent<CharacterSlot>();
        if (slot != null && slot.owner == FieldOwner.Player) return true;
        return false;
    }
    // クリックされたカードが「相手の場のキャラ/リーダー」か？
    private bool IsEnemyFieldCard(CardView card)
    {
        if (card.transform.parent == enemyLeaderArea) return true; // (相手リーダーエリアの変数が必要)
        CharacterSlot slot = card.GetComponentInParent<CharacterSlot>();
        if (slot != null && slot.owner == FieldOwner.Enemy) return true;

        return false;
    }
    //カードリストの中からIDが一致するものを探す
    public Card GetCardDataByID(int id)
    {
        return allCardDataList.Find(data => data.cardID == id);
    }
    #endregion

    //UI操作に関するメソッド
    #region UI Interaction Methods
    public void OnTurnEndButtonPressed()//UIの「ターン終了」ボタンから呼び出す
    {
        //プレイヤーがターンを終了した
        EndTurn(TurnOwner.Player);
    }
    public void OnFieldClicked(Transform fieldTransform)
    {
        //盤面選択モード出なければ、何もせず終了
        if (currentMainPhaseState != MainPhaseState.Selection|| cardToPlay == null) return;

        //プレイするカードの種類を取得
        CardType type = cardToPlay.cardData.cardType;

        //カードの種類に応じて、クリックした場所が正しいか判定
        switch (type)
        {
            //キャラクターをプレイしようとしている場合
            case CardType.Character:
                HandleCharacterClick(fieldTransform);
                break;
            case CardType.EvolveCharacter:
                HandleEvolveCharacterClick(fieldTransform);
                break;
            case CardType.Appeal:
                HandleAppealClick(fieldTransform);
                break;
            case CardType.Event:
                HandleEventClick(fieldTransform);
                break;
        }        
    }
    #endregion

    //カードタイプごとの処理に関するメソッド
    #region Card Type Flow
    private void HandleCharacterClick(Transform target)
    {
        //クリックした場所は「空のスロット」か？
        CharacterSlot clickedSlot = target.GetComponent<CharacterSlot>();
        if (clickedSlot != null)
        {
            //正しい場所→スロット処理を呼ぶ
            CardDroppedOnSlot(cardToPlay, clickedSlot);
        }

        currentMainPhaseState = MainPhaseState.Idle;
        cardToPlay = null;
        UIManager.Instance.SetTurnEndButtonActive(true);
    }
    private void HandleEvolveCharacterClick(Transform target)
    {
        //クリックした場所は「キャラクターカード」か？
        CardView baseCharacter = target.GetComponent<CardView>();
        if (baseCharacter != null && baseCharacter.cardData.cardType == CardType.Character)
        {
            //正しい場所→進化処理を呼ぶ
            CardDroppedOnCharacter(cardToPlay, baseCharacter);
        }

        currentMainPhaseState = MainPhaseState.Idle;
        cardToPlay = null;
        UIManager.Instance.SetTurnEndButtonActive(true);
    }
    private void HandleAppealClick(Transform target)
    {
        CardView clickedCard = target.GetComponent<CardView>();
        if (clickedCard == null) return;

        if (targetingState == TargetingState.SelectAlly)
        {
            if (IsMyFieldCard(clickedCard))
            {
                appealSourceCard = clickedCard;

                targetingState = TargetingState.SelectEnemy;
                Debug.Log("相手を選んでください");
            }
        }
        else if (targetingState == TargetingState.SelectEnemy)
        {
            if (IsEnemyFieldCard(clickedCard))
            {
                EffectManager.Instance.ExecuteEffect(cardToPlay.cardData, TurnOwner.Player, appealSourceCard);

                PerformAppeal(TurnOwner.Player, appealSourceCard, clickedCard);

                MoveCardToDiscard(cardToPlay, TurnOwner.Player);

                currentMainPhaseState = MainPhaseState.Idle;
                cardToPlay = null;
                UIManager.Instance.SetTurnEndButtonActive(true);
            }
        }
    }
    private void HandleEventClick(Transform target)
    {
        //クリックした場所は「呪文エリア」か？
        SpellArea spellArea = target.GetComponent<SpellArea>();
        if (spellArea != null)
        {
            //正しい場所→呪文処理を呼ぶ
            CardDroppedOnSpellArea(cardToPlay, spellArea);
        }

        currentMainPhaseState = MainPhaseState.Idle;
        cardToPlay = null;
        UIManager.Instance.SetTurnEndButtonActive(true);
    }
    #endregion

    //ゲームの状態を管理するメソッド
    #region Game State Methods
    public void EnterTargetingMode(CardView card)//zoomUIManagerから呼ばれる
    {
        currentMainPhaseState = MainPhaseState.Selection;
        cardToPlay = card;

        if (card.cardData.cardType == CardType.Appeal)
        {
            targetingState = TargetingState.SelectAlly;
        }
        else
        {
            // キャラクターなどの場合
            targetingState = TargetingState.None; // または専用の状態
        }

        UIManager.Instance.SetTurnEndButtonActive(false);

        Debug.Log("盤面選択モードに移行しました");
    }
    public MainPhaseState CurrentMainPhase()//現在、盤面選択モード中かどうかを返す
    {
        return currentMainPhaseState;
    }
    public int PlayerCharacterCount //現在のフィールドのキャラクター数分を返す
    {
        get
        {
            return playerFieldList.Count(card => card.cardType == CardType.Character);
        }
    }
    #endregion

    //ゲームのアクションに関するメソッド
    #region Game Action Methods
    public void DressUP(CardView baseView, int nextFormID, Card accessoryData)
    {
        Debug.Log($"{baseView.cardData.cardName} を ID:{nextFormID} にドレスアップさせます！");

        //進化先のデータを取得
        Card nextCardData = GetCardDataByID(nextFormID);
        if (nextCardData == null) return;

        //新しいカードオブジェクトを生成
        GameObject newCardObj = Instantiate(cardPrefab, baseView.transform.parent);
        newCardObj.transform.position = baseView.transform.position;
        newCardObj.transform.rotation = baseView.transform.rotation;

        //データのセットアップ
        CardView newView = newCardObj.GetComponent<CardView>();
        newView.SetCard(nextCardData);

        //ステータスの引き継ぎ（必要なら）
        // int damage = baseView.InitialHp - baseView.CurrentHp;
        // newView.TakeDamage(damage);

        //元のカードの処理
        //非表示にする
        baseView.gameObject.SetActive(false);

        //リストの入れ替え（GameManagerが管理しているフィールドリストなどがあれば更新）
        //playerFieldList.Remove(baseView);
        //playerFieldList.Add(newView);

        //アクセサリーカードの処理（装備状態にする）
        //ドロップしたアクセカードを手札から消し、進化後カードの子要素にする等の処理
    }
    public void PerformAppeal(TurnOwner owner, CardView ally, CardView enemy)//指定されたオーナーのアピール処理を実行する
    {
        //誰のアピールかによって
        Transform leaderArea;
        Transform characterSlotsParent;

        if (owner == TurnOwner.Player)
        {
            //プレイヤーのターンでなければ処理しない
            if (!isPlayerTurn) return;
            leaderArea = playerLeaderArea;
            characterSlotsParent = playerCharacterSlotsParent;
        }
        else
        {
            //相手のターンでなければ処理しない
            if (isPlayerTurn) return;
            leaderArea = enemyLeaderArea;
            characterSlotsParent = enemyCharacterSlotsParent;
        }

        //ターゲットのアピール力を取得
        int appealPower = 0;    //効果などを考慮した最終的なカードのアピール力
        int baseAppeal = 0;     //カードに記載された本来のアピール力
        if (ally.cardData is LeaderCard leaderCard)
        {
            baseAppeal = leaderCard.appeal;
        }
        else if (ally.cardData is CharacterCard characterCard) // 通常キャラ
        {
            baseAppeal = characterCard.appeal;
        }
        else
        {
            Debug.LogWarning(ally.cardData.cardName + " はアピール力を持ちません。");
            return;
        }

        //バフ計算
        appealPower = baseAppeal + ally.appealBuff;
        //0以下の場合はアピール失敗
        if (appealPower <= 0)
        {
            appealPower = 0;
            Debug.Log(ally.cardData.cardName + "のアピールは失敗しました。");
        }

        Debug.Log($"アピール計算：基本({baseAppeal}) + 補正({ally.appealBuff}) = {appealPower}");

        //アピールする相手がリーダーかキャラかで分岐
        if (enemy.cardData.cardType == CardType.Leader)
        {
            //合計したアピール力を、対応する側のポイントに加算する
            if (owner == TurnOwner.Player)
            {
                playerAppealPoints += appealPower;
            }
            else
            {
                enemyAppealPoints += appealPower;
            }
        }
        else if (enemy.cardData is CharacterCard characterCard)
        {
            characterCard.mental -= appealPower;
            CharacterHPCheck(enemy);
        }

        {
            Debug.Log(owner + "が" + appealPower + "アピールして、合計ポイントは" + (owner == TurnOwner.Player ? playerAppealPoints : enemyAppealPoints) + " になった！");
        }

        //UIの表示を更新する
        UIManager.Instance.UppdateAppealPointUI(playerAppealPoints, enemyAppealPoints);

        CheckGameEnd();
    }
    public void MoveCardToDiscard (CardView card, TurnOwner owner)  //使用したカードを墓地に送る
    {
        Transform targetPile = null;

        if (owner == TurnOwner.Player)
        {
            targetPile = PlayerReserveArea;
        }
        else
        {
            targetPile = EnemyReserveArea;
        }

        if (targetPile == null)
        {
            Debug.LogError(owner + "の墓地が設定されていません！カードは破棄されます。");
            Destroy(card.gameObject);
            return;
        }

        foreach (Transform existingCard in targetPile)
        {
            existingCard.gameObject.SetActive(false);
        }

        //カードの親を墓地エリアに変更
        card.transform.SetParent(targetPile, false);

        //カードの操作を不能にし、見た目をリセット
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity; //傾きをリセット
        card.transform.localScale = Vector3.one * defaultCardSize;    //大きさをリセット

        //カードをクリックやドラッグの対象外にする
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null )
        {
            cg.blocksRaycasts = false;
        }

        Debug.Log(card.cardData.cardName + "を墓地に送りました。");
    }
    public void MoveCardToDiscard(CardView card)  //使用したカードを墓地に送る
    {
        Transform targetPile = null;

        if (card.cardData.isPlayerCard)
        {
            targetPile = PlayerReserveArea;
        }
        else
        {
            targetPile = EnemyReserveArea;
        }

        if (targetPile == null)
        {
            //Debug.LogError(owner + "の墓地が設定されていません！カードは破棄されます。");
            Destroy(card.gameObject);
            return;
        }

        foreach (Transform existingCard in targetPile)
        {
            existingCard.gameObject.SetActive(false);
        }

        //カードの親を墓地エリアに変更
        card.transform.SetParent(targetPile, false);

        //カードの操作を不能にし、見た目をリセット
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity; //傾きをリセット
        card.transform.localScale = Vector3.one * defaultCardSize;    //大きさをリセット

        //カードをクリックやドラッグの対象外にする
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
        }

        Debug.Log(card.cardData.cardName + "を墓地に送りました。");
    }
    private bool PlayCard(Card cardData, TurnOwner owner)//カードプレイのルールチェックとコストの消費
    {
        //誰のターンで、誰のマナを使うか
        bool isCorrectTurn = (owner == TurnOwner.Player) ? isPlayerTurn : !isPlayerTurn;
        int currentMana = (owner == TurnOwner.Player) ? playerMana : enemyMana;

        //ターンチェック
        if (!isCorrectTurn)
        {
            Debug.Log("正しいターンではありません。");
            return false;
        }

        //フェーズチェック
        if (currentPhase != GamePhase.Main)
        {
            Debug.Log("メインフェイズ以外ではカードをプレイできません。");
            return false;
        }

        //マナチェック
        int cost = 0;
        switch (cardData.cardType)
        {
            case CardType.Character:
                cost = (cardData as CharacterCard).cost;
                break;
            case CardType.EvolveCharacter:
                cost = (cardData as EvolveCharacterCard).cost;
                break;
            case CardType.Appeal:
                cost = (cardData as  AppealCard).cost;
                break;
            case CardType.Event:
                cost = (cardData as EventCard).cost;
                break;
        }

        if (currentMana < cost)
        {
            BattleLogManager.Instance.ShowNotification("マナが足りません");
            return false;
        }

        //全てのチェックをクリア

        //実際にマナを消費する
        if (owner == TurnOwner.Player)
        {
            playerMana -= cost;
            UIManager.Instance.UpdateCheerPowertUI(playerMana, GameConstants.DefaultMaxMana, TurnOwner.Player);
        }
        else
        {
            enemyMana -= cost;
            UIManager.Instance.UpdateCheerPowertUI(enemyMana, GameConstants.DefaultMaxMana, TurnOwner.Enemy);
        }
        
        return true;
    }
    public void ProcessPlayRequest(CardView card)
    {
        //カードの種類
        switch (card.cardData.cardType)
        {
            //場所が決まっているカード
            case CardType.Event:
            case CardType.Appeal:
                if (playerSpellArea != null)
                {
                    //ドラッグ＆ドロップの時と同じ処理を呼び出す！
                    //これだけで、イベントなら即発動、アピールなら置いてターゲット待ちになる
                    CardDroppedOnSpellArea(card, playerSpellArea);
                }
                else
                {
                    Debug.LogError("PlayerSpellAreaがGameManagerに設定されていません！");
                }
                break;

            case CardType.Character:
            case CardType.EvolveCharacter:
                //これまで通り、盤面選択モードを開始する
                EnterTargetingMode(card);
                Debug.Log(card.cardData.cardName + "を出す場所(スロット/キャラ)を選択してください。");
                break;

            default:
                Debug.LogWarning("未対応のカードタイプです：" + card.cardData.cardType);
                break;
        }
    }
    public void CharacterHPCheck(CardView card)
    {
        if (card.cardData is CharacterCard character)
        {
            if (character.mental <= 0)
            {
                character.mental = 0;
                MoveCardToDiscard(card);
            }
        }
    }
    public void StartSearchTransaction(TurnOwner owner, int lookCount, int selectCount)
    {
        _currentSearchOwner = owner;

        //デッキからIDを抜き出す
        tempDrawnIds = deckManager.DrawCardsTemporary(owner, lookCount);

        //パネルを表示
        searchPanel.Open(tempDrawnIds, selectCount);
    }
    public void OnSearchCompleted(List<Card> selectedCards) //サーチ完了
    {
        //選ばれたカードを手札へ
        foreach (Card card in selectedCards)
        {
            CreateCardInHand(card);

            //全候補リストから、選ばれたIDを除外する（＝残りが戻す分）
            tempDrawnIds.Remove(card);
        }

        //選ばれなかった残りのIDをデッキに戻してシャッフル
        deckManager.ReturnCardsAndShuffle(_currentSearchOwner, tempDrawnIds);
    }
    public void CreateCardInHand(Card cardData)
    {
        //プレハブから新しいカードオブジェクトを作る
        GameObject newCardObj = Instantiate(cardPrefab, playerHandArea);

        //作ったカードにデータを持たせる
        CardView cardView = newCardObj.GetComponent<CardView>();

        //データセット
        cardView.SetCard(cardData);

        //必要ならSE（効果音）を鳴らすなど
        Debug.Log($"手札に {cardData.cardName} を生成しました");
    }
    public bool AIPlayCharacter(CardView card, CharacterSlot slot)  //AIがキャラクターをプレイする
    {
        //共通ルールチェック
        if (!PlayCard(card.cardData, TurnOwner.Enemy))
        {
            return false;
        }

        // 2. スロットの空きチェック
        if (slot.occupiedCard != null)
        {
            return false;
        }

        // --- プレイ実行 ---
        Debug.Log("AIが " + card.cardData.cardName + " をプレイしました。");

        // マナ消費
        CharacterCard charData = card.cardData as CharacterCard;

        // カードをスロットに移動
        card.transform.SetParent(slot.transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one * defaultCardSize;

        slot.occupiedCard = card;

        // 登場時効果があれば発動 (TimingID.ENTRY)
        EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Enemy);

        return true;
    }
    public bool AIPlayAppeal(CardView card, CardView appealer, CardView target) //AIがアピールをプレイする
    {
        //共通ルールチェック
        if (!PlayCard(card.cardData, TurnOwner.Enemy))
        {
            return false;
        }

        //ターゲットの有効性チェック
        bool isLeader = appealer.cardData.cardType == CardType.Leader;
        bool isCharacter = appealer.cardData.cardType == CardType.Character; // 進化キャラも含むなら調整

        if (!isLeader && !isCharacter)
        {
            return false;
        }

        //プレイ実行
        Debug.Log("AIが " + card.cardData.cardName + " をプレイしました。");

        if (enemySpellArea != null)
        {
            card.transform.SetParent(enemySpellArea, false);
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;
            card.transform.localScale = Vector3.one * defaultCardSize;
        }

        //マナ消費
        AppealCard appealData = card.cardData as AppealCard;

        //効果発動 (EffectManager)
        EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Enemy, appealer);

        //アピール実行
        PerformAppeal(TurnOwner.Enemy, appealer, target);

        //カードを墓地に送る
        MoveCardToDiscard(card, TurnOwner.Enemy);

        return true;
    }
    #endregion

    //EffectManagerから呼ばれる窓口メソッド
    #region Window EffectManager Methods
    public void RegisterBuffToClear(CardView card)
    {
        //まだリストになければ追加
        if (!buffedCardsThisTurn.Contains(card))
        {
            buffedCardsThisTurn.Add(card);
        }
    }
    #endregion
}
