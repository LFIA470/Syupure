using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CardView droppedCard = eventData.pointerDrag.GetComponent<CardView>();
        if (droppedCard == null) return;

        //GameManager�Ɂu�����G���A�ɁA���̃J�[�h���h���b�v����܂����v�ƕ�
        GameManager.Instance.CardDroppedOnSpellArea(droppedCard, this);
    }
}
