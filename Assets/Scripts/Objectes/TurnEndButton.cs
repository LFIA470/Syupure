using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndButton : MonoBehaviour
{
    //�^�[���G���h�{�^�����N���b�N���ꂽ��
    public void OnTurnEndClick()
    {
        Debug.Log("�^�[�����I�����܂����B");
        GameManager.Instance.EndTurn(TurnOwner.Player);
    }
}
