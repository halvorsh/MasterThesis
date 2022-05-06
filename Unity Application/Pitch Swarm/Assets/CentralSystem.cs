using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;

[RequireComponent(typeof(AudioSource))]
public class CentralSystem : MonoBehaviour
{
    public float sampleRate = 22050;
    public float waveLengthInSeconds = 10.0f;

    AudioSource audioSource;
    int timeIndex = 0;

    [Range(21, 108)]
    public int lowestPitch = 21;
    [Range(21, 108)]
    public int highestPitch = 108;

    [Range(3, 10000)]
    public int numAgents = 2000;

    [Range(0.01f, 5f)]
    public float howOftenToUpdate = 0.1f;
    float lastPlayed = 0f;

    Pitch[] pitches;
    Agent[] agents;

    int[] agentHistogram;
    float[] normalizedHistogram;
    Vector3[] colorHistogram;
    float[] midiHistogram;
    float[] lastOutHistogram;

    
    GameObject histDisplay;
    GameObject[] bars;

    public Material barMaterial;

    [Range(0, 50)]
    public int howLongAgentStays = 8;
    int oldHowLongAgentStays;

    [Range(0, 49)]
    public int agentRange = 7;
    int oldAgentRange;

    [Range(0, 49)]
    public int agentLookRange = 7;
    int oldAgentLookRange;

    [Range(0f,10)]
    public float midiImpact = 1;

    [Range(0f, 10)]
    public float swarmVolume = 1;
    [Range(0f, 10)]
    public float midiVolume = 0;

    public bool useMidi = false;
    bool midiChanged = false;
    float midiChangeValue = 0;

    public bool recordingOne = false;
    bool recordOneSwitch = false;
    float recordOneValue = 0;
    public bool recordingAll = false;
    bool recordAllSwitch = false;
    float recordAllValue = 0;
    List<float> timesigs = new List<float>();
    float startOfRecording = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1; //force 2D sound

        histDisplay = transform.GetChild(0).gameObject;

        oldHowLongAgentStays = howLongAgentStays;


        pitches = new Pitch[highestPitch + 1 - lowestPitch];

        bars = new GameObject[pitches.Length];

        float width = 1f / pitches.Length;

        for(int i = lowestPitch; i <= highestPitch; i++)
        {
            pitches[i - lowestPitch] = new Pitch(i);

            float x =  ((i - lowestPitch)-((highestPitch-lowestPitch)/2))*width;

            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.transform.parent = histDisplay.transform;
            bar.transform.localScale = new Vector3(width,0,1);
            bar.transform.localPosition = new Vector3(x,0,-0.5f);
            bar.GetComponent<Renderer>().material = new Material(barMaterial);
            
            bars[i - lowestPitch] = bar;
        }

        agents = new Agent[10000];
        for(int i = 0; i < agents.Length; i++)
        {
            agents[i] = new Agent(howLongAgentStays, lowestPitch, highestPitch, agentRange, agentLookRange) ;
        }

        lastOutHistogram = null;
        createHistograms();

        
    }

    private void Update()
    {
        float curTime = Time.time;
        if (useMidi)
        {
            howOftenToUpdate = MidiMaster.GetKnob(23, 0.1f);
        }

        midiChangeValue = MidiMaster.GetKnob(59, 0);
        if (midiChangeValue > 0.5f && !midiChanged)
        {
            useMidi = !useMidi;
            midiChanged = true;
        }
        else if (midiChangeValue < 0.5f)
        {
            midiChanged = false;
        }

        recordOneValue = MidiMaster.GetKnob(104, 0);
        if (recordOneValue > 0.5f && !recordOneSwitch)
        {
            recordingOne = !recordingOne;
            if (recordingOne == false)
            {
                agents[0].saveRecords("OneRun.txt", timesigs);
            }
            recordOneSwitch = true;
            startOfRecording = curTime;
        }
        else if (recordOneValue < 0.5f)
        {
            recordOneSwitch = false;
        }

        recordAllValue = MidiMaster.GetKnob(105, 0);
        if (recordAllValue > 0.5f && !recordAllSwitch)
        {
            recordingAll = !recordingAll;
            recordAllSwitch = true;
            startOfRecording = curTime;
            if(recordingAll == false)
            {
                for(int i = 0; i < 1000; i++)
                {
                    agents[i].saveRecords(i+".txt", timesigs);
                }
            }
        }
        else if (recordAllValue < 0.5f)
        {
            recordAllSwitch = false;
        }

        if (MidiMaster.GetKnob(51, 0) > 0.5)
        {
            swarmVolume = 0;
            midiVolume = 2;
            howOftenToUpdate = 0.1f;
        }
        else if (MidiMaster.GetKnob(52, 0) > 0.5)
        {
            numAgents = 2000;
            howLongAgentStays = 20;
            agentLookRange = 12;
            swarmVolume = 1;
            midiVolume = 0;
            agentRange = 7;
            howOftenToUpdate = 0.1f;
            midiImpact = 10;
        }
        else if (MidiMaster.GetKnob(53, 0) > 0.5)
        {
            numAgents = 2000;
            howLongAgentStays = 30;
            agentLookRange = 1;
            agentRange = 49;
            swarmVolume = 1;
            howOftenToUpdate = 0.1f;
        }
        else if (MidiMaster.GetKnob(54, 0) > 0.5)
        {
            numAgents = 3;
            swarmVolume = 1;
            howOftenToUpdate = 0.1f;
        }
        else if (MidiMaster.GetKnob(55, 0) > 0.5)
        {
            numAgents = 2000;
            agentRange = 1;
            howLongAgentStays = 0;
            swarmVolume = 1;
            howOftenToUpdate = 0.1f;
        }
        else if (MidiMaster.GetKnob(56, 0) > 0.5)
        {
            numAgents = 2000;
            agentRange = 7;
            howLongAgentStays = 25;
            swarmVolume = 1;
            midiVolume = 0;
            agentLookRange = 49;
            howOftenToUpdate = 0;
            midiImpact = 10;
        }
        else if (MidiMaster.GetKnob(57, 0) > 0.5)
        {
            numAgents = 10000;
            agentRange = 49;
            howLongAgentStays = 0;
            swarmVolume = 2;
            midiVolume = 0;
            agentLookRange = 49;
            howOftenToUpdate = 0;
            midiImpact = 10;
        }
        else if (MidiMaster.GetKnob(58, 0) > 0.5)
        {
            numAgents = 10000;
            agentRange = 1;
            howLongAgentStays = 0;
            swarmVolume = 2;
            midiVolume = 0;
            agentLookRange = 1;
            howOftenToUpdate = 0;
            midiImpact = 10;
        }

        midiHistogram = new float[pitches.Length];
        for (int i = lowestPitch; i <= highestPitch; i++)
        {
            float velocity = MidiMaster.GetKey(0, i);
            int index = i - lowestPitch;
            midiHistogram[index] += velocity;
        }

        if (curTime > lastPlayed + howOftenToUpdate)
        {
            if (useMidi)
            {
                float numAgentknob = MidiMaster.GetKnob(24, 0.7f) * 10;
                numAgents = (int)(numAgentknob * numAgentknob * numAgentknob * numAgentknob);
                if (numAgents < 3)
                {
                    numAgents = 3;
                }
                howLongAgentStays = (int)(MidiMaster.GetKnob(21, 0.2f) * 50);

                midiImpact = MidiMaster.GetKnob(22, 1f) * 10;

                swarmVolume = MidiMaster.GetKnob(25, 0.1f) * 10;
                midiVolume = MidiMaster.GetKnob(26, 0) * 10;

                agentRange = (int)(MidiMaster.GetKnob(27, 0.1f) * 49);
                agentLookRange = (int)(MidiMaster.GetKnob(28, 0.1f) * 49);
            }

            if (howLongAgentStays != oldHowLongAgentStays)
            {
                for (int i = 0; i < numAgents; i++)
                {
                    agents[i].SetProportion(howLongAgentStays);
                }
                oldHowLongAgentStays = howLongAgentStays;
            }

            if (agentRange != oldAgentRange)
            {
                for (int i = 0; i < numAgents; i++)
                {
                    agents[i].SetAgentRange(agentRange);
                }
                oldAgentRange = agentRange;
            }

            if (agentLookRange != oldAgentLookRange)
            {
                for (int i = 0; i < numAgents; i++)
                {
                    agents[i].SetAgentLookRange(agentLookRange);
                }
                oldAgentLookRange = agentLookRange;
            }

            lastPlayed = curTime;

            float[] modifiedHistogram = new float[pitches.Length];
            normalizedHistogram.CopyTo(modifiedHistogram, 0);

            for (int i = 0; i < pitches.Length; i++)
            {
                modifiedHistogram[i] += midiHistogram[i] * midiImpact;
            }

            for (int i = 0; i < numAgents; i++)
            {
                agents[i].Update(modifiedHistogram, colorHistogram);
            }

            createHistograms();

            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].transform.localScale = new Vector3(bars[i].transform.localScale.x,normalizedHistogram[i], bars[i].transform.localScale.z);
                bars[i].GetComponent<Renderer>().material.color = new Color(colorHistogram[i].x, colorHistogram[i].y, colorHistogram[i].z, 1);
            }

            if (recordingOne || recordingAll)
            {
                timesigs.Add(curTime - startOfRecording);
                if (recordingOne)
                {
                    agents[0].recordPosition();
                }
                if (recordingAll)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        agents[i].recordPosition();
                    }
                }
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {

        float[] oldData = new float[data.Length];
        if (lastOutHistogram != null)
        {
            int timeIndexCopy = timeIndex;
            for (int i = 0; i < oldData.Length; i += channels)
            {
                float lSum = 0;
                float rSum = 0;
                for (int p = 0; p < pitches.Length; p++)
                {
                    float proportion = (float)p / (pitches.Length - 1);
                    float sineScore = CreateSine(timeIndex, pitches[p].getFrequency(), sampleRate, normalizedHistogram[p] * swarmVolume + midiHistogram[p] * midiVolume);
                    lSum += sineScore * (1 - proportion);
                    rSum += sineScore * proportion;
                }
                oldData[i] = lSum;
                oldData[i + 1] = rSum;

                timeIndexCopy++;
            }
        }
        
        lastOutHistogram = new float[pitches.Length];
        for (int i = 0; i < data.Length; i += channels)
        {
            
            data[i] = 0;
            float lSum = 0;
            float rSum = 0;
            for(int p = 0; p < pitches.Length; p++)
            {
                float proportion = (float) p / (pitches.Length - 1);
                float sineScore = CreateSine(timeIndex, pitches[p].getFrequency(), sampleRate, normalizedHistogram[p]*swarmVolume + midiHistogram[p]*midiVolume);
                lSum += sineScore * (1-proportion);
                rSum += sineScore * proportion;
                lastOutHistogram[p] = normalizedHistogram[p] * swarmVolume + midiHistogram[p] * midiVolume;
            }


            float filterconst = 1;
            data[i] = lSum*filterconst + oldData[i]*(1-filterconst);
            data[i + 1] = rSum * filterconst + oldData[i+1] * (1-filterconst);

            timeIndex++;
            
            //if timeIndex gets too big, reset it to 0
            /*if (timeIndex >= (sampleRate * waveLengthInSeconds))
            {
                timeIndex = 0;
            }*/
        }
    }

    private void createHistograms()
    {
        
        agentHistogram = new int[pitches.Length];
        normalizedHistogram = new float[pitches.Length];
        colorHistogram = new Vector3[pitches.Length];

        for (int i = 0; i < pitches.Length; i++)
        {
            colorHistogram[i] = new Vector3(0, 0, 0);
        }

        for (int i = 0; i < numAgents; i++)
        {
            int pos = agents[i].GetPos() - lowestPitch;
            agentHistogram[pos] += 1;
            Color color = agents[i].GetColor();
            colorHistogram[pos] += new Vector3(color.r*color.r, color.g*color.g, color.b*color.b);
        }

        int highestNumOfAgents = 0;
        for (int i = 0; i < agentHistogram.Length; i++)
        {
            int num = agentHistogram[i];
            if(num > highestNumOfAgents)
            {
                highestNumOfAgents = num;
            }
        }

        //string debugString = "";
        for (int i = 0; i < normalizedHistogram.Length; i++)
        {
            normalizedHistogram[i] = (float)agentHistogram[i] / highestNumOfAgents;
            //debugString += normalizedHistogram[i] + " ";
            colorHistogram[i] /= agentHistogram[i];
            colorHistogram[i] = new Vector3(Mathf.Sqrt(colorHistogram[i].x), Mathf.Sqrt(colorHistogram[i].y), Mathf.Sqrt(colorHistogram[i].z));
        }
        //Debug.Log(debugString);*/

        
    }

    //Creates a sinewave
    public float CreateSine(int timeIndex, float frequency, float sampleRate, float gain)
    {
        if (gain > 0)
        {
            float sum = 0;
            sum += Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate) * (gain * gain);
            sum += Mathf.Sin(2 * Mathf.PI * timeIndex * (frequency*2) / sampleRate) * (gain * gain) / 2;
            return sum;
        }
        return 0;
    }

}
