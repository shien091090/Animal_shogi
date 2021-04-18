//掛載在棋子(Chess)的Prefab上, 負責執行棋子相關行為
//棋子的具體屬性則從ChessAtribute導入

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessBehavior : MonoBehaviour
{
    [Header("遊戲參數")]
    public Camps chessPlayer; //玩家陣營
    public bool isKing; //是否為"王"
    public List<DirectionType> directionType; //可移動方向(複數)

    [Header("圖形參數")]
    public Image[] directionImages; //可移動方向的Image
    public Image frameImage; //外框Image
    public Image animalIcon; //動物圖案Image

    private ChessAttribute attribute; //棋子屬性
    public ChessAttribute GetAttribure { get { return attribute; } } //取得棋子屬性

    //-------------------------------------------------------------------------------------------------------------------

    //棋子初始化
    public void ChessInitialize(ChessAttribute _attribute, Camps player, bool _isKing)
    {
        attribute = _attribute; //設定棋子屬性

        //this.name = player.ToString() + " / " + _attribute.chessName.ToString(); //更改遊戲物件名稱

        //初始化棋子尺寸
        this.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        //初始化棋子圖示
        animalIcon.sprite = attribute.animalIcon;

        //依照陣營變換棋子顏色
        if (player == Camps.正面方) frameImage.color = UIManager.Instance.playerColor;
        else if (player == Camps.反面方) frameImage.color = UIManager.Instance.enemyColor;

        chessPlayer = player; //設定陣營
        isKing = _isKing; //是否為王

        //顯示可移動方向在棋子上
        directionType = new List<DirectionType>(); //初始化可移動方向陣列
        for (int i = 0; i < directionImages.Length; i++) //先關閉全部的可移動方向
        {
            directionImages[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < attribute.directionType.Count; i++) //設定可移動方向
        {
            directionType.Add(attribute.directionType[i]);
        }

        if (player == Camps.反面方) //棋子為反面方時, 條件性翻轉可移動方向
        {
            //需實施反轉的方向(一對)
            List<DirectionType[]> exchangeDir = new List<DirectionType[]>();
            exchangeDir.Add(new DirectionType[] { DirectionType.左上, DirectionType.左下 });
            exchangeDir.Add(new DirectionType[] { DirectionType.上, DirectionType.下 });
            exchangeDir.Add(new DirectionType[] { DirectionType.右上, DirectionType.右下 });

            //int changeTypeIndex = -1; //欲更換之移動方向索引
            List<byte> scanIndex = new List<byte>(); //需搜索的反轉方向索引

            for (int i = 0; i < exchangeDir.Count; i++) //如果棋子設定中, 需實施反轉的方向一對中只存在其中一個, 則加入到索引(兩個方向都存在或是兩個方向都不存在的狀況則不加入)
            {
                if (directionType.Contains(exchangeDir[i][0]) ^ directionType.Contains(exchangeDir[i][1]))
                {
                    scanIndex.Add((byte)i);
                }
            }

            for (int i = 0; i < directionType.Count; i++) //顯示移動方向標示, 若標示需反轉則套用之
            {
                for (int j = 0; j < scanIndex.Count; j++)
                {
                    //可移動方向的標示翻轉
                    if (directionType[i] == exchangeDir[scanIndex[j]][0])
                    {
                        directionType[i] = exchangeDir[scanIndex[j]][1];
                        break;
                    }
                    else if (directionType[i] == exchangeDir[scanIndex[j]][1])
                    {
                        directionType[i] = exchangeDir[scanIndex[j]][0];
                        break;
                    }
                }
            }
        }

        //可移動方向標示顯示
        for (int i = 0; i < directionType.Count; i++)
        {
            directionImages[(byte)directionType[i]].gameObject.SetActive(true); //顯示移動方向標示
        }

    }
}
