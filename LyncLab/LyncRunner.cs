using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;

namespace LyncLab {
    public class LyncRunner {

        private LyncClient lyncClient;

        public LyncRunner() {
            lyncClient = LyncClient.GetClient(); // 1
            lyncClient.StateChanged += new EventHandler<ClientStateChangedEventArgs>(Client_StateChanged); // 2

            //Update the user interface
            UpdateUserInterface(lyncClient.State); //3

        }

        private void Client_StateChanged(object sender, ClientStateChangedEventArgs e) {
            UpdateUserInterface(e.NewState);
        }

        private void UpdateUserInterface(ClientState currentState) {
            if (currentState == ClientState.SignedIn) {
                //Listen for events of changes of the contact's information
                lyncClient.Self.Contact.ContactInformationChanged +=
                    new EventHandler<ContactInformationChangedEventArgs>(SelfContact_ContactInformationChanged);
            }
        }

        private void SelfContact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e) {
            if (lyncClient.State == ClientState.SignedIn) {
                if (e.ChangedContactInformation.Contains(ContactInformationType.Availability)) {
                    SetAvailability();
                }
            }
        }

        private void SetAvailability() {
            //Get the current availability value from Lync
            ContactAvailability currentAvailability = 0;
            currentAvailability = (ContactAvailability)lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            if (currentAvailability != 0) {
                switch (currentAvailability) {
                    case ContactAvailability.Away:
                        // do something when away
                        break;
                    case ContactAvailability.Busy:
                        // do something when busy
                        break;
                    case ContactAvailability.DoNotDisturb:
                        // do something when DND
                        break;
                    case ContactAvailability.Free:
                        // do something when FREE
                        break;
                }
            }
        }

    }
}
