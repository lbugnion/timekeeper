# Deploying Timekeeper to a dependent branch

A _dependent branch_ is a branch of Timekeeper using own branding (colors, logos etc) and living in its own Static Web App instance, but using the common Azure Functions and Azure SignalR service instances.

If you are looking to deploy a completely independent version of Timekeeper (using its own instance of Azure Functions and Azure SignalR service), [please check this page](../independent-branch/index.md).

## Preparing the new branch

1. Clone or fork this repo.
    1. If you already have this repo cloned or forked locally, make sure that you have the latest by either pulling from origin (local clone) or fetch upstream (local fork).
1. Checkout the `main` branch.
1. Create a new branch. In this document we will refer to it as `your-new-branch`.
1. With the new branch checked out, open `Client\wwwroot\appsettings.json`.
1. Replace the GUID under `BranchId` with a new one.
1. Open `Client\wwwroot\appsettings.Development.json`.
1. Replace the GUID under `BranchId` with a new one.
1. Replace `Client\wwwroot\images\header-logo.png` with a new logo.
1. Open `Client\wwwroot\css\branch.css`.
1. Edit the CSS styles.
1. Open `Client\Model\Branding.cs`.
1. Edit the properties.

### Setting the authorization

If you want the branch to be protected by roles, you need to set it up as follows:

1. In `Client\Model\Branding.cs`, set `MustAuthorize` to `true`.
1. Open `Client\wwwroot\routes.json`.
1. Modify the json as follows:

```json
{
  "routes": [
    {
      "route": "/api/*",
      "allowedRoles": [ "host" ]
    },
    {
      "route": "/*",
      "serve": "/index.html",
      "statusCode": 200
    }
  ]
}
```

## Checking the changes

If you want to verify your changes, you can run the new branch locally. Follow the steps described in [Running the app locally](../running-locally/index.md).

## Preparing the new Azure Static Web Apps instance

Once you are satisfied with your changes, you can prepare the code for automated deployment into a new Azure Static Web App instance.

1. Commit all your changes and push the new branch to the repo.
1. Go to the [Azure portal](https://portal.azure.com/).
1. Create a new Static Web App instance with the following settings:
    1. Create a new resource group.
    1. Enter the app name.
    1. Select the hosting plan you need.
    1. Select the region.
    1. Select `GitHub` under `Deployment details`.
    1. Click on `Sign in with GitHub`.
    1. Authorize Azure Static Web Apps to access your GitHub account.
    1. After you are authorized, select the organization into which your repo is located.
    1. Select the repository.
    1. Select the branch you just pushed.
    1. Under `Build Presets`, select `Blazor`.
    1. Leave `App location`, `Api location` and `Output location` to the defaults.
    1. Click `Review and create`.
    1. Click `Create`.
    1. Wait until the deployment is complete.
    1. Switch to the [Timekeeper repo's actions](https://github.com/lbugnion/timekeeper/actions).
    1. Wait until the action successfully completes.

## Configuring the Static Web App instance

1. Navigate to the new Static Web App instance in the Azure portal.
1. Open the Application Insights tab.
    1. Set `Enable Application Insights` to `Yes`.
    1. Leave the other options as is.
    1. Click on `Save`.
1. Open the Configuration tab.
    1. Set `AzureSignalRConnectionString` to the connection string for the Azure SignalR service instance.
    1. Set `AzureStorage` to the connection string for the Storage account.
1. Save the configuration.

> To obtain the connection strings, contact [support@timekeeper.cloud](mailto:support@timekeeper.cloud).

## Setting a custom domain (optional)

1. Configure the custom domain with your domain name provider.
    1. Add a CNAME record.
    1. Point the CNAME record to the Static Web App's domain.
1. Go back to the Static web app instance.
1. Click the Custom domain tab.
1. Click `Add`.
1. Enter the custom domain you just configured.
1. Click `Next`.
1. Click `Add`.

> It can take a few minutes for the configuration to come through.

## Setting the roles

If you decided to protect your branch, follow the steps to configure the roles:

1. Open the Role management tab.
1. Invite the hosts as needed.
    1. Click on Invite.
    1. Select `GitHub` or `Twitter` as `Authentication provider`.
    1. Enter the new host's handle or username.
    1. Leave the domain as is.
    1. Set the `Role` to `host`.
    1. Set the expiration time as needed.
    1. Click on `Generate`.
    1. Copy the link and send to the new host so he can log in and register.

## Setting CORS

The last step is setting up CORS in the Azure Functions instance.

1. Open the Overview tab.
1. Copy the `URL`.
1. Open the Azure Functions application.
1. Open the CORS tab.
1. Add the URL you just copied.
1. Save.

At this point the application should run fine.

## Additional configuration (optional)

Optionally you can change the following settings:

- Change the name of the GitHub action that was generated for your Static Web App instance to follow the conventions of the other actions.
- Change the name of the GitHub token used by this action.
