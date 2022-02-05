using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICar : CarHandling
{
    const float MAX_SENSOR_DIST = 50f;

    LayerMask sensorLayerMask;

    public NeuralNet NN { get; private set; } = new NeuralNet();

    private void Awake()
    {
        NN.Initialise();
    }

    // Start is called before the first frame update
    void Start()
    {
        sensorLayerMask = LayerMask.GetMask("Wall");

        _Start(); //calls start method in base class
    }

    //called at fixed intervals to ensure that AI behaviour is independent of frame rate (therefore smae DNA = same results)
    private void FixedUpdate()
    {
        if (isAlive && !SimUI.IsPaused) //Don't bother calculating neural net if not alive
        {
            //Calculate vectors for front corners of car:
            Vector3 front = transform.position + transform.forward * transform.localScale.z / 2f;
            Vector3 frontLeft = front + -transform.right * transform.localScale.x / 2f;
            Vector3 frontRight = front + transform.right * transform.localScale.x / 2f;

            //send raycasts to get sensor values:
            RaycastHit[] sensorHits = new RaycastHit[4];
            Physics.Raycast(frontLeft, transform.forward - transform.right, out sensorHits[0], MAX_SENSOR_DIST, sensorLayerMask);
            Physics.Raycast(frontLeft, transform.forward, out sensorHits[1], MAX_SENSOR_DIST, sensorLayerMask);
            Physics.Raycast(frontRight, transform.forward, out sensorHits[2], MAX_SENSOR_DIST, sensorLayerMask);
            Physics.Raycast(frontRight, transform.forward + transform.right, out sensorHits[3], MAX_SENSOR_DIST, sensorLayerMask);

            //populate inputs array with sensor values and speed:
            float[] inputs = new float[5];
            for (int i = 0; i < sensorHits.Length; i++)
            {
                float distance = sensorHits[i].distance;
                if (distance == 0) //i.e. wall further than MAX_SENSOR_DIST
                {
                    distance = MAX_SENSOR_DIST; //assume maximum distance (rather than zero)
                }
                inputs[i] = distance / MAX_SENSOR_DIST;
            }
            inputs[4] = Velocity / VMax;

            //update neural network:
            float[] neuralNetOutputs = NN.Calculate(inputs);
            throttleBrakeInput = neuralNetOutputs[0] * 2f - 1f;
            steerInput = neuralNetOutputs[1] * 2f - 1f;
        }

        _FixedUpdate(); //Calls FixedUpdate method in base class
    }

    public void ResetCar(float[] parentDNA, bool evolutionInhibit, bool mutate = false)
    {
        if (!evolutionInhibit)
        {
            //Mutate DNA and reset:
            if (mutate)
            {
                NN.Mutate(parentDNA);
            }
            else
            {
                NN.ImportDNA(parentDNA);
            }
        }
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Velocity = 0;
        isAlive = true;
    }
}
