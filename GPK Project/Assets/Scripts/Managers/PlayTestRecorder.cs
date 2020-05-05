using System.Collections.Generic;
using System.IO;
using System;

public static class PlayTestRecorder
{
    public static AllRecords records = new AllRecords();
    public static string directory = "/Playtest_Records";
    public static string gameDirectory = "/ColorBeat";
    public static string fileName = "/timing_record_";

    public static void CreateTimingRecordFile()
    {
        CalculateRecordsGeneralStats();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games";
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path += gameDirectory;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path += directory;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string text = "         General timing stats :" + Environment.NewLine + Environment.NewLine;
        text += "Story step : " + records.storyStep + Environment.NewLine;
        text += "On Beat/Miss Ratio : " + records.onBeatNumber + "/" + records.allRecords.Count + "  >  " + Math.Round(records.onBeatRatio * 100) + "%" + Environment.NewLine;
        text += "Average timing offset : " + records.offsetAverage + Environment.NewLine + Environment.NewLine;
        text += "          All timing record :";
        foreach(TimingRecord record in records.allRecords)
        {
            text += Environment.NewLine+ Environment.NewLine;
            text += record.actionName + " > " + (record.onBeat ? "On Beat" : "Missed") + " : " + record.playerOffsetWithTiming.ToString();
            text += Environment.NewLine;
            text += "Bpm : " + record.musicBpm + "  -  " + (record.inCombat ? "In combat" : "Not in combat");
        }

        int recordNumber = 0;
        while (File.Exists(path + fileName + recordNumber + ".txt"))
        {
            recordNumber++;
        }
        File.WriteAllText(path + fileName + recordNumber + ".txt", text);
    }

    public static void ClearRecords()
    {
        records = new AllRecords();
    }

    public static void CalculateRecordsGeneralStats()
    {
        int onBeatNumber = 0;
        double offsetAddition = 0;
        foreach(TimingRecord record in records.allRecords)
        {
            if (record.onBeat)
                onBeatNumber++;

            offsetAddition += record.playerOffsetWithTiming;
        }
        records.onBeatNumber = onBeatNumber;
        records.offsetAverage = offsetAddition / records.allRecords.Count;
        records.onBeatRatio = (float)onBeatNumber / (float)records.allRecords.Count;
        records.storyStep = WorldManager.currentStoryStep.ToString();
    }

    [System.Serializable]
    public class AllRecords
    {
        public List<TimingRecord> allRecords = new List<TimingRecord>();
        public float onBeatRatio;
        public int onBeatNumber;
        public double offsetAverage;
        public string storyStep;
    }

    [System.Serializable]
    public class TimingRecord
    {
        public bool onBeat;
        public double playerOffsetWithTiming;
        public string actionName;
        public float musicBpm;
        public bool inCombat;
    }
}
