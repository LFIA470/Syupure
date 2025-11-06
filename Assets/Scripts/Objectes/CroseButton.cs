using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CroseButton : MonoBehaviour
{
    //クローズボタンのクリックに関連するメソッド
    #region Click Methods
    public void OnCloseClick()  //閉じるボタンがクリックされたか
    {
        ZoomUIPanelManager.Instance.Hide();
    }
    #endregion
}
