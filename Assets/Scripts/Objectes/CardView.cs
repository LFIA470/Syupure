using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Animation;

public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    //変数宣言
    #region Variables
    [Header("CardData")]
    [SerializeField] private Card _cardData;    //Inspectorで設定する場合
    public Card cardData　=> _cardData;  //外部からは読み取り専用

    [Header("UI References")]
    public Image artworkImage;      //イメージ画像（表示用
    public Text nameText;           //名前テキスト（表示用
    public Text descriptionText;    //効果テキスト（表示用
    [SerializeField] private Image costImage;   //コスト画像（表示用
    [SerializeField] private GameObject AppealContainer;    //ハート(アピール力)を入れる箱

    [Header("Asset References")]
    [SerializeField] private GameObject HeartIconPrefab;   //ハートアイコンのプレファブ
    [SerializeField] private List<Sprite> numberSprites;    //0～10の数字スプライトを格納するリスト

    [Header("Drag & Drop")]
    private bool isDraggable = false;    //ドラッグ中か確認
    private RectTransform canvasRectTransform;  //ドラッグ座標計算用
    public Transform originalParent { get; private set; }   //カードの元の場所を覚えておく変数

    [Header("Buff")]
    public int appealBuff = 0;  //アピール力への一時的な補正値(バフ/デバフ)
    
    #endregion

    //Start,UpdateなどUnityが自動で呼ぶメソッド
    # region Unity Lifecycle Methods
    void Start()
    {
        //CanvasのRectTransformを最初に取得しておく
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }
    #endregion

    //カードを表示するためのメソッド
    #region Card View Methods
    public void SetCard(Card card)  //カード情報
    {
        _cardData = card;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //カード種類ごとの表示処理

        //コストとハート(アピール力)の表示リセット
        costImage.gameObject.SetActive(false);
        while (AppealContainer.transform.childCount > 0)
        {
            Destroy(AppealContainer.transform.GetChild(0).gameObject);
        }

        //カードタイプに応じて処理を分岐
        switch (cardData.cardType)
        {
            case CardType.Leader:
                //cardDataをLeaderCard型にキャスト
                LeaderCard leader = cardData as LeaderCard;
                //ハート(アピール力)を表示
                for (int i = 0; i < leader.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Character:
                //cardDataをCharacterCard型にキャスト
                CharacterCard character = cardData as CharacterCard;
                //コスト画像を表示
                if (character.cost >= 0 && character.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[character.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);  //対応画像がない場合は非表示
                    Debug.LogWarning("コスト" + character.cost + "に対応する画像がありません。");
                }
                //ハート(アピール力)を表示
                for (int i = 0; i < character.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.EvolveCharacter:
                //cardDataをCharacterCard型にキャスト
                EvolveCharacterCard evolveCharacter = cardData as EvolveCharacterCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[evolveCharacter.cost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < evolveCharacter.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Event:
                //cardDataをEventCard型にキャスト
                EventCard ev = cardData as EventCard;
                //コスト画像を表示
                if (ev.cost >= 0 && ev.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[ev.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);
                    Debug.LogWarning("コスト" + ev.cost + "に対応する画像がありません。");
                }
                    break;
            case CardType.Appeal:
                //cardDataをAppealCard型にキャスト
                AppealCard appeal = cardData as AppealCard;
                //コスト画像を表示
                if (appeal.cost >= 0 && appeal.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[appeal.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);
                    Debug.LogWarning("コスト" + appeal.cost + "に対応する画像がありません。");
                }
                break;
        }
    }
    #endregion

    //カードのクリックに関連するメソッド
    #region Click Methods
    public void OnPointerClick(PointerEventData eventData)  //カードがクリックされたか
    {
        //もしドラッグ中だったら、拡大表示せずに処理を終了する
        if (eventData.dragging) return;

        //GameManagerに「現在のモード」を問い合わせる
        if (GameManager.Instance.IsTargetingMode())
        {
            Debug.Log(this.cardData.cardName + "がターゲットとしてクリックされました。");
            GameManager.Instance.OnFieldClicked(this.transform);
        }
        else
        {
            Debug.Log("Card clicked: " + nameText.text);
            ZoomUIPanelManager.Instance.Show(this);
        }
    }
    #endregion

    //カードのドラッグ&ドロップに関連するメソッド
    #region Drag Drop Methods
    public void ManuallyBeginDrag   //HandAreaなど、外部のスクリプトからドラッグ操作を開始させるためのメソッド
    (PointerEventData eventData)   
    {
        // OnBeginDragと全く同じ処理を実行する

        // (ドラッグ可能かのチェック)
        if (transform.parent != GameManager.Instance.PlayerHandArea)
        {
            isDraggable = false;
            eventData.pointerDrag = null;
            return;
        }

        isDraggable = true;
        originalParent = transform.parent;
        transform.SetParent(transform.root);

        transform.localRotation = Quaternion.identity;

        GetComponent<CanvasGroup>().blocksRaycasts = false;

        // ★重要★ Unityのイベントシステムに、ドラッグ対象がこのカードに移ったことを知らせる
        eventData.pointerDrag = this.gameObject;
    }

    public void OnBeginDrag(PointerEventData eventData) //ドラッグが始まった瞬間に呼ばれる
    {
        ManuallyBeginDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)  //ドラッグ中に呼ばれる
    {
        if (!isDraggable) return;

        //スクリーン座標をCanvasのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera, //スクリーン座標を計算するためのカメラ
            out Vector2 localPosition
            );

        //変換したローカル座標をカードの座標に設定
        transform.localPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)   //ドラッグが終了した瞬間に呼ばれる
    {
        if (eventData.pointerEnter != null)
        {
            Debug.Log("ドラッグの終了地点にあるオブジェクト: " + eventData.pointerEnter.name);
        }
        else
        {
            Debug.Log("ドラッグの終了地点には何もUIがありませんでした。");
        }

        if (!isDraggable) return;

        //ドロップされなかった場合(元の場所に戻す)
        if (transform.parent == transform.root)
        {
            ReturnToOriginalParent();    //元いた手札エリアに戻す
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;  //マウスイベントを再び受け付けるようにする
    }

    public void OnDrop (PointerEventData eventData) //カードの上にカードをドロップされたか
    {
        //ドロップされたカード(進化キャラクター)を取得
        CardView droppedCard = eventData.pointerDrag.GetComponent<CardView>();
        if (droppedCard == null) return;

        CardType type = droppedCard.cardData.cardType;

        if (type == CardType.EvolveCharacter)
        {
            //GameMangerに「このカードの上に、進化カードがドロップされました」と報告
            //thisはドロップされた側(進化元)のカード
            GameManager.Instance.CardDroppedOnCharacter(droppedCard, this);
        }
    }

    public void ReturnToOriginalParent()    //プレイが失敗した時にGameManagerから呼び出す
    {
        transform.SetParent(originalParent, false);
    }
    #endregion
}
