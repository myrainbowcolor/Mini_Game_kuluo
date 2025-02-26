using System.Collections.Generic;
using UnityEngine;

public class MiniMaxAI
{
    private int playerId_1; // ���1 (ͨ���� X)  
    private int playerId_2; // ���2 (ͨ���� O)  
    private readonly int col = 0;
    private int[,] boardState;

    public MiniMaxAI(int[,] boardState, int playerId_1, int playerId_2)
    {
        this.boardState = boardState;
        this.playerId_1 = playerId_1;
        this.playerId_2 = playerId_2;
        col = boardState.GetLength(0);
    }

    // ѡ���������ӣ�  
    public Vector2Int ChooseAction(float explorationProbability)
    {
        if (Random.value < explorationProbability) // ��һ������̽��  
        {
            return ExploreRandomMove();
        }
        else
        {
            return FindBestMove();
        }
    }

    // ���ѡ��һ�������ӵĵط�  
    private Vector2Int ExploreRandomMove()
    {
        List<Vector2Int> availableMoves = GetAvailableMoves();
        int randomIndex = Random.Range(0, availableMoves.Count);
        return availableMoves[randomIndex];
    }

    // ��������ƶ�  
    private Vector2Int FindBestMove()
    {
        int bestScore = -int.MaxValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0) // ����Ƿ�Ϊ��  
                {
                    boardState[i, j] = playerId_2; // AI����  
                    int score = Minimax(false);
                    boardState[i, j] = 0; // ��������  

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

    // Minimax �㷨ʵ��  
    private int Minimax(bool isMaximizing)
    {
        if (IsWin(playerId_2)) return 1; // AI Ӯ  
        if (IsWin(playerId_1)) return -1; // ���Ӯ  
        if (IsDraw()) return 0; // ƽ��  

        if (isMaximizing) // ��ǰ�� AI �Ļغ�  
        {
            int bestScore = -int.MaxValue;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0) // ����Ƿ�Ϊ��  
                    {
                        boardState[i, j] = playerId_2; // AI ����  
                        int score = Minimax(false);
                        boardState[i, j] = 0; // ��������  
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else // ��ǰ����ҵĻغ�  
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0) // ����Ƿ�Ϊ��  
                    {
                        boardState[i, j] = playerId_1; // �������  
                        int score = Minimax(true);
                        boardState[i, j] = 0; // ��������  
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    // ������ʤ��  
    private bool IsWin(int player)
    {
        // �м��  
        for (int i = 0; i < col; i++)
        {
            if (boardState[i, 0] == player && boardState[i, 1] == player && boardState[i, 2] == player)
                return true;
        }
        // �м��  
        for (int j = 0; j < col; j++)
        {
            if (boardState[0, j] == player && boardState[1, j] == player && boardState[2, j] == player)
                return true;
        }
        // �Խ��߼��  
        if ((boardState[0, 0] == player && boardState[1, 1] == player && boardState[2, 2] == player) ||
            (boardState[0, 2] == player && boardState[1, 1] == player && boardState[2, 0] == player))
            return true;

        return false;
    }

    // ���ƽ��  
    private bool IsDraw()
    {
        foreach (int cell in boardState)
        {
            if (cell == 0)
                return false; // ֻҪ�п�λ�Ͳ���ƽ��  
        }
        return true;
    }

    // ��ȡ�����ӵĵط�  
    List<Vector2Int> GetAvailableMoves()
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0) // λ�ÿ���  
                {
                    availableMoves.Add(new Vector2Int(i, j));
                }
            }
        }
        return availableMoves;
    }
}