using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;
using System.Runtime.InteropServices;

namespace LyncHCI {

    public delegate void SetInterfaceAvailability(LyncAvailabilityState state);

    // Wrapper enum for the only available status
    public enum LyncAvailabilityState {
        Free = ContactAvailability.Free,
        Away = ContactAvailability.Away,
        Busy = ContactAvailability.Busy,
        DND = ContactAvailability.DoNotDisturb,
        Offline = ContactAvailability.Offline
    }

    // Constructor for lync worker
    public class LyncClientWorker {
        private LyncClient _lyncClient;

        public LyncClientWorker() {
            //Listen for events of changes in the state of the client
            try {
                _lyncClient = LyncClient.GetClient();
            }
            catch (ClientNotFoundException clientNotFoundException) {
                Console.WriteLine(clientNotFoundException);
                return;
            }
            catch (NotStartedByUserException notStartedByUserException) {
                Console.Out.WriteLine(notStartedByUserException);
                return;
            }
            catch (LyncClientException lyncClientException) {
                Console.Out.WriteLine(lyncClientException);
                return;
            }
            catch (SystemException systemException) {
                if (IsLyncException(systemException)) {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                    return;
                }
                else {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            // for watching out changesi 
            _lyncClient.StateChanged +=
                new EventHandler<ClientStateChangedEventArgs>(LyncStateChanged);
        }

        /// <summary>
        /// Handler for the StateChanged event of the contact. Used to update the anything with the new client state.
        /// </summary>
        private void LyncStateChanged(object sender, ClientStateChangedEventArgs e) {
            UpdateClient(e.NewState);
        }

        /// <summary>
        /// Updates whatever needs to be updated
        /// </summary>
        /// <param name="currentState"></param>
        private void UpdateClient(ClientState currentState) {
            if (currentState == ClientState.SignedIn) {
                //Listen for events of changes of the contact's information
                _lyncClient.Self.Contact.ContactInformationChanged +=
                    new EventHandler<ContactInformationChangedEventArgs>(ContactInformationChanged);
                SetAvailability();
            }
            else {
                SetAvailability(isClear:true);
            }
        }

        /// <summary>
        /// Handler for the Availability changes. Used to publish the selected availability value in Lync
        /// </summary>
        public void AvailabilityChanged(LyncAvailabilityState state) {

            //Add the availability to the contact information items to be published
            Dictionary<PublishableContactInformationType, object> newInformation =
                new Dictionary<PublishableContactInformationType, object>();
            newInformation.Add(PublishableContactInformationType.Availability, state);

            //Publish the new availability value
            try {
                _lyncClient.Self.BeginPublishContactInformation(newInformation, PublishContactInformationCallback, null);
            }
            catch (LyncClientException lyncClientException) {
                Console.WriteLine(lyncClientException);
            }
            catch (SystemException systemException) {
                if (IsLyncException(systemException)) {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the contact's current availability value from Lync and updates the corresponding elements in the user interface
        /// </summary>
        private void SetAvailability(bool isClear = false) {
            //Get the current availability value from Lync
            ContactAvailability currentAvailability = 0;
            try {
                currentAvailability = (ContactAvailability)_lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            }
            catch (LyncClientException e) {
                Console.WriteLine(e);
            }
            catch (SystemException systemException) {
                if (IsLyncException(systemException)) {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
            /*
            if (currentAvailability != 0) {
                //Update the availability ComboBox with the contact's current availability.
                switch (currentAvailability) {
                    case ContactAvailability.TemporarilyAway:
                    case ContactAvailability.Away:
                        availabilityColor = Brushes.Yellow;
                        break;
                    case ContactAvailability.BusyIdle:
                    case ContactAvailability.Busy:
                        availabilityColor = Brushes.Red;
                        break;
                    case ContactAvailability.DoNotDisturb:
                        availabilityColor = Brushes.DarkRed;
                        break;
                    case ContactAvailability.FreeIdle:
                    case ContactAvailability.Free:
                        availabilityColor = Brushes.LimeGreen;
                        break;
                    case ContactAvailability.Offline:
                        availabilityColor = Brushes.LightSlateGray;
                        break;
                    default:
                        availabilityColor = Brushes.LightSlateGray;
                        break;
                }
            }
             */
        }

        /// <summary>
        /// Handler for the ContactInformationChanged event of the contact. Used to update the contact's information in the user interface.
        /// </summary>
        private void ContactInformationChanged(object sender, ContactInformationChangedEventArgs e) {
            //Only update the contact information in the user interface if the client is signed in.
            //Ignore other states including transitions (e.g. signing in or out).
            if (_lyncClient.State == ClientState.SignedIn) {
                //Get from Lync only the contact information that changed.
                if (e.ChangedContactInformation.Contains(ContactInformationType.Availability)) {
                    //Use the current dispatcher to update the contact's availability in the user interface.
                    SetAvailability();
                }
            }
        }

        /// <summary>
        /// Callback invoked when Self.BeginPublishContactInformation is completed
        /// </summary>
        /// <param name="result">The status of the asynchronous operation</param>
        private void PublishContactInformationCallback(IAsyncResult result) {
            _lyncClient.Self.EndPublishContactInformation(result);
        }

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsLyncException(SystemException ex) {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }

    }
}
