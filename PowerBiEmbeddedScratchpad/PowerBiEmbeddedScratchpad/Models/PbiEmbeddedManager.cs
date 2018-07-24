using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using System.IO;
using System.Diagnostics;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.PowerBI.Api.V2.Models.Credentials;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;

namespace PowerBiEmbeddedScratchpad.Models {


  class PbiEmbeddedManager {

    #region "private implemntation details"

    private static string aadAuthorizationEndpoint = "https://login.windows.net/common/oauth2/authorize";
    private static string resourceUriPowerBi = "https://analysis.windows.net/powerbi/api";
    private static string urlPowerBiRestApiRoot = "https://api.powerbi.com/";

    private static string clientId = ConfigurationManager.AppSettings["client-id"];
    private static string redirectUrl = ConfigurationManager.AppSettings["reply-url"];

    private static string workspaceId = ConfigurationManager.AppSettings["app-workspace-id"];
    private static string datasetId = ConfigurationManager.AppSettings["dataset-id"];
    private static string reportId = ConfigurationManager.AppSettings["report-id"];
    private static string dashboardId = ConfigurationManager.AppSettings["dashboard-id"];

    private static string datasetWithRlsId = ConfigurationManager.AppSettings["rls-dataset-id"];
    private static string reportWithRlsId = ConfigurationManager.AppSettings["rls-report-id"];

    private static string GetAccessToken2() {

      string userName = "";
      string userPassword = "";

      // create new authentication context 
      AuthenticationContext authenticationContext = new AuthenticationContext(aadAuthorizationEndpoint);
      AuthenticationResult userAuthnResult;

      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword)) {
        // use authentication context to trigger user sign-in and return access token 
        userAuthnResult = authenticationContext.AcquireTokenAsync(
                                                  resourceUriPowerBi,
                                                  clientId,
                                                  new Uri(redirectUrl),
                                                  new PlatformParameters(PromptBehavior.Auto)).Result;
      }
      else {
        userAuthnResult = authenticationContext.AcquireTokenAsync(
                                                  resourceUriPowerBi,
                                                  clientId,
                                                  new UserPasswordCredential(userName, userPassword)).Result;
      }

      // return access token to caller
      return userAuthnResult.AccessToken;
    }



    private static string GetAccessToken() {

      string userName = ConfigurationManager.AppSettings["aad-account-name"];
      string userPassword = ConfigurationManager.AppSettings["aad-account-password"];

      // create new authentication context 
      AuthenticationContext authenticationContext = new AuthenticationContext(aadAuthorizationEndpoint);
      AuthenticationResult userAuthnResult;

      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword)) {
        // use authentication context to trigger user sign-in and return access token 
        userAuthnResult = authenticationContext.AcquireTokenAsync(
                                                  resourceUriPowerBi,
                                                  clientId,
                                                  new Uri(redirectUrl),
                                                  new PlatformParameters(PromptBehavior.Auto)).Result;
      }
      else {
        userAuthnResult = authenticationContext.AcquireTokenAsync(
                                                  resourceUriPowerBi,
                                                  clientId,
                                                  new UserPasswordCredential(userName, userPassword)).Result;
      }

      // return access token to caller
      return userAuthnResult.AccessToken;
    }

    private static PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiRestApiRoot), tokenCredentials);
    }

    #endregion

    public static ReportEmbeddingData GetReportEmbeddingDataFirstParty() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;
      var accessToken = GetAccessToken();

      return new ReportEmbeddingData {
        reportId = reportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = accessToken
      };

    }

    public static ReportEmbeddingData GetReportEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      // create token request object
      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

      // call to Power BI Service API and pass GenerateTokenRequest object to generate embed token
      string embedToken = pbiClient.Reports.GenerateTokenInGroup(workspaceId,
                                                                 report.Id,
                                                                 generateTokenRequestParameters).Token;

      return new ReportEmbeddingData {
        reportId = reportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static DashboardEmbeddingData GetDashboardEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dashboard = pbiClient.Dashboards.GetDashboardInGroup(workspaceId, dashboardId);
      var embedUrl = dashboard.EmbedUrl;
      var dashboardDisplayName = dashboard.DisplayName;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
      string embedToken = pbiClient.Dashboards.GenerateTokenInGroup(workspaceId, 
                                                                    dashboardId, 
                                                                    generateTokenRequestParameters).Token;

      return new DashboardEmbeddingData {
        dashboardId = dashboardId,
        dashboardName = dashboardDisplayName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static DashboardTileEmbeddingData GetDashboardTileEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var tiles = pbiClient.Dashboards.GetTilesInGroup(workspaceId, dashboardId).Value;

      // retrieve first tile in tiles connection
      var tile = tiles[0]; 
      var tileId = tile.Id;
      var tileTitle = tile.Title;
      var embedUrl = tile.EmbedUrl;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
      string embedToken = pbiClient.Tiles.GenerateTokenInGroup(workspaceId, 
                                                               dashboardId, 
                                                               tileId, 
                                                               generateTokenRequestParameters).Token;

      return new DashboardTileEmbeddingData {
        dashboardId = dashboardId,
        TileId = tileId,
        TileTitle = tileTitle,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static NewReportEmbeddingData GetNewReportEmbeddingData() {

      string embedUrl = "https://app.powerbi.com/reportEmbed?groupId=" + workspaceId;

      PowerBIClient pbiClient = GetPowerBiClient();

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "create", 
                                                                                     datasetId: datasetId);

      string embedToken = pbiClient.Reports.GenerateTokenForCreateInGroup(workspaceId, 
                                                                          generateTokenRequestParameters).Token;

      return new NewReportEmbeddingData {
        workspaceId = workspaceId,
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static NewReportEmbeddingData GetNewReportEmbeddingDataFirstParty() {

      string embedUrl = "https://app.powerbi.com/reportEmbed?groupId=" + workspaceId;

      return new NewReportEmbeddingData {
        workspaceId = workspaceId,
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = GetAccessToken()
      };

    }

    public static QnaEmbeddingData GetQnaEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dataset = pbiClient.Datasets.GetDatasetByIdInGroup(workspaceId, datasetId);

      string embedUrl = "https://app.powerbi.com/qnaEmbed?groupId=" + workspaceId;
      string datasetID = dataset.Id;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

      string embedToken = pbiClient.Datasets.GenerateTokenInGroup(workspaceId, 
                                                                  dataset.Id, 
                                                                  generateTokenRequestParameters).Token;

      return new QnaEmbeddingData {
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static ReportWithRlsEmbeddingData GetReportWithRlsEmbeddingData() {

      var userName = "grumpy.mcfly@toolate4u.com";

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = pbiClient.Reports.GetReportInGroup(workspaceId, reportWithRlsId);
      var datasetId = report.DatasetId;
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      Console.WriteLine("Getting RLS-enabled embed tokens");

      GenerateTokenRequest tokenRequestAllData =
        new GenerateTokenRequest(accessLevel: "view",
                                identities: new List<EffectiveIdentity> {
                                  new EffectiveIdentity(username: userName,
                                                        datasets: new List<string> { datasetWithRlsId },
                                                        roles: new List<string> { "All Sales Regions" })
                                });


      string embedTokenAllData = pbiClient.Reports.GenerateTokenInGroup(workspaceId, reportWithRlsId, tokenRequestAllData).Token;

      EffectiveIdentity identityWesternSales = new EffectiveIdentity(username: userName, roles: new List<string> { "Western Sales Region" }, datasets: new List<string> { datasetWithRlsId });
      GenerateTokenRequest tokenRequestWesternSales = new GenerateTokenRequest("view", null, identities: new List<EffectiveIdentity> { identityWesternSales });
      string embedTokenWesternSales = pbiClient.Reports.GenerateTokenInGroup(workspaceId, report.Id, tokenRequestWesternSales).Token;

      EffectiveIdentity identityCentralSales = new EffectiveIdentity(username: userName, roles: new List<string> { "Central Sales Region" }, datasets: new List<string> { datasetWithRlsId });
      GenerateTokenRequest tokenRequestCentralSales = new GenerateTokenRequest(accessLevel: "view", datasetId: datasetId, identities: new List<EffectiveIdentity> { identityCentralSales });
      string embedTokenCentralSales = pbiClient.Reports.GenerateTokenInGroup(workspaceId, report.Id, tokenRequestCentralSales).Token;

      EffectiveIdentity identityEasternSales = new EffectiveIdentity(userName, roles: new List<string> { "Eastern Sales Region" }, datasets: new List<string> { datasetWithRlsId });
      GenerateTokenRequest tokenRequestEasternSales = new GenerateTokenRequest(accessLevel: "view", datasetId: datasetId, identities: new List<EffectiveIdentity> { identityEasternSales });
      string embedTokenEasternSales = pbiClient.Reports.GenerateTokenInGroup(workspaceId, report.Id, tokenRequestEasternSales).Token;


      EffectiveIdentity identityCombo = new EffectiveIdentity(userName, roles: new List<string> { "Central Sales Region", "Eastern Sales Region" }, datasets: new List<string> { datasetWithRlsId });
      GenerateTokenRequest tokenRequestCombo = new GenerateTokenRequest(accessLevel: "view", datasetId: datasetId, identities: new List<EffectiveIdentity> { identityCombo });
      string embedTokenCombo = pbiClient.Reports.GenerateTokenInGroup(workspaceId, report.Id, tokenRequestCombo).Token;


      return new ReportWithRlsEmbeddingData {
        reportId = report.Id,
        reportName = reportName,
        embedUrl = embedUrl,
        embedTokenAllData = embedTokenAllData,
        embedTokenWesternSales = embedTokenWesternSales,
        embedTokenCentralSales = embedTokenCentralSales,
        embedTokenEasternSales = embedTokenEasternSales,
        embedTokenCombo = embedTokenCombo
      };

    }


  }
}
