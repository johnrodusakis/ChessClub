using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        //Up
        int x = currentX;
        int y = currentY + 1;
        if (y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Right
        x = currentX + 1;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Right
        x = currentX + 1;
        y = currentY;
        if (x < tileCountX)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Bottom-Right
        x = currentX + 1;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Down
        x = currentX;
        y = currentY - 1;
        if (y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Bottom-Left
        x = currentX - 1;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Left
        x = currentX - 1;
        y = currentY;
        if (x >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Left
        x = currentX - 1;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        return r;
    }
    public override List<Vector2Int> GetAttackMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        //Up
        int x = currentX;
        int y = currentY + 1;
        if (y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Right
        x = currentX + 1;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Right
        x = currentX + 1;
        y = currentY;
        if (x < tileCountX)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Bottom-Right
        x = currentX + 1;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Down
        x = currentX;
        y = currentY - 1;
        if (y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Bottom-Left
        x = currentX - 1;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Left
        x = currentX - 1;
        y = currentY;
        if (x >= 0)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        // Top-Left
        x = currentX - 1;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] != null && board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        return r;
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves, out List<Vector2Int> specialMoves)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        SpecialMove sm = SpecialMove.None;

        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRookMove = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRookMove = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        if (kingMove == null && currentX == 4)
        {
            // White team
            if (team == 0)
            {
                // Left Rook
                if (leftRookMove == null)
                    if (board[0, 0].type == ChessPieceType.Rook)
                        if (board[0, 0].team == 0)
                            if (board[3, 0] == null)
                                if (board[2, 0] == null)
                                    if (board[1, 0] == null)
                                    {
                                        r.Add(new Vector2Int(2, 0));
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        sm = SpecialMove.Castling;
                                    }

                // Right Rook
                if (rightRookMove == null)
                    if (board[7, 0].type == ChessPieceType.Rook)
                        if (board[7, 0].team == 0)
                            if (board[5, 0] == null)
                                if (board[6, 0] == null)
                                {
                                    r.Add(new Vector2Int(6, 0));
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    sm = SpecialMove.Castling;
                                }
            }
            // Black team
            else
            {

                // Left Rook
                if (leftRookMove == null)
                    if (board[0, 7].type == ChessPieceType.Rook)
                        if (board[0, 7].team == 1)
                            if (board[3, 7] == null)
                                if (board[2, 7] == null)
                                    if (board[1, 7] == null)
                                    {
                                        r.Add(new Vector2Int(2, 7));
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        sm = SpecialMove.Castling;
                                    }

                // Right Rook
                if (rightRookMove == null)
                    if (board[7, 7].type == ChessPieceType.Rook)
                        if (board[7, 7].team == 1)
                            if (board[5, 7] == null)
                                if (board[6, 7] == null)
                                {
                                    r.Add(new Vector2Int(6, 7));
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    sm = SpecialMove.Castling;
                                }
            }
        }
        specialMoves = r;
        return sm;
    }
}
