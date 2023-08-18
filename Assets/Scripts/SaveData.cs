using System;
using System.Collections.Generic;

[Serializable]
public class SaveData : IComparable
{
    public int score;
    public string name;

    public SaveData(int score, string name)
    {
        this.score = score;
        this.name  = name;
    }

    public int CompareTo(object arg)
    {
        if (score < ((SaveData)arg).score) return -1;

        return 1;
    }
}

[Serializable]
public class SaveInfo
{
    public List<SaveData> list;
}