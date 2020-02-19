using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;

namespace IotSound
{
    class WaveGenerator
    {
        private int pitch = 300;
        private int currentSample = 0;
        private AudioFrameInputNode inputNode;
        private AudioGraph graph;
        private AudioDeviceOutputNode deviceOutputNode;
        private bool on = false;
        private EnvelopeGenerator eg;
        private int keyNumber = -1;
        private int pulseWidth = 0;
        private double sampleIncrement = 0f;
        private Oscillator osc;

        //public double Amplitude { get => amplitude; set => amplitude = value; }
        public int SampleRate
        {
            get => Oscillator.SampleRate;
        }
        public int KeyNumber { get => keyNumber; set => keyNumber = value; }
        public int PulseWidth { get => pulseWidth; set => pulseWidth = value; }
        public int EGAttack { get => eg.Attack; set => eg.Attack = value; }
        public int EGDecay { get => eg.Decay; set => eg.Decay = value; }
        public double EGSustain { get => eg.Sustain; set => eg.Sustain = value; }
        public int EGRelease { get => eg.Release; set => eg.Release = value; }

        public int Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                sampleIncrement = ((Oscillator.FreqTable[value]) * (Math.PI * 2)) / SampleRate;
                osc.Pitch = pitch;
            }
        }

        public Oscillator.OscWaveformType Waveform
        {
            get => osc.Waveform; 
            set
            {
                osc.Waveform = value;
            }
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
        }

        public void Off()
        {
            //...or inputNode.Start()?
            eg.Gate = false;
            on = false;
            keyNumber = -1;
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
            inputNode = graph.CreateFrameInputNode(nodeEncodingProperties);
            inputNode.Stop();
            osc = new Oscillator();
            eg = new EnvelopeGenerator();
            eg.MaxLevel = 0.35f; //Set volume to a reasonable value;
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
                double wavValue;
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                //This is the invalid cast problem here.
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;
                
                //issue here is that theta isn't the number of samples...
                for (int i = 0; i < samples; i++)
                {
                    if (isBusy())
                    {
                        level = eg.GetLevelAtInterval(currentSample);
                        osc.Run();
                        wavValue = (level * (float)osc.Value * (1f / 65535f));
                        dataInFloat[i] = (float)wavValue;
                    } else {
                        dataInFloat[i] = 0.0f;
                    }
                    //how many samples since the note was activated?
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
