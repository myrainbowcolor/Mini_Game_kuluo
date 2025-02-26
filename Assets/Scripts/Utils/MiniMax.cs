using System.Collections.Generic;
using UnityEngine;

public class MiniMaxAI
{
    private int playerId_1; // 玩家1 (通常是 X)  
    private int playerId_2; // 玩家2 (通常是 O)  
    private readonly int col = 0;
    private int[,] boardState;

    public MiniMaxAI(int[,] boardState, int playerId_1, int playerId_2)
    {
        this.boardState = boardState;
        this.playerId_1 = playerId_1;
        this.playerId_2 = playerId_2;
        col = boardState.GetLength(0);
    }

    // 选择动作（落子）  
    public Vector2Int ChooseAction(float explorationProbability)
    {
        if (Random.value < explorationProbability) // 以一定概率探索  
        {
            return ExploreRandomMove();
        }
        else
        {
            return FindBestMove();
        }
    }

    // 随机选择一个可落子的地方  
    private Vector2Int ExploreRandomMove()
    {
        List<Vector2Int> availableMoves = GetAvailableMoves();
        int randomIndex = Random.Range(0, availableMoves.Count);
        return availableMoves[randomIndex];
    }

    // 查找最佳移动  
    private Vector2Int FindBestMove()
    {
        int bestScore = -int.MaxValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0) // 检查是否为空  
                {
                    boardState[i, j] = playerId_2; // AI落子  
                    int score = Minimax(false);
                    boardState[i, j] = 0; // 撤销落子  

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(i, j);
                    }
                }
            }
        }
        return bestMove;
    }

    // Minimax 算法实现  
    private int Minimax(bool isMaximizing)
    {
        if (IsWin(playerId_2)) return 1; // AI 赢  
        if (IsWin(playerId_1)) return -1; // 玩家赢  
        if (IsDraw()) return 0; // 平局  

        if (isMaximizing) // 当前是 AI 的回合  
        {
            int bestScore = -int.MaxValue;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0) // 检查是否为空  
                    {
                        boardState[i, j] = playerId_2; // AI 落子  
                        int score = Minimax(false);
                        boardState[i, j] = 0; // 撤销落子  
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else // 当前是玩家的回合  
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0) // 检查是否为空  
                    {
                        boardState[i, j] = playerId_1; // 玩家落子  
                        int score = Minimax(true);
                        boardState[i, j] = 0; // 撤销落子  
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    // 检查玩家胜利  
    private bool IsWin(int player)
    {
        // 行检查  
        for (int i = 0; i < col; i++)
        {
            if (boardState[i, 0] == player && boardState[i, 1] == player && boardState[i, 2] == player)
                return true;
        }
        // 列检查  
        for (int j = 0; j < col; j++)
        {
            if (boardState[0, j] == player && boardState[1, j] == player && boardState[2, j] == player)
                return true;
        }
        // 对角线检查  
        if ((boardState[0, 0] == player && boardState[1, 1] == player && boardState[2, 2] == player) ||
            (boardState[0, 2] == player && boardState[1, 1] == player && boardState[2, 0] == player))
            return true;

        return false;
    }

    // 检查平局  
    private bool IsDraw()
    {
        foreach (int cell in boardState)
        {
            if (cell == 0)
                return false; // 只要有空位就不是平局  
        }
        return true;
    }

    // 获取可落子的地方  
    List<Vector2Int> GetAvailableMoves()
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0) // 位置可用  
                {
                    availableMoves.Add(new Vector2Int(i, j));
                }
            }
        }
        return availableMoves;
    }
}