using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public static Text info;
    public Text _info;

    private void Awake()
    {
        //info = _info;
    }

    public void Init()
    {
        info = _info;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
