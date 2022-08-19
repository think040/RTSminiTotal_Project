using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SettingPanel : MonoBehaviour
{
    public Action OnEnableAction
    {
        get; set;
    } = () => { };


    void Awake()
    {
        OnEnableAction();
    }

    void OnEnable()
    {
        //OnEnableAction();
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
