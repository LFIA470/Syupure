using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellArea : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    //スペルエリアのプレイに関するメソッド
    #region Play SpellArea Methods
    public void OnPointerClick(PointerEventData eventData)
    {
        // GameManagerの交通整理員（OnFieldClicked）に報告
        GameManager.Instance.OnFieldClicked(this.transform);
    }
    public void OnDrop(PointerEventData eventData)
    {
        CardView droppedCard = eventData.pointerDrag.GetComponent<CardView>();
        if (droppedCard == null) return;

        //GameManagerに「呪文エリアに、このカードがドロップされました」と報告
        GameManager.Instance.CardDroppedOnSpellArea(droppedCard, this);
    }
    #endregion
}
