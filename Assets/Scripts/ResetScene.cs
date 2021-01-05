using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void restart()
    {
        SceneManager.LoadScene("World");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
