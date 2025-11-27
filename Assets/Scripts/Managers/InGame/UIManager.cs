using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    public static UIManager Instance;

    private void Awake()
    {
        Debug.Log("UIManagerのAwake()が呼ばれました！");

        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            // この（重複した）コンポーネントだけを破棄する
            Debug.LogWarning("UIManagerが重複しています。このコンポーネントを破棄します。");
            Destroy(this);
        }
    }

    #endregion

    //変数宣言
    #region Variables
    [Header("AppealTexts")]
    [SerializeField] private Text playerAppealPointText;
    [SerializeField] private Text enemyAppealPointText;

    [Header("CheerPower UI")]
    [SerializeField] private Transform playerCheerPowerArea; // 親オブジェクト
    [SerializeField] private Transform enemyCheerPowerArea; // 親オブジェクト※相手用
    [SerializeField] private GameObject cheerPowerIconPrefab; // アイコンのプレハブ
    [SerializeField] private Sprite iconOnSprite;  // ON画像
    [SerializeField] private Sprite iconOffSprite; // OFF画像

    private List<Image> PlayerCheerPowerIcons = new List<Image>();
    private List<Image> EnemyCheerPowerIcons = new List<Image>();

    [Header("Buttons")]
    [SerializeField] private GameObject turnEndButton;
    #endregion

    //UI更新メソッド
    #region UppdataUI Methods
    public void UppdateAppealPointUI    //GameManagerから呼び出され、アピールポイントのUIを更新する
    (int playerPoints, int enemyPoints)
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
    public void UpdateCheerPowertUI(int current, int max, TurnOwner owner)   //応援ポイント（マナ）の表示を更新する
    {
        if (owner == TurnOwner.Player)
        {
            //アイコンの数が足りなければ増やす、多すぎれば減らす
            //(最大値が変わるカード効果などに対応するため)
            while (PlayerCheerPowerIcons.Count < max)
            {
                GameObject newIcon = Instantiate(cheerPowerIconPrefab, playerCheerPowerArea);
                PlayerCheerPowerIcons.Add(newIcon.GetComponent<Image>());
            }
            while (PlayerCheerPowerIcons.Count > max)
            {
                Destroy(PlayerCheerPowerIcons[PlayerCheerPowerIcons.Count - 1].gameObject);
                PlayerCheerPowerIcons.RemoveAt(PlayerCheerPowerIcons.Count - 1);
            }

            //アイコンの画像を切り替える
            for (int i = 0; i < PlayerCheerPowerIcons.Count; i++)
            {
                if (i < current)
                {
                    //現在値より小さいインデックスは「ON」
                    PlayerCheerPowerIcons[i].sprite = iconOnSprite;
                }
                else
                {
                    //それ以外は「OFF」
                    PlayerCheerPowerIcons[i].sprite = iconOffSprite;
                }
            }
        }
        else
        {
            while (EnemyCheerPowerIcons.Count < max)
            {
                GameObject newIcon = Instantiate(cheerPowerIconPrefab, enemyCheerPowerArea);
                EnemyCheerPowerIcons.Add(newIcon.GetComponent<Image>());
            }
            while (EnemyCheerPowerIcons.Count > max)
            {
                Destroy(EnemyCheerPowerIcons[EnemyCheerPowerIcons.Count - 1].gameObject);
                EnemyCheerPowerIcons.RemoveAt(EnemyCheerPowerIcons.Count - 1);
            }

            for (int i = 0; i < EnemyCheerPowerIcons.Count; i++)
            {
                if (i < current)
                {
                    //現在値より小さいインデックスは「ON」
                    EnemyCheerPowerIcons[i].sprite = iconOnSprite;
                }
                else
                {
                    //それ以外は「OFF」
                    EnemyCheerPowerIcons[i].sprite = iconOffSprite;
                }
            }
        }        
    }
    public void SetTurnEndButtonActive(bool isActive)   //ボタンの表示・非表示を切り替えるメソッド
    {
        if (turnEndButton != null)
        {
            turnEndButton.SetActive(isActive);
        }
    }
    #endregion
}
