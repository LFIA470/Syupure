using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Text playerAppealPointText;
    [SerializeField] private Text enemyAppealPointText;

    private void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
        }
    }

    //GameManagerから呼び出され、画面のUIを更新する
    public void UppdateAppealPointUI(int playerPoints, int enemyPoints)
    {
        if (playerAppealPointText != null)
        {
            playerAppealPointText.text = playerPoints.ToString();
        }
        if (enemyAppealPointText != null)
        {
            enemyAppealPointText.text = enemyPoints.ToString();
        }
    }
}
