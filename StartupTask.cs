using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Background;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IotSound
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public sealed class StartupTask : IBackgroundTask
    {
        GPIOInterface theDevice;
        MidiUtils mrMidi;
        SoftSynth mrSound;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            theDevice = GPIOInterface.Instance;
            mrSound = new SoftSynth();
            mrMidi = new MidiUtils();
            var xx = mrMidi.Initialize();
            mrMidi.RegisterChannelCallback(0, HandleMidiMessage);
            var yy = mrMidi.StartReceive();

            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //theDevice.Dispose();
        }

        public void HandleMidiMessage(MidiMessage theMessage)
        {
            theDevice.FlashLed(1, 10);
            mrSound.Play();
        }

        public void GPIOStatus(bool status)
        {
            //we send this function into an async method 
            //and this will get called whenever it needs to report status
           
        }
    }
}
