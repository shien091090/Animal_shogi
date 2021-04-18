//遊戲進程控制
//遊戲推進順序，進行狀態控制等在此腳本實現

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameController : MonoBehaviour
{
    private static GameController _instance; //單例物件
    public static GameController Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public Camps firstPlayer; //第一手玩家

    [Header("參考物件")]
    public BoardRecordData boardRecordData; //棋譜資料(ScriptableObject)

    [Header("遊戲進行狀態")]
    public Camps nowPlayer; //目前回合玩家
    public Camps conquerPlayer = Camps.無; //預備勝利玩家(王走到底)
    public Camps winPlayer = Camps.無; //勝利玩家
    [SerializeField]
    public bool StepEnd { private set; get; } //任何玩家是否結束現在棋步
    private bool pcs_eat = false; //遊戲流程控制 : 是否完成"吃子"
    private bool pcs_levelUp = false; //遊戲流程控制 : 是否完成"升變"

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
    }

    void Start()
    {
        StartCoroutine(Cor_MainProcess(true, firstPlayer, 0)); //開始遊戲流程
    }

    void Update()
    {
        ChessboardManager.Instance.EventListen(); //玩家事件監聽
    }

    //-------------------------------------------------------------------------------------------------------------------

    //重置遊戲
    public void ResetGame()
    {
        StopAllCoroutines(); //停止全部程序
        StartCoroutine(Cor_MainProcess(false, firstPlayer, 0)); //開始遊戲流程
    }

    //棋盤初始化
    private void ChessBoardInitialize(int num)
    {
        //查找棋譜資料裡是否有相應編號
        for (int i = 0; i < boardRecordData.m_data.Count; i++)
        {
            if (boardRecordData.m_data[i].number == num) break; //找到相應編號後繼續程序
            if (i == boardRecordData.m_data.Count) throw new System.Exception("[ERROR]棋譜資料內無相應編號");
        }

        //棋盤配置
        for (int i = 0; i < boardRecordData.m_data[num].boardRecord.Count; i++)
        {
            AnimalChessName _chessName = boardRecordData.m_data[num].boardRecord[i].chessName; //取得棋子名稱
            Vector2 _pos = boardRecordData.m_data[num].boardRecord[i].pos; //取得棋子位置
            Camps _camps = boardRecordData.m_data[num].boardRecord[i].camps; //取得棋子陣營
            bool _isKing = boardRecordData.m_data[num].boardRecord[i].isKing; //是否為王

            ChessboardManager.Instance.CreateChess(_chessName, ChessboardManager.Instance.cellsBoard[(int)_pos.x, (int)_pos.y], _camps, _isKing);
        }
    }

    //遊戲結束(選擇關閉或重置)
    private IEnumerator GameOver()
    {
        yield return null;
    }
}