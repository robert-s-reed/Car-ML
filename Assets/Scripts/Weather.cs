using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    enum Condition { Dry, Wet, Ice }

    const float RAIN_TEXTURE_SMOOTHNESS = 0.8f;
    const float WET_FRICTION = 1.2f;
    const float ICE_FRICTION = 0.6f;

    [SerializeField] Renderer ground = null;
    [SerializeField] GameObject rain = null;
    [SerializeField] GameObject snow = null;
    [SerializeField] Material snowMaterial = null;

    public static float Friction { get; private set; } = 2f; //Defaults to dry friction

    // Start is called before the first frame update
    void Start()
    {
        switch ((Condition)SimParams.trackCondition)
        {
            case Condition.Wet:
                //Change friction, set ground to be smooth (since it's wet) and enable rain particles:
                Friction = WET_FRICTION;
                ground.material.SetFloat("_Glossiness", RAIN_TEXTURE_SMOOTHNESS);
                rain.SetActive(true);
                break;
            case Condition.Ice:
                //Change friction:
                Friction = ICE_FRICTION;
                ground.material = snowMaterial;
                snow.SetActive(true);
                break;
        }
    }
}
