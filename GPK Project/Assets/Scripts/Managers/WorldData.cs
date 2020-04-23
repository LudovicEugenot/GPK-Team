using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public int savedZoneBuildIndex;
    public List<ZoneHandler.Zone> worldZones;
    public List<WorldManager.WorldEvent> worldEvents;

    public WorldData(ZoneHandler zoneHandler)
    {
        worldZones = zoneHandler.zones;
        worldEvents = WorldManager.allWorldEvents;
        savedZoneBuildIndex = zoneHandler.currentZone.buildIndex;
    }
}
