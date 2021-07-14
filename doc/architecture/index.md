# Architecture and workflows

## Definitions

- **Host**: A Timekeeper user with certain privileges.
  - Create sessions.
  - Edit a session.
  - Create, delete or edit clocks.
  - Start and stop clocks.
  - Send messages to other users.
- **Guest**: A Timekeeper user who can only read the clocks and the messages.
- **Peer**: A Host or a Guest connected to the current session.
- **SWA**: Azure Static Web App

## Components

Timekeeper is composed of multiple Azure services and a cross-platform client application written in ASP.NET Blazor.

### Azure Static Web App

The Azure Static Web App (SWA) is in charge of serving the Blazor pages and the Web assembly files to the client. The SWA is also responsible for resolving custom domains, serving the TLS certificate for HTTPS, and managing roles and authentication.

[More information]()

### Azure Functions application (with auth)

### Azure Functions application (open)

### Azure Storage

### Azure SignalR Service

### ASP.NET Blazor

## Serving the pages

The Static Web App is in charge of serving the Blazor pages and the Web assembly files to the client. Because Blazor and Web assembly run on multiple platforms, you can use Timekeeper on Windows PC, Mac computers as well as Android and iOS.

The Static Web App is also responsible for resolving custom domains, serving the TLS certificate for HTTPS, and managing roles and authentication (see below).

## Logging in

> Some Timekeeper branches are using authentication (for example [the HelloWorld branch](https://helloworld.timekeeper.cloud)) while others don't (for example [the public branch](https://timekeeper.cloud)). To learn how to configure a new branch, [see this page](../dependent-branch/index.md).

This process is only required from the Host.

1. The Host requests a protected page from the Static Web App (SWA). The SWA checks if the Host is authorized. If the authorization is not found, or expired, the SWA asks the Host to log in again.
1. There are currently two authentication providers supported by Timekeeper: GitHub and Twitter. The authentication process is handled by these providers.
1. The Host enters his login information in the GitHub, respectively in the Twitter dialog, and the provider confirms the identity to the SWA.

## Connecting and registering

1. The Host or the Guest asks the open Functions app to get the SignalR service connection information.
1. This data is obtained by the open Functions app from the SignalR service [via a binding](TODO). The data is returned to the caller as a JSON object.
1. The open Azure Function sends a notification to SignalR.
1. SignalR notifies all the Hosts that a new Peer has registered. This information is used by the Host to update the number of connected Peers.

## Getting sessions

When a Host enters Timekeeper, the locally saved session is discarded and the Host must choose a session again. This guarantees that the Host always has a "fresh state" of existing sessions. This is especially important if multiple Hosts are active, so the new Host gets all the previous changes.

1. The Host requests the list of sessions from the protected Functions app.
1. The protected Functions app gets the list from the Storage account and returns them to the caller.

## Creating a session

A Host can create a new session, either from scratch or by duplicating an existing session.

1. The Host sends a request to the protected Functions app.
1. The protected Functions app saves the new session (or the edited one) to the Storage.

## Editing a session

A Host can edit an existing session. This includes:

- Editing the session name.
- Adding, deleting or editing a clock.

1. The Host sends the updated session to the protected Functions app.
1. The protected Functions app saves the edited session to the Storage.
1. The protected Functions app sends a notification to the SignalR service.
1. The SignalR service notifies all the Hosts that the current session changed.

## Host enters a session

When a Host enters a session, the session is fetched from the Storage to guarantee that the state is as fresh as possible. Also the other Hosts are notified.

1. The Host requests the list of sessions from the protected Functions app.
1. The protected Functions app gets the list from the Storage and returns it to the caller. The Host then selects the correct session based on the session ID.
1. The Host registers and connects via the open Functions app.
1. The open Functions app notifies the SignalR service that a new Host is registered.
1. The SignalR service notifies all the Hosts that a new Host is present.

## Guest enters a session

When a Guest enters a session, the process is simplified.

1. The Guest registers and connects via the open Functions app.
1. The open Functions app notifies the SignalR service that a new Guest is registered.
1. The SignalR service notifies all the Hosts that a new Guest is present.

## Guest edits their name

> Not all Timekepeer branches allow a Guest to edit their name.

## Host starts a Clock

## Host stops a Clock

## Host nudges a Clock

## Host sends a message