using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace IotSound
{
    public sealed class MidiMessage
    {
        private byte status;
        private byte data1;
        private Boolean isSet1;
        private byte data2;
        private Boolean isSet2;
        private int channel;
        //7 collections of 16 items (1 per channel)

        public static int CalculateChannel(int theStatus)
        {
            Decimal n = (theStatus - 128);
            return (int)(n - (Decimal.Truncate(n / 16) * 16));
        }

        public MidiMessage()
        {
           
        }

        public MidiMessage(byte status)
        {
            Status = status;
            Data1 = 0x0;
            Data2 = 0x0;
            isSet1 = false;
            isSet2 = false;
            //Set the Channel
            channel = CalculateChannel(status);

        }

        public byte Status { 
            get => status; 
            set { 
                status = value;
                channel = CalculateChannel(status);
            }
        }
        public byte Data1 { 
            get => data1;
            set { data1 = value; isSet1 = true; }
        }
        public byte Data2 { 
            get => data2;
            set { 
                data2 = value;
                isSet2 = true;
            } 
        }
        public int Channel { get => channel; }
        public bool IsSet1 { get => isSet1;}
        public bool IsSet2 { get => isSet2;}

        public override string ToString()
        {
            return status.ToString("x2") + "-" + data1.ToString("x2") + "-" + data2.ToString("x2");
        }
    }
}
