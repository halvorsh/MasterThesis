using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Agent
{
    int pitchPosition;
    float remainAtSame = 0;
    //int[] allowedMoves;
    int howLongToStayProportionalToStrentgh;
    int lowestPitch;
    int highestPitch;
    List<int> positionHistory = new List<int>();

    Color color = new Color(Random.Range(0,1f), Random.Range(0, 1f), Random.Range(0, 1f),1);

    int agentRange;
    int agentLookRange;

    public Agent(int proportion, int lowestPitch, int highestPitch, int agentRange, int agentLookRange)
    {
        /*allowedMoves = new int[3];
        for(int i = 0; i < allowedMoves.Length; i++)
        {
            allowedMoves[i] = Random.Range(0, 8);
        }*/

        howLongToStayProportionalToStrentgh = proportion;
        this.lowestPitch = lowestPitch;
        this.highestPitch = highestPitch;
        pitchPosition = Random.Range(lowestPitch, highestPitch + 1);

        this.agentRange = agentRange;
        this.agentLookRange = agentLookRange;
    }

    public void Update(float[] histogram, Vector3[] colHistogram)
    {
        int index = pitchPosition - lowestPitch;
        

        int desiredIndex = index;
        if (remainAtSame > histogram[desiredIndex] * howLongToStayProportionalToStrentgh)
        {
            
            float leastWeight = 2;
            for (int i = index - agentLookRange; i <= index + agentLookRange; i++)
            {
                int j = i;
                if (j < 0)
                {
                    j += histogram.Length;
                }
                else if (j >= histogram.Length)
                {
                    j -= histogram.Length;
                }
                float weight = histogram[j];
                if (weight < leastWeight)
                {
                    desiredIndex = j;
                    leastWeight = weight;
                }
            }
        }
        else if(Random.Range(0,1f) < 0.1f)
        {
            float bestWeight = -1;

            for (int i = index - agentLookRange; i <= index + agentLookRange; i++)
            {
                int j = i;
                if (j < 0)
                {
                    j += histogram.Length;
                }
                else if (j >= histogram.Length)
                {
                    j -= histogram.Length;
                }
                float weight = histogram[j];
                if (weight > bestWeight)
                {
                    desiredIndex = j;
                    bestWeight = weight;
                }
            }

            /*for (int i = 0; i < histogram.Length; i++)
            {
                float weight = histogram[i];
                if (weight > bestWeight)
                {
                    desiredIndex = i;
                    bestWeight = weight;
                }
            }*/
        }


        if (pitchPosition-lowestPitch == desiredIndex)
        {
            remainAtSame+= histogram[desiredIndex];
            //Debug.Log(remainAtSame + "," + pitchPosition);
        }
        else
        {
            if (agentRange != 0)
            {
                remainAtSame = 0;

                int move_to_make = Random.Range(1, agentRange + 1); //allowedMoves[Random.Range(0, allowedMoves.Length)];

                if (desiredIndex + lowestPitch > pitchPosition)
                {
                    pitchPosition += move_to_make;
                    if (pitchPosition > highestPitch)
                    {
                        pitchPosition -= histogram.Length;
                    }
                }
                else
                {
                    pitchPosition -= move_to_make;
                    if (pitchPosition < lowestPitch)
                    {
                        pitchPosition += histogram.Length;
                    }
                }
            }
        }
    }

    public int GetPos()
    {
        return pitchPosition;
    }

    public void SetProportion(int proportion)
    {
        howLongToStayProportionalToStrentgh = proportion;
    }

    public void SetAgentRange(int range)
    {
        agentRange = range;
    }

    public void SetAgentLookRange(int range)
    {
        agentLookRange = range;
    }

    public Color GetColor()
    {
        return color;
    }

    public void recordPosition()
    {
        positionHistory.Add(pitchPosition);
    }

    public void saveRecords(string filename, List<float> timesigs)
    {
        using (StreamWriter writer = new StreamWriter("output/"+filename))
        {
            for (int i = 0; i < positionHistory.Count; i++)
            {
                writer.WriteLine(timesigs[i] + " " + positionHistory[i]);
            }
        }
    }
}
