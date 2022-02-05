using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCar : CarHandling
{
    const float MAX_THROTTLE_IN = 0.97f; //Maximum input throttle where 100% throttle is registered (to mitigate hardware pedal deadzone)
    const float MAX_STEER_IN = 0.2f; //Steering input (as proportion of range) wher 100% steering lock is registered
    const float CRASH_INDICATOR_TIME = 0.2f;

    [SerializeField] Image crashIndicator = null;

    // Start is called before the first frame update
    void Start()
    {
        _Start(); //calls start method in base class
    }

    // Update is called once per frame
    void Update()
    {
        //Translate input axes into throttle/brake and steer:
        throttleBrakeInput = Mathf.Clamp(Input.GetAxisRaw("Throttle/Brake") / MAX_THROTTLE_IN, -1f, 1f);
        steerInput = Mathf.Clamp(Input.GetAxisRaw("Steering") / MAX_STEER_IN, -1f, 1f);
    }

    private void FixedUpdate()
    {
        _FixedUpdate(); //Calls FixedUpdate method in base class
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if collision with wall:
        if (other.CompareTag("Wall"))
        {
            StartCoroutine(Crash());
        }
    }

    IEnumerator Crash()
    {
        crashIndicator.enabled = true;
        yield return new WaitForSeconds(CRASH_INDICATOR_TIME);
        crashIndicator.enabled = false;
    }
}
