using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorToggle : MonoBehaviour
{
    public ColorSelector selector;
    public Toggle toggle;
    public Image tImage;
    public Color color;
    public int idx;

    public Action<Color, int> onApply
    {
        get; set;
    } = (color, idx) => { };

    public UnitManager unitMan;
    public Color[] skColor;
    public Color[] stColor;
    public int id;
    public Type type;

    //public ArrowManager arrowMan;
    //public Color[] arColor;

    // Start is called before the first frame update
    void OnEnable()
    {        
        //selector.red.slider.value = color.r;
        //selector.green.slider.value = color.g;
        //selector.blue.slider.value = color.b;
        //selector.color.color = color;

        selector.onApply = ApplyColor;

        toggle.onValueChanged.AddListener(
            (value) =>
            {
                if(value)
                {
                    //selector.color.color = color;
                    selector.color = tImage;
                    
                    //selector.red.slider.value = color.r;
                    //selector.green.slider.value = color.g;
                    //selector.blue.slider.value = color.b;

                    Color c = GetColor();
                    selector.red.slider.value = c.r;
                    selector.green.slider.value = c.g;
                    selector.blue.slider.value = c.b;
                    selector.onApply = ApplyColor;                    
                }      
                else
                {                   
                    //tImage.color = this.color;

                    Color c = GetColor();
                    tImage.color = c;
                }
            });

       
        //toggle.GetComponent<Image>().color = color;
    }

    void Start()
    {
        //toggle.GetComponent<Image>().color = color;

        //tImage.color = GetColor();
    }

    void ApplyColor(Color c)
    {        
        //Debug.Log(gameObject.name + " ApplyColor : " + c.ToString());
        color = selector.color.color;
        //toggle.GetComponent<Image>().color = color;

        onApply(c, idx);
        SetColor(c);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitColor()
    {
        tImage.color = GetColor();
    }

    public void SetDefColor()
    {
        if (type == Type.sk)
        {
            Color c = unitMan.skmb.skColors[id];
            tImage.color = c;
            skColor[id] = c;
            unitMan.ChangeColorSk();
        }
        else if (type == Type.st)
        {
            Color c = unitMan.skmb.stColors[id];
            tImage.color = c;
            stColor[id] = c;
            unitMan.ChangeColorSt();
        }
        else if (type == Type.ar)
        {
            Color c = ArrowManager.aColorDef[0];           
            tImage.color = c;
            ArrowManager.ChangeArrowColor1(c);
        }
    }

    public void SetColor(Color color)
    {
        if (type == Type.sk)
        {
            skColor[id] = color;
            unitMan.ChangeColorSk();
        }
        else if (type == Type.st)
        {
            stColor[id] = color;
            unitMan.ChangeColorSt();
        }
        else if (type == Type.ar)
        {
            ArrowManager.ChangeArrowColor1(color);
        }

        //tImage.color = color;
    }

    public Color GetColor()
    {
        if(type == Type.sk)
        {
            return skColor[id];
        }
        else if(type == Type.st)
        {
            return stColor[id];
        }
        else if (type == Type.ar)
        {
            return ArrowManager.aColor[0];
        }

        return Color.white;
    }

    [Serializable]
    public enum Type
    {
        sk, st, ar
    }
}
