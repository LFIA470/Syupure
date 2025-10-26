using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private GameObject m_gameObject;   //GameManager�ւ̎Q�Ƃ�ǉ�

    //�f�b�L�̌��{�i�C���X�y�N�^�[�Őݒ肷��p�j
    public List<Card> playerDeck;
    public Card playerLeader;
    public List<Card> enemyDeck;
    public Card enemyLeader;

    //���ۂɃQ�[�����Ɏg���R�D�p�̃��X�g
    private List<Card> playerDeckPile;
    private List<Card> enemyDeckPile;

    public Transform playerHandArea;
    public Transform playerFieldArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;

    void Awake()
    {
        playerDeckPile = new List<Card>(playerDeck);
        enemyDeckPile = new List<Card>(enemyDeck);
    }

    //�f�b�L�V���b�t��
    public void DeckShuffle(TurnOwner owner)
    {
        //�N���V���b�t�����邩�ɉ����āA�g���R�D�����߂�
        List<Card> targetDeck;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
        }
        else
        {
            targetDeck = enemyDeckPile;
        }

        //�V���b�t��
        //���X�g�̌�납�珇�ԂɁA�����_���Ȉʒu�̃J�[�h�ƌ������Ă���
        for (int i = targetDeck.Count - 1; i > 0; i--)
        {
            //�����_���ȃC���f�b�N�X��I��
            int j = Random.Range(0 , i + 1);

            //�I�񂾃J�[�h�ƌ��݂̃J�[�h�����ւ���
            Card temp = targetDeck[i];
            targetDeck[i] = targetDeck[j];
            targetDeck[j] = temp;
        }

        Debug.Log(owner + "�̎R�D���V���b�t�����܂����B");
    }

    //�R�D�̈�ԏォ��ꖇ����
    public void DrawCard(TurnOwner owner)
    {
        //�N���������ɉ����āA�g���R�D�Ǝ�D�����肷��
        List<Card> targetDeck;
        Transform targetHandArea;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
            targetHandArea = playerHandArea;
        }
        else
        {
            targetDeck = enemyDeckPile;
            targetHandArea = enemyHandArea;
        }

        //�R�D�̖������`�F�b�N
        if (targetDeck.Count == 0)
        {
            Debug.Log(owner + "�̎R�D������܂���B");
            return;
        }

        //�R�D�̈�ԏォ��P�����o��
        Card cardData = targetDeck[0];
        targetDeck.RemoveAt(0);

        //�I�񂾃J�[�h����D�𐶐����ĕ\������
        GameObject cardObj = Instantiate(cardPrefab, targetHandArea);
        CardView view = cardObj.GetComponent<CardView>();
        view.SetCard(cardData);

        Debug.Log(owner + "��" + cardData.cardName + "�������܂����B");
    }

    //�R�D���烉���_���Ɉꖇ����
    public void RandomDrawCard(TurnOwner owner)
    {
        //�N���������ɉ����āA�g���R�D�Ǝ�D�����肷��
        List<Card> targetDeck;
        Transform targetHandArea;

        if (owner == TurnOwner.Player)
        {
            targetDeck = playerDeckPile;
            targetHandArea = playerHandArea;
        }
        else
        {
            targetDeck = enemyDeckPile;
            targetHandArea = enemyHandArea;
        }

        //�R�D�̖������`�F�b�N
        if (targetDeck.Count == 0)
        {
            Debug.Log(owner + "�̎R�D������܂���B");
            return;
        }

        //�R�D���烉���_���ɂP���I��Ŏ��o��
        int index = Random.Range(0, targetDeck.Count);
        Card cardData = targetDeck[index];
        targetDeck.RemoveAt(index);

        //�I�񂾃J�[�h����D�𐶐����ĕ\������
        GameObject cardObj = Instantiate(cardPrefab, targetHandArea);
        CardView view = cardObj.GetComponent<CardView>();
        view.SetCard(cardData);

        Debug.Log(owner + "��" + cardData.cardName + "�������܂����B");
    }
}
