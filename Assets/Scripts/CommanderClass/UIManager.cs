using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance; //單例物件
    public static UIManager Instance { get { return _instance; } } //取得單例物件

    [Header("參考物件")]
    public GameObject chessboard; //棋盤物件
    public RectTransform screenRect; //遊戲畫面矩形(Rect)

    [Header("自定義參數")]
    public Color playerColor; //正面玩家外框色
    public Color enemyColor; //對面玩家外框色
    public Color cellColor_mouseStay; //滑鼠停滯在棋格上的顏色
    public Color cellColor_highlight; //棋格顯示為可移動方向的顏色
    public Color cellColor_selection; //選定中的棋格(我方棋子)
    public float dragingChessAlpha; //拖曳中棋子的透明度
    public Vector2 cellSize; //棋格大小
    public float spacing; //棋格間隔

    [Header("遊戲結束畫面")]
    public GameObject winnerPanel; //玩家勝利視窗
    private Text winText; //勝利訊息
    private Button resetGameButton; //重置按鈕

    private Vector2 screenEdge_x; //畫面邊界(X軸) ( [0]=左 , [1]=右 )
    public Vector2 GetScreenEdge_x { get { return screenEdge_x; } } //取得畫面邊界(X軸)
    private Vector2 screenEdge_y; //畫面邊界(Y軸) ( [0]=下 , [1]=上 )
    public Vector2 GetScreenEdge_y { get { return screenEdge_y; } } //取得畫面邊界(Y軸)

    //----------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
        winText = winnerPanel.GetComponentInChildren<Text>();
        resetGameButton = winnerPanel.GetComponentInChildren<Button>();
    }

    void Start()
    {
        //設定畫面邊界
        if (screenRect != null)
        {
            screenEdge_x = new Vector2(-( screenRect.sizeDelta.x / 2 ), screenRect.sizeDelta.x / 2);
            screenEdge_y = new Vector2(-( screenRect.sizeDelta.y / 2 ), screenRect.sizeDelta.y / 2);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------

    //呼叫玩家勝利視窗
    public void CallWinningWindow(Camps p)
    {
        winText.text = p.ToString() + " 獲勝!!";

        winnerPanel.SetActive(true);
        Text bt = resetGameButton.gameObject.GetComponentInChildren<Text>();
        bt.text = "重置遊戲";
        resetGameButton.onClick.AddListener(GameController.Instance.ResetGame);
    }

    //關閉玩家勝利視窗
    public void CloseWinningWindow()
    {
        winText.text = string.Empty;

        winnerPanel.SetActive(false);
        //resetGameButton.onClick.AddListener(null);
    }

}
