using System;
using System.Collections.Generic;

[Serializable]
public struct RecordedInputEntry
{
    public float timeStamp; 
    public PlayerInputData inputData; 
}

[Serializable]
public class RecordedInputSession
{
    public List<RecordedInputEntry> entries = new List<RecordedInputEntry>();
}