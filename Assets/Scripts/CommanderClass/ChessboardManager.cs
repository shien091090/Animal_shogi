//棋盤控制
//[Partial]主要腳本, 下棋邏輯
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMoveAction //玩家移動命令(儲存起點格與目標格)
{
    private bool isSet = false; //是否為已設定
    public bool GetState { get { return isSet; } }
    public ChessBehavior OriginChess { private set; get; } //所選棋子
    public CellBehavior TargetCell { private set; get; } //目標格
    public ChessBehavior EattedChess { private set; get; } //被吃掉的棋子
    public bool moveMode; //移動模式(true = 一般 / false = 打入)

    //建構子
    public PlayerMoveAction(ChessBehavior origin, CellBehavior target, bool mode)
    {
        isSet = true;
        OriginChess = origin;
        TargetCell = target;
        moveMode = mode;

        //檢測目標格是否有對方棋子, 若有則設定為被吃掉的棋子
        if (target.chessScript != null && target.chessScript.chessPlayer != GameController.Instance.nowPlayer)
        {
            EattedChess = target.chessScript;
        }
        else
        {
            EattedChess = null;
        }
    }

    //清空行動狀態
    public void ActionClear()
    {
        OriginChess = null;
        TargetCell = null;
        EattedChess = null;
        isSet = false;
    }
}

public partial class ChessboardManager : MonoBehaviour
{
    private static ChessboardManager _instance; //單例物件
    public static ChessboardManager Instance { get { return _instance; } } //取得單例物件

    [Header("參考物件")]
    public GameObject cellPrefab; //棋格預置體
    public GameObject chessPrefab; //棋子預置體
    public DropPawnPanelManager dropPawnPanel_player1; //玩家1的打入預備棋區域
    public DropPawnPanelManager dropPawnPanel_player2; //玩家2的打入預備棋區域
    public RectTransform dragingItemParent; //拖曳中物件的父物件
    public CellBehavior[,] cellsBoard; //棋盤(二維陣列)(儲存棋格的Prefab)

    [Header("可自訂參數")]
    public Vector2 chessBoardSize; //棋盤尺寸
    public List<AttributeData> chessSettings;

    public Dictionary<AnimalChessName, ChessAttribute> Dict_ChessAttribute;

    [Header("遊戲參數")]
    public PlayerMoveAction playerMoveAction; //玩家棋子移動命令

    [Header("遊戲進行狀態")]
    public CellBehavior stayingCell; //鼠標滯留格子
    public CellBehavior clickedCell; //已點擊的格子
    public CellBehavior mouseUpCell; //左鍵彈起時鼠標所在的格子
    public CellBehavior dragingCell; //拖曳中的格子

    public event System.EventHandler SignSetted; //標記事件
    public event System.Action<CellBehavior, List<CellBehavior>> FeedbackMovementPos; //取得指定棋格上棋子的可移動格

    public delegate void del_feedbackPos(CellBehavior cell);
    public del_feedbackPos FeedbackDropPawnPos; //可打入格判斷事件

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式

        Dict_ChessAttribute = new Dictionary<AnimalChessName, ChessAttribute>();

        //建立字典
        for (int i = 0; i < chessSettings.Count; i++)
        {
            Dict_ChessAttribute.Add(chessSettings[i].m_data.chessName, chessSettings[i].m_data);
        }

        //GameController.Instance.debugerText.text = "Test2";
    }

    //-------------------------------------------------------------------------------------------------------------------

    ////取得指定棋格上棋子的可移動格(結果存在activePosList中)
    ////(多載1/2) 輸入Vector(位置)
    //public List<CellBehavior> GetMovementPos(Vector2 selectedPos)
    //{
    //    CellBehavior cell = cellsBoard[(int)selectedPos.x, (int)selectedPos.y]; //取得棋格物件
    //    Debug.Log(selectedPos);

    //    return GetMovementPos(cell);
    //}

    //取得指定棋格上棋子的可移動格(結果存在activePosList中)
    public List<CellBehavior> GetMovementPos(CellBehavior selectedCell)
    {
        //以下條件發生時, 無法取得可移動格, 直接結束程序
        //if (selectedCell.cTag == CellTag.打入預備格) return null; //選定格為打入預備格時
        if (selectedCell.chessScript == null) return null; //選定格上無棋子時
        else if (selectedCell.chessScript != null && selectedCell.chessScript.chessPlayer != GameController.Instance.nowPlayer) return null; //選定格棋子非自己的棋子時
        else if (FeedbackMovementPos == null) return null; //呼叫事件為空時

        List<CellBehavior> resultList = new List<CellBehavior>();

        FeedbackMovementPos.Invoke(selectedCell, resultList); //取得可移動格(結果存入resultList)

        return resultList; //返回結果
    }

    //創建棋盤(row=列數 / column=欄數)
    public void SetChessBoard()
    {
        cellsBoard = new CellBehavior[(int)chessBoardSize.x, (int)chessBoardSize.y]; //創建棋盤陣列

        RectTransform boardRect = UIManager.Instance.chessboard.GetComponent<RectTransform>();
        float chessboard_width = ( UIManager.Instance.cellSize.x * chessBoardSize.x ) + ( UIManager.Instance.spacing * ( chessBoardSize.x - 1 ) ); //計算棋盤寬度
        float chessboard_height = ( UIManager.Instance.cellSize.y * chessBoardSize.y ) + ( UIManager.Instance.spacing * ( chessBoardSize.y - 1 ) ); //計算棋盤高度

        if (chessboard_width > boardRect.sizeDelta.x) throw new System.Exception("[ERROR]棋格寬度設定過寬, 請從UIManager調整之。");
        if (chessboard_height > boardRect.sizeDelta.y) throw new System.Exception("[ERROR]棋格高度設定過高, 請從UIManager調整之。");

        boardRect.sizeDelta = new Vector2(chessboard_width, boardRect.sizeDelta.y); //設定棋盤尺寸

        //Debug.Log(string.Format("[棋盤大小] X: {0}, Y: {1}", chessBoardSize.x, chessBoardSize.y));

        List<Vector2> levelUpCells_p1 = new List<Vector2>(); //正面方升變棋格
        List<Vector2> levelUpCells_p2 = new List<Vector2>(); //反面方升變棋格
        for (int i = 0; i < chessBoardSize.x; i++) //設定雙方升變棋格(預設為底部最後一排)
        {
            levelUpCells_p1.Add(new Vector2(i, chessBoardSize.y - 1)); //設定正面方
            levelUpCells_p2.Add(new Vector2(i, 0)); //設定反面方
        }

        Transform parent = UIManager.Instance.chessboard.transform; //棋格父物件

        //依據棋盤尺寸(高&寬)創立棋格物件
        for (int i = 0; i < cellsBoard.GetLength(1); i++) //高度(Y軸)
        {
            for (int j = 0; j < cellsBoard.GetLength(0); j++) //寬度(X軸)
            {

                GameObject cell = Instantiate(cellPrefab, parent); //創立棋格物件

                cellsBoard[j, (int)( chessBoardSize.y - 1 - i )] = cell.GetComponent<CellBehavior>(); //設定棋格腳本
                cellsBoard[j, (int)( chessBoardSize.y - 1 - i )].name = string.Format("Cell[{0},{1}]", j, chessBoardSize.y - 1 - i); //設定棋格物件名稱
                cellsBoard[j, (int)( chessBoardSize.y - 1 - i )].pos = new Vector2(j, (int)( chessBoardSize.y - 1 - i )); //設定棋格位置
                cellsBoard[j, (int)( chessBoardSize.y - 1 - i )].cTag = CellTag.棋盤格; //初始化棋盤格標記
            }
        }

        //for (int i = 0; i < cellsBoard.GetLength(1); i++) //高度(Y軸)
        //{
        //    for (int j = 0; j < cellsBoard.GetLength(0); j++) //寬度(X軸)
        //    {
        //        Debug.Log(string.Format("Cell[{0}, {1}] = pos[{2}, {3}]", j, i, cellsBoard[j, i].pos.x, cellsBoard[j, i].pos.y));

        //    }
        //}

        for (int k = 0; k < levelUpCells_p1.Count; k++) //設定正面方升變棋盤格
        {
            cellsBoard[(int)levelUpCells_p1[k][0], (int)levelUpCells_p1[k][1]].cTag = CellTag.正面方升變棋盤格;
        }

        for (int k = 0; k < levelUpCells_p2.Count; k++) //設定反面方升變棋盤格
        {
            cellsBoard[(int)levelUpCells_p2[k][0], (int)levelUpCells_p2[k][1]].cTag = CellTag.反面方升變棋盤格;
        }

    }

    //設置棋子於指定位置
    //[input] name=棋子名稱 / pos=位置 / player=玩家陣營(True=正面方/False=反面方) / isKing=是否為王
    public void CreateChess(AnimalChessName name, CellBehavior cell, Camps player, bool isKing)
    {
        GameObject chessGo = Instantiate(chessPrefab, cell.GetComponent<RectTransform>()); //創建指定的棋子物件
        ChessBehavior ChessScript = chessGo.GetComponent<ChessBehavior>(); //取得棋子腳本

        ChessScript.ChessInitialize(Dict_ChessAttribute[name], player, isKing); //套用棋子屬性設定

        cell.chessScript = ChessScript; //將棋子腳本註冊至所在的棋格上
    }

    //重設棋盤碰撞體範圍
    public void ResetChessboardCollider()
    {
        for (int i = 0; i < cellsBoard.GetLength(0); i++)
        {
            for (int j = 0; j < cellsBoard.GetLength(1); j++)
            {
                cellsBoard[i, j].ResetColliderRange();
            }
        }
    }

    //棋子移動(多載1/2) 從格子到格子
    public void Move(CellBehavior originCell, CellBehavior targetCell, bool clearOriginCell)
    {
        _Move(originCell, originCell.chessScript, targetCell, clearOriginCell);
    }

    //棋子移動(多載2/2) 指定棋子到格子
    public void Move(ChessBehavior originChess, CellBehavior targetCell, bool clearOriginCell)
    {
        if (originChess.transform.parent == null)
        {
            _Move(null, originChess, targetCell, clearOriginCell);
        }
        else
        {
            _Move(originChess.transform.parent.GetComponent<CellBehavior>(), originChess, targetCell, clearOriginCell);
        }
    }

    //[Private]棋子移動
    private void _Move(CellBehavior originCell, ChessBehavior originChess, CellBehavior targetCell, bool clearOriginCell)
    {
        //位置移位
        originChess.transform.SetParent(targetCell.transform);
        originChess.transform.localPosition = Vector2.zero;

        //腳本移位
        targetCell.chessScript = originChess;
        if (originCell != null && clearOriginCell) originCell.chessScript = null;

        //重設尺寸
        originChess.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        //"打入"的狀況將棋格消除
        if (originCell.cTag == CellTag.打入預備格)
        {
            originCell.GetComponentInParent<DropPawnPanelManager>().dropPawnCells.Remove(originCell);
            Destroy(originCell.gameObject);
        }
    }

    //棋子移動判斷
    //originCell : 起點格 / targetCell : 目標格
    public void ChessMoveTest(CellBehavior originCell, Vector2 targetPos)
    {
        List<CellBehavior> movementCellList = new List<CellBehavior>();
        movementCellList = GetMovementPos(originCell);

        if (movementCellList == null || movementCellList.Count == 0) //無法移動至任何格子時, 直接結束程序
        {
            Debug.Log("無法移動至任何格子");
            return;
        }

        //for (int i = 0; i < movementCellList.Count; i++)
        //{
        //    Debug.Log(string.Format("[ {0} ] : ({1}, {2})", i, movementCellList[i].pos.x, movementCellList[i].pos.y));
        //}

        //若可移動格子List中包含欲移動之位置
        bool canMove = false;
        for (int i = 0; i < movementCellList.Count; i++)
        {
            if (movementCellList[i].pos == targetPos)
            {
                canMove = true;
                break;
            }
        }

        bool moveMode = ( originCell.cTag == CellTag.打入預備格 ) ? false : true;

        if (canMove) playerMoveAction = new PlayerMoveAction(originCell.chessScript, cellsBoard[(int)targetPos.x, (int)targetPos.y], moveMode); //設定玩家棋子移動命令
        else Debug.Log("無法移動到指定位置");
    }
}
