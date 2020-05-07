using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public int savedZoneBuildIndex;
    public List<ZoneHandler.Zone> worldZones;
    public List<WorldManager.WorldEvent> worldEvents;
    public WorldManager.StoryStep storyStep;

    public WorldData(ZoneHandler zoneHandler)
    {
        worldZones = new List<ZoneHandler.Zone>(zoneHandler.zones);
        worldEvents = new List<WorldManager.WorldEvent>(WorldManager.allWorldEvents);
        storyStep = WorldManager.currentStoryStep;
        savedZoneBuildIndex = zoneHandler.currentZone.buildIndex;
    }
}
