using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameBtn : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("Level1");
    }
}
