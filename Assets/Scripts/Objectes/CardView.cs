using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Card cardData;   //ScriptableObject �ȂǂŒ�`���ꂽ�J�[�h���
   
    public Image artworkImage;      //�C���[�W�摜�i�\���p
    public Text nameText;           //���O�e�L�X�g�i�\���p
    public Text descriptionText;    //���ʃe�L�X�g�i�\���p
    [SerializeField] private Image costImage;   //�R�X�g�摜�i�\���p
    [SerializeField] private GameObject AppealContainer;    //�n�[�g(�A�s�[����)�����锠
    [SerializeField] private GameObject HeartIconPrefab;   //�n�[�g�A�C�R���̃v���t�@�u

    [SerializeField] private List<Sprite> numberSprites;    //0�`9�̐����X�v���C�g���i�[���郊�X�g

    private Transform fieldArea;    //�t�B�[���h�̈�ւ̎Q��

    private RectTransform canvasRectTransform;

    private bool isDraggble = false;    //�h���b�O�����m�F

    void Start()
    {
        //Canvas��RectTransform���ŏ��Ɏ擾���Ă���
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    //�J�[�h���
    public void SetCard(Card card)
    {
        cardData = card;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //�J�[�h��ނ��Ƃ̕\������

        //�R�X�g�ƃn�[�g(�A�s�[����)�̕\�����Z�b�g
        costImage.gameObject.SetActive(false);
        foreach (Transform child in AppealContainer.transform) { Destroy(child.gameObject); }

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
                costImage.sprite = numberSprites[character.cost];
                costImage.gameObject.SetActive(true);
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
                costImage.sprite = numberSprites[ev.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Appeal:
                //cardData��AppealCard�^�ɃL���X�g
                AppealCard appeal = cardData as AppealCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[appeal.cost];
                costImage.gameObject.SetActive(true);
                break;
        }
    }

    public void SetFieldArea(Transform area)
    {
        fieldArea = area;
    }

    //�J�[�h���N���b�N���ꂽ��
    public void OnPointerClick(PointerEventData eventData)
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

    //originalParent���A���̃X�N���v�g����ǂݎ���悤��
    public Transform originalParent { get; private set; }   //�J�[�h�̌��̏ꏊ���o���Ă����ϐ�

    //�h���b�O���n�܂����u�ԂɌĂ΂��
    public void OnBeginDrag(PointerEventData eventData)
    {
        //���̃J�[�h�̐e���AGameManager���m���Ă����D�G���A�Ɠ������ǂ������`�F�b�N
        if (transform.parent != GameManager.Instance.PlayerHandArea)
        {
            Debug.Log("��D�̃J�[�h�ł͂Ȃ����߁A�h���b�O�ł��܂���");
            isDraggble = false; //�����ȃh���b�O�Ƃ��ċL��
            eventData.pointerDrag = null;   //Unity�Ƀh���b�O������L�����Z������悤�ɓ`����
            return;
        }

        isDraggble = true;
        originalParent = transform.parent;  //�������ꏊ���L��
        transform.SetParent(transform.root);    //�ꎞ�I�ɍőO�ʂɕ\�����邽�߁A�e��Canvas�̃��[�g�ɂ���
        GetComponent<CanvasGroup>().blocksRaycasts = false; //�h���b�O���̓J�[�h���g���}�E�X�C�x���g���u���b�N���Ȃ��悤�ɂ���
    }

    //�h���b�O���ɌĂ΂��
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggble) return;

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

    //�h���b�O���I�������u�ԂɌĂ΂��
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            Debug.Log("�h���b�O�̏I���n�_�ɂ���I�u�W�F�N�g: " + eventData.pointerEnter.name);
        }
        else
        {
            Debug.Log("�h���b�O�̏I���n�_�ɂ͉���UI������܂���ł����B");
        }

        if (!isDraggble) return;

        //�h���b�v����Ȃ������ꍇ(���̏ꏊ�ɖ߂�)
        if (transform.parent == transform.root)
        {
            ReturnToOriginalParent();    //��������D�G���A�ɖ߂�
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;  //�}�E�X�C�x���g���Ăю󂯕t����悤�ɂ���
    }

    //�J�[�h�̏�ɃJ�[�h���h���b�v���ꂽ��
    public void OnDrop (PointerEventData eventData)
    {
        //�h���b�v���ꂽ�J�[�h(�i���L�����N�^�[)���擾
        CardView evoluveCard = eventData.pointerDrag.GetComponent<CardView>();
        if (evoluveCard == null) return;

        //GameManger�Ɂu���̃J�[�h�̏�ɁA�i���J�[�h���h���b�v����܂����v�ƕ�
        //this�̓h���b�v���ꂽ��(�i����)�̃J�[�h
        GameManager.Instance.CardDroppedOnCharacter(evoluveCard, this);
    }

    //�v���C�����s��������GameManager����Ăяo��
    public void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent, false);
    }
}
