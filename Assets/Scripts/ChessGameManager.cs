using UnityEngine;
using ChessDotNet;
using ChessDotNet.Pieces;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ChessGameManager : MonoBehaviour
{
    private ChessGame game; // Instancia del juego de ajedrez
    public Sprite[] pieceSprites; // Array de sprites para las piezas del ajedrez
    public Transform boardRoot; // Transform del tablero en la escena
    private string selectedSquare = null; // Variable para almacenar la casilla seleccionada
    private List<Image> highlightedSquares = new List<Image>(); // Lista para almacenar las casillas resaltadas

    void Start()
    {
        game = new ChessGame();
        InitBoard();
    }

    // Método para inicializar el tablero con las piezas
    // Recorre cada casilla del tablero y asigna el sprite correspondiente a la pieza
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

    // Método para actualizar la visualización de las piezas después de un movimiento
    // Actualiza la imagen de la pieza en la casilla de origen y destino
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

        DetailedMove lastMove = game.Moves[^1];

        // --- Manejo especial para captura al paso ---
        if (lastMove.EnPassant)
        {
            // Comprobamos el color de la pieza
            int capturedRank = piece.Owner == Player.White ? rankTo - 1 : rankTo + 1;
            string capturedSquareName = $"{to[0]}{capturedRank}";
            var capturedSquare = boardRoot.Find(capturedSquareName.ToUpper());
            if (capturedSquare != null)
            {
                var capturedImage = capturedSquare.GetComponent<Image>();
                capturedImage.sprite = null;
                capturedImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        if (lastMove.Castling != CastlingType.None)
        {
            Dictionary<(Player, CastlingType), ((string rookFrom, string rookTo), (string kingFrom, string kingTo))> castlingMap = new()
            {
                { (Player.White, CastlingType.KingSide), (("H1", "F1"), ("E1", "G1")) },
                { (Player.White, CastlingType.QueenSide), (("A1", "D1"), ("E1", "C1")) },
                { (Player.Black, CastlingType.KingSide), (("H8", "F8"), ("E8", "G8")) },
                { (Player.Black, CastlingType.QueenSide), (("A8", "D8"), ("E8", "C8")) }
        };


            var (rookMove, kingMove) = castlingMap[(lastMove.Player, lastMove.Castling)];

            // Actualizar visual del origen (borrar torre)
            fromSquare = boardRoot.Find(rookMove.rookFrom);
            if (fromSquare != null)
            {
                var fromImg = fromSquare.GetComponent<Image>();
                fromImg.sprite = null;
                fromImg.color = new Color(1f, 1f, 1f, 0f);
            }

            // Actualizar visual del destino (poner torre)
            toSquare = boardRoot.Find(rookMove.rookTo);
            if (toSquare != null)
            {
                var pos = new Position((File)Enum.Parse(typeof(File), rookMove.rookTo[0].ToString().ToUpper()), int.Parse(rookMove.rookTo[1].ToString()));
                var rook = game.GetPieceAt(pos);
                var toImg = toSquare.GetComponent<Image>();
                toImg.sprite = pieceSprites[GetIndexForPiece(rook)];
                toImg.color = new Color(1f, 1f, 1f, 1f);
            }

            // No se actualiza el rey en el origen porque ya se hizo en el movimiento del rey
            // Actualizar visual del destino (poner rey)
            var kingToSquare = boardRoot.Find(kingMove.kingTo);
            if (kingToSquare != null)
            {
                var posKingTo = new Position((File)Enum.Parse(typeof(File), kingMove.kingTo[0].ToString().ToUpper()), int.Parse(kingMove.kingTo[1].ToString()));
                var king = game.GetPieceAt(posKingTo);
                var kingToImg = kingToSquare.GetComponent<Image>();
                kingToImg.sprite = pieceSprites[GetIndexForPiece(king)];
                kingToImg.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    // Método para obtener el índice del sprite de una pieza según su tipo y color
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

    // Método para manejar la selección de una casilla
    // Si no hay casilla seleccionada, la selecciona y resalta los movimientos válidos
    public void OnSquareSelected(string positionName)
    {
        if (selectedSquare == null)
        {
            selectedSquare = positionName;

            File fileTo = (File)Enum.Parse(typeof(File), positionName[0].ToString().ToUpper());
            int rankTo = int.Parse(positionName.Substring(1));
            Position posTo = new Position(fileTo, rankTo);

            // Resaltar movimientos válidos
            HighlightValidMoves(posTo);
        }
        else
        {
            ClearHighlights(); // Limpiar resaltados anteriores
            MovePiece(selectedSquare, positionName);
            selectedSquare = null; // Reiniciar selección
        }
    }

    // Método para resaltar los movimientos válidos desde una posición dada
    // Recorre los movimientos válidos y resalta las casillas correspondientes
    void HighlightValidMoves(Position fromPosition)
    {
        foreach (var move in game.GetValidMoves(fromPosition))
        {
            string destination = move.ToString().ToUpper().Split('-')[1];
            Debug.Log($"Movimiento válido: {destination}");

            var square = boardRoot.Find(destination);
            if (square != null)
            {
                Image img = square.GetComponent<Image>();
                img.color = new Color(0f, 1f, 0f, 0.5f); // Verde semitransparente
                highlightedSquares.Add(img);
            }
        }
    }


    // Método para limpiar los resaltados de las casillas
    // Recorre la lista de casillas resaltadas y las restaura a su color original
    void ClearHighlights()
    {
        foreach (var img in highlightedSquares)
        {
            img.color = new Color(1f, 1f, 1f, 0f);
        }
        highlightedSquares.Clear();
    }

    // Método para finalizar el juego
    // Comprueba si hay jaque mate o empate y muestra un mensaje en la consola
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

    // Método para guardar el estado del juego
    void SaveGameState(ChessGame game)
    {
        game.GetFen();
        // Implementación para guardar el estado del juego
    }
}
