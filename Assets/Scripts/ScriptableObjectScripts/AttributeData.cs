using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChessAttribute
{
    public AnimalChessName chessName; //棋子名稱
    public List<DirectionType> directionType; //可移動方向(複數)
    public AnimalChessName levelUpChess; //升變後棋子
    public AnimalChessName dropPawnChess; //打入棋子(當吃掉棋子時, 棋子會變成這邊所設定的棋子加入打入預備棋)
    public Sprite animalIcon; //棋子圖示
}

[CreateAssetMenu(fileName = "New Data", menuName = "ChessAttribute/Create New", order = 1)]
public class AttributeData : ScriptableObject
{
    public ChessAttribute m_data;
}