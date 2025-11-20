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

        //もう一度手札リストを最新の状態に更新
        handCards = enemyHandArea.GetComponentsInChildren<CardView>();

        foreach (CardView card in handCards)
        {
            //マナが足りているアピールカードか？
            if (card.cardData.cardType == CardType.Appeal && GameManager.Instance.enemyMana >= ((AppealCard)card.cardData).cost)
            {
                //ベストなターゲットを探す
                //(自分の場のリーダーとキャラクターの中で、一番アピール力が高いやつを探す)
                CardView bestTarget = FindBestAppealTarget();

                if (bestTarget != null)
                {
                    //アピール実行！
                    bool success = GameManager.Instance.AIPlayAppeal(card, bestTarget);

                    if (success)
                    {
                        yield return new WaitForSeconds(1.0f);
                        //1ターンに1回アピールしたら終わるなら break
                        //複数回するなら break しない
                    }
                }
            }
        }

        //ターン終了
        Debug.Log("AI：ターンエンド！");
        GameManager.Instance.EndTurn(TurnOwner.Enemy);
    }

    private CardView FindBestAppealTarget()
    {
        CardView bestTarget = null;
        int maxAppeal = -1;

        // 1. 敵のリーダーをチェック (EnemyLeaderAreaにいるCardView)
        // (GameManagerからEnemyLeaderAreaへの参照を取得するか、EnemyAIに持たせる必要があります)
        // ここでは仮に EnemyAI に [SerializeField] private Transform enemyLeaderArea; があるとします
        /*
        CardView leader = enemyLeaderArea.GetComponentInChildren<CardView>();
        if (leader != null)
        {
             int appeal = ((LeaderCard)leader.cardData).appeal + leader.appealBuff;
             if (appeal > maxAppeal) { maxAppeal = appeal; bestTarget = leader; }
        }
        */

        // 2. 敵のフィールドのキャラクターをチェック
        // (enemyCharacterSlotsParent の子要素のスロットを確認)
        CharacterSlot[] slots = enemyCharacterSlotsParent.GetComponentsInChildren<CharacterSlot>();
        foreach (CharacterSlot slot in slots)
        {
            if (slot.occupiedCard != null)
            {
                CardView charCard = slot.occupiedCard;
                int appeal = 0;

                // 型チェックしてアピール力を取得
                if (charCard.cardData is CharacterCard c) appeal = c.appeal;
                else if (charCard.cardData is EvolveCharacterCard ec) appeal = ec.appeal;

                // バフも考慮
                appeal += charCard.appealBuff;

                // 最大値を更新
                if (appeal > maxAppeal)
                {
                    maxAppeal = appeal;
                    bestTarget = charCard;
                }
            }
        }

        // リーダーの比較ロジックもここに入れるのがベストです。
        // 簡易的に、キャラクターがいなければリーダー(null)を返す、などでも動きますが、
        // 「一番強いやつ」を選ぶロジックにすると強くなります。

        return bestTarget;
    }
}
