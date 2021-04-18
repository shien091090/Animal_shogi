//監聽來自玩家的控制資訊(滑鼠事件監聽)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance; //單例物件
    public static PlayerController Instance { get { return _instance; } } //取得單例物件

    [Header("遊戲參數")]
    public bool isWorking; //是否允許操作
    public bool isClicking = false; //滑鼠是否點擊

    private Ray ray; //滑鼠位置射線
    private RaycastHit2D raycastHit; //射線接觸資訊

    //-------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        raycastHit = Physics2D.Raycast(ray.origin, ray.direction);

        if (isWorking) InputListen(raycastHit); //玩家事件監聽
    }

    //-------------------------------------------------------------------------------------------------------------------

    //玩家事件監聽
    private void InputListen(RaycastHit2D raycastHit)
    {
        if (Input.GetMouseButtonDown(0)) //點滑鼠左鍵
        {
            ChessboardManager.Instance.mouseUpCell = null; //點擊滑鼠左鍵時, "左鍵放開格子"設為null
            isClicking = true;

            if (raycastHit.transform == null || raycastHit.transform.gameObject.GetComponent<CellBehavior>() == null) ChessboardManager.Instance.clickedCell = null; //滑鼠點擊位置沒有物件 或 點擊到的物件沒有CellBehavior時, 設"所點擊格子"為null
            else ChessboardManager.Instance.clickedCell = raycastHit.transform.gameObject.GetComponent<CellBehavior>(); //滑鼠點擊到的物件有CellBehavior, 設定為"所點擊格子"
        }

        if (Input.GetMouseButtonUp(0)) //放開滑鼠左鍵
        {
            ChessboardManager.Instance.clickedCell = null; //滑鼠左鍵放開時, "所點擊格子"設為null
            isClicking = false;

            if (raycastHit.transform == null || raycastHit.transform.gameObject.GetComponent<CellBehavior>() == null) ChessboardManager.Instance.mouseUpCell = null; //滑鼠左鍵放開位置沒有物件 或 位置上物件沒有CellBehavior時, 設"左鍵放開格子"為null
            else ChessboardManager.Instance.mouseUpCell = raycastHit.transform.gameObject.GetComponent<CellBehavior>(); //滑鼠左鍵放開位置物件有CellBehavior, 設定為"左鍵放開格子"
        }

        //測試用 -------------------------------------

        if(Input.GetKeyDown(KeyCode.F1)) //重置遊戲
        {
            GameController.Instance.ResetGame();
        }

        //--------------------------------------------

        //滑鼠停滯事件
        if (raycastHit.transform == null || raycastHit.transform.gameObject.GetComponent<CellBehavior>() == null) ChessboardManager.Instance.stayingCell = null; //滑鼠位置沒有物件 或 位置上物件沒有CellBehavior時, 設"鼠標停滯格子"為null
        else ChessboardManager.Instance.stayingCell = raycastHit.transform.gameObject.GetComponent<CellBehavior>(); //鼠標位置物件有CellBehavior, 設定為"鼠標滯留格子"
    }

}
