using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Graph;
using Azure.Identity;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ASPNETMSAL
{
    /// <summary>
    /// Helper class for functions related to MS Graph API call to help with Outlook events related CRUD ops
    /// </summary>
    public class OutlookServicesHelper
    {
        //GraphServiceCLient instance to run MS Graph API CRUD Operations
        GraphServiceClient _graphclient;
        
        // Class constrcutor requires an access token
        public OutlookServicesHelper(string accesstoken)
        {
            //Initilize graphservice client instance with access token
            _graphclient= new GraphServiceClient(
                            new DelegateAuthenticationProvider( 
                                (requestMessage) =>
                                {
                                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accesstoken);
                                    return Task.FromResult(0);
                                }));

        }

        /// <summary>
        /// Retreives all Outlook Events
        /// </summary>
        public Task<IUserEventsCollectionPage> Events
        {
            get
            {
                return _graphclient.Me.Events.Request().GetAsync();
            }
        }

        /// <summary>
        /// Create a new Outlook Event
        /// </summary>
        /// <param name="toadd"></param>
        /// <returns></returns>
        public async Task<Microsoft.Graph.Event> AddEvent(Microsoft.Graph.Event toadd)
        {
            HttpClient cl = new HttpClient();
            var addedev=await _graphclient.Me.Events.Request().AddAsync(toadd);
            return addedev;
        }
        /// <summary>
        /// Retreive an Outlook Event by Event Id
        /// </summary>
        /// <param name="evid"> Event ID</param>
        /// <returns></returns>
        public async Task<Microsoft.Graph.Event> GetEventById(string evid)
        {
            // 
            var addedev = await _graphclient.Me.Events[evid].Request()
                .Header("Prefer", "outlook.body-content-type='text'") // adding this header retreives the Event body without HTML tags
                .GetAsync();
            return addedev;
        }

        /// <summary>
        /// Update and existing Outlook Event 
        /// </summary>
        /// <param name="evid">Event Id</param>
        /// <param name="toupdate"> Properties to update</param>
        /// <returns></returns>
        public async Task<Microsoft.Graph.Event> UpdateEvent(string evid, Microsoft.Graph.Event toupdate)
        {
            var addedev = await _graphclient.Me.Events[evid].Request().UpdateAsync(toupdate);
            return addedev;
        }

        /// <summary>
        /// Delete an Outlook Event
        /// </summary>
        /// <param name="id"> Event Id to delete</param>
        /// <returns></returns>
        public async Task DeleteEvent(string id)
        {
            await _graphclient.Me.Events[id].Request().DeleteAsync();
           
        }

    }

    /// <summary>
    /// Helper class to help with Azure AD Authentication and access token related operations
    /// For this POC , Oauth Authorizatin code flow is used which is a two legged flow to get the access token
    /// First leg - Get the auth code from Azure AD authorization end point
    /// Second leg - Exchange the auth code with Access token through access token end point 
    /// Both these are handled automatically through MSAL Cient ConfidentialClientApplication class methods.
    /// </summary>
    public class AuthenticationServicesHelper
    {
    
        /// <summary>
        /// Get Client ID from Web.Config
        /// </summary>
        public string ClientId
        {
            get { return ConfigurationManager.AppSettings["ClientId"];  }
        }

        /// <summary>
        /// Get Client Secret from Web.Config
        /// </summary>
        public string ClientSecret
        {
            get { return ConfigurationManager.AppSettings["ClientSecret"]; }
        }

        /// <summary>
        /// Get Redirecturl from Web.Config
        /// </summary>
        public string RedirectUrl
        {
            get { return ConfigurationManager.AppSettings["RedirectUrl"]; }
        }

        /// <summary>
        /// Get Tenant ID from Web.Config
        /// </summary>
        public string Tenant
        {
            get { return ConfigurationManager.AppSettings["Tenant"]; }
        }

        /// <summary>
        /// Get OAuth scopes from Web.Config
        /// </summary>
        public string[] Scopes
        {
            get { return ConfigurationManager.AppSettings["Scopes"].Split('|'); }
        }

        string _cachefilepath;
        
        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="cachefilepath"> Local cachefile path to store tokens. This will be a unique file path for each logged in user</param>
        public AuthenticationServicesHelper(string cachefilepath)
        {
            _cachefilepath = cachefilepath;
            if (!System.IO.File.Exists(CacheFilePath)) System.IO.File.WriteAllText(CacheFilePath, "{}");
        }

       
        IConfidentialClientApplication ccl;
       
        /// <summary>
        /// Initialize IConfidentialClientApplication instance. This class abstracts functionalities related to authorization of 
        /// server applications like ASP.net which can store the client secret safely in a web.config file or some other location 
        /// hidden from users. For client side applications like HTML/Javascript, Angular, SPA , etc PublicClientApplication Class is used
        /// </summary>
        public IConfidentialClientApplication AuthenticationClient
        {
            get
            {

                if (ccl == null)
                {
                    ccl = ConfidentialClientApplicationBuilder.Create(ClientId)
                       .WithClientSecret(ClientSecret)
                       .WithRedirectUri(RedirectUrl) // this is the url where the authorization flow redirects with auth code and access token
                       .WithAuthority("https://login.microsoftonline.com/" + Tenant)
                       .Build();

                    // Below methods set the location where the token cache will persist . In our case 
                    //its the local cache file
                    ccl.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
                    ccl.UserTokenCache.SetAfterAccess(AfterAccessNotification);
                    


                }

                return ccl;
            }
        }

        private static readonly object FileLock = new object();

        /// <summary>
        /// retreives the token from local cache file for the user and deserializes into appropriate format 
        /// </summary>
        /// <param name="args"></param>
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(System.IO.File.ReadAllBytes(CacheFilePath));
                
            }
        }

        /// <summary>
        /// Updates token cache file is there's any change in existing tokens or additon of new token
        /// </summary>
        /// <param name="args"></param>
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    System.IO.File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
                }
            }


        }


        /// <summary>
        /// Generates the Authorization url (to initiate first leg of Authentication code flow Oauth )
        /// Application redirects to this url which in turn redirects to Azure AD login screen and allows users 
        /// to sign in and grant access to scoped permissions . It then automatically redirects to the url gotten from
        /// RedirectUrl property with 'code' parameter . The application then gets the auth code from code parameter in URL
        /// and proceeds to second authorization leg - getting access token.
        /// </summary>
        public Task<Uri> AuthorizationUri
        {
            get 
            {
                return  AuthenticationClient.GetAuthorizationRequestUrl(Scopes).ExecuteAsync();                
            }
        }

        /// <summary>
        ///  This is the second leg of OAuth authorization process. Using the auth code , this method gets the access token
        ///  which can be used to run graph API operations
        /// </summary>
        /// <param name="authcode"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> GetAccessToken(string authcode)
        {
            AuthenticationResult res = await AuthenticationClient.AcquireTokenByAuthorizationCode(Scopes, authcode).ExecuteAsync();
            return res;
        }

        /// <summary>
        /// Get all Accounts stored in User's token cache file
        /// Please note: this will be replaced by an alternative in future since GetAccountsAsync() method has been deprecated.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IAccount>> GetAllAccounts()
        {
            IEnumerable<IAccount> res = await AuthenticationClient.GetAccountsAsync();
            return res;
        }

        public IEnumerable<LoginAccount> GetAllAccountsFromCache()
        {
            //IEnumerable<IAccount> res = await AuthenticationClient.GetAccountsAsync();
            string strcache = System.IO.File.ReadAllText(CacheFilePath);
            
            JObject res = JObject.Parse(strcache);
            JToken acct= res.SelectToken("$.Account");
            IEnumerable<LoginAccount> allaccts = acct.Children().Select(t=>new LoginAccount {AccountId=t.First().Value<string>("home_account_id"),AccountName= t.First().Value<string>("username") });           
            return allaccts;
        }

        /// <summary>
        /// This removed an account from User's token cache file 
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAccount(string identifier)
        {
            try
            {
                IAccount iacc = await AuthenticationClient.GetAccountAsync(identifier);
                await AuthenticationClient.RemoveAsync(iacc);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Retreive access token from in memory cache. However since we have implemented persistence to local
        /// cache file , this will get us the access token from local cache file
        /// </summary>
        /// <param name="account"> account id</param>
        /// <returns></returns>
        public async Task<string> GetAccessTokenfromCache(string account)
        {
            AuthenticationResult res = await AuthenticationClient.AcquireTokenSilent(Scopes, account).ExecuteAsync();
            return res.AccessToken;
        }

        string CacheFilePath
        {
            get
            {
                //string strpath= HttpContext.Current.Server.MapPath("App_Data/") + "cache.json";                
                return _cachefilepath;//HttpContext.Current.Server.MapPath("App_Data/") + "/cache.json";
            }
        }

    }


    public class LoginAccount
    {
        [JsonProperty("id")]
        public string AccountId{get;set;}
        public string AccountName{get;set; }
    }
}