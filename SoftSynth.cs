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
        private AudioFrameInputNode frameInputNode1;
        private bool Node1Busy = false;
        private AudioFrameInputNode frameInputNode2;
        private bool Node2Busy = false;
        private AudioFrameInputNode frameInputNode3;
        private bool Node3Busy = false;
        private float freq1 = 440.0F;
        private float freq2 = 440.0F;
        private float freq3 = 440.0F;
        private double theta1 = 0F;
        private double theta2 = 0F;
        private double theta3 = 0F;

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
                if (!Node1Busy)
                {
                    notes[theMessage.Data1] = 1;
                    Node1Busy = true;
                    freq1 = keyboard[theMessage.Data1];
                    frameInputNode1.Start();
                } else if (!Node2Busy)
                {
                    notes[theMessage.Data1] = 2;
                    Node2Busy = true;
                    freq2 = keyboard[theMessage.Data1];
                    frameInputNode2.Start();
                }
                else if (!Node3Busy)
                {
                    notes[theMessage.Data1] = 3;
                    Node3Busy = true;
                    freq3 = keyboard[theMessage.Data1];
                    frameInputNode3.Start();
                }

            }
        }

        public void NoteOff(MidiMessage theMessage)
        {
            if (notes[theMessage.Data1] == 1)
            {
                notes[theMessage.Data1] = 0;
                frameInputNode1.Stop();
                theta1 = 0;
                Node1Busy = false;
            } else if (notes[theMessage.Data1] == 2)
            {
                notes[theMessage.Data1] = 0;
                frameInputNode2.Stop();
                theta2 = 0;
                Node2Busy = false;
            }
            else if (notes[theMessage.Data1] == 3)
            {
                notes[theMessage.Data1] = 0;
                frameInputNode3.Stop();
                theta3 = 0;
                Node3Busy = false;
            }
        }

        public void Stop()
        {
            frameInputNode1.Reset();
            frameInputNode2.Reset();
            frameInputNode3.Reset();
        }

        unsafe private AudioFrame GenerateAudioData1(uint samples)
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
                double sampleIncrement = (freq1 * (Math.PI * 2)) / sampleRate;

                // Generate a 1kHz sine wave and populate the values in the memory buffer
                for (int i = 0; i < samples; i++)
                {
                    double sinValue = amplitude * Math.Sin(theta1);
                    dataInFloat[i] = (float)sinValue;
                    theta1 += sampleIncrement;
                }
            }

            return frame;
        }

        unsafe private AudioFrame GenerateAudioData2(uint samples)
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
                double sampleIncrement = (freq2 * (Math.PI * 2)) / sampleRate;

                // Generate a 1kHz sine wave and populate the values in the memory buffer
                for (int i = 0; i < samples; i++)
                {
                    double sinValue = amplitude * Math.Sin(theta2);
                    dataInFloat[i] = (float)sinValue;
                    theta2 += sampleIncrement;
                }
            }

            return frame;
        }

        unsafe private AudioFrame GenerateAudioData3(uint samples)
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
                double sampleIncrement = (freq3 * (Math.PI * 2)) / sampleRate;

                // Generate a 1kHz sine wave and populate the values in the memory buffer
                for (int i = 0; i < samples; i++)
                {
                    double sinValue = amplitude * Math.Sin(theta3);
                    dataInFloat[i] = (float)sinValue;
                    theta3 += sampleIncrement;
                }
            }

            return frame;
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

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
 
            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            AudioEncodingProperties nodeEncodingProperties = graph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            frameInputNode1 = graph.CreateFrameInputNode(nodeEncodingProperties);
            // Initialize the Frame Input Node in the stopped state
            frameInputNode1.Stop();
            frameInputNode1.AddOutgoingConnection(deviceOutputNode);
            // This event is triggered when the node is required to provide data
            frameInputNode1.QuantumStarted += node_QuantumStarted;

            frameInputNode2 = graph.CreateFrameInputNode(nodeEncodingProperties);
            // Initialize the Frame Input Node in the stopped state
            frameInputNode2.Stop();
            frameInputNode2.AddOutgoingConnection(deviceOutputNode);
            // This event is triggered when the node is required to provide data
            frameInputNode2.QuantumStarted += node_QuantumStarted;

            frameInputNode3 = graph.CreateFrameInputNode(nodeEncodingProperties);
            // Initialize the Frame Input Node in the stopped state
            frameInputNode3.Stop();
            frameInputNode3.AddOutgoingConnection(deviceOutputNode);
            // This event is triggered when the node is required to provide data
            frameInputNode3.QuantumStarted += node_QuantumStarted;

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
                if (sender == frameInputNode1)
            {
                AudioFrame audioData = GenerateAudioData1(numSamplesNeeded);
                sender.AddFrame(audioData);
            } else if (sender == frameInputNode2)
            {
                AudioFrame audioData = GenerateAudioData2(numSamplesNeeded);
                sender.AddFrame(audioData);
            }
            else if (sender == frameInputNode3)
            {
                AudioFrame audioData = GenerateAudioData3(numSamplesNeeded);
                sender.AddFrame(audioData);
            }

            }
        }

    }
}
