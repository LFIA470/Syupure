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
            case "UP_APPEAL":
                Effect_BuffAppeal(owner, cardData.effectValue, targetCard);
                break;
            case "DOWN_APPEAL":
                Effect_DebuffAppeal(owner, cardData.effectValue, targetCard);
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
}
