using System;
using System.Collections.Generic;

[Serializable]
public struct TileState
{
    public string SpriteName;
    public string Text;
    public float Alpha;
}

[Serializable]
public struct RowState
{
    public List<TileState> Tiles;
}

[Serializable]
public struct KeyState
{
    public string Key;
    public string ColorHex;
}

[Serializable]
public struct BoardState
{
    public List<KeyState> Keyboard;
    public List<RowState> Rows;
    public List<bool> MatchedPositions;
    public List<bool> LettersRevealed;

    public int TimesGuessed;
    public int CurrentRowIndex;
    public bool IsUnlockedDefinition;
}
