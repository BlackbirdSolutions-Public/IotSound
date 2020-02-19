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
        private int pitchBendValue = 0;

        public void SetPitchBendValue(int LSB, int MSB)
        {
            int newValue = (MSB << 7) | LSB;
            pitchBendValue = newValue - 8192;
        }

        public int PitchBendNoteRadius { get => pitchBendNoteRadius; set => pitchBendNoteRadius = value; }
        public int PitchBendValue { get => pitchBendValue;}

        public float getKeyFrequency(int KeyNumber)
        {
            float freq = 440 / 32f * (float)Math.Pow(2f, ((float)KeyNumber - ((float)pitchBendNoteRadius / 8192f * ((float)pitchBendValue)) - 9f) / 12f);
            return freq;
        }
        public int getKeyPitch(int KeyNumber)
        {
            int basePitch = (300 + (100 * KeyNumber)); 
            float offset  = ((100f * pitchBendNoteRadius)/8192f) * PitchBendValue;

            return basePitch + (int)offset;
        }
    }
}
