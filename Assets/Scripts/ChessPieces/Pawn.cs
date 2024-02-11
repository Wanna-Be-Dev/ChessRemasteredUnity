using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
  public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board,int tileCountX, int tileCountY)
  {
        List<Vector2Int> r = new List<Vector2Int>();
        //white or black
        int direction = (team == 0) ? 1 : -1;

        //One in front
        if (board[currentX,currentY + direction] == null)
            r.Add(new Vector2Int(currentX,currentY + direction));

       //Two in front
        if (board[currentX, currentY + direction] == null)
        {
            //for white team
            if(team == 0 && currentY == 1 && board[currentX,currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));   
        }

        //kill move
        if (currentX != tileCountX - 1)
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY + direction));

        return r;
  }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        //EnPassant
        if(movelist.Count > 0)
        {
            Vector2Int[] lastMove = movelist[movelist.Count - 1];
            if (board[lastMove[1].x , lastMove[1].y].type == ChessPieceType.Pawn) // if the last piece was a pawn
            {

                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // last move was +2 in either direction
                {
                    if (board[lastMove[1].x , lastMove[1].y].team != team)// if the pawn is from other team
                    {
                        if (lastMove[1].y == currentY)// if both pawns same y
                        {
                            if (lastMove[1].x == currentX - 1)// landed left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1)// landed right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }

                    }

                }
            }
        }    
        return SpecialMove.None;
    }
}
