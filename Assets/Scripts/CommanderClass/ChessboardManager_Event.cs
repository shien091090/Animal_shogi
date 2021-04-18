//棋盤控制
//[Partial]棋盤事件
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class ChessboardManager : MonoBehaviour
{
    //事件監聽
    public void EventListen()
    {
        MouseStay(); //鼠標停滯事件監聽
        MouseClick(); //左鍵點擊事件監聽
        MouseUp(); //左鍵彈起事件監聽
    }

    //滑鼠停駐事件
    private void MouseStay()
    {
        //滑鼠滯留標記
        SignInfo mouseStayingArgs = new SignInfo(new List<CellBehavior>() { stayingCell }, HighlightType.鼠標滯留);
        SignSetted(this, mouseStayingArgs); //觸發標記事件

        //可移動標記
        if (stayingCell != null && stayingCell.chessScript != null && stayingCell.chessScript.chessPlayer == GameController.Instance.nowPlayer && PlayerController.Instance.isWorking) //若鼠標所停滯的格子上有棋子 且 棋子為我方的
        {
            SignInfo movementArgs = new SignInfo(GetMovementPos(stayingCell), HighlightType.可移動);
            SignSetted(this, movementArgs); //觸發標記事件
        }
        else if (dragingCell != null) //若為拖曳中狀態, 顯示標記
        {
            SignInfo movementArgs = new SignInfo(GetMovementPos(dragingCell), HighlightType.可移動);
            SignSetted(this, movementArgs); //觸發標記事件
        }
        else //非拖曳狀態時, 消除標記
        {
            SignInfo movementArgs = new SignInfo(null, HighlightType.可移動);
            SignSetted(this, movementArgs); //觸發標記事件
        }
    }

    //滑鼠點擊事件
    private void MouseClick()
    {
        if (dragingCell == null && clickedCell != null && clickedCell.chessScript != null && clickedCell.chessScript.chessPlayer == GameController.Instance.nowPlayer) //若尚未點擊 且 所點擊格子不為null 且 所點擊的格子上有棋子 且 棋子為我方的
        {
            dragingCell = clickedCell; //設定拖曳指定格子

            GameObject prefab = clickedCell.chessScript.gameObject; //取得欲生成虛擬圖案的模板圖樣
            GameObject go = Instantiate(prefab, dragingItemParent); //生成虛擬圖樣

            //初始化位置&尺寸
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.sizeDelta = UIManager.Instance.cellSize;

            //初始化顏色
            ChessBehavior chessScript = go.GetComponent<ChessBehavior>();
            Color originalColor_frame = chessScript.frameImage.color;
            Color originalColor_animal = chessScript.animalIcon.color;

            chessScript.frameImage.color = new Color(originalColor_frame.r, originalColor_frame.g, originalColor_frame.b, UIManager.Instance.dragingChessAlpha);
            chessScript.animalIcon.color = new Color(originalColor_animal.r, originalColor_animal.g, originalColor_animal.b, UIManager.Instance.dragingChessAlpha);

            StartCoroutine(DragingChess(go.transform)); //開始拖曳
        }
    }

    //滑鼠左鍵放開事件
    private void MouseUp()
    {
        if (dragingCell != null && !PlayerController.Instance.isClicking) //拖曳中狀態 且 左鍵為未點擊狀態(左鍵放開)
        {
            if (mouseUpCell != null) //所拖曳到的位置有格子
            {
                //Debug.Log("mouseUpCell : " + mouseUpCell.pos);
                ChessMoveTest(dragingCell, mouseUpCell.pos);
            }

            dragingCell = null; //清空拖曳指定格子
        }
    }

    //拖曳棋子的效果
    private IEnumerator DragingChess(Transform g)
    {
        while (clickedCell != null)
        {
            Vector3 pos; //拖曳所至位置
            RectTransform parent = UIManager.Instance.chessboard.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, new Vector2(Input.mousePosition.x, Input.mousePosition.y), Camera.main, out pos); //滑鼠位置轉換(從螢幕座標到世界座標)

            g.position = pos;

            yield return new WaitForFixedUpdate();
        }

        Destroy(g.gameObject); //銷毀虛擬圖像物件
    }

}
