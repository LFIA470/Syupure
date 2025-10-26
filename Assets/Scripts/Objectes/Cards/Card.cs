using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Leader,             //���[�_�[�J�[�h
    Character,          //�L�����N�^�[�J�[�h
    EvolveCharacter,    //�i���L�����N�^�[�J�[�h
    Appeal,             //�A�s�[���J�[�h
    Event,              //�C�x���g�J�[�h
}

public abstract class Card : ScriptableObject
{
    [Header("�S�J�[�h���ʂ̊�{���")]
    public string cardName;     //�J�[�h�l�[��
    [TextArea]
    public string description;  //���ʃe�L�X�g
    public CardType cardType;   //�J�[�h�^�C�v
    public Sprite artwork;      //�C���X�g�摜
}
