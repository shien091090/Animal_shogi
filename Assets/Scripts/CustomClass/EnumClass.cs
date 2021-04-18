using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//可移動方向
public enum DirectionType
{
    左上, 上, 右上, 左, 右, 左下, 下, 右下
}

//棋子名稱
public enum AnimalChessName
{
    小雞, 大象, 長頸鹿, 獅子, 公雞
}

//陣營
public enum Camps
{
    正面方, 反面方, 無
}

//棋格標記
public enum CellTag
{
    棋盤格, 正面方升變棋盤格 , 反面方升變棋盤格, 打入預備格
}

//棋格標示種類
public enum HighlightType
{
    鼠標滯留, 可移動
}

//曲線類型
public enum CurveType
{
    直線, 平滑, 急速切入, 漸進切入
}
