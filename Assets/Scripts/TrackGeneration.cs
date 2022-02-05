using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrackGeneration : MonoBehaviour
{
    [System.Serializable] struct SegmentInfo
    {
        public GameObject prefab;
        public float rot;
        public Vector3 startOffset; //offset from start of segment to its centre
        public Vector3 endOffset; //offset from centre of segment to its end
    }

    [SerializeField] SegmentInfo[] segmentInfos = null;
    [SerializeField] GameObject[] segmentPrefabs = null;
    [SerializeField] TextAsset tracks = null; //Text file containing tracks info (segment list)
    [SerializeField] Transform trackParent = null; //Parent object with each track segment as a child

    // Start is called before the first frame update
    void Start()
    {
        //Load track from file:
        string[] segments = tracks.text.Split('\n')[SimParams.trackNum].Split(','); //array of segment indexes for selected track
        float rot = 0; //rot of end of last segment
        Vector3 pos = Vector3.zero; //pos of end of last segment
        foreach (string segment in segments)
        {
            SegmentInfo segmentInfo = segmentInfos[int.Parse(segment)];
            pos += Quaternion.Euler(0, rot, 0) * segmentInfo.startOffset; //Calculate pos of new segment, taking rotation into account
            Instantiate(segmentInfo.prefab, pos, Quaternion.Euler(0, rot, 0),trackParent);
            pos += Quaternion.Euler(0, rot, 0) * segmentInfo.endOffset;
            rot += segmentInfo.rot;
        }

        InitialiseWaypoints();
    }

    public void InitialiseWaypoints()
    {
        //position and distance (from start of track) of previous waypoint:
        Vector3 prevPos = Vector3.zero;
        float prevDistance = -1;

        for (int i = 0; i < trackParent.childCount; i++) //for each segment of the track
        {
            Transform waypoints = trackParent.GetChild(i).Find("Waypoints");
            for (int j = 0; j < waypoints.childCount; j++) //for each waypoint in the segment
            {
                Waypoint waypoint = waypoints.GetChild(j).GetComponent<Waypoint>();
                if (prevDistance == -1) //if first waypoint
                {
                    waypoint.distance = 0;
                }
                else
                {
                    //calculates distance by adding distance of prev. waypoint to the distance between it and this:
                    waypoint.distance = prevDistance + Vector3.Distance(prevPos, waypoint.transform.position);
                }
                prevPos = waypoint.transform.position;
                prevDistance = waypoint.distance;

                //Set final waypoint as the finish line:
                if (i == trackParent.childCount - 1 && j == waypoints.childCount - 1)
                {
                    waypoint.MakeFinish();
                }
            }
        }
    }
}