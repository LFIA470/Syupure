using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    public static EffectManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    //変数宣言
    #region Variables
    private DeckManager deckManager;
    private GameManager gameManager;
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    #region Unity Lifecycle Methods
    void Start()
    {
        //他のマネージャーへの参照を取得
        gameManager = GameManager.Instance;
        deckManager = FindObjectOfType<DeckManager>();
    }
    #endregion

    public void ExecuteEffect (Card cardData, TurnOwner owner, CardView targetCard = null)
    {
        if (string.IsNullOrEmpty(cardData.effectID) || cardData.effectID == "NONE")
        {
            return;
        }

        Debug.Log("効果発動：" + cardData.effectID);

        //EffectIDに応じて、実行する処理を分岐
        switch (cardData.effectID)
        {
            case "DRAW_CARD":
                Effect_DrawCard(owner, cardData.effectValue);
                break;
            case "SEARCH_CARD":
                Effect_SearchCard(cardData, owner);
                break;
            case "UP_APPEAL":
                Effect_BuffAppeal(owner, cardData.effectValue, targetCard);
                break;
            case "UP_APPEAL_TEAM":
                Effect_BuffAppealTeam(owner, cardData.effectValue, targetCard);
                break;
            case "DOWN_APPEAL":
                Effect_DebuffAppeal(owner, cardData.effectValue, targetCard);
                break;
            case "ADD_APPEAL_POINTS":
                Effect_AddAppealPoints(owner, cardData.effectValue);
                break;
            case "HEAL_CHEER":
                Effect_HealCheer(owner, cardData.effectValue);
                break;
            case "EVOLVE":
                Effect_Evolve(cardData, targetCard);
                break;
            default:
                Debug.LogWarning("未定義の効果IDです：" + cardData.effectID);
                break;
        }
    }

    private void Effect_DrawCard(TurnOwner owner, int amount)   //ドロー効果
    {
        Debug.Log(owner + "がカードを" + amount + "枚引く。");
        for ( int i = 0; i < amount; i++ )
        {
            deckManager.DrawCard(owner);
        }
    }
    private void Effect_SearchCard(Card card, TurnOwner owner) //サーチ効果
    {
        int lookCount = card.effectValue;
        int selectCount = 1;

        if (lookCount <= 0) lookCount = 3;
        if (selectCount <= 0) selectCount = 1;

        GameManager.Instance.StartSearchTransaction(owner, lookCount, selectCount);
    }
    private void Effect_BuffAppeal(TurnOwner owner, int amount, CardView target)    //アピール力UP効果
    {
        if (target == null)
        {
            Debug.LogError("UP_APPEAL効果のターゲットが指定されていません!");
            return;
        }

        Debug.Log(target.cardData.cardName + "のアピール力を" + amount + "上昇させます。");

        //ターゲットカードの補正値に、効果の値を加算する
        target.appealBuff += amount;

        gameManager.RegisterBuffToClear(target);
    }
    private void Effect_BuffAppealTeam(TurnOwner owner, int amount, CardView target) //アピール力UP効果(キャスト数分)
    {
        if (target == null)
        {
            Debug.LogError("UP_APPEAL_TEAM効果のターゲットが指定されていません!");
            return;
        }

        int count = gameManager.PlayerCharacterCount;

        int buffAmount = (int)(count * amount);

        target.appealBuff += buffAmount;

        gameManager.RegisterBuffToClear(target);
    }
    private void Effect_DebuffAppeal(TurnOwner owner, int amount, CardView target)   //アピール力Down効果
    {
        if (target == null)
        {
            Debug.LogError("DOWN_APPEAL効果のターゲットが指定されていません!");
            return;
        }

        Debug.Log(target.cardData.cardName + "のアピール力を" + amount + "減少させます。");

        //ターゲットカードの補正値に、効果の値を加算する
        target.appealBuff -= amount;

        gameManager.RegisterBuffToClear(target);
    }
    private void Effect_AddAppealPoints(TurnOwner owner, int amount)
    {
        Debug.Log(owner + "のアピールポイントが" + amount + "UP。");
        if (owner == TurnOwner.Player) gameManager.playerAppealPoints += amount;
        else gameManager.enemyAppealPoints += amount;

        UIManager.Instance.UppdateAppealPointUI(gameManager.playerAppealPoints, gameManager.enemyAppealPoints);
    }
    private void Effect_HealCheer(TurnOwner owner, int amount)  //応援回復効果
    {
        Debug.Log(owner + "に" + amount + "の応援が回復。");
        if (owner == TurnOwner.Player)
        {
            gameManager.playerMana += amount;
            if  (gameManager.playerMana > 3)
            {
                gameManager.playerMana = 3;
            }
            UIManager.Instance.UpdateCheerPowertUI(gameManager.playerMana, GameConstants.DefaultMaxMana, TurnOwner.Player);
        }
        else
        {
            gameManager.enemyMana += amount;
            if (gameManager.enemyMana > 3)
            {
                gameManager.enemyMana = 3;
            }
            UIManager.Instance.UpdateCheerPowertUI(gameManager.enemyMana, GameConstants.DefaultMaxMana, TurnOwner.Enemy);
        }
    }
    private void Effect_Evolve(Card accessoryData, CardView baseCardView)
    {
        Debug.Log("進化チェック");
        if (baseCardView == null)
        {
            Debug.LogError("進化対象が指定されていません！");
            return;
        }

        //IDチェック（念の為の最終確認）
        AccessoryCard accessory = accessoryData as AccessoryCard;
        if (baseCardView.cardData.cardID != accessory.evolveBaseID)
        {
            Debug.LogWarning($"進化対象不一致: {baseCardView.cardData.cardName} は進化できません。");
            return;
        }

        //実処理の依頼
        GameManager.Instance.DressUP(baseCardView, accessory.evolveTargetID, accessory);
    }
}
