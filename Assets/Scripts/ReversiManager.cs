using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ReversiManager : MonoBehaviour
{
    public enum GameState
    {
        BlackTurn,
        WhiteTurn,
        BlackWin,
        WhiteWin,
        Draw
    }

    [System.NonSerialized] public GameState game_state;
    public Square[,] squares;
    [SerializeField] private Tilemap tilemap;

    void Start()
    {
        game_state = GameState.BlackTurn;
        squares = new Square[8, 8];
        InitializeSquares();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_position = Input.mousePosition;
            Vector3 world_mouse_position = Camera.main.ScreenToWorldPoint(mouse_position);
            Vector3Int clicked_cell_position = tilemap.WorldToCell(world_mouse_position);
            if (tilemap.HasTile(clicked_cell_position))
            {
                squares[-(clicked_cell_position.x - 3), clicked_cell_position.y + 4].ChangeSquareState(game_state);
                Debug.Log(squares[-(clicked_cell_position.x - 3), clicked_cell_position.y + 4].square_state);
            }
        }
    }

    //squaresはオセロ盤において左上が[0,0]で右下が[7,7]の配列
    private void InitializeSquares()
    {
        int i = 0;
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int cell_position = new Vector3Int(position.x, position.y, position.z);
            if (tilemap.HasTile(cell_position))
            {
                squares[7 - (i % 8), i / 8] = new Square(tilemap.GetCellCenterWorld(cell_position));
                i++;
            }
        }
        squares[3, 3].ChangeSquareStateIntoWhite();
        squares[4, 4].ChangeSquareStateIntoWhite();
        squares[3, 4].ChangeSquareStateIntoBlack();
        squares[4, 3].ChangeSquareStateIntoBlack();
    }
}

public class Square
{
    public enum SquareState
    {
        Empty,
        Black,
        White
    }

    public Vector3 square_position_center;
    public SquareState square_state;

    public void ChangeSquareState(ReversiManager.GameState game_state)
    {
        if(game_state == ReversiManager.GameState.BlackTurn)
        {
            square_state = SquareState.Black;
        }
        else if (game_state == ReversiManager.GameState.WhiteTurn)
        {
            square_state = SquareState.White;
        }
    }
    public void ChangeSquareStateIntoEmpty()
    {
        square_state = SquareState.Empty;
    }
    public void ChangeSquareStateIntoBlack()
    {
        square_state = SquareState.Black;
    }
    public void ChangeSquareStateIntoWhite()
    {
        square_state = SquareState.White;
    }
    public Square(Vector3 vector3)
    {
        square_position_center = vector3;
        square_state = SquareState.Empty;
    }
}