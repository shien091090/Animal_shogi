//標示元素腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignElement : MonoBehaviour
{
    [Header("參考物件")]
    public GameObject stayingObj; //滑鼠滯留特效
    public GameObject movementObj; //可移動特效

    public List<HighlightType> highlightList = new List<HighlightType>(); //目前標記中列表

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {

    }

    //-------------------------------------------------------------------------------------------------------------------

    //設定標記
    //[input] type : 標記種類 / state : 開或關
    public void SetHighlightState(HighlightType type, bool state)
    {
        //標記開啟
        System.Action<HighlightType> signOn = (x) =>
        {
            switch (x)
            {
                case HighlightType.鼠標滯留:
                    if (!stayingObj.activeSelf) stayingObj.SetActive(true);
                    break;

                case HighlightType.可移動:
                    if (!movementObj.activeSelf) movementObj.SetActive(true);
                    break;
            }

            return;
        };

        //標記關閉
        System.Action<HighlightType> signOff = (x) =>
        {
            switch (x)
            {
                case HighlightType.鼠標滯留:
                    if (stayingObj.activeSelf) stayingObj.SetActive(false);
                    break;

                case HighlightType.可移動:
                    if (movementObj.activeSelf) movementObj.SetActive(false);
                    break;
            }

            return;
        };

        bool inList = false;
        for (int i = 0; i < highlightList.Count; i++)
        {
            if (highlightList[i] == type) inList = true;
        }

        if (state && !inList) //設定標記為on 且 未執行此標記時
        {
            signOn(type); //標記開啟效果實作
            highlightList.Add(type); //加入至標記列表
        }
        else if (!state && inList) //設定標記為off 且 此標記正在執行中時
        {
            signOff(type); //標記關閉效果實作
            highlightList.Remove(type); //從列表中剔除
        }


    }
}
