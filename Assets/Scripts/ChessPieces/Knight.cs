using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
       
        List<Vector2Int> r = new List<Vector2Int>();
        // Top right
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        //Top Left
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0  && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        //Bottom right
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0 )
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        //Bottom Left
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        /* int x, y;
         int[,] offset = { { 2, -2, -1, 1 }, { 1, -1, 2, -2 } };
         for (int i = 0; i < 2; i++)
         {
             x = currentX + offset[0,i];
             y = currentY + offset[1,i];
             Debug.Log(offset[0,i]);
             Debug.Log(offset[1, i]);
             if (x < tileCountX && y < tileCountY)
                 if (board[x, y] == null || board[x, y].team != team)
                     r.Add(new Vector2Int(x, y));
         }*/
        return r;
    }
}