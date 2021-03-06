﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace IotSound
{
    public enum MsgClass
    {
        NOTE_OFF = 0,
        NOTE_ON,
        POLY_AFTERTOUCH,
        CONTROL_CHANGE,
        PROGRAM_CHANGE,
        CHANNEL_AFTERTOUCH,
        PITCH_BEND,
        SYSEX,
        QUARTER_FRAME,
        SONG_POSITION_POINTER,
        SONG_SELECT,
        TUNE_REQUEST,
        SYSEX_END,
        TIME_CLOCK,
        START,
        CONTINUE,
        STOP,
        ACTIVE_SENSING,
        SYSTEM_RESET,
        OTHER,
    }

    public sealed class MidiMessage
    {
        private byte status;
        private byte data1;
        private Boolean isSet1;
        private byte data2;
        private Boolean isSet2;
        private int channel;
        private MsgClass messageClass;

        //7 collections of 16 items (1 per channel)
        public static int CalculateChannel(int theStatus)
        {
            Decimal n = (theStatus - 128);
            return (int)(n - (Decimal.Truncate(n / 16) * 16));
        }
        public static MsgClass CalculateMessageClass(int theStatus)
        {
            return (theStatus == 255 ? MsgClass.ACTIVE_SENSING
                    : theStatus == 254 ? MsgClass.ACTIVE_SENSING
                    : theStatus > 223 ? MsgClass.PITCH_BEND 
                    : theStatus > 207 ? MsgClass.CHANNEL_AFTERTOUCH 
                    : theStatus > 191 ? MsgClass.PROGRAM_CHANGE
                    : theStatus > 175 ? MsgClass.CONTROL_CHANGE
                    : theStatus > 159 ? MsgClass.POLY_AFTERTOUCH
                    : theStatus > 143 ? MsgClass.NOTE_ON
                    : theStatus > 127 ? MsgClass.NOTE_OFF
                    : MsgClass.OTHER
                    );
        }

        public MidiMessage()
        {
           
        }

        public static int ByteCount(MsgClass theClass)
        {
            int theCount = 0;
            switch(theClass)
            {
                case MsgClass.NOTE_OFF:
                case MsgClass.NOTE_ON:
                case MsgClass.CONTROL_CHANGE:
                case MsgClass.POLY_AFTERTOUCH:
                case MsgClass.PITCH_BEND:
                case MsgClass.SONG_POSITION_POINTER:
                    theCount = 2;
                    break;
                case MsgClass.PROGRAM_CHANGE:
                case MsgClass.CHANNEL_AFTERTOUCH:
                case MsgClass.SONG_SELECT:
                    theCount = 1;
                    break;
                default:
                    break;
            }
            return theCount;
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
        public MsgClass MessageClass { get => messageClass; set => messageClass = value; }

        public override string ToString()
        {
            return status.ToString("x2") + "-" + data1.ToString("x2") + "-" + data2.ToString("x2");
        }
    }
}
