using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Smoother
{

    public static float Smooth01Start(float t, int n)
    {
        float x = t;

        for( int i = 1 ; i < n ; i++ )
        {
            x *= t;
        }

        return x;

    }

    public static float Smooth01End(float t,int n)
    {
        float x = 1 - t;

        for( int i = 1 ; i < n ; i++ )
        {
            x *= (1f - t);
        }

        return 1f - x;
    }


    public static float Ease01( float x , float easeSpeed )
    {
        float a = easeSpeed + 1f;
        return Mathf.Pow( x , a ) / (Mathf.Pow( x , a ) + Mathf.Pow( 1 - x , a ));
    }


}
