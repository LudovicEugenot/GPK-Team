using System.Collections;
using UnityEngine;

/*  DONE Peut pas changer de zone tant qu’un ennemi est vivant                      YES MON SAC EST FAIT 
 *  DONE Ajouter un feedback qui montre au joueur qu’il peut pas changer de zone quand il essaie de le faire
 *  Changer la musique quand un combat se déclenche
 *  Synchroniser la musique de combat avec la musique de non combat
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


    AudioClip ChooseAppropriateMusic(AudioClip currentMusic)
    {
        if (MusicIsInArray(currentMusic, musicSO.drop))
        {
            return musicSO.combatLoop[Random.Range(0, musicSO.combatLoop.Length)];
        }

        if (ZoneHandler.Instance.AllEnemiesConverted())
        {
            if (MusicIsInArray(currentMusic, musicSO.combatLoop))
                return musicSO.breaks[Random.Range(0, musicSO.breaks.Length)];
            else
            //if (MusicIsInArray(currentMusic, musicSO.breaks))
                return musicSO.calmLoop;
        }
        else
        {
            if (MusicIsInArray(currentMusic, musicSO.combatLoop))
            {
                if (musicSO.combatLoop.Length > 1)
                {
                    AudioClip music = musicSO.combatLoop[Random.Range(0, musicSO.combatLoop.Length)];
                    while (music == currentMusic)
                    {
                        music = musicSO.combatLoop[Random.Range(0, musicSO.combatLoop.Length)];
                    }
                    return music;
                }
            }
            if (currentMusic == musicSO.calmLoop)
                return musicSO.drop[Random.Range(0, musicSO.drop.Length)];
            else
            //if (MusicIsInArray(currentMusic, musicSO.breaks))
                return musicSO.combatLoop[Random.Range(0, musicSO.combatLoop.Length)];
        }
    }

    void ScriptableObjectSetUp()
    {
        beatManager.bpm = musicSO.bpm;
        beatManager.timingThreshold = musicSO.timingThreshold;
        beatManager.timingThresholdOffset = musicSO.timingThresholdOffset;
        beatManager.minTimeForOnBeatValidation = musicSO.minTimeForOnBeatValidation;
        beatManager.cameraBeatEffectLerpSpeed = musicSO.cameraBeatEffectLerpSpeed;
        beatManager.cameraBeatEffectAmplitude = musicSO.cameraBeatEffectAmplitude;

        beatManager.LoadMusic(musicSO.calmLoop, musicSO.calmMusicStartTimeOffset); /////////// à changer
    }

    bool MusicIsInArray(AudioClip clip, AudioClip[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (clip == array[i])
                return true;
        }
        return false;
    }
}
