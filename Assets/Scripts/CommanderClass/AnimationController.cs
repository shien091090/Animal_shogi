//動畫演出腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CurveBase
{
    public CurveType type; //曲線類型
    public AnimationCurve curve; //動畫曲線(X軸:0~1(1=結束時間) , Y軸:0~1(0=初始位置/1=目標位置))
}

public class AnimationController : MonoBehaviour
{
    private static AnimationController _instance; //單例物件
    public static AnimationController Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public List<CurveBase> curveList; //曲線設定

    [Header("棋格淡入參數")]
    public float startPosDistance; //起始格距離
    public float cellFadeInDuration; //淡入時間

    public Dictionary<CurveType, AnimationCurve> dic_curveSetting = new Dictionary<CurveType, AnimationCurve>(); //(字典)透過CurveType獲取AnimationCurve

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        for (int i = 0; i < curveList.Count; i++) //建立字典
        {
            dic_curveSetting.Add(curveList[i].type, curveList[i].curve);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------

    ////平滑移動
    ////[input] go = 預移動物件, targetPos = 目標位置, t = 所需時間, curveType = 曲線類型
    //public void SmoothMove(Transform go, Vector2 targetPos, float t, CurveType curveType)
    //{
    //    StartCoroutine(Cor_SmoothMove(go, targetPos, t, curveType)); //調用Pritvate協程
    //} 

    //平滑移動協程
    public IEnumerator Cor_SmoothMove(Transform go, Vector2 targetPos, float t, CurveType curveType)
    {
        float timer = 0; //計時器
        Vector2 originPos = go.localPosition; //原點
        AnimationCurve _curve = dic_curveSetting[curveType]; //動畫曲線

        while (timer < t)
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0, t); //計時器推進
            go.localPosition = Vector2.Lerp(originPos, targetPos, _curve.Evaluate(timer / t)); //套用位置

            yield return new WaitForEndOfFrame();
        }
    }

    //顏色變化協程
    public IEnumerator Cor_ColorChange(Image img, Color startC, Color targetC, float t, CurveType curveType)
    {
        float timer = 0; //計時器
        AnimationCurve _curve = dic_curveSetting[curveType]; //動畫曲線

        while (timer < t)
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0, t); //計時器推進
            img.color = Color.Lerp(startC, targetC, ( timer / t )); //套用顏色

            yield return new WaitForEndOfFrame();
        }
    }

}
