using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardInput : MonoBehaviour
{
    public UnityEvent moveLeft = new UnityEvent();
    public UnityEvent moveRight = new UnityEvent();
    public UnityEvent moveForward = new UnityEvent();
    public UnityEvent moveBackward = new UnityEvent();
    public UnityEvent turnLeft = new UnityEvent();
    public UnityEvent turnRight = new UnityEvent();
    public UnityEvent moveUp = new UnityEvent();
    public UnityEvent moveDown = new UnityEvent();
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
         if(Input.GetKey(KeyCode.W) )
        {
            moveForward?.Invoke();  
        }
        if(Input.GetKey(KeyCode.A))
        {
            moveLeft?.Invoke();
        }
        if(Input.GetKey(KeyCode.D))
        {
            moveRight?.Invoke();
        }
        if(Input.GetKey(KeyCode.S) )
        {
            moveBackward?.Invoke();            
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            turnLeft?.Invoke();
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            turnRight?.Invoke();
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            moveUp?.Invoke();
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            moveDown?.Invoke();
        }
        
       
    }


}
