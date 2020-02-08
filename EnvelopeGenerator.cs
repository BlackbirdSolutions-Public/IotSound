using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotSound
{
    class EnvelopeGenerator
    {
        //here is how this works:
        //if the gate is off and theta = 0; then the wave generator is idle.
        //we would always return an amplitude of 0
        //If the gate is on then the amplitude is determined by a function
        //of theta (Number of samples from the start) and the max amplitude
        //

        private double level = 0.35f; //
        private int sampleRate = 44100;
        private int attack = 1000;//time in samples
        private int decay = 43000;//time in samples
        private double sustain = 0f;//value
        private double releaseLevel = 0f;//value
        private int release = 0;//time in samples
        private double releaseSampleStart = 0;
        private bool gate = false;

        public EnvelopeGenerator()
        {
            this.sampleRate = 44100;
            this.level = 0.35f;
            this.attack = 0;
            this.decay = 0;
            this.sustain = 0.35;
            this.release = 0;
        }

        public int SampleRate { get => sampleRate; set => sampleRate = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Decay { get => decay; set => decay = value; }
        public double Sustain { get => sustain; set => sustain = value; }
        public int Release { get => release; set => release = value; }
        public double Level { get => level; set => level = value; }
        public bool Gate { get => gate; set => gate = value; }

        public double GetLevelAtInterval(int SampleIndex)
        {
            if (gate)
            {
                //the sound should be emitting
                //so reset the stating point for the release cycle
                releaseSampleStart = 0;
                if (SampleIndex <= attack && level > 0)
                {
                   releaseLevel = level / attack * SampleIndex;
                    return releaseLevel;
                }
                // decay and sustain
                if (SampleIndex <= attack + decay)
                {
                    double s = (level - sustain); //amplitude differential
                    double d = SampleIndex - attack; //how far into the decay
                    if (d <= 0) { return sustain; } //cliff to sustain level
                    if (s > 0) {
                        releaseLevel = level - (s / decay * (d));
                        return releaseLevel;
                    }
                    else  {
                        releaseLevel = sustain;
                        return releaseLevel;
                    }
                } else {
                    releaseLevel =  sustain;
                    return releaseLevel;
                }
            } else //gate is off
            {
                //entering this block for the first time
                if (releaseSampleStart == 0) { 
                    releaseSampleStart = SampleIndex; 
                }
                //release goes to 0 in the number of samples supplied
                if (SampleIndex > 0 && sustain > 0)
                {
                    //sustain/release is the level differential for each sample
                    return sustain - (sustain / release) * (SampleIndex - releaseSampleStart);
                } else
                {
                    return 0;
                }
                
            }
        }

    }
}
