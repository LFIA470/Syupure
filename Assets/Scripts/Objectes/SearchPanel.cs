using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchPanel : MonoBehaviour
{
    public GameObject cardPrefab;   //カードを表示するためのプレファブ
    public Transform cardContainer; //カードを並べる場所
    public Button decideButton;     //「決定」ボタン

    private List<Card> currentChoices = new List<Card>();   //提示されたカード
    private List<Card> selectedCards = new List<Card>();    //ユーザーが選んだカード
    private int maxSelectCount = 1; //選べる枚数

    void Start()
    {
        // 決定ボタンを押したら、GameManagerに報告してパネルを閉じる
        decideButton.onClick.AddListener(OnDecideButtonClicked);
    }

    public void Open(List<Card> choices, int selectCount)
    {
        currentChoices = choices;
        maxSelectCount = selectCount;
        selectedCards.Clear();

        // 表示のリセット
        foreach (Transform child in cardContainer) Destroy(child.gameObject);
        this.gameObject.SetActive(true);


        // カードを並べる
        foreach (Card data in choices)
        {
            GameObject obj = Instantiate(cardPrefab, cardContainer);
            CardView view = obj.GetComponent<CardView>();
            view.SetCard(data); // 表示のみなのでisPlayer=trueでOK
            view.isZoomPanel = false;

            // クリックしたら選択状態にするためのボタン機能を追加
            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardClicked(data, view));
        }
    }

    // カードをクリックした時の処理
    void OnCardClicked(Card data, CardView view)
    {
        if (selectedCards.Contains(data))
        {
            // 選択解除
            selectedCards.Remove(data);
            view.frameImage.color = Color.white; // 選択解除の色（白）
        }
        else
        {
            // 選択（上限枚数チェック）
            if (selectedCards.Count < maxSelectCount)
            {
                selectedCards.Add(data);
                view.frameImage.color = Color.yellow; // 選択中の色（黄色）
            }
        }
    }

    // 決定ボタン処理
    void OnDecideButtonClicked()
    {
        // 1枚も選んでいない場合どうするか？（必須ならここでreturn）

        // GameManagerに結果を返す
        GameManager.Instance.OnSearchCompleted(selectedCards);

        // パネルを閉じる
        this.gameObject.SetActive(false);
    }
}
