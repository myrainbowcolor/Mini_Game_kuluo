using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlay : MonoBehaviour
{
    // 常量  
    private int playerId_1 = 1;
    private int playerId_2 = 2;

    // 依赖参数  
    public Transform grid;       // 棋盘父对象  
    public Button restartButton; // 重启按钮  
    public Text statusText;      // 状态显示  
    public Transform PlayerOneVectoryInfo;
    public Transform PlayerTwoVectoryInfo;
    public Transform AllPlayerDrawInfo;
    public Transform TimeLine;
    public float ExplorationProbability;
    public AudioClip[] AudioClipArr;

    // 游戏状态  
    private int[,] boardState = new int[3, 3]; // 默认0，其余数字为玩家id  
    private bool isGameOver;
    private int preWinerPlayerId = -1; // 上一把获胜玩家ID

    // 缓存数据  
    private Dictionary<int, Transform> boardIdMap = new(); // 映射  
    private Dictionary<Transform, int> boardCellMap = new(); // 映射  
    private Dictionary<string, int> playerOneScoreMap = new();
    private Dictionary<string, int> playerTwoScoreMap = new();
    private int col = 0;
    private int row = 0;
    private Coroutine coroutine = null;
    private MiniMaxAI miniMaxAI;

    private void Awake()
    {
        //InitBoard();
    }

    void Start()
    {
        InitBoard();
        StartCoroutine(ShowTween());
    }

    private IEnumerator ShowTween(float seconds = 0f)
    {
        if (seconds != 0) yield return new WaitForSeconds(seconds); 
        TimeLine.GetComponent<PlayableDirector>().Play();
    }

    // 重置棋盘  
    private IEnumerator ClearBoard(float seconds = 0f, Action callback = null)
    {
        restartButton.interactable = false;
        if (seconds != 0f) yield return new WaitForSeconds(seconds);
        
        int i = 0;
        foreach (Transform cell in grid)
        {
            Image img = cell.GetComponent<Image>();
            Color color = img.color;
            color.a = 0; // 透明度重置为0  
            img.color = color;
            img.GetComponentInChildren<TMP_Text>().text = "";
            cell.GetComponent<Button>().interactable = true;

            boardState[i / 3, i % 3] = 0;
            i++;
        }
        isGameOver = false;
        preWinerPlayerId = -1;
        if (coroutine != null) StopCoroutine(coroutine);
        restartButton.interactable = true;
        callback?.Invoke();
    }

    // 初始化棋盘  
    void InitBoard()
    {
        // 缓存数据初始化  
        for (int i = 0; i < grid.childCount; ++i)
        {
            Transform cell = grid.GetChild(i);
            boardIdMap.Add(i, cell);
            boardCellMap.Add(cell, i);
        }

        playerOneScoreMap.Add("vectoryScore", 0);
        playerOneScoreMap.Add("drawScore", 0);
        playerOneScoreMap.Add("defeatScore", 0);
        playerTwoScoreMap.Add("vectoryScore", 0);
        playerTwoScoreMap.Add("drawScore", 0);
        playerTwoScoreMap.Add("defeatScore", 0);

        col = boardState.GetLength(0);
        row = col;

        miniMaxAI = new MiniMaxAI(boardState, playerId_1, playerId_2);

        // UI 信息初始化  
        UpdateInfo(playerId_1, 0);
        UpdateInfo(playerId_2, 0);
        UpdateInfo(null, 0);

        StartCoroutine(ClearBoard());
        BindOnclickEvent();
    }

    // 绑定点击事件  
    void BindOnclickEvent()
    {
        foreach (Transform cell in grid)
        {
            Button button = cell.GetComponent<Button>();
            button.onClick.AddListener(() => CellOnClick(playerId_1, cell));
        }
        restartButton.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ClearBoard()));
    }


    // cell被点击
    void CellOnClick(int playerId, Transform cell = null)
    {
        if (isGameOver) return;

        if (playerId == playerId_1)
        {
            MakeMove(playerId, cell);
            grid.GetComponent<CanvasGroup>().interactable = false;
            if (IsGameOver())
            {
                if (IsWin(playerId))
                {
                    UpdateInfoByWiner(playerId);
                    StartCoroutine(ClearBoard(1.5f, () => StatusInfoTips("人机先手！", () => {
                        StartCoroutine(PlayerTwoOnClick());
                    })));
                }
                else
                {
                    UpdateInfoByWiner(null);
                    // 上一把获胜玩家Id=1的情况下继续人机先手
                    if (preWinerPlayerId == playerId_1) StartCoroutine(ClearBoard(1.5f, () => StatusInfoTips("人机先手！", () => {
                        StartCoroutine(PlayerTwoOnClick());
                    })));
                    else StartCoroutine(ClearBoard(1.5f, () =>
                    {
                        StatusInfoTips("你先手！");
                        grid.GetComponent<CanvasGroup>().interactable = true;
                    }));
                    
                }
                
            }
            else coroutine = StartCoroutine(PlayerTwoOnClick(0.35f)); // 主动触发玩家2（人机）的回合  
        }
        else if (playerId == playerId_2)
        {
            Vector2Int vector2 = miniMaxAI.ChooseAction(ExplorationProbability);
            MakeMove(playerId, boardIdMap[vector2.x * row + vector2.y]);
            if (IsGameOver())
            {
                if (IsWin(playerId)) 
                {
                    UpdateInfoByWiner(playerId);
                    StartCoroutine(ClearBoard(1.5f, () =>
                    {
                        StatusInfoTips("你先手！");
                        grid.GetComponent<CanvasGroup>().interactable = true;
                    }));
                }
                else
                {
                    UpdateInfoByWiner(null);
                    // 上一把获胜玩家Id=1的情况下继续人机先手
                    if (preWinerPlayerId == playerId_1) StartCoroutine(ClearBoard(1.5f, () => StatusInfoTips("人机先手！", () => {
                        StartCoroutine(PlayerTwoOnClick());
                    })));
                    else StartCoroutine(ClearBoard(1.5f, () =>
                    {
                        StatusInfoTips("你先手！");
                        grid.GetComponent<CanvasGroup>().interactable = true;
                    }));
                }
            }
        }
    }

    // 玩家2的回合
    IEnumerator PlayerTwoOnClick(float seconds = 0.4f)
    {
        if (seconds != 0f) yield return new WaitForSeconds(seconds);

        CellOnClick(playerId_2);
        grid.GetComponent<CanvasGroup>().interactable = true;
    }


    // 执行下棋  
    void MakeMove(int playerId, Transform cell)
    {
        //表现处理
        Image img = cell.GetComponent<Image>();
        Color color = img.color;
        color.a = 1;
        img.color = color;
        img.GetComponentInChildren<TMP_Text>().text = playerId == playerId_1 ? "X" : "O";
        cell.localScale = new Vector3(0, 0, 0);
        cell.DOScale(1, 0.15f).SetEase(Ease.OutSine);

        //状态处理
        cell.GetComponent<Button>().interactable = false;
        int cellId = boardCellMap[cell];
        boardState[cellId / row, cellId % col] = playerId;

        //音效处理
        AudioSource audioSource = cell.GetComponent<AudioSource>();
        audioSource.clip = AudioClipArr[playerId == playerId_1 ? 0 : 1];
        audioSource.Play();
    }

    // 状态信息提示
    private void StatusInfoTips(string info, Action callback = null)
    {
        Text statusTextComp = statusText.GetComponent<Text>();
        statusTextComp.text = info;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(statusText.DOFade(1, 0.15f).SetEase(Ease.OutQuad))
            .AppendInterval(1f)
            .Append(statusText.DOFade(0, 0.3f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                if (callback != null) callback();
            });
    }

    // 通过获胜者更新游戏信息  
    private void UpdateInfoByWiner(int? playerId)
    {
        
        if (playerId == playerId_1)
        {
            StatusInfoTips("玩家获胜！");
            UpdateInfo(playerId, 1);
        }
        else if (playerId == playerId_2)
        {
            StatusInfoTips("人机获胜！");
            UpdateInfo(playerId, 1);
        }
        else
        {
            StatusInfoTips("平局！");
            UpdateInfo(playerId, 1);
        }
    }

    // 是否游戏结束  
    bool IsGameOver()
    {
        isGameOver = IsWin(playerId_1) || IsWin(playerId_2) || IsDraw();
        return isGameOver;
    }

    // 是否获胜  
    bool IsWin(int playerId)
    {
        for (int i = 0; i < row; ++i)
        {
            // 行检查  
            if (boardState[i, 0] == playerId && boardState[i, 1] == playerId && boardState[i, 2] == playerId)
            {
                // 闪烁动画
                for (int j = 0; j < col; ++j)
                {
                    Transform cell = boardIdMap[i * row + j];
                    TMP_Text text = cell.GetComponentInChildren<TMP_Text>();
                    text.DOFade(0, 0.3f).SetLoops(3, LoopType.Yoyo).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Color color = text.color;
                        color.a = 1;
                        text.color = color;
                    });
                }
                preWinerPlayerId = playerId;
                return true;
            }
            // 列检查  
            if (boardState[0, i] == playerId && boardState[1, i] == playerId && boardState[2, i] == playerId)
            {
                // 闪烁动画
                for (int j = 0; j < col; ++ j)
                {
                    Transform cell = boardIdMap[j * row + i];
                    TMP_Text text = cell.GetComponentInChildren<TMP_Text>();
                    text.DOFade(0, 0.3f).SetLoops(3, LoopType.Yoyo).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Color color = text.color;
                        color.a = 1;
                        text.color = color;
                    });
                }
                preWinerPlayerId = playerId;
                return true;
            }
        }

        // 对角线检查  
        if (boardState[0, 0] == playerId && boardState[1, 1] == playerId && boardState[2, 2] == playerId)
        {
            // 闪烁动画
            for (int j = 0; j < col; ++j)
            {
                Transform cell = boardIdMap[j * row + j];
                TMP_Text text = cell.GetComponentInChildren<TMP_Text>();
                text.DOFade(0, 0.3f).SetLoops(3, LoopType.Yoyo).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Color color = text.color;
                    color.a = 1;
                    text.color = color;
                });
            }
            preWinerPlayerId = playerId;
            return true;
        }
        if (boardState[0, 2] == playerId && boardState[1, 1] == playerId && boardState[2, 0] == playerId)
        {
            // 闪烁动画
            for (int j = 0; j < col; ++j)
            {
                Transform cell = boardIdMap[j * row + col - 1 - j];
                TMP_Text text = cell.GetComponentInChildren<TMP_Text>();
                text.DOFade(0, 0.3f).SetLoops(3, LoopType.Yoyo).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Color color = text.color;
                    color.a = 1;
                    text.color = color;
                });
            }
            preWinerPlayerId = playerId;
            return true;
        }
        return false;
    }

    // 是否平局  
    bool IsDraw()
    {
        foreach (int cell in boardState)
            if (cell == 0) return false;
        return true;
    }

    // 更新信息  
    void UpdateInfo(int? vectoryPlayerId = null, int score = 0)
    {
        if (vectoryPlayerId == null)
        {
            playerOneScoreMap["drawScore"] += score;
            playerTwoScoreMap["drawScore"] += score;
            Transform text = AllPlayerDrawInfo.Find("__vectorCount");
            text.GetComponent<Text>().text = playerOneScoreMap["drawScore"].ToString();
            text.DOScale(1.3f, 0.2f).SetEase(Ease.OutSine).OnComplete(() => text.localScale = new Vector3(1, 1, 1));
        }
        else
        {
            if (vectoryPlayerId == playerId_1)
            {
                playerOneScoreMap["vectoryScore"] += score;
                Transform text = PlayerOneVectoryInfo.Find("__vectorCount");
                text.GetComponent<Text>().text = playerOneScoreMap["vectoryScore"].ToString();
                text.DOScale(1.3f, 0.2f).SetEase(Ease.OutSine).OnComplete(() => text.localScale = new Vector3(1, 1, 1));
            }
            else if (vectoryPlayerId == playerId_2)
            {
                playerTwoScoreMap["vectoryScore"] += score;
                Transform text = PlayerTwoVectoryInfo.Find("__vectorCount");
                text.GetComponent<Text>().text = playerTwoScoreMap["vectoryScore"].ToString();
                text.DOScale(1.3f, 0.2f).SetEase(Ease.OutSine).OnComplete(() => text.localScale = new Vector3(1, 1, 1));
            }
        }
    }
}