using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject savePanel;
    public GameObject customLoadPanel;
    public Text subtitle;
    public List<string> subtitles;
    public float timeBetweenSubtitleChange; 

    [Header("Main Save Preview")]
    public GameObject gameSavePreviewO;
    public Text saveZoneText;
    public string saveTitleBaseText;
    public string noSaveText;
    public Image savePreviewImage;
    public Text dateTimeText;
    
    [Header("Custom Save Preview")]
    public GameObject customGameSavePreviewO;
    public Text customSaveZoneText;
    public Image customSavePreviewImage;
    public Text customDateTimeText;
    [HideInInspector] public string directoryNameSelected;



    private PreviewData previewData;
    private bool mainPanelOpen;

    private void Start()
    {
        OpenMainPanel();
        CloseSavePanel();
        InvokeRepeating("ChangeSubtitle",0.0f,timeBetweenSubtitleChange);
    }

    private void Update()
    {
        if(Input.anyKeyDown && mainPanelOpen)
        {
            OpenSavePanel();
            CloseMainPanel();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && !mainPanelOpen)
        {
            CloseSavePanel();
            OpenMainPanel();
        }
    }

    public void CloseMainPanel()
    {
        mainPanel.SetActive(false);
        mainPanelOpen = false;
    }

    public void OpenMainPanel()
    {
        mainPanel.SetActive(true);
        mainPanelOpen = true;
    }

    public void OpenSavePanel()
    {
        savePanel.SetActive(true);
        previewData = SaveSystem.LoadPreview();
        if(previewData != null)
        {
            saveZoneText.text = saveTitleBaseText + previewData.actualZoneName;
            dateTimeText.text = previewData.saveDateTime;
            savePreviewImage.sprite = previewData.SavePicture;
        }
        else
        {
            Debug.Log("No save preview found");
            saveZoneText.text = noSaveText;

        }
    }

    public void CloseSavePanel()
    {
        savePanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PreviewCustomLoad(string directoryName)
    {
        directoryNameSelected = Application.dataPath + "/Resources/Custom Saves/" + directoryName;
        previewData = SaveSystem.LoadPreview(Application.dataPath + "/Resources/Custom Saves/" + directoryName);
        if (previewData != null)
        {
            customSaveZoneText.text = saveTitleBaseText + previewData.actualZoneName;
            customDateTimeText.text = previewData.saveDateTime;
            customSavePreviewImage.sprite = previewData.SavePicture;
        }
        else
        {
            Debug.Log("No save preview found");
            customSaveZoneText.text = noSaveText;
        }
    }

    private void ChangeSubtitle()
    {
        subtitle.text = subtitles[Random.Range(0, subtitles.Count)];
    }
}
