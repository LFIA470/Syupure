using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Leader,             //リーダーカード
    Character,          //キャラクターカード
    EvolveCharacter,    //進化キャラクターカード
    Appeal,             //アピールカード
    Event,              //イベントカード
}

public abstract class Card : ScriptableObject
{
    [Header("全カード共通の基本情報")]
    public string cardName;     //カードネーム
    [TextArea]
    public string description;  //効果テキスト
    public CardType cardType;   //カードタイプ
    public Sprite artwork;      //イラスト画像
}
