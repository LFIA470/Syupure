using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Animation;

public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    //�ϐ��錾
    #region Variables
    [Header("CardData")]
    [SerializeField] private Card _cardData;    //Inspector�Őݒ肷��ꍇ
    public Card cardData�@=> _cardData;  //�O������͓ǂݎ���p

    [Header("UI References")]
    public Image artworkImage;      //�C���[�W�摜�i�\���p
    public Text nameText;           //���O�e�L�X�g�i�\���p
    public Text descriptionText;    //���ʃe�L�X�g�i�\���p
    [SerializeField] private Image costImage;   //�R�X�g�摜�i�\���p
    [SerializeField] private GameObject AppealContainer;    //�n�[�g(�A�s�[����)�����锠

    [Header("Asset References")]
    [SerializeField] private GameObject HeartIconPrefab;   //�n�[�g�A�C�R���̃v���t�@�u
    [SerializeField] private List<Sprite> numberSprites;    //0�`10�̐����X�v���C�g���i�[���郊�X�g

    [Header("Drag & Drop")]
    private bool isDraggable = false;    //�h���b�O�����m�F
    private RectTransform canvasRectTransform;  //�h���b�O���W�v�Z�p
    public Transform originalParent { get; private set; }   //�J�[�h�̌��̏ꏊ���o���Ă����ϐ�
    #endregion

    //Start,Update�Ȃ�Unity�������ŌĂԃ��\�b�h
    # region Unity Lifecycle Methods
    void Start()
    {
        //Canvas��RectTransform���ŏ��Ɏ擾���Ă���
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }
    #endregion

    //�J�[�h��\�����邽�߂̃��\�b�h
    #region Card View Methods
    public void SetCard(Card card)  //�J�[�h���
    {
        _cardData = card;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //�J�[�h��ނ��Ƃ̕\������

        //�R�X�g�ƃn�[�g(�A�s�[����)�̕\�����Z�b�g
        costImage.gameObject.SetActive(false);
        while (AppealContainer.transform.childCount > 0)
        {
            Destroy(AppealContainer.transform.GetChild(0).gameObject);
        }

        //�J�[�h�^�C�v�ɉ����ď����𕪊�
        switch (cardData.cardType)
        {
            case CardType.Leader:
                //cardData��LeaderCard�^�ɃL���X�g
                LeaderCard leader = cardData as LeaderCard;
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < leader.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Character:
                //cardData��CharacterCard�^�ɃL���X�g
                CharacterCard character = cardData as CharacterCard;
                //�R�X�g�摜��\��
                if (character.cost >= 0 && character.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[character.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);  //�Ή��摜���Ȃ��ꍇ�͔�\��
                    Debug.LogWarning("�R�X�g" + character.cost + "�ɑΉ�����摜������܂���B");
                }
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < character.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.EvolveCharacter:
                //cardData��CharacterCard�^�ɃL���X�g
                EvolveCharacterCard evolveCharacter = cardData as EvolveCharacterCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[evolveCharacter.evolveCost];
                costImage.gameObject.SetActive(true);
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < evolveCharacter.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Event:
                //cardData��EventCard�^�ɃL���X�g
                EventCard ev = cardData as EventCard;
                //�R�X�g�摜��\��
                if (ev.cost >= 0 && ev.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[ev.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);
                    Debug.LogWarning("�R�X�g" + ev.cost + "�ɑΉ�����摜������܂���B");
                }
                    break;
            case CardType.Appeal:
                //cardData��AppealCard�^�ɃL���X�g
                AppealCard appeal = cardData as AppealCard;
                //�R�X�g�摜��\��
                if (appeal.cost >= 0 && appeal.cost < numberSprites.Count)
                {
                    costImage.sprite = numberSprites[appeal.cost];
                    costImage.gameObject.SetActive(true);
                }
                else
                {
                    costImage.gameObject.SetActive(false);
                    Debug.LogWarning("�R�X�g" + appeal.cost + "�ɑΉ�����摜������܂���B");
                }
                break;
        }
    }
    #endregion

    //�J�[�h�̃N���b�N�Ɋ֘A���郁�\�b�h
    #region Click Methods
    public void OnPointerClick(PointerEventData eventData)  //�J�[�h���N���b�N���ꂽ��
    {
        //�����h���b�O����������A�g��\�������ɏ������I������
        if (eventData.dragging) return;

        //GameManager�Ɂu���݂̃��[�h�v��₢���킹��
        if (GameManager.Instance.IsTargetingMode())
        {
            Debug.Log(this.cardData.cardName + "���^�[�Q�b�g�Ƃ��ăN���b�N����܂����B");
            GameManager.Instance.OnFieldClicked(this.transform);
        }
        else
        {
            Debug.Log("Card clicked: " + nameText.text);
            ZoomUIPanelManager.Instance.Show(this);
        }
    }
    #endregion

    //�J�[�h�̃h���b�O&�h���b�v�Ɋ֘A���郁�\�b�h
    #region Drag Drop Methods
    public void OnBeginDrag(PointerEventData eventData) //�h���b�O���n�܂����u�ԂɌĂ΂��
    {
        //���̃J�[�h�̐e���AGameManager���m���Ă����D�G���A�Ɠ������ǂ������`�F�b�N
        if (transform.parent != GameManager.Instance.PlayerHandArea)
        {
            Debug.Log("��D�̃J�[�h�ł͂Ȃ����߁A�h���b�O�ł��܂���");
            isDraggable = false; //�����ȃh���b�O�Ƃ��ċL��
            eventData.pointerDrag = null;   //Unity�Ƀh���b�O������L�����Z������悤�ɓ`����
            return;
        }

        isDraggable = true;
        originalParent = transform.parent;  //�������ꏊ���L��
        transform.SetParent(transform.root);    //�ꎞ�I�ɍőO�ʂɕ\�����邽�߁A�e��Canvas�̃��[�g�ɂ���
        GetComponent<CanvasGroup>().blocksRaycasts = false; //�h���b�O���̓J�[�h���g���}�E�X�C�x���g���u���b�N���Ȃ��悤�ɂ���
    }
    
    public void OnDrag(PointerEventData eventData)  //�h���b�O���ɌĂ΂��
    {
        if (!isDraggable) return;

        //�X�N���[�����W��Canvas�̃��[�J�����W�ɕϊ�
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera, //�X�N���[�����W���v�Z���邽�߂̃J����
            out Vector2 localPosition
            );

        //�ϊ��������[�J�����W���J�[�h�̍��W�ɐݒ�
        transform.localPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)   //�h���b�O���I�������u�ԂɌĂ΂��
    {
        if (eventData.pointerEnter != null)
        {
            Debug.Log("�h���b�O�̏I���n�_�ɂ���I�u�W�F�N�g: " + eventData.pointerEnter.name);
        }
        else
        {
            Debug.Log("�h���b�O�̏I���n�_�ɂ͉���UI������܂���ł����B");
        }

        if (!isDraggable) return;

        //�h���b�v����Ȃ������ꍇ(���̏ꏊ�ɖ߂�)
        if (transform.parent == transform.root)
        {
            ReturnToOriginalParent();    //��������D�G���A�ɖ߂�
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;  //�}�E�X�C�x���g���Ăю󂯕t����悤�ɂ���
    }

    public void OnDrop (PointerEventData eventData) //�J�[�h�̏�ɃJ�[�h���h���b�v���ꂽ��
    {
        //�h���b�v���ꂽ�J�[�h(�i���L�����N�^�[)���擾
        CardView evolveCard = eventData.pointerDrag.GetComponent<CardView>();
        if (evolveCard == null) return;

        //GameManger�Ɂu���̃J�[�h�̏�ɁA�i���J�[�h���h���b�v����܂����v�ƕ�
        //this�̓h���b�v���ꂽ��(�i����)�̃J�[�h
        GameManager.Instance.CardDroppedOnCharacter(evolveCard, this);
    }

    public void ReturnToOriginalParent()    //�v���C�����s��������GameManager����Ăяo��
    {
        transform.SetParent(originalParent, false);
    }
    #endregion
}
