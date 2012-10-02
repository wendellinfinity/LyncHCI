using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LyncHCI {
    // Console program for receiving the data
    class Program {
        static void Main(string[] args) {
            SerialWorker sw;
            LyncClientWorker lc = new LyncClientWorker();

            // delegate when a message from the device arrives
            ParseMessage parseMessage = new ParseMessage(delegate(string message) {
                switch (message) {
                    case "FREE":
                        Console.WriteLine("I am Free");
                        lc.AvailabilityChanged(LyncAvailabilityState.Free);
                        break;
                    case "BUSY":
                        Console.WriteLine("I am Busy");
                        lc.AvailabilityChanged(LyncAvailabilityState.Busy);
                        break;
                    case "AWAY":
                        Console.WriteLine("I am Away");
                        lc.AvailabilityChanged(LyncAvailabilityState.Away);
                        break;
                    case "DND":
                        Console.WriteLine("DND");
                        lc.AvailabilityChanged(LyncAvailabilityState.DND);
                        break;
                }
            });
            // create the SerialWorker along with the delegate
            sw = new SerialWorker("COM8", parseMessage);
            // trigger the listener
            sw.ListenPort();
        }


    }
}
