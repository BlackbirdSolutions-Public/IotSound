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
        private AudioFrameInputNode frameInputNode;
        private float freq = 440.0F;
        private double theta = 0F;
        private readonly float[] keyboard = new float[127];
        private bool frameStatus = false;

        public bool FrameStatus { get => frameStatus; set => frameStatus = value; }

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
            for (int i = 0; i < 127; i++)
            {
                double j = (double)i + 1d;
                double z = ((j - 49) / 12);
                keyboard[i] = (float)Math.Pow(2.0, z) * 440;
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
            freq = keyboard[theMessage.Data1];
            if (!FrameStatus)
            {
                FrameStatus = true;
                frameInputNode.Start();
            }
        }

        public void NoteOff(MidiMessage theMessage)
        {
            freq = keyboard[theMessage.Data1];
            if (FrameStatus)
            {
                FrameStatus = false;
                frameInputNode.Stop();
            }
        }

        public void Stop()
        {
            frameInputNode.Stop();
        }

        unsafe private AudioFrame GenerateAudioData(uint samples)
        {
            // Buffer size is (number of samples) * (size of each sample)
            // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
            AudioFrame frame = new Windows.Media.AudioFrame(samples * sizeof(float));
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                //This is the invalid cast problem here.
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;

                float amplitude = 0.3f;
                int sampleRate = (int)graph.EncodingProperties.SampleRate;
                double sampleIncrement = (freq * (Math.PI * 2)) / sampleRate;

                // Generate a 1kHz sine wave and populate the values in the memory buffer
                for (int i = 0; i < samples; i++)
                {
                    double sinValue = amplitude * Math.Sin(theta);
                    dataInFloat[i] = (float)sinValue;
                    theta += sampleIncrement;
                }
            }

            return frame;
        }
        private async Task CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
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

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
 
            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            AudioEncodingProperties nodeEncodingProperties = graph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            frameInputNode = graph.CreateFrameInputNode(nodeEncodingProperties);
            frameInputNode.AddOutgoingConnection(deviceOutputNode);
            // Initialize the Frame Input Node in the stopped state
            frameInputNode.Stop();

            // Hook up an event handler so we can start generating samples when needed
            // This event is triggered when the node is required to provide data
            frameInputNode.QuantumStarted += node_QuantumStarted;

            // Start the graph since we will only start/stop the frame input node
            graph.Start();
        }

        private void node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            // GenerateAudioData can provide PCM audio data by directly synthesizing it or reading from a file.
            // Need to know how many samples are required. In this case, the node is running at the same rate as the rest of the graph
            // For minimum latency, only provide the required amount of samples. Extra samples will introduce additional latency.
            uint numSamplesNeeded = (uint)args.RequiredSamples;

            if (numSamplesNeeded != 0)
            {
                AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
                frameInputNode.AddFrame(audioData);
            }
        }

    }
}
