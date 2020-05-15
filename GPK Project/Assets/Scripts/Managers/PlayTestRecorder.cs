using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public static class PlayTestRecorder
{
    public static TimingRecords records = new TimingRecords();
    public static List<ZoneRecord> zoneRecords = new List<ZoneRecord>();
    public static ZoneRecord currentZoneRecord;
    public static string directory = "/Playtest_Records";
    public static string gameDirectory = "/ColorBeat";
    public static string fileName = "/timing_record_";
    public static string spreadSheetFileName = "/timing_record_spreadSheet_";
    public static string zoneRecordFileName = "/zone_record_";

    public static void RefreshCurrentZone(string zoneName)
    {
        bool newZonePreexisting = false;
        for (int i = 0; i < zoneRecords.Count; i++)
        {
            if (zoneRecords[i].zoneName == zoneName)
            {
                currentZoneRecord = zoneRecords[i];

                newZonePreexisting = true;
            }
        }

        if(!newZonePreexisting)
        {
            currentZoneRecord = new ZoneRecord(GameManager.Instance.zoneName);
        }
    }

    public static void SaveCurrentZone()
    {
        bool lastZonePreexisting = false;
        for (int i = 0; i < zoneRecords.Count; i++)
        {
            if (zoneRecords[i].zoneName == currentZoneRecord.zoneName)
            {
                zoneRecords[i] = currentZoneRecord;
                lastZonePreexisting = true;
            }
        }

        if (!lastZonePreexisting)
        {
            zoneRecords.Add(currentZoneRecord);
        }
    }

    public static void CreateZoneRecordFile()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games";
        if (!Directory.Exists(path))
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

        string text = "         Zone records    :";
        foreach(ZoneRecord zoneRecord in zoneRecords)
        {
            text += Environment.NewLine + Environment.NewLine;
            text += "      " + zoneRecord.zoneName + " :" + Environment.NewLine;
            text += "Time spent : " + zoneRecord.timeSpent + " secondes" + Environment.NewLine;
            text += "Lore interacted : ";
            foreach(string lore in zoneRecord.loreHolderInteracted)
            {
                text += Environment.NewLine + "   - " + lore;
            }
        }

        int recordNumber = 0;
        while (File.Exists(path + zoneRecordFileName + recordNumber + ".txt"))
        {
            recordNumber++;
        }
        File.WriteAllText(path + zoneRecordFileName + recordNumber + ".txt", text);

    }

    public static void CreateTimingRecordFiles()
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

        string spreadSheet = "Action;OnBeat;Offset;Bpm;InCombat;Zone";

        string text = "         General timing stats :" + Environment.NewLine + Environment.NewLine;
        text += "Story step : " + records.storyStep + Environment.NewLine;
        text += "On Beat/Miss Ratio : " + records.onBeatNumber + "/" + records.allRecords.Count + "  >  " + Math.Round(records.onBeatRatio * 100) + "%" + Environment.NewLine;
        text += "Average timing offset : " + records.offsetAverage + Environment.NewLine + Environment.NewLine;

        text += "Blink on beat Ratio : " + records.blinkOnBeatNumber + "/" + records.blinkNumber + "  >  " + Math.Round(records.blinkOnBeatRatio * 100) + "%" + Environment.NewLine;
        text += "Blink average timing offset : " + records.blinkOffsetAverage + Environment.NewLine + Environment.NewLine;

        text += "Attack on Beat Ratio : " + records.attackOnBeatNumber + "/" + records.attackNumber + "  >  " + Math.Round(records.attackOnBeatRatio * 100) + "%" + Environment.NewLine;
        text += "Attack average timing offset : " + records.attackOffsetAverage + Environment.NewLine + Environment.NewLine;

        text += "          All timing record :";
        foreach(TimingRecord record in records.allRecords)
        {
            text += Environment.NewLine+ Environment.NewLine;
            text += record.actionName + " > " + (record.onBeat ? "On Beat" : "Missed") + " : " + record.playerOffsetWithTiming.ToString();
            text += Environment.NewLine;
            text += "Bpm : " + record.musicBpm + "  -  " + (record.inCombat ? "In combat" : "Not in combat") + "  >  " + record.zone;

            spreadSheet += Environment.NewLine + record.actionName + ";" + record.onBeat + ";" + record.playerOffsetWithTiming + ";" + record.musicBpm + ";" + record.inCombat + ";" + record.zone;
        }

        int recordNumber = 0;
        while (File.Exists(path + fileName + recordNumber + ".txt"))
        {
            recordNumber++;
        }
        File.WriteAllText(path + fileName + recordNumber + ".txt", text);
        File.WriteAllText(path + spreadSheetFileName + recordNumber + ".csv", spreadSheet);
    }

    public static void ClearRecords()
    {
        records = new TimingRecords();
    }

    public static void CalculateRecordsGeneralStats()
    {
        int onBeatNumber = 0;
        int blinkNumber = 0;
        int attackNumber = 0;
        int onBeatBlinkNumber = 0;
        int onBeatAttackNumber = 0;
        double offsetAddition = 0;
        double blinkOffsetAddition = 0;
        double attackOffsetAddition = 0;
        foreach(TimingRecord record in records.allRecords)
        {
            if (record.onBeat)
                onBeatNumber++;

            offsetAddition += record.playerOffsetWithTiming;

            if (record.actionName == "Blink")
            {
                blinkNumber++;
                blinkOffsetAddition += record.playerOffsetWithTiming;
                if(record.onBeat)
                {
                    onBeatBlinkNumber++;
                }
            }
            else if (record.actionName == "AttackRelease")
            {
                attackOffsetAddition += record.playerOffsetWithTiming;
                attackNumber++;
                if (record.onBeat)
                {
                    onBeatAttackNumber++;
                }
            }
        }

        records.blinkNumber = blinkNumber;
        records.attackNumber = attackNumber;
        records.blinkOnBeatNumber = onBeatBlinkNumber;
        records.attackOnBeatNumber = onBeatAttackNumber;
        records.blinkOnBeatRatio = (float)onBeatBlinkNumber / (float)blinkNumber;
        records.attackOnBeatRatio = (float)onBeatAttackNumber / (float)attackNumber;
        records.blinkOffsetAverage = blinkOffsetAddition / blinkNumber;
        records.attackOffsetAverage = attackOffsetAddition / attackNumber;

        records.onBeatNumber = onBeatNumber;
        records.offsetAverage = offsetAddition / records.allRecords.Count;
        records.onBeatRatio = (float)onBeatNumber / (float)records.allRecords.Count;
        records.storyStep = WorldManager.currentStoryStep.ToString();
    }

    [System.Serializable]
    public class ZoneRecord
    {
        public string zoneName;
        public float timeSpent;
        public List<string> loreHolderInteracted;

        public ZoneRecord(string name)
        {
            zoneName = name;
            timeSpent = 0;
            loreHolderInteracted = new List<string>();
        }
    }

    [System.Serializable]
    public class TimingRecords
    {
        public List<TimingRecord> allRecords = new List<TimingRecord>();
        public float onBeatRatio;
        public int onBeatNumber;
        public double offsetAverage;

        public int blinkNumber;
        public float blinkOnBeatRatio;
        public int blinkOnBeatNumber;
        public double blinkOffsetAverage;

        public int attackNumber;
        public float attackOnBeatRatio;
        public int attackOnBeatNumber;
        public double attackOffsetAverage;

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
        public string zone;
    }
}
