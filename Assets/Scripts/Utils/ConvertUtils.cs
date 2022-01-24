using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConvertUtils 
{
    public static string ConvertDistanceToReadableValue( Vector3 pos1, Vector3 pos2 )
    {
        return ( (int)( 2 * Vector3.Distance( pos1, pos2 ) ) ).ToString() + "cm";
    }
}
