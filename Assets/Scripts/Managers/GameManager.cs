using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TurnOwner
{
    Player,
    Enemy,
}

public class GameManager : MonoBehaviour
{
    //�V���O���g��
    #region Singleton
    //�V���O���g���ݒ�(�ǂ�����ł��A�N�Z�X�o����i�ߓ��ɂ���)
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    //�ϐ��錾
    #region Variables
    [Header("Component References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Transform playerHandArea;  //��D�G���A��Transform��ݒ�
    public Transform PlayerHandArea { get { return playerHandArea; } }
        
    [SerializeField] private Transform playerLeaderArea;    //���[�_�[�G���A��Transform��ݒ�
    public Transform PlayerLeaderArea { get { return playerLeaderArea; } }

    [SerializeField] private Transform playerCharacterSlotsParent;  //�L�����N�^�[�X���b�g��Trasnform��ݒ�
    public Transform PlayerCharacterSlotsParent { get { return playerCharacterSlotsParent; } }

    [SerializeField] private Transform enemyLeaderArea; //���[�_�[�G���A��Transform��ݒ聦����p
    public Transform EnemyLeaderArea { get {return enemyLeaderArea; } }

    [SerializeField] private Transform enemyCharacterSlotsParent;   //�L�����N�^�[�X���b�g��Transform��ݒ聦����p
    public Transform EnemyCharacterSlotsParent { get {return enemyCharacterSlotsParent; } }

    [Header("Game State")]
    public bool isPlayerTurn = true;        //�^�[���m�F
    private bool isTargetingMode = false;   //�ՖʑI�����[�h�����ǂ����̃t���O
    private CardView cardToPlay;            //�v���C���悤�Ƃ��Ă���J�[�h

    [Header("Player & Enemy Stats")]
    public int playerMana = 0;              //�}�i(�v���C���[)
    public int playerAppealPoints = 0;      //�v���C���[�A�s�[���|�C���g(��������)
    public int enemyMana = 0;               //�}�i(����)
    public int enemyAppealPoints = 0;       //����A�s�[���|�C���g(����̏�������)

    public CardView selectedCard;           //�I������Ă���J�[�h���ꎞ�I�ɕۑ�����
    #endregion

    //Start,Update�Ȃ�Unity�������ŌĂԃ��\�b�h
    #region Unity Lifecycle Methods
    private void Start()
    {
        //�Q�[���X�^�[�g
        StartGame();
    }
    #endregion

    //�Q�[���̗�����Ǘ����郁�\�b�h
    #region Game Flow Methods
    public void StartGame() //�Q�[���̏����ƊJ�n���s��
    {
        Debug.Log("�Q�[�����J�n���܂��B");

        //���v���C���[�̃f�b�L�V���b�t��
        deckManager.DeckShuffle(TurnOwner.Player);
        deckManager.DeckShuffle(TurnOwner.Enemy);

        //������D��z��
        for (int i = 0; i < GameConstants.StartingHanSize; i++)
        {
            deckManager.DrawCard(TurnOwner.Player);
            deckManager.DrawCard(TurnOwner.Enemy);
        }

        //��U�E��U�����߂�

        //�ŏ��̃^�[�����J�n����
        StartTurn(TurnOwner.Player);
    }
    public void StartTurn(TurnOwner owner)  //�^�[���ڍs(�J�n)
    {
        Debug.Log(owner + "�̃^�[�����J�n���܂��B");

        if (owner == TurnOwner.Player)
        {
            isPlayerTurn = true;

            //�}�i��
            playerMana = GameConstants.DefaultMaxMana;

            //�}�i��UI���X�V

            //�J�[�h���ꖇ����
            deckManager.DrawCard(TurnOwner.Player);
        }
        else
        {
            isPlayerTurn = false;
            //����̃}�i����
            enemyMana = GameConstants.DefaultMaxMana;

            //����̃}�iUI���X�V

            //���肪�J�[�h���ꖇ����
            deckManager.DrawCard(TurnOwner.Enemy);
        }
    }
    public void EndTurn(TurnOwner owner)//�^�[���ڍs(�I��)
    {
        switch (owner)
        {
            case TurnOwner.Player:
                //�v���C���[���^�[�����I�������ꍇ
                if (isPlayerTurn)   //�����̃^�[������Ȃ��̂ɉ������̂�h��
                {
                    isPlayerTurn = false;
                    Debug.Log("�v���C���[�̃^�[���I���B����̃^�[�����J�n���܂��B");
                    StartTurn(TurnOwner.Enemy);
                }
                break;
            case TurnOwner.Enemy:
                //���肪�^�[�����I�������ꍇ
                if (!isPlayerTurn)
                {
                    isPlayerTurn = true;
                    Debug.Log("����̃^�[���I���B�v���C���[�̃^�[�����J�n���܂��B");
                    StartTurn(TurnOwner.Player);
                }
                break;
        }
    }
    #endregion

    //�J�[�h�̃v���C�Ɋւ��郁�\�b�h
    #region Card Playing Methods
    public void OnCardClicked(CardView cardView)//�J�[�h���N���b�N���ꂽ����CardView����Ă΂��
    {
        //���łɑI�𒆂̃J�[�h������΁A�������U���Z�b�g(�I������)
        if (selectedCard != null)
        {
            //�I�𒆂̃J�[�h�̌����ڂ����ɖ߂�(�ǉ��\��)
        }

        //�V�����N���b�N���ꂽ�J�[�h��I�𒆂ɂ���
        selectedCard = cardView;
        Debug.Log(selectedCard.cardData.cardName + "��I�����܂���");

        //�I�����ꂽ�J�[�h�̌����ڂ�ς��鏈��
    }
    public void CardDroppedOnSlot(CardView card, CharacterSlot slot)//�L�����N�^�[�X���b�g�Ƀh���b�v���ꂽ���̏���
    {
        //���[���`�F�b�N
        //�J�[�h�^�C�v���L�����N�^�[���ǂ���
        if (card.cardData.cardType != CardType.Character)
        {
            Debug.Log("�L�����N�^�[�J�[�h�ȊO�̓t�B�[���h�ɏo���܂���");
            card.ReturnToOriginalParent();
            return;
        }

        //�L�����N�^�[�G���A���g�p�ς݂��ǂ���
        if (slot.occupiedCard != null)
        {
            Debug.Log("���̃X���b�g�͊��Ɏg�p����Ă��܂��B");
            card.ReturnToOriginalParent();
            return;
        }

        //���ʃ��[���`�F�b�N
        bool canPlay = PlayCard(card.cardData, TurnOwner.Player);

        //���s����
        if (canPlay)
        {
            //���������ꍇ�̂݁A�J�[�h���X���b�g�ɔz�u���A�X���b�g�Ɏg�p���ł��邱�Ƃ��L��������
            Debug.Log(card.cardData.cardName + "���X���b�g�ɔz�u���܂����B");
            card.transform.SetParent(slot.transform, false);
            slot.occupiedCard = card;
        }
        else
        {
            //���ʃ��[���`�F�b�N�Ŏ��s�����ꍇ
            card.ReturnToOriginalParent();
        }
    }
    public void CardDroppedOnCharacter(CardView evolveCard, CardView baseCharacter)//�L�����N�^�[�̏�Ƀh���b�v���ꂽ���̏���(�i��)
    {
        //���[���`�F�b�N
        //�J�[�h�^�C�v���i���J�[�h���ǂ���
        if (evolveCard.cardData.cardType != CardType.EvolveCharacter)
        {
            evolveCard.ReturnToOriginalParent();
            return;
        }
        //�i�����̃J�[�h���L�����N�^�[�J�[�h���ǂ���
        if (baseCharacter.cardData.cardType != CardType.Character)
        {
            evolveCard.ReturnToOriginalParent();
            return;
        }

        //���ʃ��[���`�F�b�N
        bool canPlay = PlayCard(evolveCard.cardData, TurnOwner.Player);


        //���s����
        if (canPlay)
        {
            Debug.Log(evolveCard.cardData.cardName + "�ɐi�����܂����B");
            
            //�i�����̃L�����N�^�[�������X���b�g��T��
            CharacterSlot slot = baseCharacter.transform.parent.GetComponent<CharacterSlot>();
            if (slot == null)
            {
                Debug.LogError("�i�����̃L�����N�^�[���X���b�g�ɂ��܂���I");
                evolveCard.ReturnToOriginalParent();
                return;
            }

            //�i���J�[�h���A�i�����J�[�h�̎q�I�u�W�F�N�g�ɂ���
            evolveCard.transform.SetParent(baseCharacter.transform, false);

            //�i���J�[�h�̈ʒu��e�i�i�����j�̐^��Ƀs�b�^�����킹��
            evolveCard.transform.localPosition = Vector3.zero;

            //�q�I�u�W�F�N�g�ɂȂ�����A���[�J���X�P�[����(1, 1, 1)�Ƀ��Z�b�g����
            evolveCard.transform.localScale = Vector3.one;

            //�X���b�g�́u�g�p���̃J�[�h�v�����A�V�����i���J�[�h�ɍX�V����
            slot.occupiedCard = evolveCard;

            //�i���J�[�h�̌����ڂƓ����蔻�������
            CanvasGroup baseCardCanvasGroup = baseCharacter.GetComponent<CanvasGroup>();
            if (baseCardCanvasGroup != null)
            {
                baseCardCanvasGroup.alpha = 0;
                baseCardCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            //���ʃ��[���`�F�b�N�Ŏ��s�����ꍇ
            evolveCard.ReturnToOriginalParent();
        }
        
    }
    public void CardDroppedOnSpellArea(CardView card, SpellArea spellArea)//�A�s�[���E�C�x���g�G���A�Ƀh���b�v���ꂽ���̏���
    {
        //���[���`�F�b�N
        //�J�[�h�^�C�v���A�s�[�����C�x���g���ǂ���
        if (card.cardData.cardType != CardType.Appeal && card.cardData.cardType != CardType.Event)
        {
            card.ReturnToOriginalParent();
            return;
        }

        //���ʃ��[���`�F�b�N
        bool canPlay = PlayCard(card.cardData, TurnOwner.Player);

        //���s����
        if (canPlay)
        {
            Debug.Log(card.cardData.cardName + "���g�p����܂����B");
            //�J�[�h�̎�ނɉ����Č��ʂ𕪊�
            switch (card.cardData.cardType)
            {
                case CardType.Appeal:
                    PerformAppeal(TurnOwner.Player);
                    break;
                case CardType.Event:
                    break;
            }
        }

        //�S��OK�Ȃ���ʔ���
        Debug.Log(card.cardData.cardName + "�̌��ʂ𔭓��I");

        //���ʏ���

        //���ʔ����㏈��(�g���b�V���ɑ���Ȃ�)
        Destroy(card.gameObject);
    }
      #endregion

    //���[���̔���Ɋւ��郁�\�b�h
    #region Rule Check Methods
    public bool IsPlayable(CardView cardView)//�J�[�h���v���C�\�����f����
    {
        Card cardData = cardView.cardData;

        int cardCost = 0;   //�`�F�b�N�Ɏg�����߁A�J�[�h�̃R�X�g���ꎞ�I�ɕۑ�����

        switch (cardData.cardType)
        {
            case CardType.Character:
                //cardData��Character�^�ɃL���X�g���ăR�X�g���擾
                cardCost = (cardData as CharacterCard).cost;
                break;
            case CardType.Event:
                //cardData��Event�^�ɃL���X�g���ăR�X�g���擾
                cardCost = (cardData as EventCard).cost;
                break;
            case CardType.Appeal:
                //cardData��Appeal�^�ɃL���X�g���ăR�X�g���擾
                cardCost = (cardData as AppealCard).cost;
                break;
            case CardType.Leader:
                Debug.Log("���[�_�[�J�[�h����D����v���C�ł��܂���B");
                return false;
        }

        //�����P�F�v���C���[�̃^�[�����H
        bool condition1 = isPlayerTurn;

        //�����Q�F�J�[�h�͎�D�ɂ��邩�H
        bool condition2 = cardView.transform.parent == playerHandArea;

        //�����R�F�}�i�͑���Ă邩�H
        bool condition3 = playerMana >= cardCost;

        //�R�̏�����S�Ė������Ă���� true (�v���C�\)��Ԃ�
        return condition1 && condition2 && condition3;
    }
    #endregion

    //UI����Ɋւ��郁�\�b�h
    #region UI Interaction Methods
    public void OnTurnEndButtonPressed()//UI�́u�^�[���I���v�{�^������Ăяo��
    {
        //�v���C���[���^�[�����I������
        EndTurn(TurnOwner.Player);
    }
    public void OnFieldClicked(Transform fieldTransform)//FieldManager����Ă΂��
    {
        //�ՖʑI�����[�h���łȂ���΁A�������Ȃ�
        if (!isTargetingMode || cardToPlay == null)
        {
            return;
        }

        Debug.Log("GameManager���Ֆʂ̃N���b�N�����m���܂����BisTargetingMode��" + isTargetingMode + "�ł�");

        //�v���C���悤�Ƃ��Ă���J�[�h�@�̎�ނ��m�F����
        CardType type = cardToPlay.cardData.cardType;

        //�J�[�h�̎�ނɉ����āA�������ꏊ�ɒu�����Ƃ��Ă��邩�`�F�b�N����
        switch (type)
        {
            case CardType.Character:
                CharacterSlot slot = fieldTransform.GetComponent<CharacterSlot>();
                if (slot != null)
                {
                    //�������ꏊ�Ȃ̂ŁA�L������u�����̃��\�b�h���Ă�
                    CardDroppedOnSlot(cardToPlay, slot);
                }
                else
                {
                    //�Ԉ�����ꏊ���N���b�N����
                    Debug.Log("�L�����N�^�[�J�[�h�̓L�����N�^�[�X���b�g�ɂ����u���܂���B");
                }
                break;
            case CardType.EvolveCharacter:
                //�i���J�[�h�̏ꍇ�F�N���b�N���ꂽ�ꏊ���L�����N�^�[�J�[�h���H
                CardView baseCharacter = fieldTransform.GetComponent<CardView>();
                if (baseCharacter != null && baseCharacter.cardData.cardType == CardType.Character)
                {
                    //�������^�[�Q�b�g�Ȃ̂ŁA�i����������̃��\�b�h���Ă�
                    CardDroppedOnCharacter(cardToPlay, baseCharacter);
                }
                else
                {
                    Debug.Log("�i���J�[�h�̓t�B�[���h�̃L�����N�^�[�̏�ɂ����u���܂���B");
                }
                break;

            case CardType.Appeal:
            case CardType.Event:
                //�N���b�N���ꂽ�ꏊ���A�s�[��/�C�x���g�G���A���H
                SpellArea spellArea = fieldTransform.GetComponent<SpellArea>();
                if (spellArea != null)
                {
                    //�������ꏊ�Ȃ̂ŁA�A�s�[��/�C�x���g���v���C������̃��\�b�h���Ă�
                    CardDroppedOnSpellArea(cardToPlay, spellArea);
                }
                else
                {
                    //�Ԉ�����ꏊ���N���b�N����
                    Debug.Log("�A�s�[��/�C�x���g�J�[�h�͐�p�̃G���A�ł����g���܂���B");
                }
                break;
        }

        //�I����Ԃ��������Ď��ɔ�����
        isTargetingMode = false;
        cardToPlay = null;
    }
    #endregion

    //�Q�[���̏�Ԃ��Ǘ����郁�\�b�h
    #region Game State Methods
    public void EnterTargetingMode(CardView card)//zoomUIManager����Ă΂��
    {
        isTargetingMode = true;
        cardToPlay = card;
        Debug.Log("�ՖʑI�����[�h�Ɉڍs���܂���");
    }
    public bool IsTargetingMode()//���݁A�ՖʑI�����[�h�����ǂ�����Ԃ�
    {
        return isTargetingMode;
    }
    #endregion

    //�Q�[���̃A�N�V�����Ɋւ��郁�\�b�h
    #region Game Action Methods
    public void PerformAppeal(TurnOwner owner)//�w�肳�ꂽ�I�[�i�[�̃A�s�[�����������s����
    {
        //�N�̃A�s�[�����ɂ����
        Transform leaderArea;
        Transform characterSlotsParent;

        if (owner == TurnOwner.Player)
        {
            //�v���C���[�̃^�[���łȂ���Ώ������Ȃ�
            if (!isPlayerTurn) return;
            leaderArea = playerLeaderArea;
            characterSlotsParent = playerCharacterSlotsParent;
        }
        else
        {
            //����̃^�[���łȂ���Ώ������Ȃ�
            if (isPlayerTurn) return;
            leaderArea = enemyLeaderArea;
            characterSlotsParent = enemyCharacterSlotsParent;
        }

        int totalAppealPower = 0;
        
        //���[�_�[�̃A�s�[���͂����v�ɉ�����
        CardView leaderView = leaderArea.GetComponentInChildren<CardView>();
        if (leaderView != null)
        {
            //���[�_�[�J�[�h�ɃL���X�g���ăA�s�[���͂��擾
            if (leaderView.cardData is LeaderCard leaderCard)
            {
                totalAppealPower += leaderCard.appeal;
            }
        }

        //�t�B�[���h��̑S�L�����N�^�[�̃A�s�[���͂����v�ɉ�����
        foreach (CharacterSlot slot in characterSlotsParent.GetComponentsInChildren<CharacterSlot>())
        {
            if (slot.occupiedCard != null)
            {
                //�L�����N�^�[�J�[�h�ɃL���X�g���ăA�s�[���͂��擾
                if (slot.occupiedCard.cardData is CharacterCard characterCard)
                {
                    totalAppealPower += characterCard.appeal;
                }
            }
        }

        //���v�����A�s�[���͂��A�Ή����鑤�̃|�C���g�ɉ��Z����
        if (owner == TurnOwner.Player)
        {
            playerAppealPoints += totalAppealPower;
        }
        else
        {
            enemyAppealPoints += totalAppealPower;
        }

        Debug.Log(owner + "��" + totalAppealPower + "�A�s�[�����āA���v�|�C���g��" + (owner == TurnOwner.Player ? playerAppealPoints : enemyAppealPoints) + " �ɂȂ����I");

        //UI�̕\�����X�V����
        UIManager.Instance.UppdateAppealPointUI(playerAppealPoints, enemyAppealPoints);
    }
    private bool PlayCard(Card cardData, TurnOwner owner)//�J�[�h�v���C�̃��[���`�F�b�N�ƃR�X�g�̏���
    {
        //�N�̃^�[���ŁA�N�̃}�i���g����
        bool isCorrectTurn = (owner == TurnOwner.Player) ? isPlayerTurn : !isPlayerTurn;
        int currentMana = (owner == TurnOwner.Player) ? playerMana : enemyMana;

        //�^�[���`�F�b�N
        if (!isCorrectTurn)
        {
            Debug.Log("�������^�[���ł͂���܂���B");
            return false;
        }

        //�}�i�`�F�b�N
        int cost = 0;
        switch (cardData.cardType)
        {
            case CardType.Character:
                cost = (cardData as CharacterCard).cost;
                break;
            case CardType.EvolveCharacter:
                cost = (cardData as EvolveCharacterCard).evolveCost;
                break;
            case CardType.Appeal:
                cost = (cardData as  AppealCard).cost;
                break;
            case CardType.Event:
                cost = (cardData as EventCard).cost;
                break;
        }

        if (currentMana < cost)
        {
            Debug.Log("�}�i������܂���B");
            return false;
        }

        //�S�Ẵ`�F�b�N���N���A

        //���ۂɃ}�i�������
        if (owner == TurnOwner.Player)
        {
            playerMana -= cost;
        }
        else
        {
            enemyMana -= cost;
        }

        //UI�X�V
        //UIManager.Instance.UpdateManaUI
    
        return true;
    }
    #endregion
}
