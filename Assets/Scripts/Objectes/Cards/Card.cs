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
    public CardType cardType;   //カードタイプ
    public string description;  //効果テキスト
    public Sprite artwork;      //イラスト画像

    [Header("効果")]
    public string effectID;     //効果ID
    public int effectValue;     //効果値
    public string effectTarget; //効果対象
    public string timingID;     //カード使用タイミングID
} 
