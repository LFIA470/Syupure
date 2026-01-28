using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform enemyHandArea;
    [SerializeField] private Transform enemyCharacterSlotsParent;
    [SerializeField] private Transform enemyLeaderArea;
    [SerializeField] private Transform enemySpellArea;
    [SerializeField] private Transform playerLeaderArea;

    private bool isThinking = false;

    public void StartEnemyTurn()    //相手のターンの行動を開始する
    {
        StartCoroutine(ThinkAndActRoutine());
    }

    private IEnumerator ThinkAndActRoutine()
    {
        isThinking = true;

        //まず少し待つ（人間らしさの演出）
        yield return new WaitForSeconds(1.5f);

        //手札からカードをプレイする
        yield return StartCoroutine(ProcessPlayCards());

        //やることがなくなったらターン終了
        yield return new WaitForSeconds(1.0f);
        Debug.Log("AI: ターン終了");
        GameManager.Instance.EndTurn(TurnOwner.Enemy);

        isThinking = false;
    }

    private IEnumerator ProcessPlayCards()
    {
        //敵の手札にあるカードを全部取得してリスト化
        List<CardView> handCards = new List<CardView>();
        if (enemyHandArea != null)
        {
            foreach (Transform child in enemyHandArea)
            {
                CardView view = child.GetComponent<CardView>();
                if (view != null) handCards.Add(view);
            }
        }

        //コストの低い順に並べ替え
        var sortedHand = handCards.OrderBy(c => GetCardCost(c.cardData)).ToList();

        //ループ中に手札リストが変わるとエラーになるのでコピーして回す
        List<CardView> tryList = new List<CardView>(sortedHand);

        foreach (CardView card in tryList)
        {
            //すでにプレイされて消えている、または墓地に行っている場合はスキップ
            if (card == null || card.transform.parent != enemyHandArea) continue;

            //プレイ可能かチェック
            if (CanAIPlay(card))
            {
                //ちょっと考える演出
                yield return new WaitForSeconds(0.5f);

                //プレイ実行
                //失敗しても、ループを止めずに次のカードを試すのが大事
                bool played = TryPlayCard(card);

                if (played)
                {
                    card.SetCard(card.cardData);
                    //プレイ成功
                    Debug.Log($"AI: {card.cardData.cardName} (Cost:{GetCardCost(card.cardData)}) をプレイしました");
                }
            }
        }
    }

    private IEnumerator ProcessAttacks()
    {
        //自分の場のキャラを取得
        List<CardView> myCharacters = GetMyFieldCards();

        foreach (CardView attacker in myCharacters)
        {
            //攻撃済みならスキップ

            //誰を狙う？
            CardView target = GetBestTarget();

            if (target != null)
            {
                // 攻撃演出
                yield return new WaitForSeconds(0.5f);
                GameManager.Instance.PerformAppeal(TurnOwner.Enemy, attacker, target);
            }
        }
    }

    private int GetCardCost(Card card)
    {
        if (card is CharacterCard ch) return ch.cost;
        if (card is AccessoryCard ac) return ac.cost;
        if (card is AppealCard ap) return ap.cost;
        if (card is EventCard ev) return ev.cost;
        return 0;
    }

    private int GetTotalAppealPower(CardView card)
    {
        int baseAppeal = 0;

        if (card.cardData is CharacterCard charCard)
        {
            baseAppeal = charCard.appeal;
        }
        else if (card.cardData is LeaderCard leaderCard)
        {
            baseAppeal = leaderCard.appeal;
        }

        return baseAppeal + card.appealBuff;
    }
    private bool CanAIPlay(CardView card)
    {
        int cost = GetCardCost(card.cardData);

        // マナチェック
        if (GameManager.Instance.enemyMana < cost) return false;

        if (card.cardData.cardType == CardType.Character || card.cardData.cardType == CardType.EvolveCharacter)
        {
            return GetEmptyEnemySlot() != null;
        }
        else if (card.cardData.cardType == CardType.Appeal)
        {
            return GetMyFieldCards().Count > 0;
        }
        else if (card.cardData.cardType == CardType.Event)
        {
            return true;
        }

        return true;
    }

    private bool TryPlayCard(CardView card)
    {
        //カードタイプで分岐
        if (card.cardData.cardType == CardType.Character)
        {
            //空いているスロットを探す
            CharacterSlot emptySlot = GetEmptyEnemySlot();
            if (emptySlot != null)
            {
                return GameManager.Instance.AIPlayCharacter(card, emptySlot);
            }
        }
        else if (card.cardData.cardType == CardType.Appeal)
        {
            //誰がアピールする？
            List<CardView> potentialAppealers = new List<CardView>();

            //フィールドのリーダーとキャラクターを追加
            potentialAppealers.AddRange(GetMyFieldCards());

            if (potentialAppealers.Count == 0) return false; //アピールする人がいない

            CardView bestAppealer = potentialAppealers.OrderByDescending(c => GetTotalAppealPower(c)).FirstOrDefault();

            //誰を狙う？
            CardView target = GetBestTarget();

            if (bestAppealer != null && target != null)
            {
                Debug.Log($"AI:{bestAppealer.cardData.cardName} (Power:{GetTotalAppealPower(bestAppealer)})でアピールします");
                //GameManagerのアピール処理を呼ぶ
                return GameManager.Instance.AIPlayAppeal(card, bestAppealer, target);
            }
        }
        else if (card.cardData.cardType == CardType.Event)
        {
            //イベント用メソッドを呼ぶ
            if (enemySpellArea != null)
            {
                //スペルエリアに置いて発動
                card.transform.SetParent(enemySpellArea, false);
                //コスト支払いと効果発動
                if (GameManager.Instance.PlayCard(card.cardData, TurnOwner.Enemy))
                {
                    EffectManager.Instance.ExecuteEffect(card.cardData, TurnOwner.Enemy);
                    GameManager.Instance.MoveCardToDiscard(card, TurnOwner.Enemy);
                    return true;
                }
            }
        }

        return false;
    }

    private List<CardView> GetMyFieldCards()
    {
        List<CardView> list = new List<CardView>();
        foreach (Transform slotTrans in GameManager.Instance.EnemyCharacterSlotsParent)
        {
            CharacterSlot slot = slotTrans.GetComponent<CharacterSlot>();
            if (slot != null && slot.occupiedCard != null) list.Add(slot.occupiedCard);
        }

        Transform leaderArea = GameManager.Instance.EnemyLeaderArea;
        if (leaderArea.childCount > 0)
        {
            CardView leaderCard = leaderArea.GetChild(0).GetComponent<CardView>();

            if (leaderCard != null)
            {
                list.Add(leaderCard);
            }
        }

        return list;
    }

    private CardView GetBestTarget()
    {
        //とりあえずプレイヤーリーダーを返す
        if (GameManager.Instance.PlayerLeaderArea.childCount > 0)
        {
            return GameManager.Instance.PlayerLeaderArea.GetChild(0).GetComponent<CardView>();
        }
        return null;
    }

    private CharacterSlot GetEmptyEnemySlot()
    {
        foreach (Transform child in GameManager.Instance.EnemyCharacterSlotsParent)
        {
            CharacterSlot slot = child.GetComponent<CharacterSlot>();
            if (slot != null && slot.occupiedCard == null) return slot;
        }
        return null;
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
                CardView bestAppealer = FindBestAppealTarget();
                CardView bestTarget = playerLeaderArea.GetComponentInChildren<CardView>();

                if (bestTarget != null)
                {
                    //アピール実行！
                    bool success = GameManager.Instance.AIPlayAppeal(card, bestAppealer, bestTarget);

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
        CardView leader = enemyLeaderArea.GetComponentInChildren<CardView>();
        if (leader != null)
        {
            int appeal = ((LeaderCard)leader.cardData).appeal + leader.appealBuff;
            if (appeal > maxAppeal) { maxAppeal = appeal; bestTarget = leader; }
        }

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
