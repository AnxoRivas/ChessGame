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
    public Transform boardRoot;
    private string selectedSquare = null;

    void Start()
    {
        game = new ChessGame();
        //Debug.Log(game.GetValidMoves(Player.Black) + " valid moves for Black");
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
                boardRoot.Find($"{file}{rank}").GetComponent<Image>().color = piece != null ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0f);

            }

        }
    }

    // Método para mover una pieza de una casilla a otra
    public bool MovePiece(string from, string to)
    {
        Move move = new Move(from, to, game.WhoseTurn);

        if (game.IsValidMove(move))
        {
            game.MakeMove(move, true);
            UpdatePieceVisual(from, to);
            finishGame();
            return true; // Movimiento válido
        }
        else
        {
            Debug.LogWarning($"Movimiento inválido: {from} → {to}");
            return false; // Movimiento inválido
        }
    }

    void UpdatePieceVisual(String from, String to)
    {
        // Obtener el nuevo estado de la pieza en la posición destino
        File fileTo = (File)Enum.Parse(typeof(File), to[0].ToString().ToUpper());
        int rankTo = int.Parse(to.Substring(1));
        Position posTo = new Position(fileTo, rankTo);
        Piece piece = game.GetPieceAt(posTo);

        // Actualizar la casilla destino (mostrar nueva pieza)
        var toSquare = boardRoot.Find(to.ToUpper());
        if (toSquare != null)
        {
            var toImage = toSquare.GetComponent<Image>();
            toImage.sprite = piece != null ? pieceSprites[GetIndexForPiece(piece)] : null;
            toImage.color = piece != null ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0f);
        }

        // Limpiar la casilla origen
        var fromSquare = boardRoot.Find(from.ToUpper());
        if (fromSquare != null)
        {
            var fromImage = fromSquare.GetComponent<Image>();
            fromImage.sprite = null;
            fromImage.color = new Color(1f, 1f, 1f, 0f);
        }

    }

    int GetIndexForPiece(Piece piece)
    {
        int baseIndex = piece.Owner == Player.White ? 0 : 6; // Asumiendo que los sprites están organizados por color
        if (piece is King) return baseIndex + 0;
        if (piece is Pawn) return baseIndex + 1;
        if (piece is Knight) return baseIndex + 2;
        if (piece is Bishop) return baseIndex + 3;
        if (piece is Rook) return baseIndex + 4;
        if (piece is Queen) return baseIndex + 5;

        return 12; // Retorna -12 si no se encuentra la pieza
    }

    public void OnSquareSelected(string positionName)
    {
        if (selectedSquare == null)
        {
            selectedSquare = positionName;
            Debug.Log($"Casilla seleccionada: {selectedSquare}");
        }
        else
        {
            MovePiece(selectedSquare, positionName);
            selectedSquare = null; // Reiniciar selección
        }
    }

    void finishGame()
    {
        if (game.IsCheckmated(game.WhoseTurn))
        {
            Debug.Log($"{game.WhoseTurn} ha sido jaque mateado.");
        }
        else if (game.IsDraw())
        {
            Debug.Log("El juego ha terminado en empate.");
        }
    }

    void SaveGameState(ChessGame game)
    {
        game.GetFen();
        // Implementación para guardar el estado del juego
    }
}
