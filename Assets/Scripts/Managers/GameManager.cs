using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum TurnOwner
{
    Player,
    Enemy,
}

public enum GamePhase
{
    Start,
    Main,
    Appeal, // アピールカード使用時のターゲット選択中など
    End
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

    [SerializeField] private Transform EnemyReserveArea;    //トラッシュのTransformを設定※相手用

    [Header("Game State")]
    public bool isPlayerTurn = true;        //ターン確認
    public GamePhase currentPhase = GamePhase.Start;
    private bool isTargetingMode = false;   //盤面選択モード中かどうかのフラグ
    private CardView cardToPlay;            //プレイしようとしているカード
    private List<CardView> buffedCardsThisTurn = new List<CardView>();

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
                OnStartPhase();
                break;
            case GamePhase.Main:
                if (isPlayerTurn)
                {
                    Debug.Log("メインフェイズ：カードをプレイ出来ます。");
                }
                else
                {
                    Debug.Log("メインフェイズ：相手の番です。AI起動。");
                    enemyAI.StartEnemyTurn();
                }
                break;
            case GamePhase.Appeal:
                Debug.Log("アピールステップ：ターゲットを選択してください。");
                break;
            case GamePhase.End:
                OnEndPhase();
                break;
        }
    }
    private void OnStartPhase()
    {
        //マナ回復
        if (isPlayerTurn) playerMana = GameConstants.DefaultMaxMana;
        else enemyMana = GameConstants.DefaultMaxMana;

        //カードを引く
        deckManager.DrawCard(isPlayerTurn ? TurnOwner.Player : TurnOwner.Enemy);

        //スタートフェイズの効果処理

        //自動的にメインフェイズへ移行
        ChangePhase(GamePhase.Main);
    }
    private void OnEndPhase()
    {
        //エンドフェイズの効果処理

        //バフの解除
        ClearAllBuffs();

        //相手にターンを渡す
        if (isPlayerTurn) StartTurn(TurnOwner.Enemy);
        else StartTurn(TurnOwner.Player);
    }
    public void StartGame() //ゲームの準備と開始を行う
    {
        Debug.Log("ゲームを開始します。");

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
        Debug.Log(owner + "のターンを開始します。");
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
            Debug.Log("メインフェイズ以外ではターンを終了出来ません。");
        }
    }
    private void CheckGameEnd() //勝敗がついたかチェック
    {
        //プレイヤーが勝ったか?
        if (playerAppealPoints >= GameConstants.WinAppealPoint)
        {
            Debug.Log("プレイヤーの勝利です！");
        }
        else if (enemyAppealPoints >= GameConstants.WinAppealPoint)
        {
            Debug.Log("あなたの敗北です。");
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
            Debug.Log("キャラクターカード以外はフィールドに出せません");
            card.ReturnToOriginalParent();
            return;
        }

        //キャラクターエリアが使用済みかどうか
        if (slot.occupiedCard != null)
        {
            Debug.Log("このスロットは既に使用されています。");
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
            card.transform.localScale = Vector3.one * 0.8f;
            card.transform.localRotation = Quaternion.identity;
            slot.occupiedCard = card;
        }
        else
        {
            //共通ルールチェックで失敗した場合
            card.ReturnToOriginalParent();
        }
    }
    public void CardDroppedOnCharacter(CardView evolveCard, CardView baseCharacter)//キャラクターの上にドロップされた時の処理(進化)
    {
        //ルールチェック
        //カードタイプが進化カードかどうか
        if (evolveCard.cardData.cardType != CardType.EvolveCharacter)
        {
            evolveCard.ReturnToOriginalParent();
            return;
        }
        //進化元のカードがキャラクターカードかどうか
        if (baseCharacter.cardData.cardType != CardType.Character)
        {
            evolveCard.ReturnToOriginalParent();
            return;
        }

        //共通ルールチェック
        bool canPlay = PlayCard(evolveCard.cardData, TurnOwner.Player);


        //実行処理
        if (canPlay)
        {
            Debug.Log(evolveCard.cardData.cardName + "に進化しました。");
            
            //進化元のキャラクターがいたスロットを探す
            CharacterSlot slot = baseCharacter.transform.parent.GetComponent<CharacterSlot>();
            if (slot == null)
            {
                Debug.LogError("進化元のキャラクターがスロットにいません！");
                evolveCard.ReturnToOriginalParent();
                return;
            }

            //進化カードを、進化元カードの子オブジェクトにする
            evolveCard.transform.SetParent(baseCharacter.transform, false);

            //進化カードの位置を親（進化元）の真上にピッタリ合わせる
            evolveCard.transform.localPosition = Vector3.zero;

            //子オブジェクトになった後、ローカルスケールを(1, 1, 1)にリセットする
            evolveCard.transform.localScale = Vector3.one;

            //スロットの「使用中のカード」情報を、新しい進化カードに更新する
            slot.occupiedCard = evolveCard;

            //進化カードの見た目と当たり判定を消す
            CanvasGroup baseCardCanvasGroup = baseCharacter.GetComponent<CanvasGroup>();
            if (baseCardCanvasGroup != null)
            {
                baseCardCanvasGroup.alpha = 0;
                baseCardCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            //共通ルールチェックで失敗した場合
            evolveCard.ReturnToOriginalParent();
        }
        
    }
    public void CardDroppedOnSpellArea(CardView card, SpellArea spellArea)//アピール・イベントエリアにドロップされた時の処理
    {
        //ルールチェック
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
                //card.transform.localPosition = Vector2.zero;    //エリアの真ん中に配置
                card.transform.localRotation = Quaternion.identity;
                card.transform.localScale = Vector3.one * 0.8f;
                EnterTargetingMode(card);
                break;
            case CardType.Event:
                Debug.Log(card.cardData.cardName + " の効果を発動します。");
                EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Player);
                MoveCardToDiscard(card, TurnOwner.Player);
                break;
        }

        //全てOKなら効果発動
        Debug.Log(card.cardData.cardName + "の効果を発動！");

        //効果処理

        //効果発動後処理(トラッシュに送るなど)
        //Destroy(card.gameObject);
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
        if (!isTargetingMode || cardToPlay == null) return;

        //プレイするカードの種類を取得
        CardType type = cardToPlay.cardData.cardType;

        //カードの種類に応じて、クリックした場所が正しいか判定
        switch (type)
        {
            //キャラクターをプレイしようとしている場合
            case CardType.Character:
                //クリックした場所は「空のスロット」か？
                CharacterSlot slot = fieldTransform.GetComponent<CharacterSlot>();
                if (slot != null)
                {
                    //正しい場所→スロット処理を呼ぶ
                    CardDroppedOnSlot(cardToPlay, slot);
                }
                break;
            case CardType.EvolveCharacter:
                //クリックした場所は「キャラクターカード」か？
                CardView baseCharacter = fieldTransform.GetComponent<CardView>();
                if (baseCharacter != null && baseCharacter.cardData.cardType == CardType.Character)
                {
                    //正しい場所→進化処理を呼ぶ
                    CardDroppedOnCharacter(cardToPlay, baseCharacter);
                }
                break;
            case CardType.Appeal:
                //クリックした場所は「リーダー」か「キャラクター」か？
                CardView targetCard = fieldTransform.GetComponent<CardView>();
                if (targetCard != null && (targetCard.cardData.cardType == CardType.Leader || targetCard.cardData.cardType == CardType.Character))
                {
                    //アピール効果実行
                    EffectManager.Instance.ExecuteEffect(cardToPlay.cardData, TurnOwner.Player, targetCard);

                    //アピール処理を実行
                    PerformAppeal(TurnOwner.Player, targetCard);

                    //待機していたアピールカードを破棄
                    MoveCardToDiscard(cardToPlay, TurnOwner.Player);
                }
                break;
            case CardType.Event:
                //クリックした場所は「呪文エリア」か？
                SpellArea spellArea = fieldTransform.GetComponent<SpellArea>();
                if (spellArea != null)
                {
                    //正しい場所→呪文処理を呼ぶ
                    CardDroppedOnSpellArea(cardToPlay, spellArea);
                }
                break;
        }
        //モード解除
        isTargetingMode = false;
        cardToPlay = null;
    }
    #endregion

    //ゲームの状態を管理するメソッド
    #region Game State Methods
    public void EnterTargetingMode(CardView card)//zoomUIManagerから呼ばれる
    {
        isTargetingMode = true;
        cardToPlay = card;
        Debug.Log("盤面選択モードに移行しました");
    }
    public bool IsTargetingMode()//現在、盤面選択モード中かどうかを返す
    {
        return isTargetingMode;
    }
    #endregion

    //ゲームのアクションに関するメソッド
    #region Game Action Methods
    public void PerformAppeal(TurnOwner owner, CardView targetCard)//指定されたオーナーのアピール処理を実行する
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
        if (targetCard.cardData is LeaderCard leaderCard)
        {
            baseAppeal = leaderCard.appeal;
        }
        else if (targetCard.cardData is CharacterCard characterCard) // 通常キャラ
        {
            baseAppeal = characterCard.appeal;
        }
        else if (targetCard.cardData is EvolveCharacterCard evolveCard) // 進化キャラ
        {
            baseAppeal = evolveCard.appeal;
        }
        else
        {
            Debug.LogWarning(targetCard.cardData.cardName + " はアピール力を持ちません。");
            return;
        }

        //バフ計算
        appealPower = baseAppeal + targetCard.appealBuff;
        //0以下の場合はアピール失敗
        if (appealPower <= 0)
        {
            appealPower = 0;
            Debug.Log(targetCard.cardData.cardName + "のアピールは失敗しました。");
        }

        Debug.Log($"アピール計算：基本({baseAppeal}) + 補正({targetCard.appealBuff}) = {appealPower}");


        //合計したアピール力を、対応する側のポイントに加算する
        if (owner == TurnOwner.Player)
        {
            playerAppealPoints += appealPower;
        }
        else
        {
            enemyAppealPoints += appealPower;
        }

        Debug.Log(owner + "が" + appealPower + "アピールして、合計ポイントは" + (owner == TurnOwner.Player ? playerAppealPoints : enemyAppealPoints) + " になった！");

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

        //カードの親を墓地エリアに変更
        card.transform.SetParent(targetPile, false);

        //カードの操作を不能にし、見た目をリセット
        card.transform.localRotation = Quaternion.identity; //傾きをリセット
        card.transform.localScale = Vector3.one * 0.8f;    //大きさをリセット

        //カードをクリックやドラッグの対象外にする
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null )
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
            Debug.Log("マナが足りません。");
            return false;
        }

        //全てのチェックをクリア

        //実際にマナを消費する
        if (owner == TurnOwner.Player)
        {
            playerMana -= cost;
        }
        else
        {
            enemyMana -= cost;
        }

        //UI更新
        //UIManager.Instance.UpdateManaUI
    
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
        enemyMana -= charData.cost;
        // (EnemyのマナUI更新があればここで)

        // カードをスロットに移動
        card.transform.SetParent(slot.transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one * 0.8f;

        slot.occupiedCard = card;

        // 登場時効果があれば発動 (TimingID.ENTRY)
        EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Enemy);

        return true;
    }
    public bool AIPlayAppeal(CardView card, CardView target) //AIがアピールをプレイする
    {
        //共通ルールチェック
        if (!PlayCard(card.cardData, TurnOwner.Enemy))
        {
            return false;
        }

        //ターゲットの有効性チェック
        bool isLeader = target.cardData.cardType == CardType.Leader;
        bool isCharacter = target.cardData.cardType == CardType.Character; // 進化キャラも含むなら調整

        if (!isLeader && !isCharacter)
        {
            return false;
        }

        //プレイ実行
        Debug.Log("AIが " + card.cardData.cardName + " をプレイしました。");

        //マナ消費
        AppealCard appealData = card.cardData as AppealCard;
        enemyMana -= appealData.cost;

        //効果発動 (EffectManager)
        EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Enemy, target);

        //アピール実行
        PerformAppeal(TurnOwner.Enemy, target);

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
