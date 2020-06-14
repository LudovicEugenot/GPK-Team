using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutiveAppearence : MonoBehaviour
{
    public List<GameObject> goToDisable;

    public WorldManager.StoryStep firstStoryStep = WorldManager.StoryStep.Tutorial;
    public WorldManager.StoryStep LastStoryStep = WorldManager.StoryStep.EndGame;

    public List<WorldManager.EventName> eventsRequired;
    public List<WorldManager.EventName> eventsCompromising;

    void Update()
    {
        if (!(WorldManager.currentStoryStep >= firstStoryStep
            && WorldManager.currentStoryStep <= LastStoryStep))
        {
            SetActiveThings(false);
        }
        else
        {
            bool valid = true;
            foreach(WorldManager.EventName worldEvent in eventsRequired)
            {
                if(!WorldManager.GetWorldEvent(worldEvent).occured)
                {
                    valid = false;
                }
            }
            foreach (WorldManager.EventName worldEvent in eventsCompromising)
            {
                if (WorldManager.GetWorldEvent(worldEvent).occured)
                {
                    valid = false;
                }
            }
            SetActiveThings(valid);
        }
    }

    private void SetActiveThings(bool active)
    {
        foreach(GameObject thing in goToDisable)
        {
            thing.SetActive(active);
        }
    }
}
