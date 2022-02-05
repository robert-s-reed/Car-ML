using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    public float relativeProb; //relative probability
    public Vector2Int[] occupiedSquares;
    public Vector2Int nextSegmentPos; //position of next segment relative to the start of this one
    public int rot; //rotation of end of segment relative to the start

    [HideInInspector] public float probUpperLim; //the segment with this value above and closest to a randomly generated number will be added
}
