using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LyncHCI {
    // Console program for receiving the data
    class Program {
        static void Main(string[] args) {

            SerialWorker serialDevice = null;
            LyncClientWorker lyncHandler = null;
            bool isFromDevice = false; // this tells the handler that the trigger made from device will not trigger another updateAvailability
            // delegate when a message from the device arrives
            ParseIncomingMessage parseMessage = new ParseIncomingMessage(delegate(string message) {
                isFromDevice = true;
                switch (message) {
                    case "FREE":
                        Console.WriteLine("I am Free");
                        lyncHandler.UpdateLyncAvailability(LyncAvailabilityState.Free);
                        break;
                    case "BUSY":
                        Console.WriteLine("I am Busy");
                        lyncHandler.UpdateLyncAvailability(LyncAvailabilityState.Busy);
                        break;
                    case "AWAY":
                        Console.WriteLine("I am Away");
                        lyncHandler.UpdateLyncAvailability(LyncAvailabilityState.Away);
                        break;
                    case "DND":
                        Console.WriteLine("DND");
                        lyncHandler.UpdateLyncAvailability(LyncAvailabilityState.DND);
                        break;
                }
            });

            SetDeviceAvailability updateAvailability = new SetDeviceAvailability(delegate(LyncAvailabilityState state) {
                if (!isFromDevice) {
                    // process the callback here
                    if (serialDevice != null) {
                        switch (state) {
                            case LyncAvailabilityState.Free:
                                serialDevice.SendMessage("FREE");
                                break;
                            case LyncAvailabilityState.Away:
                                serialDevice.SendMessage("AWAY");
                                break;
                            case LyncAvailabilityState.Busy:
                                serialDevice.SendMessage("BUSY");
                                break;
                            case LyncAvailabilityState.DND:
                                serialDevice.SendMessage("DND");
                                break;
                            case LyncAvailabilityState.Offline:
                                serialDevice.SendMessage("OFF");
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    isFromDevice = false;
                }
            });

            // create the SerialWorker along with the delegate and message terminators
            serialDevice = new SerialWorker("COM8", parseMessage, '[', ']');
            // trigger the listener, this starts the process
            serialDevice.ListenPort(); // --> this has a thread in it, and this app will run until thread is destroyed

            // create the LyncClient along with the callback
            lyncHandler = new LyncClientWorker(updateAvailability);

        }


    }
}
