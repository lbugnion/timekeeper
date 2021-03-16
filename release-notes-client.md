# Release notes for [Client](https://github.com/lbugnion/timekeeper/projects/1)

## Known issues (still open)

### Planned for [V0.5.4.2 UI enhancements](https://github.com/lbugnion/timekeeper/milestone/20) *(open)*

[bug # 96](https://github.com/lbugnion/timekeeper/issues/96) *(open)*: /{session} redirects to /guest/{session} but causes an error in the UI

[enhancement # 87](https://github.com/lbugnion/timekeeper/issues/87) *(open)*: When a guest connects, send the latest message with the clock too

[enhancement # 86](https://github.com/lbugnion/timekeeper/issues/86) *(open)*: Separate the lines more in the Message section

### Planned for [V0.6 Enhancements](https://github.com/lbugnion/timekeeper/milestone/15) *(open)*

[enhancement # 95](https://github.com/lbugnion/timekeeper/issues/95) *(open)*: Enable Live metrics for Blazor client

[enhancement # 92](https://github.com/lbugnion/timekeeper/issues/92) *(open)*: Support multiple hosts

[enhancement # 88](https://github.com/lbugnion/timekeeper/issues/88) *(open)*: When a host reconnects, it should get all the information about running clocks, last message sent, number of guests 

[enhancement # 82](https://github.com/lbugnion/timekeeper/issues/82) *(open)*: When a guest reconnects, Announce their name to the Host

### Planned for [V0.5.5 Active clocks only](https://github.com/lbugnion/timekeeper/milestone/17) *(open)*

[enhancement # 77](https://github.com/lbugnion/timekeeper/issues/77) *(open)*: consider only showing active clocks to guests?

### Planned for [V0.6 Groups](https://github.com/lbugnion/timekeeper/milestone/10) *(open)*

[question # 73](https://github.com/lbugnion/timekeeper/issues/73) *(open)*: Should we also show the number of connected guests on the Guest page?

[bug # 67](https://github.com/lbugnion/timekeeper/issues/67) *(open)*: In case of error, hide the Edit Session Name link

[enhancement # 65](https://github.com/lbugnion/timekeeper/issues/65) *(open)*: Move the guest list to the right when window is wide enough 

[enhancement # 48](https://github.com/lbugnion/timekeeper/issues/48) *(open)*: When Host changes the session name, broadcast to all the logged in guests

[enhancement # 45](https://github.com/lbugnion/timekeeper/issues/45) *(open)*: Support multiple sessions for the Host to pre-create and log into

[enhancement # 33](https://github.com/lbugnion/timekeeper/issues/33) *(open)*: Prevent multiple hosts to interact with the Start / Stop functions.

[enhancement # 20](https://github.com/lbugnion/timekeeper/issues/20) *(open)*: Show Connected status (icon)

### Planned for [Later](https://github.com/lbugnion/timekeeper/milestone/2) *(open)*

[question # 64](https://github.com/lbugnion/timekeeper/issues/64) *(open)*: Should we add possibility to send a message from a guest to the host? 

[enhancement # 63](https://github.com/lbugnion/timekeeper/issues/63) *(open)*: Add possibilty to send message to one guest only or to all

[question # 50](https://github.com/lbugnion/timekeeper/issues/50) *(open)*: Instead of using GUID for session ID, can we use simple random names 

[enhancement # 37](https://github.com/lbugnion/timekeeper/issues/37) *(open)*: Show Busy status (icon)

[enhancement # 22](https://github.com/lbugnion/timekeeper/issues/22) *(open)*: Find way to keep Functions Key out of GitHub

[enhancement # 15](https://github.com/lbugnion/timekeeper/issues/15) *(open)*: Settings page for Host where they can curate the predefined messages

[enhancement # 14](https://github.com/lbugnion/timekeeper/issues/14) *(open)*: Add a list of predefined message that the Host can send

[enhancement # 13](https://github.com/lbugnion/timekeeper/issues/13) *(open)*: Add a "Resume clock" function

[enhancement # 10](https://github.com/lbugnion/timekeeper/issues/10) *(open)*: Support QR code for joining a group

## Closed issues

### Fixed issues in [V0.5.4 Nudge up and down](https://github.com/lbugnion/timekeeper/milestone/18) *(closed on 08 Mar 2021)*

[enhancement # 76](https://github.com/lbugnion/timekeeper/issues/76) *(closed on 08 Mar 2021)*: Nudge Up|Down buttons for active timer

### Fixed issues in [V0.5.3.2 UI enhancements](https://github.com/lbugnion/timekeeper/milestone/16) *(closed on 28 Feb 2021)*

[enhancement # 75](https://github.com/lbugnion/timekeeper/issues/75) *(closed on 28 Feb 2021)*: bind [ENTER] to post the message, same as the [Send Message] button click

### Fixed issues in [V0.5.2 Security](https://github.com/lbugnion/timekeeper/milestone/12) *(closed on 21 Feb 2021)*

[enhancement # 68](https://github.com/lbugnion/timekeeper/issues/68) *(closed on 21 Feb 2021)*: Move API to Static web app and secure

### Fixed issues in [V0.5.1 Branding](https://github.com/lbugnion/timekeeper/milestone/13) *(closed on 21 Feb 2021)*

[enhancement # 72](https://github.com/lbugnion/timekeeper/issues/72) *(closed on 21 Feb 2021)*: Show number of connected guests in LARGE on the top right

[enhancement # 71](https://github.com/lbugnion/timekeeper/issues/71) *(closed on 21 Feb 2021)*: Create favicon.ico for Hello World

[bug # 70](https://github.com/lbugnion/timekeeper/issues/70) *(closed on 21 Feb 2021)*: Branding doesn't work on Saturdays and Sundays 

[enhancement # 69](https://github.com/lbugnion/timekeeper/issues/69) *(closed on 21 Feb 2021)*: Implement branding for Hello World Backstage Channel

[enhancement # 31](https://github.com/lbugnion/timekeeper/issues/31) *(closed on 21 Feb 2021)*: Create favicon.ico

### Fixed issues in [V0.3 Bugfix](https://github.com/lbugnion/timekeeper/milestone/11) *(closed on 21 Feb 2021)*

[bug, wontfix # 66](https://github.com/lbugnion/timekeeper/issues/66) *(closed on 21 Feb 2021)*: When a Guest session is disconnected, the Guest Name is deleted

[bug, wontfix # 61](https://github.com/lbugnion/timekeeper/issues/61) *(closed on 21 Feb 2021)*: When the connection to the API fails, the Delete Session button doesn't work

[bug # 60](https://github.com/lbugnion/timekeeper/issues/60) *(closed on 21 Feb 2021)*: When the connection to the API fails, make sure that the link for the guests is hidden

[bug, wontfix # 57](https://github.com/lbugnion/timekeeper/issues/57) *(closed on 21 Feb 2021)*: Create a new session, go to Configure page and reload

### Fixed issues in [V0.5 Multi-timers](https://github.com/lbugnion/timekeeper/milestone/9) *(closed on 21 Feb 2021)*

[enhancement # 58](https://github.com/lbugnion/timekeeper/issues/58) *(closed on 21 Feb 2021)*: Plan to have multiple timers per session

### Fixed issues in [V0.4.0 Groups](https://github.com/lbugnion/timekeeper/milestone/7) *(closed on 20 Feb 2021)*

[enhancement # 49](https://github.com/lbugnion/timekeeper/issues/49) *(closed on 20 Feb 2021)*: When guest changes their name, let the Host know

[enhancement # 47](https://github.com/lbugnion/timekeeper/issues/47) *(closed on 20 Feb 2021)*: When guest logs in, send message to the host to let them know a guest joined

[wontfix # 44](https://github.com/lbugnion/timekeeper/issues/44) *(closed on 20 Feb 2021)*: Implement a back channel for management messages 

[enhancement # 38](https://github.com/lbugnion/timekeeper/issues/38) *(closed on 20 Feb 2021)*: Show a warning dialog before deleting the session

### Fixed issues in [V0.3 Settings](https://github.com/lbugnion/timekeeper/milestone/8) *(closed on 18 Feb 2021)*

[enhancement # 56](https://github.com/lbugnion/timekeeper/issues/56) *(closed on 18 Feb 2021)*: Implement reconnect button

[enhancement # 55](https://github.com/lbugnion/timekeeper/issues/55) *(closed on 18 Feb 2021)*: Host can edit the friendly name of the session

[enhancement # 43](https://github.com/lbugnion/timekeeper/issues/43) *(closed on 18 Feb 2021)*: Allow the Host to give a friendly name to the session. 

[enhancement # 27](https://github.com/lbugnion/timekeeper/issues/27) *(closed on 18 Feb 2021)*: Add color configuration dialog to settings page

[enhancement # 17](https://github.com/lbugnion/timekeeper/issues/17) *(closed on 18 Feb 2021)*: Settings page for Host where they can set Red and Yellow time

[enhancement # 16](https://github.com/lbugnion/timekeeper/issues/16) *(closed on 18 Feb 2021)*: Settings page for Host where they can set the countdown time

### Fixed issues in [V0.2 Groups](https://github.com/lbugnion/timekeeper/milestone/4) *(closed on 15 Feb 2021)*

[bug # 51](https://github.com/lbugnion/timekeeper/issues/51) *(closed on 15 Feb 2021)*: "Stop the clock" doesn't work in the Test branch

[enhancement # 46](https://github.com/lbugnion/timekeeper/issues/46) *(closed on 15 Feb 2021)*: Implement direct login for Guest by using a unique link with session ID

[enhancement # 9](https://github.com/lbugnion/timekeeper/issues/9) *(closed on 15 Feb 2021)*: Use Groups to implement channels on single SignalR hub

### Fixed issues in [V0.0](https://github.com/lbugnion/timekeeper/milestone/5) *(closed on 13 Feb 2021)*

[enhancement # 3](https://github.com/lbugnion/timekeeper/issues/3) *(closed on 13 Feb 2021)*: Create a basic client to demonstrate the timekeeping functionality

### Fixed issues in [V0.1](https://github.com/lbugnion/timekeeper/milestone/1) *(closed on 10 Feb 2021)*

[bug # 26](https://github.com/lbugnion/timekeeper/issues/26) *(closed on 10 Feb 2021)*: Guests don't start the clock when receiving the instruction

[bug # 25](https://github.com/lbugnion/timekeeper/issues/25) *(closed on 10 Feb 2021)*: Colored background doesn't show right on time

[enhancement # 11](https://github.com/lbugnion/timekeeper/issues/11) *(closed on 10 Feb 2021)*: Disable Start and Stop buttons according to state

[enhancement # 7](https://github.com/lbugnion/timekeeper/issues/7) *(closed on 10 Feb 2021)*: Simplify the client layout

### Fixed issues in [V0.5.4.1 UI enhancements](https://github.com/lbugnion/timekeeper/milestone/19) *(open)*

[enhancement # 91](https://github.com/lbugnion/timekeeper/issues/91) *(closed on 01 Jan 0001)*: Simplify how branches are created and configured with different branding

[enhancement # 89](https://github.com/lbugnion/timekeeper/issues/89) *(closed on 01 Jan 0001)*: Synchronize clocks better when Starting all clocks, and when multiple clocks run on one machine

[bug # 85](https://github.com/lbugnion/timekeeper/issues/85) *(closed on 01 Jan 0001)*: Improve the Host header design for mobile 

[enhancement # 84](https://github.com/lbugnion/timekeeper/issues/84) *(closed on 01 Jan 0001)*: Make the Text field a Text area

[enhancement # 83](https://github.com/lbugnion/timekeeper/issues/83) *(closed on 01 Jan 0001)*: Move the About text to its own page

[enhancement # 81](https://github.com/lbugnion/timekeeper/issues/81) *(closed on 01 Jan 0001)*: Pass branding information to client via Branding class that can easily be customized

[enhancement # 80](https://github.com/lbugnion/timekeeper/issues/80) *(closed on 01 Jan 0001)*: Make Hello World changes in a branch and deploy to a separate Static web app

[enhancement # 78](https://github.com/lbugnion/timekeeper/issues/78) *(closed on 01 Jan 0001)*: Add About page and remove the "in your face" text on the Host and Guest pages

### Fixed issues in [V0.6 Groups](https://github.com/lbugnion/timekeeper/milestone/10) *(open)*

[enhancement # 62](https://github.com/lbugnion/timekeeper/issues/62) *(closed on 01 Jan 0001)*: Add guest count on top / right next to the Connected icon

[enhancement # 12](https://github.com/lbugnion/timekeeper/issues/12) *(closed on 01 Jan 0001)*: Save clock settings to storage so client continues to run even in case of a reload

### Fixed issues in [Later](https://github.com/lbugnion/timekeeper/milestone/2) *(open)*

[question # 42](https://github.com/lbugnion/timekeeper/issues/42) *(closed on 01 Jan 0001)*: Should we allow multiple clocks to be started on one single PC? 

[enhancement # 8](https://github.com/lbugnion/timekeeper/issues/8) *(closed on 01 Jan 0001)*: Separate page in Host and Guest pages

### Fixed issues in [No milestone set](https://github.com/lbugnion/timekeeper/issues?q=is%3Aissue+is%3Aclosed) *(open)*

[enhancement # 21](https://github.com/lbugnion/timekeeper/issues/21) *(closed on 01 Jan 0001)*: Disable UI when error occurs

