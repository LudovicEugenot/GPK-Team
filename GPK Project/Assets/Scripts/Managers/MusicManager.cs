using System.Collections;
using UnityEngine;

/*  DONE Peut pas changer de zone tant qu’un ennemi est vivant                      YES MON SAC EST FAIT 
 *  DONE Ajouter un feedback qui montre au joueur qu’il peut pas changer de zone quand il essaie de le faire
 *  Changer la musique quand un combat se déclenche
 *  Synchroniser la musique de combat avec la musique de non combat
 *  
 *  Je peux transitionner sur le 4ème beat
 *  break moment calme
 *  into drop
 *  into boucle (moment énervé) qui peuvent s'enchaîner dans n'importe quel ordre
 *  
 *  début : 
 */

public class MusicManager : MonoBehaviour
{
    #region Initialization
    public MusicSO musicSO;
    BeatManager beatManager;
    #endregion

    private void Start()
    {
        beatManager = BeatManager.Instance;
        ScriptableObjectSetUp();
    }



    void ScriptableObjectSetUp()
    {
        beatManager.bpm = musicSO.bpm;
        beatManager.timingThreshold = musicSO.timingThreshold;
        beatManager.timingThresholdOffset = musicSO.timingThresholdOffset;
        beatManager.minTimeForOnBeatValidation = musicSO.minTimeForOnBeatValidation;
        beatManager.cameraBeatEffectLerpSpeed = musicSO.cameraBeatEffectLerpSpeed;
        beatManager.cameraBeatEffectAmplitude = musicSO.cameraBeatEffectAmplitude;

        //beatManager.LoadMusic(musicSO.explorationLoop, musicSO.explorationBeatStartTimeOffset);
        beatManager.LoadMusic(musicSO.explorationLoop);
    }
}
