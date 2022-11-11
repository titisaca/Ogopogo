using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaterBehaviour : MonoBehaviour
{
    public UnityEvent playerSplash = new UnityEvent();
    [SerializeField] ParticleSystem splash = null;

    

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            
            Splash();
            
           
        }
    }

    public void Splash() {
        splash.Play();

        // Debug.Log("Point");
    }
}
