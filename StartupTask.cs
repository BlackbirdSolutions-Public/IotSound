using System.Threading;
using Windows.ApplicationModel.Background;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IotSound
{
    public sealed class StartupTask : IBackgroundTask
    {
        GPIOInterface theDevice;
        MidiUtils mrMidi;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            theDevice = new GPIOInterface();
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
            var res = theDevice.FlashLed(1, 10, GPIOStatus);
        }

        public void GPIOStatus(bool status)
        {
            //we send this function into an async method 
            //and this will get called whenever it needs to report status
           
        }
    }
}
