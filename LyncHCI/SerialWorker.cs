using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace LyncHCI {

    public delegate void ParseMessage(string message);

    public class SerialWorker {

        private SerialPort _port;
        private ParseMessage _parseMessage;
        private Thread _listenerThread;

        public SerialWorker(string serialPort, ParseMessage parseMessage) {
            this._parseMessage = parseMessage;
            this._port = new SerialPort(serialPort, 9600, Parity.None, 8, StopBits.One);
        }

        public void ListenPort() {
            _listenerThread = new Thread(new ParameterizedThreadStart(delegate(object o) {
                // initialize the sensor port, mine was registered as COM8, you may check yours
                // through the hardware devices from control panel
                int bytesToRead = 0;
                string chunk, message;
                _port.Open();
                try {
                    bool start;
                    start = false;
                    message = "";
                    while (true) {
                        // check if there are bytes incoming
                        bytesToRead = _port.BytesToRead;
                        if (bytesToRead > 0) {
                            byte[] input = new byte[bytesToRead];
                            // read the serial input
                            _port.Read(input, 0, bytesToRead);
                            // convert the bytes into string
                            chunk = System.Text.Encoding.UTF8.GetString(input);
                            if (chunk.IndexOf("[") >= 0) {
                                start = true;
                            }
                            if (start) {
                                message += chunk;
                            }
                            if (chunk.IndexOf("]") >= 0) {
                                start = false;
                                // clean up code
                                message = message.Trim().Replace("[", "").Replace("]", "");
                                // call the delegate
                                this._parseMessage(message);
                                message = "";
                            }
                            Console.WriteLine("");
                        }
                    }

                }
                finally {
                    // again always close the serial ports!
                    _port.Close();
                }
            }));
            // start the thread
            _listenerThread.Start();
        }

    }
}
