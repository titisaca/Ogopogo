using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointsText = null;
    private int points = 0;
    // Start is called before the first frame update
    void Start()
    {
        pointsText.text = "Points: 0"; 
    }
    public void AddPoints(int pointsToAdd) {
        points += pointsToAdd;
        pointsText.text = "Points: " + Player.points.ToString();
    }
}
