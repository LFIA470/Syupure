using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Card cardData;   //ScriptableObject などで定義されたカード情報
   
    public Image artworkImage;      //イメージ画像（表示用
    public Text nameText;           //名前テキスト（表示用
    public Text descriptionText;    //効果テキスト（表示用
    [SerializeField] private Image costImage;   //コスト画像（表示用
    [SerializeField] private GameObject AppealContainer;    //ハート(アピール力)を入れる箱
    [SerializeField] private GameObject HeartIconPrefab;   //ハートアイコンのプレファブ

    [SerializeField] private List<Sprite> numberSprites;    //0～9の数字スプライトを格納するリスト

    private Transform fieldArea;    //フィールド領域への参照

    private RectTransform canvasRectTransform;

    private bool isDraggble = false;    //ドラッグ中か確認

    void Start()
    {
        //CanvasのRectTransformを最初に取得しておく
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    //カード情報
    public void SetCard(Card card)
    {
        cardData = card;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //カード種類ごとの表示処理

        //コストとハート(アピール力)の表示リセット
        costImage.gameObject.SetActive(false);
        foreach (Transform child in AppealContainer.transform) { Destroy(child.gameObject); }

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
                costImage.sprite = numberSprites[character.cost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < character.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.EvolveCharacter:
                //cardDataをCharacterCard型にキャスト
                EvolveCharacterCard evolveCharacter = cardData as EvolveCharacterCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[evolveCharacter.evolveCost];
                costImage.gameObject.SetActive(true);
                //ハート(アピール力)を表示
                for (int i = 0; i < evolveCharacter.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Event:
                //cardDataをEventCard型にキャスト
                EventCard ev = cardData as EventCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[ev.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Appeal:
                //cardDataをAppealCard型にキャスト
                AppealCard appeal = cardData as AppealCard;
                //コスト画像を表示
                costImage.sprite = numberSprites[appeal.cost];
                costImage.gameObject.SetActive(true);
                break;
        }
    }

    public void SetFieldArea(Transform area)
    {
        fieldArea = area;
    }

    //カードがクリックされたか
    public void OnPointerClick(PointerEventData eventData)
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

    //originalParentを、他のスクリプトから読み取れるように
    public Transform originalParent { get; private set; }   //カードの元の場所を覚えておく変数

    //ドラッグが始まった瞬間に呼ばれる
    public void OnBeginDrag(PointerEventData eventData)
    {
        //このカードの親が、GameManagerが知っている手札エリアと同じかどうかをチェック
        if (transform.parent != GameManager.Instance.PlayerHandArea)
        {
            Debug.Log("手札のカードではないため、ドラッグできません");
            isDraggble = false; //無効なドラッグとして記憶
            eventData.pointerDrag = null;   //Unityにドラッグ操作をキャンセルするように伝える
            return;
        }

        isDraggble = true;
        originalParent = transform.parent;  //元いた場所を記憶
        transform.SetParent(transform.root);    //一時的に最前面に表示するため、親をCanvasのルートにする
        GetComponent<CanvasGroup>().blocksRaycasts = false; //ドラッグ中はカード自身がマウスイベントをブロックしないようにする
    }

    //ドラッグ中に呼ばれる
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggble) return;

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

    //ドラッグが終了した瞬間に呼ばれる
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            Debug.Log("ドラッグの終了地点にあるオブジェクト: " + eventData.pointerEnter.name);
        }
        else
        {
            Debug.Log("ドラッグの終了地点には何もUIがありませんでした。");
        }

        if (!isDraggble) return;

        //ドロップされなかった場合(元の場所に戻す)
        if (transform.parent == transform.root)
        {
            ReturnToOriginalParent();    //元いた手札エリアに戻す
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;  //マウスイベントを再び受け付けるようにする
    }

    //カードの上にカードをドロップされたか
    public void OnDrop (PointerEventData eventData)
    {
        //ドロップされたカード(進化キャラクター)を取得
        CardView evoluveCard = eventData.pointerDrag.GetComponent<CardView>();
        if (evoluveCard == null) return;

        //GameMangerに「このカードの上に、進化カードがドロップされました」と報告
        //thisはドロップされた側(進化元)のカード
        GameManager.Instance.CardDroppedOnCharacter(evoluveCard, this);
    }

    //プレイが失敗した時にGameManagerから呼び出す
    public void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent, false);
    }
}
