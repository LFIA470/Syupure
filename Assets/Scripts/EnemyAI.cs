using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform enemyHandArea;
    [SerializeField] private Transform enemyCharacterSlotsParent;

    public void StartEnemyTurn()    //相手のターンの行動を開始する
    {
        StartCoroutine(AIActionRoutine());
    }

    private IEnumerator AIActionRoutine()
    {
        Debug.Log("AI：考え中...");

        //人間らしく見せるために少し待つ
        yield return new WaitForSeconds(1.5f);

        // 手札のカードリストを取得
        // (GetComponentsInChildrenを使うと、手札にある全CardViewが取れます)
        CardView[] handCards = enemyHandArea.GetComponentsInChildren<CardView>();

        // スロットのリストを取得
        CharacterSlot[] slots = enemyCharacterSlotsParent.GetComponentsInChildren<CharacterSlot>();

        //カードを出す処理
        // 手札を1枚ずつチェック
        foreach (CardView card in handCards)
        {
            // キャラクターカードか？
            if (card.cardData.cardType == CardType.Character)
            {
                // 空いているスロットを探す
                foreach (CharacterSlot slot in slots)
                {
                    if (slot.occupiedCard == null)
                    {
                        // 出そうとしてみる（GameManagerに依頼）
                        bool success = GameManager.Instance.AIPlayCharacter(card, slot);

                        if (success)
                        {
                            // 出せたら、少し待ってから次の行動へ（あるいはループを続ける）
                            yield return new WaitForSeconds(1.0f);
                            break; // このカードは出したので、次のカードへ
                        }
                    }
                }
            }
        }

        //ターン終了
        Debug.Log("AI：ターンエンド！");
        GameManager.Instance.EndTurn(TurnOwner.Enemy);
    }
}
