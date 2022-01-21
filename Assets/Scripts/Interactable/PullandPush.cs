using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullandPush : Interactable
{
    // Start is called before the first frame update
    
    [SerializeField] private Boolean IsStatic = true;

    public Boolean getIsStatic(){
        return IsStatic;
    }
    public override void beInteractable(){

    }

}
