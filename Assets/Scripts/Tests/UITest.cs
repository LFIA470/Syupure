using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        // マウスの左クリックが押された瞬間に実行
        if (Input.GetMouseButtonDown(0))
        {
            // ここから下が表示確認用のコード
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // マウスカーソル直下にあるUI要素をすべて表示
                Debug.Log("---------- マウス直下のUIリスト ----------");
                foreach (var result in results)
                {
                    Debug.Log("ヒットしたオブジェクト: " + result.gameObject.name, result.gameObject);
                }
                Debug.Log("----------------------------------------");
            }
        }
    }
}