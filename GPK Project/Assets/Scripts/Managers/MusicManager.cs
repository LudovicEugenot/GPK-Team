using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    #region Initialization
    //Ce manager n'est pas DontDestroyOnLoad donc toutes les infos persistentes sont gardées sur le beatManager
    public MusicSO musicSO;
    BeatManager beatManager;

    private bool playBreak;
    private bool startFlag;
    #endregion

    private void Start()
    {
        startFlag = true;
    }

    private void Update()
    {
        StartManager();

        if (!startFlag && beatManager.onBeatSingleFrame)
        {
            beatManager.currentBarProgression = beatManager.currentBarProgression == beatManager.beatToSwitchTo ? 1 : beatManager.currentBarProgression + 1;
            beatManager.currentSongProgression++;

            if (beatManager.newMusicPlaying)
            {
                beatManager.newMusicPlaying = false;
                LoadNextMusic();
            }

            if (EnemyStatusHasChanged() && musicSO.combatLoop.Length > 0)
            {
                if (!MusicIsInArray(beatManager.currentSongName, musicSO.breaks))
                {
                    LoadNextMusic();
                    ChangeMusicASAP();
                }
            }

            if (CurrentMusicIsPlayingItsLastBar() && beatManager.currentBarProgression == beatManager.beatToSwitchTo)
            {
                if (playBreak)
                {
                    playBreak = false;
                    beatManager.PlayBreakNextBeat();
                    beatManager.currentSongProgression = 0;
                }
                else
                {
                    beatManager.PlayMusicLoadedNextBeat();
                    beatManager.currentSongProgression = 0;
                }
            }

            beatManager.changingMusicZone = false;
        }
    }


    private void StartManager()
    {
        if (ZoneHandler.Instance.zoneInitialized && startFlag)
        {
            startFlag = false;
            beatManager = BeatManager.Instance;
            if (musicSO.name != beatManager.currentMusicSOName || beatManager.currentMusicSOName == "")
            {
                beatManager.changingMusicZone = true;
                beatManager.currentMusicSOName = musicSO.name;
                beatManager.currentSongName = musicSO.calmLoop.name;
                beatManager.currentSongProgression = 0;
                LoadNextMusic();
                beatManager.PlayMusicLoadedNextBeat();
                ScriptableObjectSetUp();
                beatManager.StartNewMusic();
                beatManager.currentEnemyStatus = ZoneHandler.Instance.AllEnemiesConverted();
            }
        }
    }

    void LoadNextMusic()
    {
        string currentMusic = beatManager.currentSongName;
        if (!beatManager.breakLoaded)
        {
            int musicChosen = Random.Range(0, musicSO.breaks.Length);
            beatManager.LoadBreak(musicSO.breaks[musicChosen], musicSO.breakMusicStartTimeOffset[musicChosen]);
        }

        if (ZoneHandler.Instance.AllEnemiesConverted() || musicSO.combatLoop.Length == 0)
        {
            if (MusicIsInArray(currentMusic, musicSO.combatLoop) || MusicIsInArray(currentMusic, musicSO.drops))
            {
                playBreak = true;
                return;
            }
            else
            {
                //if (MusicIsInArray(currentMusic, musicSO.breaks))
                LoadCalmMusicLoop();
                return;
            }
        }
        else
        {
            if (MusicIsInArray(currentMusic, musicSO.combatLoop))
            {
                if (musicSO.combatLoop.Length > 1)
                {
                    int musicChosen = Random.Range(0, musicSO.combatLoop.Length);
                    while (musicSO.combatLoop[musicChosen].name == currentMusic)
                    {
                        musicChosen = Random.Range(0, musicSO.combatLoop.Length);
                    }
                    beatManager.LoadMusic(musicSO.combatLoop[musicChosen], musicSO.combatMusicStartTimeOffset[musicChosen]);
                    return;
                }
                else
                {
                    beatManager.LoadMusic(musicSO.combatLoop[0], musicSO.combatMusicStartTimeOffset[0]);
                    return;
                }
            }
            else if (currentMusic == musicSO.calmLoop.name && musicSO.drops.Length > 0)
            {
                int musicChosen = Random.Range(0, musicSO.drops.Length);
                beatManager.LoadMusic(musicSO.drops[musicChosen], musicSO.dropMusicStartTimeOffset[musicChosen]);
                return;
            }
            else
            {
                //if (MusicIsInArray(currentMusic, musicSO.breaks) || MusicIsInArray(currentMusic, musicSO.drop))
                int musicChosen = Random.Range(0, musicSO.combatLoop.Length);
                beatManager.LoadMusic(musicSO.combatLoop[musicChosen], musicSO.combatMusicStartTimeOffset[musicChosen]);
                return;
            }
        }
    }

    void ScriptableObjectSetUp()
    {
        beatManager.bpm = musicSO.bpm;
        beatManager.beatStartTimeOffset = musicSO.generalStartTimeOffset;
        beatManager.timingThreshold = musicSO.timingThreshold;
        //beatManager.timingThresholdOffset = musicSO.timingThresholdOffset;
        //beatManager.minTimeForOnBeatValidation = musicSO.minTimeForOnBeatValidation;
        //beatManager.cameraBeatEffectLerpSpeed = musicSO.cameraBeatEffectLerpSpeed;
        //beatManager.cameraBeatEffectAmplitude = musicSO.cameraBeatEffectAmplitude;
        beatManager.beatToSwitchTo = musicSO.canSwitchOnBeat2 ? 2 : 4;
        beatManager.changingMusicZone = true;
        beatManager.breakLoaded = false;

    }

    bool MusicIsInArray(string clipName, AudioClip[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (clipName == array[i].name)
                return true;
        }
        return false;
    }

    bool EnemyStatusHasChanged()
    {
        if (beatManager.currentEnemyStatus == ZoneHandler.Instance.AllEnemiesConverted())
            return false;
        beatManager.currentEnemyStatus = ZoneHandler.Instance.AllEnemiesConverted();
        return true;
    }

    void ChangeMusicASAP()
    {
        if (beatManager.currentSongName == musicSO.calmLoop.name)
        {
            beatManager.PlayMusicLoadedNextBeat();
            beatManager.currentSongProgression = 0;
            return;
        }

        beatManager.PlayMusicLoadedInSomeBeats(beatManager.beatToSwitchTo - beatManager.currentBarProgression);
        beatManager.currentSongProgression = beatManager.beatToSwitchTo - beatManager.currentBarProgression - 1;
    }

    bool CurrentMusicIsPlayingItsLastBar() //la musique joue ses derniers 4 beats ou 2 selon beatManager.beatToSwitchTo
    {
        string currentMusic = beatManager.currentSongName;

        if (MusicIsInArray(currentMusic, musicSO.drops))
        {
            if (beatManager.currentSongProgression > 6 - beatManager.beatToSwitchTo)
                return true;
            else
                return false;
        }

        if (MusicIsInArray(currentMusic, musicSO.combatLoop))
        {
            if (beatManager.currentSongProgression > 8 - beatManager.beatToSwitchTo)
                return true;
            else
                return false;
        }

        if (MusicIsInArray(currentMusic, musicSO.breaks))
        {
            if (beatManager.currentSongProgression > 8 - beatManager.beatToSwitchTo)
                return true;
            else
                return false;
        }

        if (currentMusic == musicSO.calmLoop.name)
        {
            if (beatManager.currentSongProgression > musicSO.numberOfBeatsUntilLoop - beatManager.beatToSwitchTo)
                return true;
            else
                return false;
        }

        Debug.LogWarning("La musique " + currentMusic + " n'a pas été trouvée dans le scriptableObject");
        return false;
    }

    void LoadCalmMusicLoop()
    {
        if (beatManager.changingMusicZone)
        {
            beatManager.LoadMusic(musicSO.calmLoop, musicSO.calmMusicStartTimeOffset);
        }
        else
        {
            int rand = Random.Range(0, musicSO.beatsToInsertCalmMusic.Length);
            beatManager.LoadMusic(musicSO.calmLoop, musicSO.calmMusicStartTimeOffset + beatManager.BeatTime * musicSO.beatsToInsertCalmMusic[rand]);
        }
    }
}
