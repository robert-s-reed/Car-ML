using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    [SerializeField] InputField inputField = null;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        SliderChanged();
    }

    public void SliderChanged()
    {
        //Set input field text to match slider value:
        inputField.text = slider.value.ToString();
    }

    public void TextChanged()
    {
        //Set slider value to match input field text:
        int index;
        if (int.TryParse(inputField.text, out index)) //If valid integer input
        {
            slider.value = index;
            
            //Update text if slider had to change value since it was out of slider range:
            if (slider.value != index)
            {
                SliderChanged();
            }
        }
    }
}
