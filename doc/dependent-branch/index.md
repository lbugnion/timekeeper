# Deploying Timekeeper to a dependent branch

A _dependent branch_ is a branch of Timekeeper using own branding (colors, logos etc) and living in its own Static Web App instance, but using the common Azure Functions and Azure SignalR service instances.

If you are looking to deploy a completely independent version of Timekeeper (using its own instance of Azure Functions and Azure SignalR service), [please check this page](../independent-branch).

## Preparing the new branch

1. Clone or fork this repo.
    1. If you already have this repo cloned or forked locally, make sure that you have the latest by either pulling from origin (local clone) or fetch upstream (local fork).
1. Checkout the `main` branch.
1. Create a new branch. In this document we will refer to it as `your-new-branch`.
1. 

## Preparing the new Azure Static Web Apps instance