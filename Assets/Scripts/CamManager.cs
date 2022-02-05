using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamManager : MonoBehaviour
{
    const float SWITCH_TIME = 1f;
    const float MAX_WHEEL_ANGLE = 90f;
    const float MIN_SPEEDO_ANGLE = 150f;
    const float SPEEDO_DEG_PER_MPH = 30f / 20f;

    static CamManager camManager; //static reference to this (and only) instance to allow static methods to access non-static fields
    static Transform fittestCar = null;
    static CarHandling fittestCarHandling;
    static Vector3 startOfSwitchPos;
    static float switchTime;

    [SerializeField] Transform playerCar = null;
    [SerializeField] RectTransform steeringWheel = null;
    [SerializeField] Slider throttleSlider = null;
    [SerializeField] Slider brakeSlider = null;
    [SerializeField] RectTransform speedo = null;
    [SerializeField] Text speedText = null;
    [SerializeField] Transform dashcam = null;

    Vector3 posOffset;
    CarHandling playerCarHandling;

    // Start is called before the first frame update
    void Start()
    {
        camManager = this;
        posOffset = transform.position;
        startOfSwitchPos = posOffset;
        playerCarHandling = playerCar.GetComponent<CarHandling>();
    }

    // Update is called once per frame
    void Update()
    {
        //Follow fittest car with a position offset of its starting position, smoothly transitioning to its position:
        if (fittestCar != null)
        {
            if (switchTime < 1)
            {
                if (switchTime == 0)
                {
                    startOfSwitchPos = transform.position;
                }

                //Calculate switchTime as a proportion of SWITCH_TIME that has elapsed since start of switch:
                switchTime += Time.deltaTime / SWITCH_TIME;
            }

            //Establish position of target car (either lead AI or player) as well as its CarHandling:
            Transform target;
            CarHandling targetHandling;
            if (CarsManager.evolutionInhibit)
            {
                target = playerCar;
                targetHandling = playerCarHandling;
            }
            else
            {
                target = fittestCar;
                targetHandling = fittestCarHandling;
            }

            //Update dashcam parent if needed:
            if (camManager.dashcam.parent != target)
            {
                camManager.dashcam.SetParent(target, false);
                camManager.dashcam.transform.localPosition = Vector3.forward / 2f;
            }

            //Move to smoothly transition to and follow lead or player car:
            transform.position = Vector3.Lerp(startOfSwitchPos, target.position + posOffset, switchTime);

            //Update telemetry UI:
            throttleSlider.value = Mathf.Max(targetHandling.throttleBrakeInput, 0);
            brakeSlider.value = Mathf.Max(-targetHandling.throttleBrakeInput, 0);
            steeringWheel.rotation = Quaternion.Euler(0, 0, -targetHandling.steerInput * MAX_WHEEL_ANGLE);
            float mph = targetHandling.Velocity * CarHandling.MPS2MPH;
            speedo.rotation = Quaternion.Euler(0, 0, MIN_SPEEDO_ANGLE - (mph * SPEEDO_DEG_PER_MPH));
            speedText.text = mph.ToString("0"); //Speed displayed in mph
        }
    }

    public static void NewFittest(Transform newCar)
    {
        fittestCar = newCar;
        fittestCarHandling = newCar.GetComponent<CarHandling>();

        //Only reset switchTime if following AI to prevent camera falling behind player:
        if (!CarsManager.evolutionInhibit)
        {
            switchTime = 0f;
        }
    }
}
