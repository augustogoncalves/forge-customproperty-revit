# Extract Compound Layer information with Revit engine

![Platforms](https://img.shields.io/badge/Webapp-Windows|MacOS|Linux-lightgray.svg)
![.NET Core](https://img.shields.io/badge/.NET%20Core-3.0-blue.svg)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-3.0-blue.svg)](https://asp.net/)
[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer.autodesk.com/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer.autodesk.com/)
[![Design-Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)

![Platforms](https://img.shields.io/badge/Plugins-Windows-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7-blue.svg)
[![Revit](https://img.shields.io/badge/Revit-2020-lightblue.svg)](http://developer.autodesk.com/)

![Intermediate](https://img.shields.io/badge/Level-Intermediate-green.svg)
[![License](https://img.shields.io/:license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

# Description

This sample demonstrates using the Forge Design Automation for Revit API to extract additional information from a model.
The user can select a RVT file hosted on BIM 360 Document Manager or A360 and view it using the Forge Viewer. 

Once the file has been loaded, a Design Automation workitem will run a .NET plugin in the background to extract Compound Layer Structure information from the file. When ready, the information will be visible in the property panel. 

This sample is based on the [Learn Forge](http://learnforge.autodesk.io) tutorials in the section *List hubs &amp; projects*.

# Thumbnail

![](/thumbnail.gif)

# Setup

## Prerequisites

1. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). 
2. **Visual Studio**: Either Community (Windows) or Code (Windows, MacOS). 
3. **.NET Core**: basic knowledge of C#.
4. **ngrok**: Routing tool, [download here](https://ngrok.com/).
7. **Revit** 2020: required to compile changes into the plugin. Windows only.

Use of this sample requires Autodesk developer credentials.
Visit the [Forge Developer Portal](https://developer.autodesk.com), sign up for an account
and [create an app](https://developer.autodesk.com/myapps/create) that uses Data Management and Model Derivative APIs.
For this new app, use `http://localhost:3000/api/forge/callback/oauth` as Callback URL, although is not used in a 2-legged flow.
Finally, make a note of the **Client ID** and **Client Secret**.

## Running locally

Clone this project or download it.
We recommend installing [GitHub desktop](https://desktop.github.com/).
To clone it via command line, use the following (**Terminal** on MacOSX/Linux, **Git Shell** on Windows):

    git clone https://github.com/augustogoncalves/forge-customproperty-revit


**Visual Studio** (Windows):

Right-click on the project, then go to **Debug**. Adjust the settings as shown below. 

![](readme/visual_studio_settings.png)

**Visual Sutdio Code** (Windows, MacOS):

Open the folder, at the bottom-right, select **Yes** and **Restore**. This restores the packages (e.g. Autodesk.Forge) and creates the `launch.json` file. See *Tips & Tricks* for .NET Core on MacOS.

![](readme/visual_code_restore.png)

**ngrok**

Run `ngrok http 3000 -host-header="localhost:3000"` to create a tunnel to your local machine, then copy the address into the `FORGE_WEBHOOK_URL` environment variable.

**Environment variables**

At the `.vscode\launch.json`, find the env vars and add your Forge Client ID, Secret and callback URL. Also define the `ASPNETCORE_URLS` variable. The end result should be as shown below:

```json
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS" : "http://localhost:3000",
    "FORGE_CLIENT_ID": "your id here",
    "FORGE_CLIENT_SECRET": "your secret here",
    "FORGE_CALLBACK_URL": "http://localhost:3000/api/forge/callback/oauth",
    "FORGE_WEBHOOK_URL": "your ngrok address here: e.g. http://abcd1234.ngrok.io"
},
```

**Revit plugin**

A compiled version of the `Revit` plugin (.bundles) is included on the `webapp` module in the `wwwroot/bundles` folder.
Any changes to these plugins will require the creation of a new .bundle; the **Post-build** event should handle that.

Start the app.

Open `http://localhost:3000` to start the app and select a RVT file.
A pop-up will indicate when the style information is ready. 

## Deployment

To deploy this application to Heroku, the **Callback URL** for Forge must use your `.herokuapp.com` address.
After clicking on the button below on the the Heroku 'Create New App' page, set your Client ID, Secret and Callback URL for Forge.

[![Deploy](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy)

# Further Reading

Documentation:

- [BIM 360 API](https://developer.autodesk.com/en/docs/bim360/v1/overview/) and [App Provisioning](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps)
- [Data Management API](https://developer.autodesk.com/en/docs/data/v2/overview/)
- [Viewer](https://developer.autodesk.com/en/docs/viewer/v7) 
- [Design Automation](https://forge.autodesk.com/en/docs/design-automation/v3/developers_guide/overview/)

### Troubleshooting

1. **Cannot see my BIM 360 projects**: Make sure to provision the Forge App Client ID within the BIM 360 Account; [learn more here](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps). This requires the Account Admin permission.

2. **error setting certificate verify locations** error: may happen on Windows, use the following: `git config --global http.sslverify "false"`

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

Augusto Goncalves [@augustomaia](https://twitter.com/augustomaia), [Forge Advocate](http://forge.autodesk.com)