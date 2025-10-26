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

        //GameManagerに「呪文エリアに、このカードがドロップされました」と報告
        GameManager.Instance.CardDroppedOnSpellArea(droppedCard, this);
    }
}
