# MyTranscripts

__MyTranscripts__ is a Blazor Server web application that leverages Azure Cognitive Services and Microsoft Graph to provide a simple way to transcribe audio files and store them in OneDrive.

## Prerequisites

- An [Azure subscription](https://azure.microsoft.com/free/) with permissions to create the required resources
- An [Azure Cognitive Services](https://docs.microsoft.com/azure/cognitive-services/cognitive-services-apis-create-account) resource or [Speech Services](https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started) resource
- An [Azure Active Directory App Registration](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app) with the following permissions:
  - `Files.ReadWrite.All` (OneDrive)
  - `User.Read` (User profile)
- [OneDrive](https://onedrive.live.com/about/en-us/) account
- (Optional) An [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal) resource

## Azure setup

1. Create the Azure Cognitive Services resource or Speech Services resource. Make sure to copy the key and region for later use.
2. Create the Azure Active Directory App Registration, and create a client secret for the app registration. Make sure to copy the client ID and client secret for later use. 
3. Add the `Files.ReadWrite.All` and `User.Read` permissions to the app registration. 
4. Add the redirect URIs `http://localhost:5297/signin-oidc` and `https://localhost:7074/signin-oidc` to the app registration.
5. (Optional) Create the Azure Key Vault resource. Make sure to copy the key vault name for later use.

## Application setup

1. Clone the repository.
2. Replace the `{YOUR_SPEECH_SERVICE_REGION}` and `{YOUR_SPEECH_SERVICE_SUBSCRIPTION_KEY}` placeholders in `appsettings.json` respectively with your Azure Cognitive Services region and key.
3. Replace the `{YOUR_TENANT_ID}`, `{YOUR_CLIENT_ID}`, and `{YOUR_CLIENT_SECRET}` placeholders in `appsettings.json` respectively with your app registration tenant ID, client ID, and client secret. Set the Tenant ID to `common` if you want to allow users from any tenant to sign in to the application.
4. (Optional) Set the `KeyVaultName` value in `appsettings.json` to the name of your Azure Key Vault. You can use the key vault to store your Azure Cognitive Services key and endpoint and your Microsoft Graph application registration client ID and client secret instead of storing them in `appsettings.json`. If you choose to use the key vault, make sure to create the secrets in the key vault with the same names as the values in `appsettings.json`, and to make sure your application has access to the key vault.
5. Run the application, and sign in with a Microsoft account that has access to OneDrive.

## Usage

1. Click the bottom-right `+` button to open the upload dialog.
2. Select an audio file to upload.
3. Select the language of the audio file.
4. Click the `Start` button to upload the file and start the transcription.
5. The transcription progress will be displayed on the main page.
6. Once the transcription is complete, click the `Save` button to open the save dialog.
7. Enter a name for the transcript.
8. Click the `Save` button to upload the transcript to OneDrive as a .txt file.

## Current limitations

- Only .wav files are supported
- The application has only been tested with short-medium length audio files (less than 5 minutes)

## Deploying to Azure

This repository contains an automated workflow that will deploy the application to Azure App Service. To deploy the application to Azure, follow these steps:

1. Fork the repository.
2. Create an Azure App Service resource.
3. Set the publish profile for the Azure App Service resource in the repository secrets as `AZURE_PUBLISH_PROFILE`.
4. Set the Aure App Service name in the repository secrets as `AZURE_APP_SERVICE_NAME`.

The application will be deployed to the Azure App Service when a commit is pushed to the `main` branch.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.