using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CroseButton : MonoBehaviour
{
    //����{�^�����N���b�N���ꂽ��
    public void OnCloseClick()
    {
        ZoomUIPanelManager.Instance.Hide();
    }
}
