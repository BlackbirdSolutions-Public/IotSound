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
        public EnvelopeState state;
        
        public enum EnvelopeState
        {
            Init = 0,
            Attack,
            Decay,
            Sustain,
            Release
        }

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
        public bool Gate
        {
            get => gate;
            set
            {
                gate = value;
                state = EnvelopeState.Init;
            }
        }

        public double GetLevelAtInterval(int SampleIndex)
        {
            if (gate)
            {
                releaseSampleStart = 0; //while gate is on, we reset this to zero
                switch (state)
                {
                    case EnvelopeState.Init:
                    case EnvelopeState.Attack:
                        //set state to attack;
                        state = EnvelopeState.Attack;
                        if (SampleIndex>=attack) //handles a 0 attack value
                        {
                            releaseLevel = level; //max level
                            state = EnvelopeState.Decay; //move to next state
                        } else if (attack > 0)
                        {
                            releaseLevel = (level / attack) * SampleIndex;
                        }
                        break;
                    case EnvelopeState.Decay:
                        if (SampleIndex <= attack + decay)
                        {
                            //what to do during decay and sustain
                            double s = (level - sustain); //amplitude differential
                            double d = SampleIndex - attack; //how far into the decay
                            if (d <= 0) { return sustain; } //cliff to sustain level
                            if (s > 0)
                            {
                                releaseLevel = level - (s / decay * (d));
                            }
                            else
                            {
                                releaseLevel = sustain;
                            }
                        }
                        else
                        {
                            state = EnvelopeState.Sustain; //changing state
                            releaseLevel = sustain;
                        }
                        break;
                    case EnvelopeState.Sustain:
                        releaseLevel = sustain;
                        break;
                    default:
                        break;    
                }
                return releaseLevel;
            } else //gate is off
            {
                state = EnvelopeState.Release;
                //entering this block for the first time
                if (releaseSampleStart == 0) { 
                    releaseSampleStart = SampleIndex; 
                }
                //release goes to 0 in the number of samples supplied
                if (SampleIndex > 0 && sustain > 0)
                {
                    //sustain/release is the level differential for each sample
                    return releaseLevel - (releaseLevel / release) * (SampleIndex - releaseSampleStart);
                } else
                {
                    return 0;
                }
                
            }
        }

    }
}
