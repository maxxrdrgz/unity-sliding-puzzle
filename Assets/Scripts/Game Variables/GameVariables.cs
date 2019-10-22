using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameVariables
{
    public static int MaxRows = 4;
    public static int MaxCols = 4;
    public static int MaxSize = MaxRows * MaxCols;
}

public enum GameState {
    Playing,
    Animating,
    End
}
