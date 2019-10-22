using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectPuzzleController : MonoBehaviour
{
    public void SelectPuzzle(){
        string[] name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.Split();
        int index = int.Parse(name[1]);
        if(GameManager.instance != null){
            GameManager.instance.SetPuzzleIndex(index);
        }
        switch (index)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
        }
        SceneManager.LoadScene("Gameplay");
    }

    public void LoadMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }
}
