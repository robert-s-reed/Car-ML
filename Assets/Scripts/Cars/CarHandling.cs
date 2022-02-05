using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarHandling : MonoBehaviour
{
    public const float MPS2MPH = 2.23694f;

    const float g = 9.81f; //acceleration due to gravity
    const float rho = 1.2f; //fluid (air) density
    const float c_d = 0.23f; //drag coefficient
    const float MASS = 1000f;
    const float FRONTAL_A = 4f;
    const float WHEELBASE = 3f;
    const float ENGINE_F = 5000f;
    const float BRAKING_F = 8000f;
    const float MAX_STEER_ANGLE = 30f * Mathf.Deg2Rad;

    [HideInInspector] public bool isAlive = true;
    [HideInInspector] public float throttleBrakeInput = 0;
    [HideInInspector] public float steerInput = 0;

    public float Velocity { get; internal set; } = 0f;

    public static float VMax { get; private set; }

    // Start is called before the first frame update
    protected void _Start()
    {
        VMax = Mathf.Sqrt((ENGINE_F - 0.00976f * MASS * g) / (0.00000586f * MASS * g + 0.138f * FRONTAL_A));
    }

    protected void _FixedUpdate()
    {
        if (isAlive && !SimUI.IsPaused) //if the car is still 'alive' (i.e., not crashed)
        {
            float squareVelocity = Velocity * Velocity;

            //longitudinal movement:
            float resistanceForce = 0; //rolling resistance force
            float dragForce = 0;
            if (Velocity > 0)
            {
                float resistanceCoefficient = 0.00976f + 0.00000586f * squareVelocity; //rolling resistance coefficient
                resistanceForce = resistanceCoefficient * MASS * g;
                dragForce = 0.5f * c_d * rho * squareVelocity * FRONTAL_A;
            }
            float inputForce;
            if (throttleBrakeInput >= 0)
            {
                inputForce = ENGINE_F * throttleBrakeInput;
            }
            else
            {
                inputForce = BRAKING_F * throttleBrakeInput;
            }
            float resultantForce = inputForce - resistanceForce - dragForce;
            float acceleration = resultantForce / MASS;
            Velocity += acceleration * Time.deltaTime;
            if (Velocity < 0)
            {
                Velocity = 0;
            }
            transform.Translate(Velocity * Vector3.forward * Time.fixedDeltaTime);

            //steering:
            float steerAngle = MAX_STEER_ANGLE * Mathf.Abs(steerInput);
            float linearTurnRadius = WHEELBASE / Mathf.Sin(steerAngle); //R
            float maxTurnRadius = squareVelocity / (Weather.Friction * g); //R-min
            float turnRadius = Mathf.Max(linearTurnRadius, maxTurnRadius);
            float angularV = (Velocity / turnRadius) * Mathf.Sign(steerInput);
            transform.Rotate(angularV * Mathf.Rad2Deg * Vector3.up * Time.fixedDeltaTime);
        }
    }
}
