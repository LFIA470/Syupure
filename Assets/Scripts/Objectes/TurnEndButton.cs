using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndButton : MonoBehaviour
{
    //ターンエンドボタンがクリックされたか
    public void OnTurnEndClick()
    {
        Debug.Log("ターンが終了しました。");
        GameManager.Instance.EndTurn(TurnOwner.Player);
    }
}
