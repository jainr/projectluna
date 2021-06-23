// var BASE_URL = "https://localhost:44334/api"
//var BASE_URL = "https://lunaaitest-apiapp.azurewebsites.net/api";
var BASE_URL = "https://lunatest-gateway.azurewebsites.net/api";
var HEADER_HEX_COLOR = "#3376CD";
var SITE_TITLE = "Luna Machine Learning Gallery";
var MSAL_CONFIG = {
  tenantId: "72f988bf-86f1-41af-91ab-2d7cd011db47",
  appId: "1158aaa3-b79f-42b4-8c07-10b7da5fb0fb",
  //redirectUri: "https://lunaaitest-portal.azurewebsites.net/",
  redirectUri: "http://localhost:3000/",
  scopes: [
    "user.read",
    "User.ReadBasic.All"
  ]
};
