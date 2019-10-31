using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectPuzzleController : MonoBehaviour
{
    /** 
        This function will get the name of the button that was pressed, which
        is the name of the puzzle that the player has selected. It will then
        set the index in the game manager.
    */
    public void SelectPuzzle(){
        string[] name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.Split();
        int index = int.Parse(name[1]);
        if(GameManager.instance != null){
            GameManager.instance.SetPuzzleIndex(index);
        }

        SceneManager.LoadScene("Gameplay");
    }

    public void LoadMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }
}
