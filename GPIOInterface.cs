using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotSound
{
    public class GPIOInterface : IDisposable
    {
        private static GPIOInterface instance = null;
        private static readonly object padlock = new object();
        private const int LED_PIN = 5;
        private GpioPin pin;
        private GpioController gpio;

        public static GPIOInterface Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new GPIOInterface();
                    }
                    return instance;
                }
            }
        }

        private GPIOInterface()
        {
            //Initialize the controller
            gpio = GpioController.GetDefault();

            // Set up our GPIO pin for setting values.
            // If this next line crashes with a NullReferenceException,
            // then the problem is that there is no GPIO controller on the device.
            pin = gpio.OpenPin(LED_PIN);

            // Configure pin for output.
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void FlashLed(int count, int interval)
        {
            bool state = false;
            for (int o = 0; o < count; o++)
            {
                state = !state;
                pin.Write(GpioPinValue.Low);
                System.Threading.Thread.Sleep(interval);
                state = !state;
                pin.Write(GpioPinValue.High);
                System.Threading.Thread.Sleep(interval);
            }
            //return await Task.FromResult((uint)0);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    pin.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.


                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GPIOInterface()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
