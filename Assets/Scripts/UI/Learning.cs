using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Learning : MonoBehaviour
{
    [System.Serializable] struct ClipInfo
    {
        public VideoClip clip;
        public string description;
    }

    [SerializeField] Image infoImage = null;
    [SerializeField] Sprite nnSprite = null;
    [SerializeField] Sprite mlSprite = null;
    [SerializeField] VideoPlayer vidPlayer = null;
    [SerializeField] ClipInfo[] exemplarClips = null;
    [SerializeField] GameObject clipsInfo = null;
    [SerializeField] Text clipInfoText = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadPage(string page)
    {
        infoImage.enabled = true; //Show info image in preparation for showing

        //Load the page corresponding to the page parameter for the button pressed:
        if (page == "Exemplar")
        {
            infoImage.enabled = false; //Hide info page image
            vidPlayer.enabled = true; //Show video player
            clipsInfo.SetActive(true); //Show exemplar clip info
        }
        else
        {
            infoImage.enabled = true; //Show info page image in preparation for setting its sprite
            vidPlayer.enabled = false; //Hide video player used for exemplar
            clipsInfo.SetActive(false); //Hide exemplar clip info
            clipInfoText.text = ""; //Reset clips description to prevent it displaying the previous clip info if the exemplar section is returned to

            //Set image according to selected page:
            if (page == "NN")
            {
                infoImage.sprite = nnSprite;
            }
            else
            {
                infoImage.sprite = mlSprite;
            }
        }
    }

    public void PlayClip(int clipIndex)
    {
        vidPlayer.clip = exemplarClips[clipIndex].clip;
        clipInfoText.text = exemplarClips[clipIndex].description;
    }
}
