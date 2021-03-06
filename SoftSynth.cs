﻿using System;
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

            kb = new Keyboard();
            kb.PitchBendNoteRadius = 2;
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
                    //occasionally, the note off command derives from note on
                    //with a velocity of 0
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
                case MsgClass.PROGRAM_CHANGE:
                    ProgramChange(theMessage);
                    break;
                default:
                    break;
            }    

        }
        
        public void PitchBend(MidiMessage theMessage)
        {
            kb.SetPitchBendValue(theMessage.Data1, theMessage.Data2);
            if (wg1.isBusy() && wg1.KeyNumber != -1)
            {
                wg1.Pitch = kb.getKeyPitch(wg1.KeyNumber);
            }
            if (wg2.isBusy() && wg2.KeyNumber != -1)
            {
                wg2.Pitch = kb.getKeyPitch(wg2.KeyNumber);
            }
            if (wg3.isBusy() && wg3.KeyNumber != -1)
            {
                wg3.Pitch = kb.getKeyPitch(wg3.KeyNumber);
            }
        }

        public void NoteOn(MidiMessage theMessage)
        {
            //float localFreq = kb.getKeyFrequency(theMessage.Data1);
            int localPitch = kb.getKeyPitch(theMessage.Data1);
            if (notes[theMessage.Data1] == 0)
            {
                if (!wg1.isBusy())
                {
                    notes[theMessage.Data1] = 1;
                    wg1.On();
                    wg1.Pitch = localPitch;
                    wg1.KeyNumber = theMessage.Data1;
                } else if (!wg2.isBusy())
                {
                    notes[theMessage.Data1] = 2;
                    wg2.On();
                    wg2.Pitch = localPitch;
                    wg2.KeyNumber = theMessage.Data1;
                } else if (!wg3.isBusy())
                {
                    notes[theMessage.Data1] = 3;
                    wg3.On();
                    wg3.Pitch = localPitch;
                    wg3.KeyNumber = theMessage.Data1;
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
                    wg1.EGAttack = theMessage.Data2;
                    wg2.EGAttack = theMessage.Data2;
                    wg3.EGAttack = theMessage.Data2;
                    break;
                case 1:
                    wg1.EGDecay = theMessage.Data2;
                    wg2.EGDecay = theMessage.Data2;
                    wg3.EGDecay = theMessage.Data2;
                    break;
                case 2:
                    wg1.EGSustain = theMessage.Data2;
                    wg2.EGSustain = theMessage.Data2;
                    wg3.EGSustain = theMessage.Data2;
                    break;
                case 3:
                    wg1.EGRelease = theMessage.Data2;
                    wg2.EGRelease = theMessage.Data2;
                    wg3.EGRelease = theMessage.Data2;
                    break;
                default:
                    break;
            }
        }

        public void ProgramChange(MidiMessage theMessage)
        {
            int programNum = theMessage.Data1;
            switch (programNum)
            {
                case 0:
                    wg1.Waveform = Oscillator.OscWaveformType.SINE;
                    wg2.Waveform = Oscillator.OscWaveformType.SINE;
                    wg3.Waveform = Oscillator.OscWaveformType.SINE;
                    break;
                case 1:
                    wg1.Waveform = Oscillator.OscWaveformType.TRI;
                    wg2.Waveform = Oscillator.OscWaveformType.TRI;
                    wg3.Waveform = Oscillator.OscWaveformType.TRI;
                    break;
                case 2:
                    wg1.Waveform = Oscillator.OscWaveformType.SAW;
                    wg2.Waveform = Oscillator.OscWaveformType.SAW;
                    wg3.Waveform = Oscillator.OscWaveformType.SAW;
                    break;
                case 3:
                    wg1.Waveform = Oscillator.OscWaveformType.PULSE;
                    wg2.Waveform = Oscillator.OscWaveformType.PULSE;
                    wg3.Waveform = Oscillator.OscWaveformType.PULSE;
                    break;
                case 4:
                    wg1.Waveform = Oscillator.OscWaveformType.NOISE;
                    wg2.Waveform = Oscillator.OscWaveformType.NOISE;
                    wg3.Waveform = Oscillator.OscWaveformType.NOISE;
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
