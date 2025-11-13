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

    public void ExecuteEffect (Card cardData, TurnOwner owner)
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
            default:
                Debug.LogWarning("未定義の効果IDです：" + cardData.effectID);
                break;
        }
    }

    private void Effect_DrawCard(TurnOwner owner, int amount)
    {
        Debug.Log(owner + "がカードを" + amount + "枚引く。");
        for ( int i = 0; i < amount; i++ )
        {
            deckManager.DrawCard(owner);
        }
    }
}
