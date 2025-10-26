using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        // �}�E�X�̍��N���b�N�������ꂽ�u�ԂɎ��s
        if (Input.GetMouseButtonDown(0))
        {
            // �������牺���\���m�F�p�̃R�[�h
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // �}�E�X�J�[�\�������ɂ���UI�v�f�����ׂĕ\��
                Debug.Log("---------- �}�E�X������UI���X�g ----------");
                foreach (var result in results)
                {
                    Debug.Log("�q�b�g�����I�u�W�F�N�g: " + result.gameObject.name, result.gameObject);
                }
                Debug.Log("----------------------------------------");
            }
        }
    }
}