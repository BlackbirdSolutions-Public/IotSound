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
        private int messageClass;

        private static readonly int cLASS_NOTE_OFF = 0;
        private static readonly int cLASS_NOTE_ON = 1;
        private static readonly int cLASS_POLY_AFTERTOUCH = 2;
        private static readonly int cLASS_CONTROL_CHANGE = 3;
        private static readonly int cLASS_PROGRAM_CHANGE = 4;
        private static readonly int cLASS_CHANNEL_AFTERTOUCH = 5;
        private static readonly int cLASS_PITCH_BEND = 6;
        private static readonly int cLASS_OTHER = 7;

        //7 collections of 16 items (1 per channel)
        public static int CalculateChannel(int theStatus)
        {
            Decimal n = (theStatus - 128);
            return (int)(n - (Decimal.Truncate(n / 16) * 16));
        }
        public static int CalculateMessageClass(int theStatus)
        {
            return ( theStatus > 223 ? CLASS_PITCH_BEND 
                    : theStatus > 207 ? CLASS_CHANNEL_AFTERTOUCH 
                    : theStatus > 191 ? CLASS_PROGRAM_CHANGE
                    : theStatus > 175 ? CLASS_CONTROL_CHANGE
                    : theStatus > 159 ? CLASS_POLY_AFTERTOUCH
                    : theStatus > 143 ? CLASS_NOTE_ON
                    : theStatus > 127 ? CLASS_NOTE_OFF
                    : CLASS_OTHER
                    );
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
            messageClass = CalculateMessageClass(status);

        }

        public byte Status { 
            get => status; 
            set { 
                status = value;
                channel = CalculateChannel(status);
                messageClass = CalculateMessageClass(status);
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
        public int MessageClass { get => messageClass; set => messageClass = value; }

        public static int CLASS_NOTE_OFF => cLASS_NOTE_OFF;

        public static int CLASS_NOTE_ON => cLASS_NOTE_ON;

        public static int CLASS_POLY_AFTERTOUCH => cLASS_POLY_AFTERTOUCH;

        public static int CLASS_CONTROL_CHANGE => cLASS_CONTROL_CHANGE;

        public static int CLASS_PROGRAM_CHANGE => cLASS_PROGRAM_CHANGE;

        public static int CLASS_CHANNEL_AFTERTOUCH => cLASS_CHANNEL_AFTERTOUCH;

        public static int CLASS_PITCH_BEND => cLASS_PITCH_BEND;

        public static int CLASS_OTHER => cLASS_OTHER;

        public override string ToString()
        {
            return status.ToString("x2") + "-" + data1.ToString("x2") + "-" + data2.ToString("x2");
        }
    }
}
