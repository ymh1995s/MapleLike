using System.Collections.Generic;

public enum ApName
{
    STR,
    DEX,
    INT,
    LUK,
}

public class AbilityPoint
{
    List<int> ap = new List<int>() { 10, 10, 10, 10 };
    
    public List<int> Ap 
    { 
        get { return ap; }
        set { value = ap; }
    }
}
