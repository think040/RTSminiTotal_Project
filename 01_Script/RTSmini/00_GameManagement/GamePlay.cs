using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay
{
    public static bool isResume
    {
        get; set;
    } = true;

    public static bool isNvStop
    {
        get; set;
    } = false;

    public static Action ResumeAction
    {
        get; set;
    } = () => { };

    public static Action PauseAction
    {
        get; set;
    } = () => { };

    public static void Update()
    {
        UpdateAI();

        UpdateTransform();

        UpdateBone();
        
        UpdateGResource();
    }

    public static Action UpdateAI
    {
        get; set;
    } = () => { };

    public static Action UpdateTransform
    {
        get; set;
    } = () => { };

    public static Action UpdateBone
    {
        get; set;
    } = () => { };
    
    public static Action UpdateGResource
    {
        get; set;
    } = () => { };
}
