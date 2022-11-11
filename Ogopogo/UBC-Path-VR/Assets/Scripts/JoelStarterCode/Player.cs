using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Player
{
    // Start is called before the first frame update
    public static int points = 0;
    public static void AddPoints(int pointsToAdd) {
        points += pointsToAdd;
    }
}
