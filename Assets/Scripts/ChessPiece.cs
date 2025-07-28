using System;
using ChessDotNet;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChessPiece : MonoBehaviour, IPointerDownHandler
{
    private String positionName;
    [SerializeField] private ChessGameManager chessManager;


    public void OnPointerDown(PointerEventData eventData)
    {
        chessManager.OnSquareSelected(gameObject.name);
    }
}
