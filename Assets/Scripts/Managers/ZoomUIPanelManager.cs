using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomUIPanelManager : MonoBehaviour
{
    public static ZoomUIPanelManager Instance;

    public GameObject zoomPanel;       
    public Image artworkImage;         //�C���[�W�摜�i�\���p
    public Text nameText;              //���O�e�L�X�g�i�\���p
    public Text descriptionText;       //���ʃe�L�X�g�i�\���p
    [SerializeField] private Image costImage;       //�R�X�g�摜�i�\���p
    [SerializeField] private GameObject AppealContainer;    //�g��\���p�̃n�[�g�u����
    [SerializeField] private GameObject HeartIconPrefab;   //�n�[�g����̃v���t�@�u

    [SerializeField] private List<Sprite> numberSprites;    //0�`9�̐����X�v���C�g���i�[���郊�X�g

    public GameObject playButton;       //�v���C�{�^��
    private CardView zoomedCardView;    //�ǂ�cardView���g�債�Ă��邩���L��

    void Awake()
    {
        // Singleton�ݒ�
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        zoomPanel.SetActive(false); // �ŏ��͔�\��
    }

    //�g��\��
    public void Show(CardView cardView)
    {
        //�g��\���p�̃J�[�h���I�΂�Ă��邩�H
        if (cardView == null) return;
        zoomedCardView = cardView;  //�g�匳�̃J�[�h���L��

        Card card = cardView.cardData;

        nameText.text = card.cardName;
        descriptionText.text = card.description;
        artworkImage.sprite = card.artwork;

        //�J�[�h���Ƃ̕\������

        //�R�X�g�ƃn�[�g(�A�s�[����)�̕\�����Z�b�g
        costImage.gameObject.SetActive(false);
        foreach (Transform child in AppealContainer.transform) { Destroy(child.gameObject); }

        //�J�[�h�^�C�v�ɉ����ď����𕪊�
        switch (cardView.cardData.cardType)
        {
            case CardType.Leader:
                //cardData��LeaderCard�^�ɃL���X�g
                LeaderCard leader = cardView.cardData as LeaderCard;
                costImage.sprite = numberSprites[leader.evolveCost];
                costImage.gameObject.SetActive(true);
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < leader.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Character:
                //cardData��CharacterCard�^�ɃL���X�g
                CharacterCard character = cardView.cardData as CharacterCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[character.cost];
                costImage.gameObject.SetActive(true);
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < character.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.EvolveCharacter:
                //cardData��CharacterCard�^�ɃL���X�g
                EvolveCharacterCard evolveCharacter = cardView.cardData as EvolveCharacterCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[evolveCharacter.evolveCost];
                costImage.gameObject.SetActive(true);
                //�n�[�g(�A�s�[����)��\��
                for (int i = 0; i < evolveCharacter.appeal; i++) { Instantiate(HeartIconPrefab, AppealContainer.transform); }
                break;
            case CardType.Event:
                //cardData��EventCard�^�ɃL���X�g
                EventCard ev = cardView.cardData as EventCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[ev.cost];
                costImage.gameObject.SetActive(true);
                break;
            case CardType.Appeal:
                //cardData��AppealCard�^�ɃL���X�g
                AppealCard appeal = cardView.cardData as AppealCard;
                //�R�X�g�摜��\��
                costImage.sprite = numberSprites[appeal.cost];
                costImage.gameObject.SetActive(true);
                break;

        }

        //�������̃J�[�h����D�ɂ����ăv���C�\�Ȃ�u�v���C�v�{�^����\��
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

    //�v���C�{�^���������ꂽ���ɌĂ΂��
    public void OnPlayButtonPressed()
    {
        //GameManger�ɔՖʑI�����[�h�J�n����悤�ɖ���
        GameManager.Instance.EnterTargetingMode(zoomedCardView);
        Debug.Log("�Ֆʂ�I�����Ă�������");

        //�g��\�������
        Hide();
    }

    //����{�^��
    public void Hide()
    {
        zoomedCardView = null;
        zoomPanel.SetActive(false);
    }
}
