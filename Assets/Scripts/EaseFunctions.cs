using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EaseFunctions 
{
    public static float easeInOutCirc(float x){
        if(x < 0.5){
            return (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2;
        }
        else{
            return (Mathf.Sqrt(1 - Mathf.Pow(-2 *  x + 2, 2)) + 1 ) / 2;
        }
    }

    public static float easeInOutBack(float x){

        float c1 = 1.170158f;
        float c2 = c1 * 1.525f;

        if(x < 0.5){
            return (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2;
        }
        else {
            return (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }
        
    }

    public static float easeInBack(float x){

        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return c3 * x * x * x - c1 * x * x;
    }

    public static float easeInExpo(float x){
        if(x == 0){
            return 0;
        }
        else{
            return Mathf.Pow(2, 10 * x -10);
        }
    }

    public static float easeInCubic(float x){
        return x * x * x;
    }

    public static float easeInOutCubic(float x){
        if(x < 0.5){
            return 4 * x * x * x;
        }
        else{
            return 1 - Mathf.Pow(-2 * x +2, 3) / 2;
        }
    }

    public static float easeInSine(float x){
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }
    public static float easeOutBack(float x){
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
    public static float easeOutExpo(float x){
        if(x == 1){
            return 1;
        }
        else{
            return 1 - Mathf.Pow(2, -10 *x);
        }
    }
    public static float easeInOutQuint(float x){
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }
}
