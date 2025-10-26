using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LadderSave : SingletonPersistent<LadderSave>
{
    public void SaveLadder()
    {
        var (frameStates, tileStates, isAllTileRevealed, isMatchWord) = LadderMode.Instance.GetLadderState();

        LadderState ladderState = new()
        {
            FrameStates = frameStates,
            TileStates = tileStates,
            IsAllTileRevealed = isAllTileRevealed,
            IsMatchWord = isMatchWord
        };

        var path = $"{Application.persistentDataPath}/LadderMode.json";
        var json = JsonUtility.ToJson(ladderState, true);

        File.WriteAllText(path, json);
        Debug.Log("LadderState saved to: " + path);
    }

    public LadderState LoadLadderFile()
    {
        var path = $"{Application.persistentDataPath}/LadderMode.json";
        if (!File.Exists(path))
        {
            Debug.Log("No ladder save file found at: " + path);
            return new();
        }

        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<LadderState>(json);
    }

    public void DeleteLadderSave()
    {
        var path = $"{Application.persistentDataPath}/LadderMode.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Ladder save file deleted at: " + path);
        }
        else
        {
            Debug.Log("No Ladder save file to delete at: " + path);
        }
    }
}

[Serializable]
public struct LadderState
{
    public List<bool> FrameStates;
    public List<TileLadder> TileStates;

    public bool IsAllTileRevealed;
    public bool IsMatchWord;
}

[Serializable]
public struct TileLadder
{
    public string sprite;
    public string letter;
}