using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    public CardView occupiedCard = null;    //このスロットに置かれているカード

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("クリック成功！");

        //GameManagerに報告する
        GameManager.Instance.OnFieldClicked(this.transform);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("ドロップ成功！");

        //ドロップされたカードを取得
        CardView droppedCard = eventData.pointerDrag.GetComponent<CardView>();
        if (droppedCard == null) return;

        //GameManagerに、「このスロットに、このカードがドロップされた」と報告
        GameManager.Instance.CardDroppedOnSlot(droppedCard, this);
    }
}
