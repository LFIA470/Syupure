using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CroseButton : MonoBehaviour
{
    //閉じるボタンがクリックされたか
    public void OnCloseClick()
    {
        ZoomUIPanelManager.Instance.Hide();
    }
}
