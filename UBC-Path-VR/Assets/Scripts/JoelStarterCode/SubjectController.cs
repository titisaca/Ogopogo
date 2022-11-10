using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 20.0f;
    [SerializeField]  private Rigidbody rb;
    
    private Vector3 velocity;
    [SerializeField] private float lookSpeed = 10f;
    [SerializeField] private float moveForce = 0.1f;
    private Vector2 rotation = Vector2.zero;
    private CharacterController characterController;
    void OnEnable() {
       
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
    }


    private void FixedUpdate() {
        velocity = rb.velocity;      //to get a Vector3 representation of the velocity
        float speed = velocity.magnitude;             // to get magnitude


    }
    
    private void Translate(Vector3 direction)
    {
        float singleStep = movementSpeed * Time.deltaTime;
        transform.Translate(direction.normalized * singleStep, Space.World);
        //rb.AddForce(direction.normalized * singleStep, ForceMode.Impulse);
    }
    public void MoveForward()
    {
        //Translate(transform.forward);
        rb.AddForce(transform.forward * moveForce, ForceMode.Impulse);
    }

    public void MoveBackward()
    {
        //Translate(-transform.forward);
        rb.AddForce(-transform.forward.normalized * moveForce, ForceMode.Impulse);
    }

    public void MoveRight()
    {
        //Translate(transform.right);
        rb.AddForce(transform.right.normalized * moveForce, ForceMode.Impulse);
    }

    public void MoveLeft()
    {
        //Translate(-transform.right);
        rb.AddForce(-transform.right.normalized * moveForce, ForceMode.Impulse);
    }

    public void Jump() 
    {
     
            if(rb.velocity.y < 0.1f) {
                rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
            }
            
    }
        
    
    public void MoveDown() {
        Translate(-transform.up);
    }

    public void TurnLeft() {
        //TurnTowards(-transform.right);
        transform.Rotate(-Vector3.up * lookSpeed * Time.deltaTime, Space.Self);

    }
    public void TurnRight() {
        //TurnTowards(transform.right);
        transform.Rotate(Vector3.up * lookSpeed * Time.deltaTime, Space.Self);

    }

    public void TurnUp() {
    
        transform.Rotate(1.0f, 0.0f,0.0f,Space.Self);

    }

    public void TurnDown() {
    
        transform.Rotate(-1.0f, 0.0f,0.0f,Space.Self);

    }

    public void Print()
    {
        Debug.Log($"=========  {transform.name}");
    }

    public void Look() // Look rotation (UP down is Camera) (Left right is Transform rotation)
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, -25f, 25f);
        transform.eulerAngles = new Vector2(0,rotation.y) * lookSpeed;
        //Camera.main.transform.localRsotation = Quaternion.Euler(rotation.x * lookSpeed, 0, 0);
    }
}
