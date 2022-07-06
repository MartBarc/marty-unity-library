using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    [SerializeField] public GridHandler gridHandler;

    // Start is called before the first frame update
    void Start()
    {
        gridHandler.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
