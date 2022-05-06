using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitch
{
    float freqency;

    public Pitch(int midiNumber)
    {
        freqency = Mathf.Pow(2, (midiNumber - 69) / 12f) * 440;
    }

    public float getFrequency()
    {
        return freqency;
    }
}
