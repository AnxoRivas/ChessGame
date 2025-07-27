using UnityEngine;
using ChessDotNet;
using ChessDotNet.Pieces;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ChessGameManager : MonoBehaviour
{
    private ChessGame game;
    public Sprite[] pieceSprites;
    public GameObject piecePrefab;
    public Transform boardRoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        game = new ChessGame("nbrqkbnr/pppppppp/8/8/8/8/PPPPPPPP/NBRQKBNR w KQkq - 0 1");
        game.MakeMove(new Move("e2", "e4", Player.White), true); // Example move
        Debug.Log(game.GetFen());
        InitBoard();

    }

    void InitBoard()
    {
        foreach (File file in System.Enum.GetValues(typeof(File)))
        {
            if ((int)file < 0) continue; // Salteamos el File.None

            for (int rank = 1; rank <= 8; rank++)
            {
                Position pos = new Position(file, rank);
                Piece piece = game.GetPieceAt(pos);



                

                    boardRoot.Find($"{file}{rank}").GetComponent<Image>().sprite = piece != null ? pieceSprites[GetIndexForPiece(piece)] : null;
                


                Debug.Log($"Posición: {pos.ToString()}, Pieza: {piece?.ToString() ?? "vacía"}, Propietario: {piece?.Owner.ToString() ?? "Ninguno"}");
            }

        }
    }

    void UpdatePieceVisual(Piece piece)
    {
        //string key = square.name;

    }

    int GetIndexForPiece(Piece piece)
    {
        if (piece == null) return -1;
        int baseIndex = piece.Owner == Player.White ? 0 : 6; // Asumiendo que los sprites están organizados por color
        if (piece is King) return baseIndex + 0;
        if (piece is Pawn) return baseIndex + 1;
        if (piece is Knight) return baseIndex + 2;
        if (piece is Bishop) return baseIndex + 3;
        if (piece is Rook) return baseIndex + 4;
        if (piece is Queen) return baseIndex + 5;

        return -1; // Retorna -1 si no se encuentra la pieza
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SaveGameState(ChessGame game)
    {
        game.GetFen();
        // Implementación para guardar el estado del juego
    }
}
