using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardSave : SingletonPersistent<BoardSave>
{
    public void SaveBoard()
    {
        if (string.IsNullOrEmpty(Board.Instance.CurrentWord)) return;
        
        BoardState boardState = new()
        {
            Rows = new(),
            TimesGuessed = Board.Instance.TimesGuessed,
            CurrentRowIndex = Board.Instance.CurrentRowIndex,
            IsUnlockedDefinition = Definition.Instance.IsUnlocked(),
            Keyboard = Keyboard.Instance.GetKeyStates(),
            MatchedPositions = new(BoosterManager.Instance.MatchedPositions),
            LettersRevealed = new(BoosterManager.Instance.LettersRevealed)
        };

        foreach (var row in Board.Instance.GetRows())
        {
            RowState rowState = new() { Tiles = new() };

            foreach (var tile in row.GetTiles())
            {
                var cg = tile.GetComponent<CanvasGroup>();
                var img = tile.GetComponent<Image>();
                var text = tile.GetComponentInChildren<TextMeshProUGUI>();

                TileState tileState = new()
                {
                    SpriteName = img.sprite.name,
                    Text = text.text,
                    Alpha = cg.alpha
                };

                rowState.Tiles.Add(tileState);
            }

            boardState.Rows.Add(rowState);
        }

        var path = GetSaveFilePath();
        var json = JsonUtility.ToJson(boardState, true);

        File.WriteAllText(path, json);
        Debug.Log("Board saved to: " + path);
    }

    public BoardState LoadBoardFile()
    {
        var path = GetSaveFilePath();
        if (!File.Exists(path))
        {
            Debug.Log("No board save file found at: " + path);
            return new();
        }

        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<BoardState>(json);
    }

    public void DeleteBoardSave()
    {
        var path = GetSaveFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Board save file deleted at: " + path);
        }
        else
        {
            Debug.Log("No board save file to delete at: " + path);
        }
    }

    private string GetSaveFilePath()
    {
        var gameMode = GameManager.Instance.CurrentGameMode.ToString();
        var fileName = gameMode;

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.ThemedMode)
        {
            fileName += $"_{ThemedWordList.Instance.CurrentTheme}";
        }

        fileName += "_BoardSave.json";
        fileName = fileName.Replace(" ", "");
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
