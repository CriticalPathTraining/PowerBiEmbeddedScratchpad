# The Power BI Embedded Scratchpad App
**PowerBiEmbeddedScratchpad** is a Visual Studio project for a C# console  application which demonstrates techniques for embedding Power BI reports and dashboards on a web page using the Power BI JavaScript API. You can download the PowerBiEmbeddedScratchpad project from this reposity and then open it up using Visual Studio 2017. There are instructions below which explain the steps to set up this sample application which involve creating an app workspace with reports and dashboards and then configuring the project's **app.config** file with the GUID-based IDs for the app workspace, dataset, report and  dashboard in your Power BI environment. 

**PowerBiEmbeddedScratchpad** is a proof of concept (POC) application that abstracts away server-side implementation details (e.g. using ASP.NET MVC vs Node.JS) so you can just examine, test and edit simple HTML pages with  JavaScript code required to use specific techniques with Power BI Embedding. The reason I wrote this ample application is to teach developers how to write the JavaScript code required in specific embedding scenarios  such as navigating between pages, applying bookmarks, applying custom filtering and embedding the Power BI QnA experience. 

----
## Setup
### (A) Create a new app workspace and add content

1. Log into Power BI and create a new app workspace.
2. Inside the PBIX folder, locate the PBIX file named Wingtip Sales Analysis.pbix and upload it to your app workspace
3. Pin visuals from the report named Wingtip Sales Analysis to create new dashboard.

### (B) Get the Configuration Data from the App Workspace 
1. Use the address bar to determine the GIUDs for the App Workspace
2. Use the address bar to determine the GIUDs for the Dataset
3. Use the address bar to determine the GIUDs for the Report
4. Use the address bar to determine the GIUDs for the Dashboard
5. Record these GUIDs in Notepad so you can add them to the sample app

### (C) Create a new Azure AD application in the Azure portal 
1.	Create a new application as a Native app and use a reply URL of https://localhost/app1234
2.	Configure the app with Power BI API permissions – give the app all delegated permissions to Power BI
3.	Click Grant Permissions to grant user consent
4.	Record the application ID (aka Client ID) and the reply url in Notepad

### (D) Download and Open the **PowerBiEmbeddedScratchpad** project
1. Download the project from its GitHub repo and copy the top folder to your local hard drive:
https://github.com/CriticalPathTraining/PowerBiEmbeddedScratchpad/archive/master.zip

2. Open the PowerBiEmbeddedScratchpad sample application in Visual Studio 2017
3. Open **app.config**.
4. Enter the values for ID settings using the GUIDs you copied into Notepad. 
5. Open program.cs. The first method should be uncommented
6.	Run project by pressing **{F5}**.
7.	You should see a page that embeds a simple report.
8.  Experiment by uncommenting other methods in **progam.cs** and examinng the JavaScript programming used to acheive various techniques. 

---
## Related Power BI Embedding Demos
The **[DailyReporterPro](https://github.com/CriticalPathTraining/DailyReporterPro)** sample application demonstrates an application you would create for real customers. It demonstrates how to embed Power BI reports and dashboards in an ASP.NET MVC application using 3rd party embedding (ie.  App Owns Data) and shows how to take advantage of advanced Power BI embedding features such as allowing users to edit existing reports and to create new reports using an existing dataset.