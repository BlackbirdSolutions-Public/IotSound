using System;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IotSound
{

    public sealed class SoftSynth //default was 'internal class'
    {
        private AudioGraph graph;
        private int pitchBendNoteRadius = 0; //number of +- notes that will span 00-127 values
        private int pitchBendValue = 63; // 63 is the 0 point for pitch bend
        private AudioDeviceOutputNode deviceOutputNode;
        private WaveGenerator wg1;
        private WaveGenerator wg2;
        private WaveGenerator wg3;
        private Keyboard kb;

        private readonly float[] keyboard = new float[127];
        private int[] notes = new int[127];

        public int PitchBendNoteRadius { get => pitchBendNoteRadius; set => pitchBendNoteRadius = value; }
        public int PitchBendValue { get => pitchBendValue; set => pitchBendValue = value; }

        public SoftSynth()
        {
            InitializeMe();
        }

        private void InitializeMe()
        {
            //a virtual 128 note keyboard.
            //all possible midi note values
            //float a = 440; // a is 440 hz...
            //for (int x = 0; x < 127; ++x)
            //{
            //=(440/32) * POWER(2,(A2-((pitchBendNoteRadius/64))*(63-pitchBendValue)-9)/12)
            //keyboard[x] = (a / 32f) * (float)Math.Pow(2f, ((x - ((pitchBendNoteRadius / 64)) * (63 - pitchBendValue) - 9f) / 12f));

            //    keyboard[x] = (a / 32f) * (float)Math.Pow(2f, ((x - 9f) / 12f));                
            //    notes[x] = 0;
            //}
            kb = new Keyboard();
            kb.PitchBendNoteRadius = 12;
            var xx = CreateAudioGraph();
            xx.Wait();
        }

        public void ProcessMessage(MidiMessage theMessage)
        {
            switch (theMessage.MessageClass)
            {
                case MsgClass.NOTE_OFF:
                    NoteOff(theMessage);
                    break;
                case MsgClass.NOTE_ON:
                    if (theMessage.Data2 == 0)
                    {
                        NoteOff(theMessage);
                    } else
                    {
                        NoteOn(theMessage);
                    }
                    break;
                case MsgClass.PITCH_BEND:
                    PitchBend(theMessage);
                    break;
                case MsgClass.CONTROL_CHANGE:
                    ControlChange(theMessage);
                    break;
                default:
                    break;
            }    

        }
        
        public void PitchBend(MidiMessage theMessage)
        {
            kb.SetPitchBendValue(theMessage.Data1, theMessage.Data2);
            //Trace.WriteLine(kb.PitchBendValue);
            if (wg1.isBusy() && wg1.KeyNumber != -1)
            {
                wg1.Freq = kb.getKeyFrequency(wg1.KeyNumber);
                //Trace.WriteLine(wg1.Freq);
            }
            if (wg2.isBusy() && wg2.KeyNumber != -1)
            {
                wg2.Freq = kb.getKeyFrequency(wg2.KeyNumber);
            }
            if (wg3.isBusy() && wg3.KeyNumber != -1)
            {
                wg3.Freq = kb.getKeyFrequency(wg3.KeyNumber);
            }
        }

        public void NoteOn(MidiMessage theMessage)
        {
            float localFreq = kb.getKeyFrequency(theMessage.Data1);
            if (notes[theMessage.Data1] == 0)
            {
                if (!wg1.isBusy())
                {
                    notes[theMessage.Data1] = 1;
                    wg1.On();
                    wg1.Freq = localFreq;
                    wg1.KeyNumber = theMessage.Data1;
                    //wg1.Freq = keyboard[theMessage.Data1];
                } else if (!wg2.isBusy())
                {
                    notes[theMessage.Data1] = 2;
                    wg2.On();
                    wg2.Freq = localFreq;
                    wg2.KeyNumber = theMessage.Data1;
                    //wg2.Freq = keyboard[theMessage.Data1];
                } else if (!wg3.isBusy())
                {
                    notes[theMessage.Data1] = 3;
                    wg3.On();
                    wg3.Freq = localFreq;
                    wg3.KeyNumber = theMessage.Data1;
                    //wg3.Freq = keyboard[theMessage.Data1];
                }

            }
        }

        public void NoteOff(MidiMessage theMessage)
        {
            if (notes[theMessage.Data1] == 1)
            {
                notes[theMessage.Data1] = 0;
                wg1.Release();
            } else if (notes[theMessage.Data1] == 2)
            {
                notes[theMessage.Data1] = 0;
                wg2.Release();
            }
            else if (notes[theMessage.Data1] == 3)
            {
                notes[theMessage.Data1] = 0;
                wg3.Release();
            }
        }

        public void ControlChange(MidiMessage theMessage)
        {
            //1=Attack
            //2=Decay
            //3=Sustain
            //4=Release
            int controlNum = theMessage.Data1;
            switch (controlNum)
            {
                case 0:
                    wg1.Attack(theMessage.Data2);
                    wg2.Attack(theMessage.Data2);
                    wg3.Attack(theMessage.Data2);
                    break;
                case 1:
                    wg1.Decay(theMessage.Data2);
                    wg2.Decay(theMessage.Data2);
                    wg3.Decay(theMessage.Data2);
                    break;
                case 2:
                    wg1.Sustain(theMessage.Data2);
                    wg2.Sustain(theMessage.Data2);
                    wg3.Sustain(theMessage.Data2);
                    break;
                case 3:
                    wg1.Release(theMessage.Data2);
                    wg2.Release(theMessage.Data2);
                    wg3.Release(theMessage.Data2);
                    break;
                default:
                    break;
            }
        }

        public void Stop()
        {
            wg1.Off();
            wg2.Off();
            wg3.Off();
        }

        private async Task CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create graph
                return;
            }

            graph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
            }
            //Only need one of these
            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            wg1 = new WaveGenerator(graph);
            wg1.SetDeviceOutputNode(deviceOutputNode);
            wg2 = new WaveGenerator(graph);
            wg2.SetDeviceOutputNode(deviceOutputNode);
            wg3 = new WaveGenerator(graph);
            wg3.SetDeviceOutputNode(deviceOutputNode);

            // Start the graph since we will only start/stop the frame input node
            graph.Start();
        }


    }
}
