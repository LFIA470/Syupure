using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderManager : MonoBehaviour
{
    [SerializeField] private DeckManager _deckManager;

    [SerializeField] private Transform playerLeaderArea;
    [SerializeField] private Transform enemyLeaderArea;
    [SerializeField] private GameObject cardPrefab;

    private void Start()
    {
        DrawLeader();
    }

    //���[�_�[�\��
    public void DrawLeader()
    {
        //���[�_�[�J�[�h���擾
        Card playerLeader = _deckManager.playerLeader;
        Card enemyLeader = _deckManager.enemyLeader;

        CreateLeaderCard(playerLeader, playerLeaderArea);
        CreateLeaderCard(enemyLeader, enemyLeaderArea);
    }
    
    //���[�_�[���
    private void CreateLeaderCard(Card card, Transform parentArea)
    {
        //�J�[�h�𐶐�
        GameObject cardObj = Instantiate(cardPrefab, parentArea);   //�J�[�h�̃v���n�u���C���X�^���X��

        //�J�[�h�T�C�Y�ύX
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(160, 216); // �K�؂ȃT�C�Y�ɒ���

        CardView view = cardObj.GetComponent<CardView>();   //CardView�R���|�[�l���g�ǉ�
        view.SetCard(card); //�J�[�h����UI�ɔ��f
    }
}
