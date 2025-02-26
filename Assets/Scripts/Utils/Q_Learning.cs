using System.Collections.Generic;
using UnityEngine;

public class TicTacToeAI
{
    private int[,] boardState;
    private int playerId;
    private Dictionary<string, float> qTable = new Dictionary<string, float>(); // Q��  
    private const float learningRate = 0.1f; // ѧϰ��  
    private const float discountFactor = 0.9f; // �ۿ�����  
    private readonly int col = 0;

    public TicTacToeAI(int[,] boardState, int playerId)
    {
        this.boardState = boardState;
        this.playerId = playerId;
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
            return ExploitBestMove();
        }
    }

    // ���ѡ��һ�������ӵĵط�  
    Vector2Int ExploreRandomMove()
    {
        List<Vector2Int> availableMoves = GetAvailableMoves();
        int randomIndex = Random.Range(0, availableMoves.Count);
        return availableMoves[randomIndex];
    }

    // ����Q��ѡ�����Ŷ���  
    Vector2Int ExploitBestMove()
    {
        float bestValue = -float.MaxValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        foreach (var move in GetAvailableMoves())
        {
            // �����µ�����״̬��ת��Ϊ��  
            int currentRow = move.x;
            int currentCol = move.y;
            boardState[currentRow, currentCol] = playerId; // AI����  
            string key = StateToString();

            float qValue = qTable.ContainsKey(key) ? qTable[key] : 0f;
            if (qValue > bestValue)
            {
                bestValue = qValue;
                bestMove = new Vector2Int(currentRow, currentCol);
            }
            boardState[currentRow, currentCol] = 0; // ����  
        }
        return bestMove;
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

    // ���� Q ��  
    public void UpdateQTable(string currentState, string nextState, float reward)
    {
        float currentQValue = qTable.ContainsKey(currentState) ? qTable[currentState] : 0;
        float maxFutureQValue = qTable.ContainsKey(nextState) ? qTable[nextState] : 0;

        // Q-learning ���¹�ʽ  
        float newQValue = currentQValue + learningRate * (reward + discountFactor * maxFutureQValue - currentQValue);
        qTable[currentState] = newQValue;
    }

    // ��״̬תΪ�ַ���  
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