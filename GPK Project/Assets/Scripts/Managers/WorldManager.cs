using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public static class WorldManager
{
    public enum StoryStep {Tutorial1, FirstFreedom2, VillageArrival3, VillageConverted4}

    /// <summary>
    /// To add an event you need to open the WorldManager script and add a name to the enum list
    /// </summary>
    public enum EventName {NullEvent, RemoteDestruction, ShortCut1, TalkedToLuthier1} // Add an event in there

    public static StoryStep currentStoryStep;

    public static List<WorldEvent> allWorldEvents = new List<WorldEvent>();

    [System.Serializable]
    public class WorldEvent
    {
        public EventName name;
        public bool occured;

        public WorldEvent(EventName _name)
        {
            name = _name;
            occured = false;
        }
    }

    public static WorldEvent GetWorldEvent(WorldManager.EventName eventName)
    {
        WorldEvent worldEvent = null;
        int i = 0;
        while(worldEvent == null && i < allWorldEvents.Count)
        {
            if(eventName == allWorldEvents[i].name)
            {
                worldEvent = allWorldEvents[i];
            }
            i++;
        }

        return worldEvent;
    }

    public static void InitializeWorldEvents()
    {
        for(int i = 0; i < Enum.GetNames(typeof(EventName)).Length; i++)
        {
            allWorldEvents.Add(new WorldEvent((EventName)i));
        }
    }
}
