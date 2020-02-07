using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotSound
{
    class EnvelopeGenerator
    {
        private double level = 0.35f; //
        private int sampleRate = 44100;
        private int attack = 1000;//time in samples
        private int decay = 43000;//time in samples
        private double sustain = 0f;//value
        private int release = 0;//time in samples
        private double releaseThetaStart = 0;

        public EnvelopeGenerator(int sampleRate, double level, int attack, int decay, float sustain, int release)
        {
            this.sampleRate = sampleRate;
            this.level = level;
            this.attack = attack;
            this.decay = decay;
            this.sustain = sustain;
            this.release = release;
        }

        public int SampleRate { get => sampleRate; set => sampleRate = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Decay { get => decay; set => decay = value; }
        public double Sustain { get => sustain; set => sustain = value; }
        public int Release { get => release; set => release = value; }
        public double Level { get => level; set => level = value; }

        public double GetLevelAtInterval(double theta, bool gateOn)
        {
            double value = 0f;
            theta = theta + 1;

            if (gateOn)
            {
                //the sound should be emitting
                //so reset the stating point for the release cycle
                releaseThetaStart = 0;
                if (theta <= attack && level > 0)
                {
                    value = (level / attack * theta);
                }

                if (theta <= attack + decay)
                {
                    double s = (level - sustain); //amplitude differential
                    double d = theta - attack; //how far into the decay
                    if (d <= 0) { return sustain; } //cliff to sustain level
                    if (s > 0)
                    {
                        value = level - (s / decay * (d));
                    }
                    else
                    {
                        value = level;
                    }
                }
            } else //we are in release mode
            {
                if (releaseThetaStart == 0) { releaseThetaStart = theta; }
                //release goes to 0 in the number of samples supplied
                if (sustain > 0)
                {
                    //sustain/release is the level differential for each sample
                    value = (sustain / release) * (theta - releaseThetaStart);
                } else
                {
                    value = 0;
                }
            }

            return value;
        }

    }
}
