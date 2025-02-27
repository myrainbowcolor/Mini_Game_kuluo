using System.Collections.Generic;
using UnityEngine;

public class MiniMaxAI
{
    private int playerId_1;
    private int playerId_2;
    private readonly int col;
    private int[,] boardState;

    public MiniMaxAI(int[,] boardState, int playerId_1, int playerId_2)
    {
        this.boardState = boardState;
        this.playerId_1 = playerId_1;
        this.playerId_2 = playerId_2;
        col = boardState.GetLength(0);
    }

    public Vector2Int ChooseAction(float explorationProbability)
    {
        if (Random.value < explorationProbability)
        {
            return ExploreRandomMove();
        }
        else
        {
            return FindBestMove();
        }
    }

    private Vector2Int ExploreRandomMove()
    {
        List<Vector2Int> availableMoves = GetAvailableMoves();
        int randomIndex = Random.Range(0, availableMoves.Count);
        return availableMoves[randomIndex];
    }

    private Vector2Int FindBestMove()
    {
        int bestScore = -int.MaxValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0)
                {
                    boardState[i, j] = playerId_2;
                    int score = Minimax(false, -int.MaxValue, int.MaxValue);
                    boardState[i, j] = 0;

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

    private int Minimax(bool isMaximizing, int alpha, int beta)
    {
        if (IsWin(playerId_2)) return 1;
        if (IsWin(playerId_1)) return -1;
        if (IsDraw()) return 0;

        if (isMaximizing)
        {
            int bestScore = -int.MaxValue;

            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0)
                    {
                        boardState[i, j] = playerId_2;
                        int score = Minimax(false, alpha, beta);
                        boardState[i, j] = 0;

                        bestScore = Mathf.Max(score, bestScore);
                        alpha = Mathf.Max(alpha, score);

                        if (beta <= alpha)
                        {
                            break; // Beta pruning  
                        }
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (boardState[i, j] == 0)
                    {
                        boardState[i, j] = playerId_1;
                        int score = Minimax(true, alpha, beta);
                        boardState[i, j] = 0;

                        bestScore = Mathf.Min(score, bestScore);
                        beta = Mathf.Min(beta, score);

                        if (beta <= alpha)
                        {
                            break; // Alpha pruning  
                        }
                    }
                }
            }
            return bestScore;
        }
    }

    private bool IsWin(int player)
    {
        for (int i = 0; i < col; i++)
        {
            if (boardState[i, 0] == player && boardState[i, 1] == player && boardState[i, 2] == player)
                return true;
        }
        for (int j = 0; j < col; j++)
        {
            if (boardState[0, j] == player && boardState[1, j] == player && boardState[2, j] == player)
                return true;
        }
        if ((boardState[0, 0] == player && boardState[1, 1] == player && boardState[2, 2] == player) ||
            (boardState[0, 2] == player && boardState[1, 1] == player && boardState[2, 0] == player))
            return true;

        return false;
    }

    private bool IsDraw()
    {
        foreach (int cell in boardState)
        {
            if (cell == 0)
                return false;
        }
        return true;
    }

    List<Vector2Int> GetAvailableMoves()
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (boardState[i, j] == 0)
                {
                    availableMoves.Add(new Vector2Int(i, j));
                }
            }
        }
        return availableMoves;
    }
}