using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    public CardView occupiedCard = null;    //���̃X���b�g�ɒu����Ă���J�[�h

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("�N���b�N�����I");

        //GameManager�ɕ񍐂���
        GameManager.Instance.OnFieldClicked(this.transform);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("�h���b�v�����I");

        //�h���b�v���ꂽ�J�[�h���擾
        CardView droppedCard = eventData.pointerDrag.GetComponent<CardView>();
        if (droppedCard == null) return;

        //GameManager�ɁA�u���̃X���b�g�ɁA���̃J�[�h���h���b�v���ꂽ�v�ƕ�
        GameManager.Instance.CardDroppedOnSlot(droppedCard, this);
    }
}
