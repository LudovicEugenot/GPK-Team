using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject savePanel;
    [Space]
    public GameObject gameSavePreviewO;
    public Text saveZoneText;
    public string saveTitleBaseText;
    public string noSaveText;
    public Image savePreviewImage;
    public Text dateTimeText;

    private PreviewData previewData;
    private bool mainPanelOpen;

    private void Start()
    {
        OpenMainPanel();
        CloseSavePanel();
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
}
