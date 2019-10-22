using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayController : MonoBehaviour
{
    public void BackToSelectPuzzleMenu(){
        SceneManager.LoadScene("SelectPuzzleMenu");
    }
}
