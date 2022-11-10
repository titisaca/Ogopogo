using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class PointObject : MonoBehaviour
{
    public UnityEvent onPoint = new UnityEvent();
    [SerializeField] ParticleSystem explode = null;
    [SerializeField] private TextMeshProUGUI pointsText = null;
    

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            
            Collect();
            
           
        }
    }

    public void Collect() {
        explode.Play();
        AddPoints(1);
        Destroy(this.gameObject, 0.25f);
        // Debug.Log("Point");
    }
    
    public void AddPoints(int pointsToAdd) {
        Player.points += pointsToAdd;
        pointsText.text = "Points: " + Player.points.ToString();
    }
}
