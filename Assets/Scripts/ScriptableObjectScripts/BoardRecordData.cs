using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "BoardRecord/Create New", order = 1)]
public class BoardRecordData : ScriptableObject
{
    public List<BoardRecord> m_data = new List<BoardRecord>();
}

//棋子配置
[System.Serializable]
public class ChessDispose
{
    //ChessboardManager.Instance.CreateChess(AnimalChessName.小雞, ChessboardManager.Instance.cellsBoard[1, 0], Camps.正面方);
    public AnimalChessName chessName; //棋子名稱
    public Vector2 pos; //位置([0,0]為最左下)
    public Camps camps; //陣營
    public bool isKing; //是否為王
}

//棋譜配置
[System.Serializable]
public class BoardRecord
{
    public int number; //棋譜編號
    public List<ChessDispose> boardRecord; //棋譜配置
}