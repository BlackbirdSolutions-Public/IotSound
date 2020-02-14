using System;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;

namespace IotSound
{
    class WaveGenerator
    {
        private float freq = 440.0F;
        private double theta = 0F;
        //private double amplitude = 0.3F;
        private int sampleRate = 44100;
        private int currentSample = 0;
        private AudioFrameInputNode inputNode;
        private AudioGraph graph;
        private AudioDeviceOutputNode deviceOutputNode;
        private bool on = false;
        private EnvelopeGenerator eg;
        private int keyNumber = -1;

        public float Freq { get => freq; set => freq = value; }
        public double Theta { get => theta; set => theta = value; }
        //public double Amplitude { get => amplitude; set => amplitude = value; }
        public int SampleRate { get => sampleRate; }
        public int KeyNumber { get => keyNumber; set => keyNumber = value; }

        public void Attack(int newValue)
        {
            //0-127 *350 ~ 1 second;
            eg.Attack = newValue*350;
        }
        public void Decay(int newValue)
        {
            //0-127 *350 ~ 1 second;
            eg.Decay = newValue * 350;
        }
        public void Release(int newValue)
        {
            //0-127 *350 ~ 1 second;
            eg.Release = newValue * 350;
        }
        public void Sustain(int newValue)
        {
            //0-127 *350 ~ 1 second;
            eg.Sustain = newValue * 0.0027f;
        }

        public void adjustFreq(float newFreq)
        {
            freq = newFreq;
        }
        public bool isBusy()
        {
            return on;
        }

        public void On()
        {
            //...or inputNode.Start()?
            eg.Gate = true;
            on = true;
            currentSample = 0;
            theta = 0f;
        }

        public void Off()
        {
            //...or inputNode.Start()?
            eg.Gate = false;
            on = false;
            keyNumber = -1;
            //currentSample = 0;
            //theta = 0f;
        }
        public void Release()
        {
            //...or inputNode.Stop()?
            eg.Gate = false;
        }

        public WaveGenerator(AudioGraph theGraph)
        {
            graph = theGraph;
            AudioEncodingProperties nodeEncodingProperties = graph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            sampleRate = 44100;
            inputNode = graph.CreateFrameInputNode(nodeEncodingProperties);
            inputNode.Stop();
            eg = new EnvelopeGenerator();
            eg.Level = 0.35f; //Set volume to a reasonable value;
            Off();
        }

        public void SetDeviceOutputNode(AudioDeviceOutputNode theNode)
        {
            deviceOutputNode = theNode;
            inputNode.AddOutgoingConnection(deviceOutputNode);
            // This event is triggered when the node is required to provide data
            inputNode.QuantumStarted += node_QuantumStarted;
            on = false;
            inputNode.Start();
        }
        public AudioFrameInputNode getFrameInputNode()
        {
            return inputNode;
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
                sender.AddFrame(audioData);
            }
        }

        unsafe private AudioFrame GenerateAudioData(uint samples)
        {

            // Buffer size is (number of samples) * (size of each sample)
            // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
            AudioFrame frame = new Windows.Media.AudioFrame(samples * sizeof(float));
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                double level = 0.0f;
                double sinValue;
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                //This is the invalid cast problem here.
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;
                double sampleIncrement = (freq * (Math.PI * 2)) / sampleRate;

                //issue here is that theta isn't the number of samples...
                for (int i = 0; i < samples; i++)
                {
                    if (isBusy())
                    {
                        level = eg.GetLevelAtInterval(currentSample);
                        sinValue = level * Math.Sin(theta);
                        dataInFloat[i] = (float)sinValue;

                    } else
                    {
                        dataInFloat[i] = 0.0f;
                    }
                    theta += sampleIncrement;
                    currentSample += 1;
                    if (!eg.Gate)
                    {
                        if (level <= 0 && isBusy())
                        {
                            Off();
                        }
                    }
                }
            }

            return frame;
        }
    }
}
