using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // Top-Right
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // HighMiddle-Right
        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // LowMiddle-Right
        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // Bottom-Right
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Left
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // HighMiddle-Left
        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // LowMiddle-Left
        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // Bottom-Left
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        return r;
    }
    public override List<Vector2Int> GetAttackMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;


        // Top-Right
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // HighMiddle-Right
        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // LowMiddle-Right
        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // Bottom-Right
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Left
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // HighMiddle-Left
        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // LowMiddle-Left
        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        // Bottom-Left
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        return r;
    }
}
