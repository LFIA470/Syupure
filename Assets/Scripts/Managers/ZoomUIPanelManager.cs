using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomUIPanelManager : MonoBehaviour
{
    public static ZoomUIPanelManager Instance;

    public GameObject zoomPanel;       
    public Image artworkImage;         //イメージ画像（表示用
    public Text nameText;              //名前テキスト（表示用
    public Text descriptionText;       //効果テキスト（表示用
    [SerializeField] private Image costImage;       //コスト画像（表示用
    [SerializeField] private GameObject AppealContainer;    //拡大表示用のハート置き場
    [SerializeField] private GameObject HeartIconPrefab;   //ハート一つ分のプレファブ

    [SerializeField] private List<Sprite> numberSprites;    //0～9の数字スプライトを格納するリスト

    public GameObject playButton;       //プレイボタン
    private CardView zoomedCardView;    //どのcardViewを拡大しているかを記憶

    void Awake()
    {
        // Singleton設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        zoomPanel.SetActive(false); // 最初は非表示
    }

    //拡大表示
    public void Show(CardView cardView)
    {
        //拡大表示用のカードが選ばれているか？
        if (cardView == null) return;
        zoomedCardView = cardView;  //拡大元のカードを記憶

        Card card = cardView.cardData;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //カードごとの表示処理

        //コストとハート(アピール力)の表示リセット
        costImage.gameObject.SetActive(false);
        foreach (Transform child in AppealContainer.transform) { Destroy(child.gameObject); }

        //カードタイプに応じて処理を分岐
        switch (cardView.cardData.cardType)
        {
            case CardType.Leader:
                //cardDataをLeaderCard型にキャスト
                LeaderCard leader = cardView.cardData as LeaderCard;
                costImage.sprite = numberSprites[leader.evolveCost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < leader.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Character:
                //cardDataをCharacterCard型にキャスト
                CharacterCard character = cardView.cardData as CharacterCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[character.cost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < character.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.EvolveCharacter:
                //cardDataをCharacterCard型にキャスト
                EvolveCharacterCard evolveCharacter = cardView.cardData as EvolveCharacterCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[evolveCharacter.evolveCost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < evolveCharacter.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Event:
                //cardDataをEventCard型にキャスト
                EventCard ev = cardView.cardData as EventCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[ev.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Appeal:
                //cardDataをAppealCard型にキャスト
                AppealCard appeal = cardView.cardData as AppealCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[appeal.cost];
                costImage.gameObject.SetActive(true);
                break;

        }

        //もしそのカードが手札にあってプレイ可能なら「プレイ」ボタンを表示
        if (GameManager.Instance.IsPlayable(cardView))
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }

        zoomPanel.SetActive(true);
    }

    //プレイボタンが押された時に呼ばれる
    public void OnPlayButtonPressed()
    {
        //GameMangerに盤面選択モード開始するように命令
        GameManager.Instance.EnterTargetingMode(zoomedCardView);
        Debug.Log("盤面を選択してください");

        //拡大表示を閉じる
        Hide();
    }

    //閉じるボタン
    public void Hide()
    {
        zoomedCardView = null;
        zoomPanel.SetActive(false);
    }
}
