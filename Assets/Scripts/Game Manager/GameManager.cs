using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private GameObject[] puzzlePieces;
    private Sprite[] puzzleImages;
    private Vector3 screenPosToAnimate;
    private PuzzlePiece PieceToAnimate;
    private int toAnimateRow, toAnimateCol;
    private float animSpeed = 10f;
    private int puzzleIndex;
    private GameState gameState;

    private void Awake() {
        MakeSingleton();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void MakeSingleton(){
        if(instance !=null){
            Destroy(gameObject);
        }else{
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetPuzzleIndex(int puzzleIndex){
        this.puzzleIndex = puzzleIndex;
    }
}
