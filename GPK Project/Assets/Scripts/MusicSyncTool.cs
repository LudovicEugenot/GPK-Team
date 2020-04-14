using UnityEngine;
using UnityEditor;
using System.Reflection;


public class MusicSyncTool : EditorWindow
{
    #region Initialization
    public Object music;
    private float imageWidth = 200f;
    private float imageHeight = 20f;
    Texture2D texture;
    GUILayoutOption[] objectFieldOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20f) };
    #endregion


    [MenuItem("Tools/Music Synchronizer")]
    public static void ShowWindow()
    {
        GetWindow<MusicSyncTool>("Music Synchronizer Tool");
    }

    private void OnGUI()
    {
        music = EditorGUILayout.ObjectField("Musique à synchro", music, typeof(AudioClip), false, objectFieldOptions);
        EditorGUILayout.Space(20f);

        GUILayout.Box(AssetPreview.GetAssetPreview(music), objectFieldOptions); // go voir d'autres trucs que GUILayout.Box pour étendre l'asset preview à plus que un carré et mettre une barre de scroll

        imageWidth = EditorGUILayout.Slider("Longueur de l'image ↓", imageWidth, 0f, 400f);
        imageHeight = EditorGUILayout.Slider("Hauteur de l'image ↓", imageHeight, 0f, 100f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(AssetPreview.GetAssetPreview(music), objectFieldOptions); // go voir d'autres trucs que GUILayout.Box pour étendre l'asset preview à plus que un carré et mettre une barre de scroll

        GUILayout.FlexibleSpace();

        texture = AssetPreview.GetAssetPreview(music);
        texture.Resize((int)imageWidth, texture.height);
        texture.SetPixels((int)imageWidth / 3, texture.height / 2, 50, 20, new Color[] { Color.black });
        texture.Apply();    

        GUILayout.Label(texture, new GUILayoutOption[] { GUILayout.Width(imageWidth), GUILayout.Height(imageHeight) }); // go voir d'autres trucs que GUILayout.Box pour étendre l'asset preview à plus que un carré et mettre une barre de scroll
        EditorGUILayout.EndHorizontal();
        if(GUILayout.Button("Play Audio", new GUILayoutOption[] { GUILayout.ExpandWidth(true) }))
        {
            PlayClip((AudioClip)music);
        }
        Rect scale = GUILayoutUtility.GetLastRect();

        EditorGUIUtility.fieldWidth = scale.width;
    }

    public static void PlayClip(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] {
                typeof(AudioClip)
            },
            null
        );
        method.Invoke(
            null,
            new object[] {
                clip
            }
        );
    }
}