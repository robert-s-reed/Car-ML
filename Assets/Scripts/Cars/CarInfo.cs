using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInfo : MonoBehaviour
{
    const float V_AVG_WEIGHT = 0.2f; //weight given to average speed in calculating fitness
    const float MIN_V = 0.4f; //min. speed of car (below that it is at risk of timing out and dying)
    const float TIMEOUT = 5f;
    const float MIN_KILL_TIME = 0.4f;
    const float DISABLED_ALPHA = 0.2f;

    [HideInInspector] public Renderer rend;
    [HideInInspector] public AICar ai;

    [SerializeField] GameObject headlights = null;
    [SerializeField] GameObject brakeLights = null;
    [SerializeField] GameObject parentSpotlight = null;

    CarHandling handling;
    bool isFocused = false;
    int speciesIndex;
    float time;
    float minSpeedTime;

    public bool IsAlive { get => handling.isAlive; set => handling.isAlive = value; }

    public bool IsParent { get; private set; } = false;

    public float Distance { get; private set; }

    public float AvgV { get => Distance / time; }

    public float Fitness { get => Distance + AvgV * V_AVG_WEIGHT; } //fitness equation (modelling)

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        ai = GetComponent<AICar>();
        handling = GetComponent<CarHandling>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAlive && !SimUI.IsPaused)
        {
            time += Time.deltaTime;
            //Note current time if speed drops below min. speed
            if (handling.Velocity < MIN_V)
            {
                //Kill car if below min. V for too long
                if (time - minSpeedTime >= TIMEOUT)
                {
                    KillCar();
                }

                //If car was previously above min. V (minSpeedTime only -1 if previous frame speed was about min.)
                if (minSpeedTime == -1)
                {
                    minSpeedTime = time;
                }
            }
            else if (minSpeedTime != -1)
            {
                minSpeedTime = -1; //set to -1 to indicate car was above min. V this frame
            }
        }

        if (isFocused)
        {
            if (handling.throttleBrakeInput < 0)
            {
                brakeLights.SetActive(true);
            }
            else
            {
                brakeLights.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Waypoint":
                //Set car's distance as distance from start of waypoint:
                Waypoint waypoint = other.GetComponent<Waypoint>();
                Distance = waypoint.distance;
                if (waypoint.isFinish)
                {
                    KillCar(true);
                }
                break;
            case "Wall":
                //Check to ensure car is not already dead so KillCar won't be called twice...
                //Also prevent bug where - with low frame rates - cars mistakenly think they collide with a wall:
                if (IsAlive && transform.position != Vector3.zero)
                {
                    KillCar();
                }
                break;
        }
    }

    public void ToggleLights()
    {
        isFocused = !isFocused;
        headlights.SetActive(isFocused);
        if (!isFocused && brakeLights.activeSelf)
        {
            brakeLights.SetActive(false);
        }
    }

    public void ResetCar(int species, bool isParent = false)
    {
        speciesIndex = species;
        IsParent = isParent;
        Distance = 0;
        time = 0;
        minSpeedTime = -1;
        rend.material.color = CarsManager.This.speciesInfo[species].colour;
        if (isParent && !parentSpotlight.activeSelf)
        {
            parentSpotlight.SetActive(true);
        }
        else if (!isParent && parentSpotlight.activeSelf)
        {
            parentSpotlight.SetActive(false);
        }
    }

    void KillCar(bool isFinished = false)
    {
        //Check to ensure KillCar has not mistakenly been called due to low frame rate at the start of a generation:
        if (time > MIN_KILL_TIME)
        {
            IsAlive = false;
            if (!isFinished)
            {
                //Disable car and make it transparent if collided with wall:
                Color colour = rend.material.color;
                colour.a = DISABLED_ALPHA;
                rend.material.color = colour;
            }
            CarsManager.KillCar(speciesIndex, isFinished);
        }
    }
}
