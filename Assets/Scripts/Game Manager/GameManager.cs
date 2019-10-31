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
        if(SceneManager.GetActiveScene().name == "Gameplay"){
            switch (gameState)
            {
                case GameState.Playing:
                    CheckInput();
                    break;
                case GameState.Animating:
                    AnimateMovement(PieceToAnimate);
                    CheckIfAnimationEnded();
                    break;
                case GameState.End:
                    print("game over");
                    return;
                    break;
            }
        }
    }

    /** 
        Creates a singleton that persists after loading a new scene
    */
    void MakeSingleton(){
        if(instance !=null){
            Destroy(gameObject);
        }else{
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /** 
        Sets the local puzzle index variable
    */
    public void SetPuzzleIndex(int puzzleIndex){
        this.puzzleIndex = puzzleIndex;
    }

    /** 
        This function stores all of the puzzle images using the local puzzle index.
        Then set all of the sprites in the puzzle holder with the puzzle images.
    */
    void LoadPuzzle(){
        puzzleImages = Resources.LoadAll<Sprite>("Sprites/BG "+puzzleIndex);
        puzzlePieces = GameObject.Find("Puzzle Holder").GetComponent<PuzzleHolder>().puzzlePieces;

        for(int i = 0; i < puzzlePieces.Length; i++){
            puzzlePieces[i].GetComponent<SpriteRenderer>().sprite = puzzleImages[i];
        }

    }

    /** 
        This function checks if the level that was loaded was the Gameplay level.
        If so, the functions loadPuzzle and gamestarted will execute.
    */
    void OnLevelWasLoaded(){
        if(SceneManager.GetActiveScene().name == "Gameplay"){
            if(puzzleIndex > 0){
                LoadPuzzle();
                GameStarted();
            }
        }
    }

    /** 
        This function will setup the intial puzzle. It'll deactivate one puzzle
        piece. It'll also place the puzzle pieces, track the rows and cols and
        then it'll shuffle puzzle pieces. Lastly, it'll set the gamestate.
    */
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
                    print("row: " + row + " col: " + col);
                    print(puzzlePieces[row * GameVariables.MaxCols + col].name);
                }else{
                    matrix[row, col] = null;
                }
            }
        }
        Shuffle();
        gameState = GameState.Playing;
    }

    /** 
        This function will convert the rows and column positions into x y 
        coordinates. The new vector3 will create a .225f difference in the x
        cooridinate and a .772f difference in the y position. The vector3
        containing the new cooridnates.

        @param {int} the row of the image
        @param {int} the col of the image

        @returns {Vectoe3} coordinates from the viewport to world point
    */
    private Vector3 GetScreenCoordinatesFromViewPort(int row, int col){
        Vector3 point = Camera.main.ViewportToWorldPoint(new Vector3(0.225f * row, 1 - 0.228f * col, 0));
        point.z = 0;
        return point;
    }

    /** 
        This function will go through each of the images, generate a new row 
        and column and swaps the current image with the generated one.
    */
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

    /** 
        This function will swap the puzzlePiece object at both the given
        row, col and rand_row, rand_col. Then it will update the puzzlePiece's
        current row and column.

        @param {int} row of the first puzzle piece
        @param {int} col of the first puzzle piece
        @param {int} row of the second puzzle piece that will be swapped with the first
        @param {int} col of the second puzzle piece that will be swapped with the first
    */
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

    /** 
        This function will detect when the user clicks on the left mouse button,
        and cast a ray into the 2d plane. It will check if the user has clicked
        on a puzzle piece. If true, it will then check if the empty space is
        next to piece the user clicked on. If that is true, the piece will swap
        with the empty piece.
    */
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
                }else if(colFound < GameVariables.MaxCols -1 && matrix[rowFound, colFound +1] == null){
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

    /** 
        This function will move a puzzle piece to the desired transform.position to 
        the position defined in screenPosToAnimate.

        @param {PuzzlePiece} puzzle piece that will be moved
    */
    private void AnimateMovement(PuzzlePiece toMove){
        toMove.GameObject.transform.position = Vector2.MoveTowards(toMove.GameObject.transform.position, screenPosToAnimate, animSpeed*Time.deltaTime);
    }

    /** 
        This function will check if the PuzzlePiece that had just moved, is now very close to
        the position defined in screenPosToAnimate. If so, it will swap the empty
        space with the current puzzlePiece. Once finished, it'll check if the 
        game has completed.
    */
    private void CheckIfAnimationEnded(){
        if(Vector2.Distance(PieceToAnimate.GameObject.transform.position, screenPosToAnimate) < 0.1f){
            Swap(PieceToAnimate.CurrentRow, PieceToAnimate.CurrentCol, toAnimateRow, toAnimateCol);
            gameState = GameState.Playing;
            CheckForVictory();
        }
    }

    /** 
        This function will go through every puzzle piece, and check if their
        current row and col matches their original row and column. If so, that
        means the puzzle piece, is in the right place. If they're all in their
        right places, then the gamestate will be set to end.
    */
    private void CheckForVictory(){
        for(int row = 0; row < GameVariables.MaxRows; row++){
            for(int col = 0; col < GameVariables.MaxCols; col++){
                if(matrix[row, col] == null){
                    print("row: " + row + " col: " + col);
                    print("equals null");
                    continue;
                }
                if(matrix[row, col].CurrentRow != matrix[row, col].OriginalRow ||
                matrix[row, col].CurrentCol != matrix[row, col].OriginalCol){
                    print("------ CheckForVictory ------");
                    print("row: " + row + " col: " + col);
                    print("matrix: " +matrix[row, col]);
                    print("CurrentRow: " + matrix[row, col].CurrentRow + " CurrentCol: " + matrix[row, col].CurrentCol);
                    print("OriginalRow: " + matrix[row, col].OriginalRow + " OriginalCol: " + matrix[row, col].OriginalCol);

                    return;
                }
            }
        }
        gameState = GameState.End;
    }
}
