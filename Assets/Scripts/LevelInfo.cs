using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelInfo {
    Dictionary<string, string> validCombos = new Dictionary<string, string>();
    public int lastSpawnable;
    public int firstComplete;
    public int indexOfFirstElement;
    public List<comboInfo>levelCombos=new List<comboInfo>();
    public LevelInfo()
    {
        

    }
}
public struct comboInfo {
    public int firstItem, secondItem, result;
    public comboInfo(int first, int second, int res)
    {
        this.firstItem = first;
        this.secondItem = second;
        this.result = res;
    }
}