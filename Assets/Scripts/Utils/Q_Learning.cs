using System.Collections.Generic;
using UnityEngine;

public class TicTacToeAI
{
    private int[,] boardState;
    private int playerId;
    private Dictionary<string, float> qTable = new Dictionary<string, float>(); // Q表  
    private const float learningRate = 0.1f; // 学习率  
    private const float discountFactor = 0.9f; // 折扣因子  
    private readonly int col = 0;

    public TicTacToeAI(int[,] boardState, int playerId)
    {
        this.boardState = boardState;
        this.playerId = playerId;
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
            return ExploitBestMove();
        }
    }

    // 随机选择一个可落子的地方  
    Vector2Int ExploreRandomMove()
    {
        List<Vector2Int> availableMoves = GetAvailableMoves();
        int randomIndex = Random.Range(0, availableMoves.Count);
        return availableMoves[randomIndex];
    }

    // 利用Q表选择最优动作  
    Vector2Int ExploitBestMove()
    {
        float bestValue = -float.MaxValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        foreach (var move in GetAvailableMoves())
        {
            // 创建新的棋盘状态并转换为键  
            int currentRow = move.x;
            int currentCol = move.y;
            boardState[currentRow, currentCol] = playerId; // AI落子  
            string key = StateToString();

            float qValue = qTable.ContainsKey(key) ? qTable[key] : 0f;
            if (qValue > bestValue)
            {
                bestValue = qValue;
                bestMove = new Vector2Int(currentRow, currentCol);
            }
            boardState[currentRow, currentCol] = 0; // 撤销  
        }
        return bestMove;
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

    // 更新 Q 表  
    public void UpdateQTable(string currentState, string nextState, float reward)
    {
        float currentQValue = qTable.ContainsKey(currentState) ? qTable[currentState] : 0;
        float maxFutureQValue = qTable.ContainsKey(nextState) ? qTable[nextState] : 0;

        // Q-learning 更新公式  
        float newQValue = currentQValue + learningRate * (reward + discountFactor * maxFutureQValue - currentQValue);
        qTable[currentState] = newQValue;
    }

    // 将状态转为字符串  
    public string StateToString()
    {
        string stateKey = "";
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < col; j++)
            {
                stateKey += boardState[i, j].ToString();
            }
        }
        return stateKey;
    }
}