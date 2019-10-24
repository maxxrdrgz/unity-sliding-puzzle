using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private PuzzlePiece[,] matrix = new PuzzlePiece[GameVariables.MaxRows, GameVariables.MaxCols];

    private void Awake() {
        MakeSingleton();
    }

    private void Start() {
        puzzleIndex = -1;
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

    void LoadPuzzle(){
        puzzleImages = Resources.LoadAll<Sprite>("Sprites/BG "+puzzleIndex);
        puzzlePieces = GameObject.Find("Puzzle Holder").GetComponent<PuzzleHolder>().puzzlePieces;

        for(int i = 0; i < puzzlePieces.Length; i++){
            puzzlePieces[i].GetComponent<SpriteRenderer>().sprite = puzzleImages[i];
        }

    }

    void OnLevelWasLoaded(){
        if(SceneManager.GetActiveScene().name == "Gameplay"){
            if(puzzleIndex > 0){
                LoadPuzzle();
                GameStarted();
            }
        }
    }

    void GameStarted(){
        int index = Random.Range(0, GameVariables.MaxSize);
        puzzlePieces[index].SetActive(false);

        for(int row = 0; row < GameVariables.MaxRows; row++){
            for(int col = 0; col < GameVariables.MaxCols; col++){
                if(puzzlePieces[row * GameVariables.MaxCols + col].activeInHierarchy){
                    Vector3 point = GetScreenCoordinatesFromViewPort(row, col);
                    puzzlePieces[row * GameVariables.MaxCols + col].transform.position = point;
                    matrix[row, col] = new PuzzlePiece();
                    matrix[row, col].GameObject = puzzlePieces[row * GameVariables.MaxCols + col];
                    matrix[row, col].OriginalRow = row;
                    matrix[row, col].OriginalCol = col;
                }else{
                    matrix[row, col] = null;
                }
            }
        }
        Shuffle();
        gameState = GameState.Playing;
    }

    private Vector3 GetScreenCoordinatesFromViewPort(int row, int col){
        Vector3 point = Camera.main.ViewportToWorldPoint(new Vector3(0.225f * row, 1 - 0.228f * col, 0));
        point.z = 0;
        return point;
    }

    private void Shuffle(){
        for(int row = 0; row < GameVariables.MaxRows; row++){
            for(int col = 0; col < GameVariables.MaxCols; col++){
                if(matrix[row, col] == null){
                    continue;
                }
                int random_row = Random.Range(0, GameVariables.MaxRows);
                int random_col = Random.Range(0, GameVariables.MaxCols);
                Swap(row, col, random_row, random_col);
            }
        }
    }

    private void Swap(int row, int col, int rand_row, int rand_col){
        PuzzlePiece temp = matrix[row, col];
        matrix[row, col] = matrix[rand_row, rand_col];
        matrix[rand_row, rand_col] = temp;

        if(matrix[row, col] != null){
            matrix[row, col].GameObject.transform.position = GetScreenCoordinatesFromViewPort(row, col);
            matrix[row, col].CurrentRow = row;
            matrix[row, col].CurrentCol = col;
        }
        matrix[rand_row, rand_col].GameObject.transform.position = GetScreenCoordinatesFromViewPort(rand_row, rand_col);
        matrix[rand_row, rand_col].CurrentRow = rand_row;
        matrix[rand_row, rand_col].CurrentCol = rand_col;
    }

    private void CheckInput(){
        if(Input.GetMouseButtonDown(0)){
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(r.origin, r.direction);
            if(hit.collider != null){
                string[] parts = hit.collider.gameObject.name.Split('-');
                int rowPart = int.Parse(parts[1]);
                int colPart = int.Parse(parts[2]);
                int rowFound = -1;
                int colFound = -1;

                for(int row = 0; row < GameVariables.MaxRows; row++){
                    if(rowFound != -1){
                        break;
                    }
                    for(int col = 0; col < GameVariables.MaxCols; col++){
                        if(rowFound!= -1){
                            break;
                        }
                        if(matrix[row, col] == null){
                            continue;
                        }
                        if(matrix[row, col].OriginalRow == rowPart && matrix[row, col].OriginalCol == colPart){
                            rowFound = row;
                            colFound = col;
                        }
                    }
                }
                bool pieceFound = false;
                if(rowFound > 0 && matrix[rowFound -1, colFound] == null){
                    pieceFound = true;
                    toAnimateRow = rowFound -1;
                    toAnimateCol = colFound;
                }else if(colFound > 0 && matrix[rowFound, colFound -1] == null){
                    pieceFound = true;
                    toAnimateRow = rowFound;
                    toAnimateCol = colFound -1;
                }else if(rowFound < GameVariables.MaxRows -1 && matrix[rowFound+1, colFound] == null){
                    pieceFound = true;
                    toAnimateRow = rowFound +1;
                    toAnimateCol = colFound;
                }else if(colFound == GameVariables.MaxCols -1 && matrix[rowFound, colFound +1] == null){
                    pieceFound = true;
                    toAnimateRow = rowFound;
                    toAnimateCol = colFound + 1;
                }

                if(pieceFound){
                    screenPosToAnimate = GetScreenCoordinatesFromViewPort(toAnimateRow, toAnimateCol);
                    PieceToAnimate = matrix[rowFound, colFound];
                    gameState = GameState.Animating;
                }
            }
        }
    }
}
