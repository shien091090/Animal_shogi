//遊戲進程控制
//[partial]協程放在這裡

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameController : MonoBehaviour
{
    //遊戲流程控制
    private IEnumerator Cor_MainProcess(bool isAwake, Camps firstPlayer, int boardRecord)
    {
        if (isAwake) yield return StartCoroutine(Cor_BuildEnvironment()); //遊戲初運行時, 先建置環境
        yield return StartCoroutine(Cor_GameInitialize(firstPlayer, boardRecord)); //遊戲初始化
        StartCoroutine(Cor_GameStart()); //遊戲程序
    }

    //建置遊戲環境
    private IEnumerator Cor_BuildEnvironment()
    {
        //棋格設定初始化
        GridLayoutGroup lg = UIManager.Instance.chessboard.GetComponent<GridLayoutGroup>();
        lg.cellSize = UIManager.Instance.cellSize; //設定棋格尺寸
        lg.spacing = new Vector2(UIManager.Instance.spacing, UIManager.Instance.spacing); //設定棋格間隔距離

        //棋盤配置初始化
        ChessboardManager.Instance.SetChessBoard(); //創建棋盤

        //棋盤淡入動畫
        yield return StartCoroutine(Cor_ChessboardFadeIn());

        //Debug.Log("動畫結束");

        ChessboardManager.Instance.ResetChessboardCollider(); //重設棋盤碰撞體範圍
        ChessboardManager.Instance.dropPawnPanel_player1.AutoLayout(); //初始化玩家1打入預備棋區域的元素尺寸
        ChessboardManager.Instance.dropPawnPanel_player2.AutoLayout(); //初始化玩家1打入預備棋區域的元素尺寸
    }

    //遊戲初始化
    private IEnumerator Cor_GameInitialize(Camps firstPlayer, int boardRecord)
    {
        PlayerController.Instance.isWorking = false; //玩家禁止操作

        //UI重置
        UIManager.Instance.CloseWinningWindow(); //關閉玩家勝利視窗

        //遊戲過程參數重置
        pcs_eat = false;
        pcs_levelUp = false;
        conquerPlayer = Camps.無;
        winPlayer = Camps.無;

        //滑鼠事件初始化
        ChessboardManager.Instance.stayingCell = null;
        ChessboardManager.Instance.clickedCell = null;
        ChessboardManager.Instance.mouseUpCell = null;

        //清空棋子
        CellBehavior[,] _cellsBoard = ChessboardManager.Instance.cellsBoard; //取得棋盤

        //Debug.Log("----------------清空棋盤[Start]----------------");
        for (int i = 0; i < _cellsBoard.GetLength(1); i++) //高度(Y軸)
        {
            for (int j = 0; j < _cellsBoard.GetLength(0); j++) //寬度(X軸)
            {
                if (_cellsBoard[j, i].chessScript != null)
                {
                    //Debug.Log(string.Format("[{0}, {1}] : {2}", j, i, _cellsBoard[j, i].chessScript.gameObject.name));

                    GameObject go = _cellsBoard[j, i].chessScript.gameObject;
                    _cellsBoard[j, i].chessScript = null; //清除腳本參考
                    Destroy(go); //清除物件
                }
            }
        }
        //Debug.Log("----------------清空棋盤[End]----------------");

        //Debug.Log("----------------清空打入預備區[Start]----------------");
        DropPawnPanelManager[] dropPawnPanels = new DropPawnPanelManager[2] { ChessboardManager.Instance.dropPawnPanel_player1, ChessboardManager.Instance.dropPawnPanel_player2 };

        for (int i = 0; i < dropPawnPanels.Length; i++)
        {
            for (int j = 0; j < dropPawnPanels[i].dropPawnCells.Count; j++)
            {
                Destroy(dropPawnPanels[i].dropPawnCells[j].chessScript.gameObject); //銷毀棋子
                Destroy(dropPawnPanels[i].dropPawnCells[j].gameObject); //銷毀棋格
            }

            dropPawnPanels[i].dropPawnCells = new List<CellBehavior>(); //List初始化
        }
        //Debug.Log("----------------清空打入預備區[End]----------------");

        ChessBoardInitialize(boardRecord); //棋盤初始化
        nowPlayer = firstPlayer; //設定第一手玩家

        yield return new WaitForEndOfFrame();

        PlayerController.Instance.isWorking = true; //玩家可操作
        StepEnd = false; //玩家下完棋狀態設為"尚未"(false)
    }

    //主要遊戲程序
    private IEnumerator Cor_GameStart()
    {
        while (true)
        {
            if (conquerPlayer != nowPlayer) //王走到底到下一回合後沒有被吃掉則直接勝利
            {
                yield return new WaitUntil(() => ( ChessboardManager.Instance.playerMoveAction.GetState )); //若玩家有執行下棋命令

                PlayerController.Instance.isWorking = false; //玩家不可操作

                yield return StartCoroutine(Cor_ChessMove()); //執行玩家動作

                //等待玩家動作
                yield return new WaitUntil(() => StepEnd); //等待玩家下完後
            }
            else
            {
                winPlayer = nowPlayer; //設定勝利玩家
                PlayerController.Instance.isWorking = false; //玩家不可操作
            }

            ChessboardManager.Instance.playerMoveAction.ActionClear(); //清除玩家下棋資訊

            //動作完畢後, 進行勝負判斷
            if (winPlayer != Camps.無)
            {
                UIManager.Instance.CallWinningWindow(winPlayer);
                break; //若分出勝負則跳出迴圈
            }

            byte playerNum = (byte)nowPlayer;
            nowPlayer = (Camps)( ( playerNum + 1 ) % (byte)Camps.無 ); //換另一名玩家行動
            PlayerController.Instance.isWorking = true; //玩家可操作
            StepEnd = false; //玩家下完棋狀態設為"尚未"(false)
        }
    }

    //棋子移動
    private IEnumerator Cor_ChessMove()
    {
        //TODO:棋子移動特效

        ChessBehavior originChess = ChessboardManager.Instance.playerMoveAction.OriginChess; //所選棋子
        CellBehavior targetCell = ChessboardManager.Instance.playerMoveAction.TargetCell; //目標格

        ChessboardManager.Instance.Move(originChess, targetCell, true); //棋子移動程序

        if (( originChess.chessPlayer == Camps.正面方 && originChess.isKing && targetCell.cTag == CellTag.正面方升變棋盤格 ) ||
        ( originChess.chessPlayer == Camps.反面方 && originChess.isKing && targetCell.cTag == CellTag.反面方升變棋盤格 ))
        {
            if (conquerPlayer == Camps.無) conquerPlayer = nowPlayer; //若王走到底, 則記錄玩家陣營至下一回合判斷勝負
        }

        StartCoroutine(Cor_LevelUp(originChess, targetCell, ChessboardManager.Instance.playerMoveAction.moveMode)); //升變
        StartCoroutine(Cor_Eat()); //吃子

        yield return new WaitUntil(() => ( pcs_eat && pcs_levelUp )); //吃子與升變程序都完成時
        pcs_eat = false;
        pcs_levelUp = false;

        StepEnd = true; //玩家下棋指令結束
    }

    //吃子
    private IEnumerator Cor_Eat()
    {
        if (ChessboardManager.Instance.playerMoveAction.EattedChess == null) //若目標格中無棋子
        {
            Debug.Log("目標格中無棋子, 不執行吃子程序");
        }
        else //若有子可吃
        {
            //TODO:吃子特效

            if (ChessboardManager.Instance.playerMoveAction.EattedChess.isKing)
            {
                if (conquerPlayer != nowPlayer) conquerPlayer = Camps.無; //吃掉對方走到底的王
                winPlayer = nowPlayer; //吃掉王直接勝利
            }

            yield return StartCoroutine(Cor_AddDropPawnCell()); //將吃掉的子加入打入預備格中
        }

        pcs_eat = true;
    }

    //升變
    private IEnumerator Cor_LevelUp(ChessBehavior chess, CellBehavior targetCell, bool moveMode)
    {
        if (!moveMode) goto Ending; //打入不會觸發升變, 故直接結束程序
        if (( chess.chessPlayer == Camps.正面方 && targetCell.cTag != CellTag.正面方升變棋盤格 ) ||
            ( chess.chessPlayer == Camps.反面方 && targetCell.cTag != CellTag.反面方升變棋盤格 )) goto Ending; //若目標格並非升變格
        if (chess.GetAttribure.levelUpChess == chess.GetAttribure.chessName) goto Ending; //若升變指定棋子=自身時, 表示不發生升變

        //TODO : 升變特效
        yield return null;

        chess.ChessInitialize(ChessboardManager.Instance.Dict_ChessAttribute[chess.GetAttribure.levelUpChess], nowPlayer, chess.isKing);

Ending:
        pcs_levelUp = true;
    }

    //加入打入預備棋格
    private IEnumerator Cor_AddDropPawnCell()
    {
        //TODO:加入預備棋格特效

        ChessBehavior targetChess = ChessboardManager.Instance.playerMoveAction.EattedChess;

        //加入打入預備棋區域
        if (targetChess.chessPlayer == Camps.反面方) //被吃掉的是反面方棋子
        {
            ChessboardManager.Instance.dropPawnPanel_player1.AddDropPawnChess(targetChess, Camps.正面方);
        }
        else //被吃掉的是正面方棋子
        {
            ChessboardManager.Instance.dropPawnPanel_player2.AddDropPawnChess(targetChess, Camps.反面方);
        }

        yield return null;
    }

    //(動畫)棋盤淡入
    private IEnumerator Cor_ChessboardFadeIn()
    {
        //Debug.Log("Cor_ChessboardFadeIn");

        CellBehavior[,] _cellsBoard = ChessboardManager.Instance.cellsBoard; //棋盤
        List<RectTransform> _originPosList = new List<RectTransform>(); //原點紀錄列表

        //關閉Image顯示
        for (int i = 0; i < _cellsBoard.GetLength(1); i++) //高度(Y軸)
        {
            for (int j = 0; j < _cellsBoard.GetLength(0); j++) //寬度(X軸)
            {
                _cellsBoard[j, i].GetComponent<Image>().enabled = false;
                _originPosList.Add(_cellsBoard[j, i].GetComponent<RectTransform>());
            }
        }

        GridLayoutGroup lg = UIManager.Instance.chessboard.GetComponent<GridLayoutGroup>();

        yield return new WaitForEndOfFrame(); //需追加1幀等待才可順利調整LayoutGroup
        lg.enabled = false; //關閉GridLayoutGroup組件(配置棋盤後關閉之以節省消耗)

        //for (int i = 0; i < _originPosList.Count; i++)
        //{
        //    Debug.Log(string.Format("[{0}][{1}] ({2}, {3})", i, _originPosList[i].name, _originPosList[i].localPosition.x, _originPosList[i].localPosition.y));
        //}

        List<ArrayList> cellTrailLogs = new List<ArrayList>(); //棋格軌跡儲存(淡入動畫用) List index=第N格 , ArrayList[0] = transform(棋格物件) / [1] = Vector2(移入位置)
        float _d = AnimationController.Instance.startPosDistance;

        for (int i = 0; i < _originPosList.Count; i++)
        {
            ArrayList _trailInfo = new ArrayList();
            _trailInfo.Add(_originPosList[i]);
            _trailInfo.Add(_originPosList[i].localPosition);

            cellTrailLogs.Add(_trailInfo);

            //以下為設定棋格淡入起始位置(畫面外)

            //X軸起始點
            Vector2 startPos_x = new Vector2(UIManager.Instance.GetScreenEdge_x.x - ( UIManager.Instance.cellSize.x / 2 ), UIManager.Instance.GetScreenEdge_x.y + ( UIManager.Instance.cellSize.x / 2 ));

            //Y軸起始點
            Vector2 startPos_y = new Vector2(UIManager.Instance.GetScreenEdge_y.x - ( UIManager.Instance.cellSize.y / 2 ), UIManager.Instance.GetScreenEdge_y.y + ( UIManager.Instance.cellSize.y / 2 ));

            //Debug.Log(string.Format("[X軸] = {0}~{1} , [Y軸] = {2}~{3}", startPos_x.x, startPos_x.y, startPos_y.x, startPos_y.y));

            //重設物件錨點
            _originPosList[i].anchorMax = new Vector2(0.5f, 0.5f);
            _originPosList[i].anchorMin = new Vector2(0.5f, 0.5f);

            //隨機位置
            if (Random.Range(0, 2) == 0) //X軸變動, Y軸固定
            {
                Vector3 _p = (Vector3)_trailInfo[1];
                _originPosList[i].localPosition = new Vector2(Random.Range(0, 2) == 0 ? startPos_x.x - _d : startPos_x.y + _d, _p.y);
            }
            else //X軸固定, Y軸變動
            {
                Vector3 _p = (Vector3)_trailInfo[1];
                _originPosList[i].localPosition = new Vector2(_p.x, Random.Range(0, 2) == 0 ? startPos_y.x - _d : startPos_y.y + _d);
            }

            //Debug.Log(string.Format("{0} [{1}] ({2}, {3})", _originPosList[i].name, i, _originPosList[i].localPosition.x, _originPosList[i].localPosition.y));
        }

        yield return new WaitForEndOfFrame();

        //開啟Image顯示
        for (int i = 0; i < _cellsBoard.GetLength(1); i++) //高度(Y軸)
        {
            for (int j = 0; j < _cellsBoard.GetLength(0); j++) //寬度(X軸)
            {
                _cellsBoard[j, i].GetComponent<Image>().enabled = true;
            }
        }

        //執行淡入動畫
        for (int i = 0; i < cellTrailLogs.Count; i++)
        {
            Transform _go = (Transform)cellTrailLogs[i][0];
            Vector3 _targetPos = (Vector3)cellTrailLogs[i][1];

            if (i == cellTrailLogs.Count - 1) yield return StartCoroutine(AnimationController.Instance.Cor_SmoothMove(_go, _targetPos, 1.5f, CurveType.平滑));
            else StartCoroutine(AnimationController.Instance.Cor_SmoothMove(_go, _targetPos, 1.5f, CurveType.平滑));
        }

        yield return null;
    }
}