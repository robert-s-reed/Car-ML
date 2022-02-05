using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{
    class Series
    {
        public List<float> data = new List<float>();
        public List<Vector2> points = new List<Vector2>();
        public List<RectTransform> lines = new List<RectTransform>();
    }

    const float ADDED_LENGTH = 5f; //Added length to each graph line to make up for sprite having a small border

    [SerializeField] GameObject linePrefab = null;
    [SerializeField] Text maxText = null;

    Vector2 size;
    Series[] seriesArray;
    float minVal = Mathf.Infinity;
    float maxVal = Mathf.NegativeInfinity;

    //Map x (a < x < b) between c and d
    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
    }

    //Calculate point coord
    Vector2 GetPoint(Series series, int index, int dataCountOffset = 0)
    {
        return new Vector2(Map(index, 0, series.data.Count - dataCountOffset, 0, size.x), Map(series.data[index], minVal, maxVal, 0, size.y));
    }

    IEnumerator LateStart()
    {
        //Wait for the end of the frame to ensure Start is called on both graphs before deactivating object:
        yield return new WaitForEndOfFrame();
        transform.parent.gameObject.SetActive(false);
    }

    private void Awake()
    {
        //Initialise series Array:
        seriesArray = new Series[SimParams.numSpecies];
        for (int i = 0; i < seriesArray.Length; i++)
        {
            seriesArray[i] = new Series();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        size = GetComponent<RectTransform>().rect.size;

        //Hide graphs panel (which has to be enabled/visible by default in order to initialise)...
        StartCoroutine(LateStart());
    }

    public void AddData(int seriesIndex, float value)
    {
        Series series = seriesArray[seriesIndex];
        if (value > maxVal)
        {
            maxVal = value;
        }
        if (value < minVal)
        {
            minVal = value;
        }

        //Update points and lines based on new domain and range
        for (int i = 0; i < series.data.Count; i++)
        {
            series.points[i] = GetPoint(series, i);

            //Update line (ignoring first as it accesses the previous line index to avoid using points yet to be updated)
            if (i > 0)
            {
                UpdateLine(series, i - 1);
            }
        }
        series.data.Add(value);
        series.points.Add(GetPoint(series, series.data.Count - 1, 1));

        //Create line if there are at least 2 points:
        if (series.data.Count >= 2)
        {
            RectTransform newLine = Instantiate(linePrefab, transform).GetComponent<RectTransform>();
            newLine.GetComponent<Image>().color = CarsManager.This.speciesInfo[seriesIndex].colour;
            series.lines.Add(newLine);
            UpdateLine(series, series.lines.Count - 1);

            //Update text to show highest 'current' value:
            float maxCurrent = Mathf.NegativeInfinity;
            for (int i = 0; i < seriesArray.Length; i++)
            {
                float currentData = seriesArray[i].data[seriesArray[i].data.Count - 1];
                if (currentData > maxCurrent)
                {
                    maxCurrent = currentData;
                    maxText.text = currentData.ToString("0.0");
                    maxText.rectTransform.anchoredPosition = seriesArray[i].points[seriesArray[i].points.Count - 1];
                }
            }
            maxText.transform.SetAsLastSibling(); //To ensure the text is displayed on top of the lines
        }
    }

    void UpdateLine(Series series, int index)
    {
        RectTransform line = series.lines[index];
        Vector2 point1 = series.points[index];
        Vector2 point2 = series.points[index + 1];
        line.anchoredPosition = (point1 + point2) / 2f;
        line.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg);
        line.sizeDelta = new Vector2(Vector2.Distance(point1, point2) + ADDED_LENGTH, line.sizeDelta.y);
    }
}
