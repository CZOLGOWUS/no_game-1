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

}
