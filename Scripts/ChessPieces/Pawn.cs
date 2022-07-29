using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        //One in front
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));

        //Two in front
        if (board[currentX, currentY + direction] == null)
        {
            // White Team
            if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            // Black Team
            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
        }


        // Kill Move
        int x = currentX + 1;
        int y = currentY + (1 * direction);
        if (x < tileCountX && y < tileCountY)
        {
            if (board[x, y] != null && board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 1;
        y = currentY + (1 * direction);
        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] != null && board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        return r;
    }
    public override List<Vector2Int> GetAttackMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;


        // Kill Move
        int x = currentX + 1;
        int y = currentY + (1 * direction);
        if (x < tileCountX && y < tileCountY)
        {
            if (board[x, y] != null && board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 1;
        y = currentY + (1 * direction);
        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] != null && board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        return r;
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves, out List<Vector2Int> specialMoves)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        if((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
            specialMoves = r;
            return SpecialMove.Promotion;
        }

        // En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) // If the last piece was a pawn
            {
                if(Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // If the last move was a +2 in either direction
                {
                    if(board[lastMove[1].x, lastMove[1].y].team != team) // If the move was from the other team
                    {
                        if(lastMove[1].y == currentY) // If both pawns are on the same Y
                        {
                            if (lastMove[1].x == currentX - 1) // Landed left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                r.Add(new Vector2Int(currentX - 1, currentY + direction));
                                specialMoves = r;
                                return SpecialMove.EnPassant;
                            }
                            if(lastMove[1].x == currentX + 1) // Landed right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                r.Add(new Vector2Int(currentX + 1, currentY + direction));
                                specialMoves = r;
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }
        specialMoves = r;
        return SpecialMove.None;
    }
}
