using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;  //Unityエディタの機能を扱うために必要
using System.IO;    //ファイルやフォルダを扱うために必要
using System;
using System.Linq.Expressions;
using Unity.VisualScripting;       //Enum.Parse(文字列をenum型に変換)のために必要

public class CardDataImporter : EditorWindow
{
    //変数宣言
    #region Variables
    private TextAsset csvFile;  //インスペクターで設定するCSVファイル
    private string saveFolderPath = "Assets/GameData/Cards/";   //.assetファイルの保存先
    private string artworkFolderPath = "Assets/Images/Images/"; //イラスト画像の保存先
    #endregion

    //Unityに表示するウィンドウメソッド
    #region Window Methods
    [MenuItem("My Tools/カードデータ取込")] //Unityの上部メニューに「Mu Tools > カードデータ取込」を追加
    
    public static void ShowWindou() //ウィンドウ表示
    {
        //ウィンドウを開く
        GetWindow<CardDataImporter>("Card Importer");
    }

    void OnGUI()    //ウィンドウのUIを描画する処理
    {
        GUILayout.Label("カードデータ　インポーター", EditorStyles.boldLabel);

        //UI要素の作成
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSVファイル", csvFile, typeof(TextAsset), false);
        saveFolderPath = EditorGUILayout.TextField("アセット保存先フォルダ", saveFolderPath);
        artworkFolderPath = EditorGUILayout.TextField("イラスト画像フォルダ", artworkFolderPath);

        //「インポート実行」ボタン
        if (GUILayout.Button("インポート実行"))
        {
            if (csvFile == null)
            {
                Debug.LogError("CSVファイルが設定されていません！");
            }
            else
            {
                ImportCards();
            }
        }
    }
    #endregion

    //インポートボタン押下時のメソッド
    #region Import Methods
    private void ImportCards()
    {
        //フォルダが存在しなければ作成
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        //CSVデータを改行で分割して「行」の配列にする
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    
        if (lines.Length < 2)
        {
            Debug.LogError("CSVデータが不正です。ヘッダー行とデータ行が必要です。");
            return;
        }

        //１行目はヘッダーなので、２行目からデータの読み込みを開始する
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');  //１行をカンマで分割

            try
            {
                //CSVの列の順番に合わせてデータを読み込む
                //(0:カードID, 1:カード名, 2:カードタイプ, 3:イラストファイル名,
                //4:効果ID, 5:効果値, 6:効果対象, 7:効果テキスト,
                //8:消費コスト, 9:アピール力, 10:進化元名
                string cardID = values[0];
                string cardName = values[1];
                CardType cardType = (CardType)Enum.Parse(typeof(CardType), values[2]);
                string artworkFileName = values[3];
                string effectID = values[4];
                int effectValue = int.Parse(values[5]);
                string effectTarget = values[6];
                string description = values[7];
                int cost = int.Parse(values[8]);
                int appeal = int.Parse(values[9]);
                string evolveBaseName = values[10];

                //ScriptableObjectアセットの作成

                //アセットの保存パスを決定
                string assetPath = Path.Combine(saveFolderPath, $"{cardName}_{cardID}.asset");

                //既にアセットが存在するか確認
                Card existingCard = AssetDatabase.LoadAssetAtPath<Card>(assetPath);

                if (existingCard == null)
                {
                    //アセットが存在しない -> 新規作成
                    CreateNewCardAsset(assetPath, cardName, cardType, artworkFileName,
                        effectID, effectValue, effectTarget, description,
                        cost, appeal, evolveBaseName);
                }
                else
                {
                    //アセットが存在する -> 更新
                    UpdateCardAsset(existingCard, cardName, cardType, artworkFileName,
                        effectID, effectValue, effectTarget, description,
                        cost, appeal, evolveBaseName);
                }
            }
            catch (Exception e)
            {
                //エラーが発生した行をログに出力
                Debug.LogError($"CSVの{i + 1}行目の処理中にエラーが発生しました：{lines[i]}\n{e.Message}");

            }

            AssetDatabase.SaveAssets(); //変更をプロジェクトに保存
            AssetDatabase.Refresh();    //プロジェクトビューを更新
            Debug.Log("カードデータのインポートが完了しました！");
        }
    }
    #endregion

    //アセットの作成・更新に関するメソッド
    #region Asset Methods
    private void CreateNewCardAsset //新規アセット作成メソッド
    (string assetPath, string cardName, CardType cardType, string artworkFileName,
    string effectID, int effectValue, string effectTarget, string description,
    int cost, int appeal, string evolveBaseName)   
    {
        Card newCard = null;

        //カードタイプに応じて、作成するScriptableObjectの型を決める
        switch (cardType)
        {
            case CardType.Leader:
                newCard = ScriptableObject.CreateInstance<LeaderCard>();
                // LeaderCard固有の値を設定
                (newCard as LeaderCard).appeal = appeal;
                (newCard as LeaderCard).evolveBaseName = evolveBaseName;
                break;
            case CardType.Character:
                newCard = ScriptableObject.CreateInstance<CharacterCard>();
                (newCard as CharacterCard).appeal = appeal;
                (newCard as CharacterCard).cost = cost;
                break;
            case CardType.EvolveCharacter:
                newCard = ScriptableObject.CreateInstance<EvolveCharacterCard>();
                (newCard as EvolveCharacterCard).appeal = appeal;
                (newCard as EvolveCharacterCard).cost = cost;
                (newCard as EvolveCharacterCard).evolveBaseName = evolveBaseName;
                break;
            case CardType.Event:
                newCard = ScriptableObject.CreateInstance<EventCard>();
                (newCard as EventCard).cost = cost;
                break;
            case CardType.Appeal:
                newCard = ScriptableObject.CreateInstance<AppealCard>();
                (newCard as AppealCard).cost = cost;
                break;
            default:
                Debug.LogError($"未知のカードタイプ:{cardType}");
                return;
        }

        //全カード共通の値を設定
        newCard.cardName = cardName;
        newCard.cardType = cardType;
        newCard.description = description;
        newCard.artwork = LoadSprite(artworkFileName);
        

        //アセットとしてプロジェクトに保存
        AssetDatabase.CreateAsset(newCard, assetPath);
    }

    private void UpdateCardAsset    //既存アセット更新メソッド
    (Card existingCard, string cardName, CardType cardType, string artworkFileName,
    string effectID, int effectValue, string effectTarget, string description,
    int cost, int appeal, string evolveBaseName)
    {
        // 既存のアセットの値を上書き
        // (面倒なので、CreateNewCardAssetとほぼ同じ内容をもう一度書く)

        existingCard.cardName = cardName;
        existingCard.cardType = cardType;
        existingCard.description = description;
        existingCard.artwork = LoadSprite(artworkFileName);
        // existingCard.effectID = effectID;
        // ...

        // カードタイプに応じて固有の値を更新
        switch (cardType)
        {
            case CardType.Leader:
                (existingCard as LeaderCard).appeal = appeal;
                (existingCard as LeaderCard).evolveBaseName = evolveBaseName;
                break;
                // ... 他のカードタイプも同様に ...
        }

        EditorUtility.SetDirty(existingCard); // 変更をマーク
        Debug.Log($"アセットを更新: {existingCard.name}");
    }
    #endregion

    //画像ロードメソッド
    #region Load Methods
    private Sprite LoadSprite   //イラスト画像をファイル名から読み込むヘルパーメソッド
    (string fileName)
    {
        // (例: "pipo-enemy014a.png")
        // ".png"などの拡張子が含まれていたら取り除く
        string spriteName = Path.GetFileNameWithoutExtension(fileName);

        // フォルダパスと結合 (例: "Assets/GameData/Artworks/pipo-enemy014a.png")
        string spritePath = Path.Combine(artworkFolderPath, fileName);

        // Spriteを読み込む
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogWarning($"スプライトが見つかりません: {spritePath}");
        }
        return sprite;
    }
    #endregion
}
