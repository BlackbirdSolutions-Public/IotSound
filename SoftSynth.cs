using System;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage.Streams;
using System.Threading.Tasks;


namespace IotSound
{

    public sealed class SoftSynth //default was 'internal class'
    {
        private AudioGraph graph;
        private AudioDeviceOutputNode deviceOutputNode;
        private WaveGenerator wg1;
        private WaveGenerator wg2;
        private WaveGenerator wg3;

        private readonly float[] keyboard = new float[127];
        private int[] notes = new int[127];

        //unsafe interface IMemoryBufferByteAccess
        //{
        //    void GetBuffer(out byte* buffer, out uint capacity);
        //}

        public SoftSynth()
        {
            InitializeMe();
        }

        private void InitializeMe()
        {
            //a virtual 128 note keyboard.
            //all possible midi note values
            float a = 440; // a is 440 hz...
            for (int x = 0; x < 127; ++x)
            {
                keyboard[x] = (a / 32f) * (float)Math.Pow(2f, ((x - 9f) / 12f));
                notes[x] = 0;
            }
            var xx = CreateAudioGraph();
        }

        public void ProcessMessage(MidiMessage theMessage)
        {
            switch (theMessage.MessageClass)
            {
                case 0:
                    NoteOff(theMessage);
                    break;
                case 1:
                    NoteOn(theMessage);
                    break;
                default:
                    break;
            }    

        }
        
        public void NoteOn(MidiMessage theMessage)
        {
            if (notes[theMessage.Data1] == 0)
            {
                if (!wg1.isBusy())
                {
                    notes[theMessage.Data1] = 1;
                    wg1.On();
                    wg1.Freq = keyboard[theMessage.Data1];
                } else if (!wg2.isBusy())
                {
                    notes[theMessage.Data1] = 2;
                    wg2.On();
                    wg2.Freq = keyboard[theMessage.Data1];
                } else if (!wg3.isBusy())
                {
                    notes[theMessage.Data1] = 3;
                    wg3.On();
                    wg3.Freq = keyboard[theMessage.Data1];
                }

            }
        }

        public void NoteOff(MidiMessage theMessage)
        {
            if (notes[theMessage.Data1] == 1)
            {
                notes[theMessage.Data1] = 0;
                wg1.Off();
            } else if (notes[theMessage.Data1] == 2)
            {
                notes[theMessage.Data1] = 0;
                wg2.Off();
            }
            else if (notes[theMessage.Data1] == 3)
            {
                notes[theMessage.Data1] = 0;
                wg3.Off();
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
