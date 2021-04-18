using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//標記輸入資訊(事件用)
public class SignInfo : System.EventArgs
{
    public List<CellBehavior> targetCells { private set; get; }
    public HighlightType highlightType { private set; get; }

    //建構子
    public SignInfo(List<CellBehavior> targets, HighlightType type)
    {
        targetCells = targets;
        highlightType = type;
    }
}

public class CellBehavior : MonoBehaviour
{
    public GameObject stayFrame; //滑鼠停滯指示框
    public ChessBehavior chessScript; //擺在此格上的棋子
    public SignElement signElement; //標記腳本
    public Vector2 pos; //棋格編號
    public CellTag cTag; //棋格標記

    private BoxCollider2D cld;

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        cld = this.GetComponent<BoxCollider2D>();
        ChessboardManager.Instance.SignSetted += SignEvent; //標記事件
        //if (cTag != CellTag.打入預備格) ChessboardManager.Instance.FeedbackActivePos += ActiveCellJudgement; //回傳可移動格呼叫事件
        if (cTag != CellTag.打入預備格) ChessboardManager.Instance.FeedbackMovementPos += ActiveCellJudgement;
    }

    //-------------------------------------------------------------------------------------------------------------------

    //可移動格邏輯判斷演算
    //傳入的棋格物件(欲判斷棋格)
    private void ActiveCellJudgement(CellBehavior focusCell, List<CellBehavior> resultList)
    {
        //Debug.Log(string.Format("[Func Test] Cell[{0}, {1}]", pos[0], pos[1]));

        if (focusCell.chessScript == null) return; //若棋格不含棋子物件時, 返回null並跳出

        ChessBehavior focusChess = focusCell.chessScript; //棋格上的棋子腳本

        if (focusCell.cTag == CellTag.打入預備格) //打入的狀況
        {
            if (chessScript == null) resultList.Add(this); //若棋格上無棋子即可打入
        }
        else //一般下棋的狀況
        {
            //滿足條件時要執行的程式
            UnityEngine.Events.UnityAction resultAction = () =>
            {
                if (chessScript != null && chessScript.chessPlayer == GameController.Instance.nowPlayer) return; //若格子上的棋子為自己陣營的棋子時, 不可移動

                //ChessboardManager.Instance.activePosList.Add(pos);
                resultList.Add(this);
                return;
            };

            for (int i = 0; i < focusChess.directionType.Count; i++) //遍歷指定棋子的所有可走方向, 若有符合者則追加至可移動列表(ChessboardManager的activePosList)
            {
                switch (focusChess.directionType[i])
                {
                    case DirectionType.左上:
                        if (focusCell.pos.x - 1 == pos.x && focusCell.pos.y + 1 == pos.y) resultAction();
                        break;

                    case DirectionType.上:
                        if (focusCell.pos.x == pos.x && focusCell.pos.y + 1 == pos.y) resultAction();
                        break;

                    case DirectionType.右上:
                        if (focusCell.pos.x + 1 == pos.x && focusCell.pos.y + 1 == pos.y) resultAction();
                        break;

                    case DirectionType.左:
                        if (focusCell.pos.x - 1 == pos.x && focusCell.pos.y == pos.y) resultAction();
                        break;

                    case DirectionType.右:
                        if (focusCell.pos.x + 1 == pos.x && focusCell.pos.y == pos.y) resultAction();
                        break;

                    case DirectionType.左下:
                        if (focusCell.pos.x - 1 == pos.x && focusCell.pos.y - 1 == pos.y) resultAction();
                        break;

                    case DirectionType.下:
                        if (focusCell.pos.x == pos.x && focusCell.pos.y - 1 == pos.y) resultAction();
                        break;

                    case DirectionType.右下:
                        if (focusCell.pos.x + 1 == pos.x && focusCell.pos.y - 1 == pos.y) resultAction();
                        break;
                }
            }
        }

    }

    //重設棋格碰撞範圍
    public void ResetColliderRange()
    {
        cld.size = this.GetComponent<RectTransform>().sizeDelta;
    }

    //標記事件
    private void SignEvent(object sendor, System.EventArgs e)
    {
        if (signElement == null) return; //若此格子沒有標記功能, 則直接結束程序

        SignInfo info = (SignInfo)e;

        if (info.targetCells == null) //若標記作用格為null, 則視為"清空"標記
        {
            signElement.SetHighlightState(info.highlightType, false); //設定指定標記類型為Off
        }
        else if (info.targetCells.Contains(this)) //若為標記作用格
        {
            signElement.SetHighlightState(info.highlightType, true); //設定指定標記類型為On
        }
        else //非標記作用格
        {
            signElement.SetHighlightState(info.highlightType, false); //設定指定標記類型為Off
        }
    }
}
