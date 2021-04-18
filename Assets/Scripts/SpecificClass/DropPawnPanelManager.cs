//顯示打入預備棋的UI區域控制腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DropPawnPanelManager : MonoBehaviour
{
    //public GridLayoutGroup chessboardLayout; //棋盤的GridLayoutGroup
    public Vector2 defultSpacing; //預設間隔
    public List<CellBehavior> dropPawnCells = new List<CellBehavior>(); //打入預備棋格列表

    private GridLayoutGroup dropPawnPanelLayout; //此物件的GridLayoutGroup

    //-------------------------------------------------------------------------------------------------------------------
    void Awake()
    {
        dropPawnPanelLayout = this.GetComponent<GridLayoutGroup>();

    }

    //-------------------------------------------------------------------------------------------------------------------

    //設定UI元素尺寸
    public void SetElementSize(Vector2 cellSize, Vector2 spacing)
    {
        StartCoroutine(Cor_SetElementSize(cellSize, spacing));
    }

    //自動調整UI元素尺寸
    public void AutoLayout()
    {
        float elementHeight =
            dropPawnPanelLayout.padding.top
            + ( dropPawnCells.Count * dropPawnPanelLayout.cellSize.y )
            + ( ( dropPawnCells.Count - 1 ) * dropPawnPanelLayout.spacing.y );

        //若UI元素超出邊界
        if (elementHeight > dropPawnPanelLayout.gameObject.GetComponent<RectTransform>().sizeDelta.y)
        {
            float reduceRate = ( dropPawnPanelLayout.gameObject.GetComponent<RectTransform>().sizeDelta.y - dropPawnPanelLayout.padding.top ) / ( elementHeight - dropPawnPanelLayout.padding.top );
            SetElementSize(new Vector2(dropPawnPanelLayout.cellSize.x * reduceRate, dropPawnPanelLayout.cellSize.y * reduceRate), new Vector2(dropPawnPanelLayout.spacing.x, dropPawnPanelLayout.spacing.y * reduceRate));
        }
        else
        {
            SetElementSize(UIManager.Instance.chessboard.GetComponent<GridLayoutGroup>().cellSize, defultSpacing); //設為預設元素尺寸
        }
    }

    //加入打入預備棋
    public void AddDropPawnChess(ChessBehavior originChess, Camps changeTo)
    {
        GameObject cellGo = Instantiate(ChessboardManager.Instance.cellPrefab, this.transform);
        CellBehavior cell = cellGo.GetComponent<CellBehavior>();

        dropPawnCells.Add(cell); //加入打入預備棋格物件

        cell.cTag = CellTag.打入預備格; //設定格子類型
        cell.pos = new Vector2(-100, -100); //設定格子位置

        AutoLayout(); //自動調整尺寸

        ChessboardManager.Instance.Move(originChess, cell, false); //將棋子移動到打入預備格

        originChess.ChessInitialize(ChessboardManager.Instance.Dict_ChessAttribute[originChess.GetAttribure.dropPawnChess], changeTo, originChess.isKing); //若棋子被吃掉後會改變型態, 則更新棋子狀態
    }

    //設定UI元素尺寸
    private IEnumerator Cor_SetElementSize(Vector2 cellSize, Vector2 spacing)
    {
        dropPawnPanelLayout.enabled = true;

        dropPawnPanelLayout.cellSize = cellSize;
        dropPawnPanelLayout.spacing = spacing;

        yield return new WaitForFixedUpdate();

        dropPawnPanelLayout.enabled = false;
    }
}
