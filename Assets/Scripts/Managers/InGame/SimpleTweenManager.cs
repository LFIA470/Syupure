using UnityEngine;
using System.Collections;
using System;

public class SimpleTweenManager : MonoBehaviour
{
    public static SimpleTweenManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// UIを指定した位置へ移動させる（スライド演出用）
    /// </summary>
    /// <param name="target">動かす対象のRectTransform</param>
    /// <param name="targetPosition">移動先の座標</param>
    /// <param name="duration">移動にかかる時間（秒）</param>
    /// <param name="delayBefore">移動前の待機時間</param>
    /// <param name="delayAfter">移動後の待機時間</param>
    /// <param name="curve">動きのカーブ（緩急）</param>
    /// <param name="onComplete">完了後に実行したい処理（あれば）</param>
    public void SlideUI(RectTransform target, Vector2 targetPosition, float duration, float delayBefore, float delayAfter, AnimationCurve curve, Action onComplete = null)  //二点間移動演出呼び出し
    {
        StartCoroutine(SlideRoutine(target, targetPosition, duration, delayBefore, delayAfter, curve, onComplete));
    }

    private IEnumerator SlideRoutine(RectTransform target, Vector2 targetPos, float duration, float delayBefore, float delayAfter, AnimationCurve curve, Action onComplete) //二点間移動演出
    {
        //移動前の待機
        if (delayBefore > 0)
        {
            yield return new WaitForSeconds(delayBefore);
        }

        //移動処理
        Vector2 startPos = target.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            //進行度（0～1）を計算
            float t = Mathf.Clamp01(elapsedTime / duration);

            //カーブを使って緩急をつける
            //カーブの縦軸の値（Value）を取得して、それをLerpのtとして使う
            float curveValue = curve.Evaluate(t);

            //座標を更新
            target.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);

            yield return null; //次のフレームまで待つ
        }

        //最終的な位置をズレないように確定させる
        target.anchoredPosition = targetPos;

        //移動後の待機
        if (delayAfter > 0)
        {
            yield return new WaitForSeconds(delayAfter);
        }

        //完了通知（次の処理があれば実行）
        onComplete?.Invoke();
    }
}