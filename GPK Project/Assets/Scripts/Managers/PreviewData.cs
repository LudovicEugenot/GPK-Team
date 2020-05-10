using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PreviewData
{
    public string saveDateTime;
    public byte[] savePictureBytes;
    public string actualZoneName;

    public PreviewData(ZoneHandler zoneHandler ,Texture2D screenTexture)
    {
        savePictureBytes = screenTexture.EncodeToPNG();
        saveDateTime = System.DateTime.Now.ToString();
        actualZoneName = zoneHandler.currentZone.name;
    }

    public Sprite SavePicture
    {
        get
        {
            Texture2D texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(savePictureBytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1920, 1080), new Vector2(0.5f, 0.0f), 1.0f);
            return sprite;
        }
    }
}
