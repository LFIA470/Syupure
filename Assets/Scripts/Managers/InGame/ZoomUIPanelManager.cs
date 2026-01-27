using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomUIPanelManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    public static ZoomUIPanelManager Instance;

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
    #endregion

    //変数宣言
    #region Variables
    public GameObject zoomPanel;       
    public Image artworkImage;          //イメージ画像（表示用
    public Image frameImage;            //フレーム画像（表示用
    public Text nameText;               //名前テキスト（表示用
    public Text descriptionText;        //効果テキスト（表示用
    [SerializeField] private Image costImage;       //コスト画像（表示用
    [SerializeField] private Image apeealPowerImage;    //アピール力画像（表示用
    [SerializeField] private Image apeealPowerIcon;     //アピール力アイコン（表示用
    [SerializeField] private Image mentalImage;         //メンタル画像（表示用
    [SerializeField] private Image mentalIcon;          //メンタルアイコン（表示用
    [SerializeField] private GameObject AppealContainer;    //拡大表示用のハート置き場
    [SerializeField] private GameObject HeartIconPrefab;   //ハート一つ分のプレファブ

    [SerializeField] private List<Sprite> frameSprites;     //フレームのスプライトを格納するリスト
    [SerializeField] private List<Sprite> numberSprites;    //0～9の数字スプライトを格納するリスト
    [SerializeField] private List<Sprite> powerNumberSprites;

    public GameObject playButton;       //プレイボタン
    private CardView zoomedCardView;    //どのcardViewを拡大しているかを記憶
    #endregion

    //拡大パネルの表示に関するメソッド
    #region Show Methods
    public void Show(CardView cardView) //拡大表示
    {
        //拡大表示用のカードが選ばれているか？
        if (cardView == null) return;
        if (cardView.isFaceDown) return;
        zoomedCardView = cardView;  //拡大元のカードを記憶

        Card card = cardView.cardData;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //カードごとの表示処理

        //コストとハート(アピール力)の表示リセット
        costImage.gameObject.SetActive(false);
        apeealPowerIcon.gameObject.SetActive(false);
        apeealPowerImage.gameObject.SetActive(false);
        mentalIcon.gameObject.SetActive(false);
        mentalImage.gameObject.SetActive(false);


        //カードタイプに応じて処理を分岐
        switch (cardView.cardData.cardType)
        {
            case CardType.Leader:
                //cardDataをLeaderCard型にキャスト
                LeaderCard leader = cardView.cardData as LeaderCard;
                frameImage.sprite = frameSprites[(int)leader.cardType];
                costImage.gameObject.SetActive(false);
                //ハート(アピール力)を表示
                if (leader.appeal >= 0)
                {
                    apeealPowerImage.sprite = powerNumberSprites[leader.appeal];
                    apeealPowerImage.gameObject.SetActive(true);
                    apeealPowerIcon.gameObject.SetActive(true);
                }
                break;
            case CardType.Character:
                //cardDataをCharacterCard型にキャスト
                CharacterCard character = cardView.cardData as CharacterCard;
                frameImage.sprite = frameSprites[(int)character.cardType];
                //コスト画像を表示
                costImage.sprite = numberSprites[character.cost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                if (character.appeal >= 0)
                {
                    apeealPowerImage.sprite = powerNumberSprites[character.appeal];
                    apeealPowerImage.gameObject.SetActive(true);
                    apeealPowerIcon.gameObject.SetActive(true);
                }
                if (character.mental >= 0)
                {
                    mentalImage.sprite = powerNumberSprites[character.mental];
                    mentalImage.gameObject.SetActive(true);
                    mentalIcon.gameObject.SetActive(true);
                }
                break;
            case CardType.EvolveCharacter:
                //cardDataをCharacterCard型にキャスト
                EvolveCharacterCard evolveCharacter = cardView.cardData as EvolveCharacterCard;
                frameImage.sprite = frameSprites[(int)evolveCharacter.cardType];
                //コスト画像を表示
                costImage.sprite = numberSprites[evolveCharacter.cost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                if (evolveCharacter.appeal >= 0)
                {
                    apeealPowerImage.sprite = powerNumberSprites[evolveCharacter.appeal];
                    apeealPowerImage.gameObject.SetActive(true);
                    apeealPowerIcon.gameObject.SetActive(true);
                }
                if (evolveCharacter.mental >= 0)
                {
                    mentalImage.sprite = powerNumberSprites[evolveCharacter.mental];
                    mentalImage.gameObject.SetActive(true);
                    mentalIcon.gameObject.SetActive(true);
                }
                break;
            case CardType.Accessory:
                AccessoryCard accessory = cardView.cardData as AccessoryCard;
                frameImage.sprite = frameSprites[(int)accessory.cardType];
                //コスト画像を表示
                costImage.sprite = numberSprites[accessory.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Appeal:
                //cardDataをAppealCard型にキャスト
                AppealCard appeal = cardView.cardData as AppealCard;
                frameImage.sprite = frameSprites[(int)appeal.cardType];
                //コスト画像を表示
                costImage.sprite = numberSprites[appeal.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Event:
                //cardDataをEventCard型にキャスト
                EventCard ev = cardView.cardData as EventCard;
                frameImage.sprite = frameSprites[(int)ev.cardType];
                //コスト画像を表示
                costImage.sprite = numberSprites[ev.cost];
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

    public void Hide()  //拡大パネル非表示
    {
        zoomedCardView = null;
        zoomPanel.SetActive(false);
    }
    #endregion

    //ボタン操作に関するメソッド
    #region Operation Button
    public void OnPlayButtonPressed()   //プレイボタンが押された時に呼ばれる
    {
        //GameMangerに盤面選択モード開始するように命令
        GameManager.Instance.ProcessPlayRequest(zoomedCardView);
        Debug.Log("盤面を選択してください");

        //拡大表示を閉じる
        Hide();
    }
    #endregion
}
