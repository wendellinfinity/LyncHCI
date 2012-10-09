using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace LyncHCI {

    public delegate void ParseIncomingMessage(string message);

    public class SerialWorker {

        private SerialPort _port;
        private ParseIncomingMessage _parseMessage;
        private Thread _listenerThread;
        private Tuple<char, char> _terminators;

        public SerialWorker(string serialPort, ParseIncomingMessage parseMessage, char messageStartBit, char messageEndBit)
            : this(serialPort, parseMessage) {
            this._terminators = new Tuple<char, char>(messageStartBit, messageEndBit);
        }

        public SerialWorker(string serialPort, ParseIncomingMessage parseMessage) {
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
                            if (_terminators != null) {
                                if (chunk.IndexOf(_terminators.Item1) >= 0) {
                                    start = true;
                                }
                                if (start) {
                                    message += chunk;
                                }
                                if (chunk.IndexOf(_terminators.Item2) >= 0) {
                                    start = false;
                                    // clean up code
                                    message = message.Trim().Replace(_terminators.Item1.ToString(), "").Replace(_terminators.Item2.ToString(), "");
                                    // call the delegate
                                    this._parseMessage(message);
                                    message = "";
                                }
                            }
                            else {
                                // send message by byte
                                this._parseMessage(chunk);
                            }
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


        /// <summary>
        /// Send message using a string, terminators are USED. ASCII only
        /// </summary>
        /// <param name="message">ASCII message</param>
        public void SendMessage(string message) {
            if (message != null && !string.Empty.Equals(message)) {
                if (_terminators != null) {
                    message = string.Format("{0}{1}{2}", _terminators.Item1, message, _terminators.Item2);
                }
                this.SendMessage(System.Text.Encoding.ASCII.GetBytes(message));
            }
        }

        /// <summary>
        /// Send message using byte array, terminators are IGNORED
        /// </summary>
        /// <param name="message">Pure byte message</param>
        public void SendMessage(byte[] message) {
            if (_port.IsOpen) {
                _port.Write(message, 0, message.Length);
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(message));
            }
        }

    }
}
