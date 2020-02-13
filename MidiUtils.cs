using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace IotSound
{
    // We are initializing a COM interface for use within the namespace
    // This interface allows access to memory at the byte level which we need to populate audio data that is generated

    class MidiUtils : IDisposable
    {

        //When the buffers are full, we start throwing away data
        //4k is arbitrary, but it seems adequate
        public const int _MaxBufferSize = 4096;
        Action<MidiMessage> Channel0Message = null;
        Action<MidiMessage> Channel1Message = null;
        Action<MidiMessage> Channel2Message = null;

        private SerialDevice UartPort;
        private DataReader DataReaderObject = null;
        private DataWriter DataWriterObject;
        private CancellationTokenSource ReadCancellationTokenSource;
        public const uint _MIDI_BAUD_RATE = 31250;

        public void RegisterChannelCallback(uint Channel, Action<MidiMessage> callback)
        {
            switch (Channel)
            {
                case 0:
                    Channel0Message = callback;
                    break;
                case 1:
                    Channel1Message = callback;
                    break;
                case 2:
                    Channel2Message = callback;
                    break;
                default:
                    break;
            }
        }

        public void RouteMessage(MidiMessage msg)
        {
            switch (msg.Channel)
            {
                case 0:
                    Channel0Message?.Invoke(msg);
                    break;
                case 1:
                    Channel1Message?.Invoke(msg);
                    break;
                case 2:
                    Channel2Message?.Invoke(msg);
                    break;
                default:
                    break;
            }
        }

        public async Task<int> Initialize()     //NOTE - THIS IS AN ASYNC METHOD!
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector("UART0");
                var dis = await DeviceInformation.FindAllAsync(aqs);
                UartPort = await SerialDevice.FromIdAsync(dis[0].Id);

                //Configure serial settings
                UartPort.WriteTimeout = TimeSpan.FromMilliseconds(100);    //mS before a time-out occurs when a write operation does not finish (default=InfiniteTimeout).
                UartPort.ReadTimeout = TimeSpan.FromMilliseconds(100);     //mS before a time-out occurs when a read operation does not finish (default=InfiniteTimeout).
                UartPort.BaudRate = _MIDI_BAUD_RATE;
                UartPort.Parity = SerialParity.None;
                UartPort.StopBits = SerialStopBitCount.One;
                UartPort.DataBits = 8;

                DataReaderObject = new DataReader(UartPort.InputStream)
                {
                    InputStreamOptions = InputStreamOptions.Partial
                };
                DataWriterObject = new DataWriter(UartPort.OutputStream);

            }
            catch (Exception ex)
            {
                throw new Exception("Uart Initialize Error", ex);
            }
            return 0;
        }

        //***********************************
        //***********************************
        //********** RECEIVE BYTES **********
        //***********************************
        //***********************************
        //This is all a bit complex....but necessary if you want receive to happen asynchrously and for your app to be notified instead of your code having to poll it (this code is basically polling it to create a receive event)


        //ASYNC METHOD TO CREATE THE LISTEN LOOP
        public async Task<int> StartReceive()
        {
            ReadCancellationTokenSource = new CancellationTokenSource();
            GPIOInterface theDevice = GPIOInterface.Instance;
            theDevice.FlashLed(1, 10);
            while (true)
            {
                await Listen().ConfigureAwait(false);
                if ((ReadCancellationTokenSource.Token.IsCancellationRequested) || (UartPort == null))
                    break;
            }
            return await Task.FromResult(0);
        }

        //LISTEN FOR NEXT RECEIVE
        private async Task<int> Listen()
        {
            const int NUMBER_OF_BYTES_TO_RECEIVE = 1;           //<<<<<SET THE NUMBER OF BYTES YOU WANT TO WAIT FOR

            byte[] ReceiveData;
            UInt32 bytesRead;
            MidiMessage ChannelMessage = new MidiMessage();
            //MidiMessage GlobalMessage = new MidiMessage();

            try
            {
                if (UartPort != null)
                {
                    while (true)
                    {
                        //###### WINDOWS IoT MEMORY LEAK BUG 2017-03 - USING CancellationToken WITH LoadAsync() CAUSES A BAD MEMORY LEAK.  WORKAROUND IS
                        //TO BUILD RELEASE WITHOUT USING THE .NET NATIVE TOOLCHAIN OR TO NOT USE A CancellationToken IN THE CALL #####
                        //bytesRead = await DataReaderObject.LoadAsync(NUMBER_OF_BYTES_TO_RECEIVE).AsTask(ReadCancellationTokenSource.Token);	//Wait until buffer is full

                        Windows.Foundation.IAsyncOperation<uint> taskLoad = DataReaderObject.LoadAsync(NUMBER_OF_BYTES_TO_RECEIVE);
                        taskLoad.AsTask().Wait();
                        bytesRead = taskLoad.GetResults();

                        //bytesRead = await DataReaderObject.LoadAsync(NUMBER_OF_BYTES_TO_RECEIVE).AsTask();  //Wait until buffer is full
                        

                        if ((ReadCancellationTokenSource.Token.IsCancellationRequested) || (UartPort == null))
                            break;

                        if (bytesRead > 0)
                        {
                            ReceiveData = new byte[NUMBER_OF_BYTES_TO_RECEIVE];

                            DataReaderObject.ReadBytes(ReceiveData);

                            foreach (byte Data in ReceiveData)
                            {
                                //-------------------------------
                                //-------------------------------
                                //----- RECEIVED NEXT BYTE ------
                                //-------------------------------
                                //-------------------------------

                                if (Data >= 0x80 && Data <= 0xEF) //new Channel Message
                                {
                                    if (ChannelMessage.IsSet1) //We already have data in this object
                                    {
                                        //there is data in the current message
                                        RouteMessage(ChannelMessage);
                                        ChannelMessage = new MidiMessage(Data);
                                    } else { //No data in the message
                                        ChannelMessage.Status = Data;
                                        if (MidiMessage.ByteCount(ChannelMessage.MessageClass) == 0)
                                        {
                                            RouteMessage(ChannelMessage);
                                            ChannelMessage = new MidiMessage();
                                        }
                                    }
                                }
                                else //not a status byte
                                {
                                    if (ChannelMessage.Status!=0 && !ChannelMessage.IsSet1)
                                    {
                                        ChannelMessage.Data1 = Data;
                                        if (MidiMessage.ByteCount(ChannelMessage.MessageClass) == 1)
                                        {
                                            RouteMessage(ChannelMessage);
                                            ChannelMessage = new MidiMessage();
                                        }
                                    }
                                    else //Data1 is full
                                    {
                                        if (ChannelMessage.Status != 0 && !ChannelMessage.IsSet2)
                                        {
                                            ChannelMessage.Data2 = Data;
                                            RouteMessage(ChannelMessage);
                                            ChannelMessage = new MidiMessage();
                                        }
                                    }
                                }
                            }


                            /*if (Data > 0xF0 ) //SysEx message (Global)
                            {
                                //Throw away data until we get to F7
                            }*/

                        }
                        
                    } //while
                } //check the port
            } catch (Exception e)
            {
                //We will get here often if the USB serial cable is removed so reset ready for a new connection (otherwise a never ending error occurs)
                if (ReadCancellationTokenSource != null)
                    ReadCancellationTokenSource.Cancel();
                System.Diagnostics.Debug.WriteLine("UART ReadAsync Exception: {0}", e.Message);
            }
            return await Task.FromResult(0);
        }

        //********************************
        //********** SEND BYTES **********
        //********************************
        //********************************
        public async Task<int> SendBytes(byte[] TxData)
        {
            try
            {
                //Send data to UART
                DataWriterObject.WriteBytes(TxData);
                await DataWriterObject.StoreAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Uart Tx Error", ex);
            }
            return 0;
        }

        public void Dispose()
        {
            UartPort.Dispose();
            DataReaderObject.Dispose();
            DataWriterObject.Dispose();
            ReadCancellationTokenSource.Dispose();
        }
    }
}