using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class CardDatabase
{
    public static List<Card> GetAllCards()
    {
        //Cardsフォルダの中にある Card型のファイルを全部ロードする
        Card[] loadedCards = Resources.LoadAll<Card>("Cards");

        //配列をリストに変換して返す
        return loadedCards.ToList();
    }

    //IDを指定して1枚だけ取ってくるメソッド
    public static Card GetCardByID(int id)
    {
        List<Card> allCards = GetAllCards();
        return allCards.Find(c => c.cardID == id);
    }
}
