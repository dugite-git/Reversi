using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static ReversiManager;
using static Square;
using TMPro;

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
    private GameObject blackdisc;
    private GameObject whitedisc;
    [SerializeField] private TextMeshProUGUI Turn_Screen;

    void Start()
    {
        game_state = GameState.BlackTurn;
        Turn_Screen.text = "Black Turn";
        squares = new Square[8, 8];
        blackdisc = Resources.Load<GameObject>("Prefabs/BlackDisc");
        whitedisc = Resources.Load<GameObject>("Prefabs/WhiteDisc");
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
                if(game_state == GameState.BlackWin) return;
                if (game_state == GameState.WhiteWin) return;
                if (game_state == GameState.Draw) return;
                if (squares[-(clicked_cell_position.x - 3), clicked_cell_position.y + 4].square_state != Square.SquareState.Empty) return;
                if (!squares[-(clicked_cell_position.x - 3), clicked_cell_position.y + 4].IsPlaceableSquare(game_state)) return;

                squares[-(clicked_cell_position.x - 3), clicked_cell_position.y + 4].ChangeSquareState(game_state);

                PlaceDisc(-(clicked_cell_position.x - 3), clicked_cell_position.y + 4);

                FlipDiscs(-(clicked_cell_position.x - 3), clicked_cell_position.y + 4);

                ChangeGameState();
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
                Vector3Int local_square_position = new Vector3Int(7 - (i % 8), i / 8, 0);
                squares[7 - (i % 8), i / 8] = new Square(tilemap.GetCellCenterWorld(cell_position), local_square_position, squares);
                squares[7 - (i % 8), i / 8].square_position_center.z = -1;
                i++;
            }
        }
        //初期配置
        squares[3, 3].ChangeSquareStateIntoWhite();
        GameObject newDisc = Instantiate(whitedisc, squares[3, 3].square_position_center, Quaternion.identity);
        squares[3, 3].disc = newDisc;
        squares[4, 4].ChangeSquareStateIntoWhite();
        newDisc = Instantiate(whitedisc, squares[4, 4].square_position_center, Quaternion.identity);
        squares[4, 4].disc = newDisc;
        squares[3, 4].ChangeSquareStateIntoBlack();
        newDisc = Instantiate(blackdisc, squares[3, 4].square_position_center, Quaternion.identity);
        squares[3, 4].disc = newDisc;
        squares[4, 3].ChangeSquareStateIntoBlack();
        newDisc = Instantiate(blackdisc, squares[4, 3].square_position_center, Quaternion.identity);
        squares[4, 3].disc = newDisc;
    }

    private void PlaceDisc(int disc_position_x, int disc_position_y)
    {
        if (game_state == GameState.BlackTurn)
        {
            GameObject newDisc = Instantiate(blackdisc, squares[disc_position_x, disc_position_y].square_position_center, Quaternion.identity);
            squares[disc_position_x, disc_position_y].disc = newDisc;
        }
        else if (game_state == GameState.WhiteTurn)
        {
            GameObject newDisc = Instantiate(whitedisc, squares[disc_position_x, disc_position_y].square_position_center, Quaternion.identity);
            squares[disc_position_x, disc_position_y].disc = newDisc;
        }
    }

    public void FlipDiscs(int x, int y)
    {
        int[,] directions = new int[,]
        {
        {-1, -1}, // 左上
        {-1, 0},  // 上
        {-1, 1},  // 右上
        {0, -1},  // 左
        {0, 1},   // 右
        {1, -1},  // 左下
        {1, 0},   // 下
        {1, 1}    // 右下
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];
            int newX = x + dx;
            int newY = y + dy;

            if (newX >= 0 && newX < squares.GetLength(0) && newY >= 0 && newY < squares.GetLength(1) && squares[newX, newY].square_state == OppositeColor(game_state))
            {
                while (newX >= 0 && newX < squares.GetLength(0) && newY >= 0 && newY < squares.GetLength(1) && squares[newX, newY].square_state == OppositeColor(game_state))
                {
                    newX += dx;
                    newY += dy;
                }

                if (newX >= 0 && newX < squares.GetLength(0) && newY >= 0 && newY < squares.GetLength(1) && squares[newX, newY].square_state == CurrentColor(game_state))
                {
                    newX -= dx;
                    newY -= dy;
                    while (newX != x || newY != y)
                    {
                        squares[newX, newY].square_state = CurrentColor(game_state);
                        Destroy(squares[newX, newY].disc);
                        GameObject discPrefab = CurrentColor(game_state) == Square.SquareState.Black ? blackdisc : whitedisc;
                        GameObject newDisc = Instantiate(discPrefab, squares[newX, newY].square_position_center, Quaternion.identity);
                        squares[newX, newY].disc = newDisc;
                        newX -= dx;
                        newY -= dy;
                    }
                }
            }
        }
    }

    public Square.SquareState CurrentColor(GameState color)
    {
        return color == GameState.BlackTurn ? Square.SquareState.Black : Square.SquareState.White;
    }

    public Square.SquareState OppositeColor(GameState color)
    {
        return color == GameState.BlackTurn ? Square.SquareState.White : Square.SquareState.Black;
    }

    public ReversiManager.GameState OppositeGameState(GameState color)
    {
        return color == GameState.BlackTurn ? GameState.WhiteTurn : GameState.BlackTurn;
    }

    private void ChangeGameState()
    {
        //石の数を数える
        int black_disc_count = 0;
        int white_disc_count = 0;
        for (int i = 0; i < squares.GetLength(1); i++)
        {
            for (int j = 0; j < squares.GetLength(0); j++)
            {
                if (squares[i, j].square_state == SquareState.Black)
                {
                    black_disc_count++;
                }
                else if (squares[i, j].square_state == SquareState.White)
                {
                    white_disc_count++;
                }
            }
        }

        //片方の色の石がなくなったらもう片方の勝利
        if (white_disc_count == 0)
        {
            game_state = GameState.BlackWin;
            Turn_Screen.text = "Black Win";
            return;
        }
        else if (black_disc_count == 0)
        {
            game_state = GameState.WhiteWin;
            Turn_Screen.text = "White Win";
            return;
        }

        //盤面が埋まったら勝敗判定
        if (black_disc_count + white_disc_count == squares.Length)
        {
            if (black_disc_count > white_disc_count)
            {
                game_state = GameState.BlackWin;
                Turn_Screen.text = "Black Win";
                return;
            }
            else if (black_disc_count < white_disc_count)
            {
                game_state = GameState.WhiteWin;
                Turn_Screen.text = "White Win";
                return;
            }
            else
            {
                game_state = GameState.Draw;
                Turn_Screen.text = "Draw";
                return;
            }
        }

        //石を置けるかどうかの判定
        bool canCurrentPlayerPlace = false;
        for (int i = 0; i < squares.GetLength(1); i++)
        {
            for (int j = 0; j < squares.GetLength(0); j++)
            {
                if (squares[i, j].IsPlaceableSquare(game_state))
                {
                    canCurrentPlayerPlace = true;
                    break;
                }
            }
            if (canCurrentPlayerPlace) break;
        }

        //次のプレイヤーが石を置けるかどうかの判定
        bool canNextPlayerPlace = false;
        for (int i = 0; i < squares.GetLength(1); i++)
        {
            for (int j = 0; j < squares.GetLength(0); j++)
            {
                if (squares[i, j].IsPlaceableSquare(OppositeGameState(game_state)))
                {
                    canNextPlayerPlace = true;
                    break;
                }
            }
            if (canNextPlayerPlace) break;
        }

        //両者石を置けない場合ゲーム終了
        if (!canCurrentPlayerPlace && !canNextPlayerPlace)
        {
            if (black_disc_count > white_disc_count)
            {
                game_state = GameState.BlackWin;
                Turn_Screen.text = "Black Win";
                return;
            }
            else if (black_disc_count < white_disc_count)
            {
                game_state = GameState.WhiteWin;
                Turn_Screen.text = "White Win";
                return;
            }
            else
            {
                game_state = GameState.Draw;
                Turn_Screen.text = "Draw";
                return;
            }
        }

        //現在のプレイヤーが石を置けるが次のプレイヤーが置けない場合、次のプレイヤーのターンをスキップ
        if (canCurrentPlayerPlace && !canNextPlayerPlace)
        {
            return;
        }

        //それ以外の場合、次のプレイヤーのターン
        game_state = OppositeGameState(game_state);
        if (game_state == GameState.BlackTurn)
        {
            Turn_Screen.text = "Black Turn";
        }
        else if (game_state == GameState.WhiteTurn)
        {
            Turn_Screen.text = "White Turn";
        }
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
    public Vector3Int local_square_position;
    public SquareState square_state;
    private Square[,] board;
    public GameObject disc;

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

    public bool IsPlaceableSquare(ReversiManager.GameState game_state)
    { 
        if(square_state != SquareState.Empty) return false;
        int[,] directions = new int[,]
        {
                {-1, -1}, // 左上
                {-1, 0},  // 上
                {-1, 1},  // 右上
                {0, -1},  // 左
                {0, 1},   // 右
                {1, -1},  // 左下
                {1, 0},   // 下
                {1, 1}    // 右下
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];
            int newX = local_square_position.x + dx;
            int newY = local_square_position.y + dy;

            if (newX >= 0 && newX < board.GetLength(0) && newY >= 0 && newY < board.GetLength(1) && board[newX, newY].square_state == OppositeColor(game_state))
            {
                while (newX >= 0 && newX < board.GetLength(0) && newY >= 0 && newY < board.GetLength(1) && board[newX, newY].square_state == OppositeColor(game_state))
                {
                    newX += dx;
                    newY += dy;
                }

                if (newX >= 0 && newX < board.GetLength(0) && newY >= 0 && newY < board.GetLength(1) && board[newX, newY].square_state == CurrentColor(game_state))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Square.SquareState CurrentColor(GameState color)
    {
        return color == GameState.BlackTurn ? Square.SquareState.Black : Square.SquareState.White;
    }

    public Square.SquareState OppositeColor(GameState color)
    {
        return color == GameState.BlackTurn ? Square.SquareState.White : Square.SquareState.Black;
    }

    public Square(Vector3 input_square_position_center, Vector3Int input_local_square_position, Square[,] board)
    {
        square_position_center = input_square_position_center;
        local_square_position = input_local_square_position;
        square_state = SquareState.Empty;
        this.board = board;
    }
}