using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotSound
{
    class Keyboard
    {
        private int pitchBendNoteRadius = 0;
        private int pitchBendValue = 8192;

        public void SetPitchBendValue(int LSB, int MSB)
        {
            int newValue = (MSB << 7) | LSB;
            pitchBendValue = newValue;
        }

        public int PitchBendNoteRadius { get => pitchBendNoteRadius; set => pitchBendNoteRadius = value; }
        public int PitchBendValue { get => pitchBendValue;}

        public float getKeyFrequency(int KeyNumber)
        {
            float freq = 440 / 32f * (float)Math.Pow(2f, ((float)KeyNumber - ((float)pitchBendNoteRadius / 8192f * (8192f - (float)pitchBendValue)) - 9f) / 12f);
            return freq;
        }
    }
}
