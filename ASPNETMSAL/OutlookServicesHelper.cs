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
using System.IO;
using System.Web;

namespace ASPNETMSAL
{
    public class OutlookServicesHelper
    {
        GraphServiceClient _graphclient;
        public OutlookServicesHelper(string accesstoken)
        {
            _graphclient= new GraphServiceClient(
                            new DelegateAuthenticationProvider(
                                (requestMessage) =>
                                {
                                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accesstoken);
                                    return Task.FromResult(0);
                                }));

        }


        public Task<IUserEventsCollectionPage> Events
        {
            get
            {
                return _graphclient.Me.Events.Request().GetAsync();
            }
        }

        public async Task<Microsoft.Graph.Event> AddEvent(Microsoft.Graph.Event toadd)
        {
            HttpClient cl = new HttpClient();
            var addedev=await _graphclient.Me.Events.Request().AddAsync(toadd);
            return addedev;
        }

        public async Task<Microsoft.Graph.Event> GetEventById(string evid)
        {
            var addedev = await _graphclient.Me.Events[evid].Request().Header("Prefer", "outlook.body-content-type='text'").GetAsync();
            return addedev;
        }

        public async Task<Microsoft.Graph.Event> UpdateEvent(string evid, Microsoft.Graph.Event toupdate)
        {
            var addedev = await _graphclient.Me.Events[evid].Request().UpdateAsync(toupdate);
            return addedev;
        }

        public async Task DeleteEvent(string id)
        {
            await _graphclient.Me.Events[id].Request().DeleteAsync();
            //return Task.CompletedTask;
           
        }

    }

    public class AuthenticationServicesHelper
    {
    
        public string ClientId
        {
            get { return ConfigurationManager.AppSettings["ClientId"];  }
        }
        public string ClientSecret
        {
            get { return ConfigurationManager.AppSettings["ClientSecret"]; }
        }
        public string RedirectUrl
        {
            get { return ConfigurationManager.AppSettings["RedirectUrl"]; }
        }
        public string Tenant
        {
            get { return ConfigurationManager.AppSettings["Tenant"]; }
        }
        public string[] Scopes
        {
            get { return ConfigurationManager.AppSettings["Scopes"].Split('|'); }
        }

        string _cachefilepath;
        public AuthenticationServicesHelper(string cachefilepath)
        {
            _cachefilepath = cachefilepath;
            if (!System.IO.File.Exists(CacheFilePath)) System.IO.File.WriteAllText(CacheFilePath, "{}");
        }

        IConfidentialClientApplication ccl;
        public IConfidentialClientApplication AuthenticationClient
        {
            get
            {

                if (ccl == null)
                {
                    ccl = ConfidentialClientApplicationBuilder.Create(ClientId)
                       .WithClientSecret(ClientSecret)
                       .WithRedirectUri(RedirectUrl)
                       .WithAuthority("https://login.microsoftonline.com/" + Tenant)
                       .Build();

                    ccl.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
                    ccl.UserTokenCache.SetAfterAccess(AfterAccessNotification);
                }

                return ccl;
            }
        }

        private static readonly object FileLock = new object();

        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(System.IO.File.ReadAllBytes(CacheFilePath));
            }
        }

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



        public Task<Uri> AuthorizationUri
        {
            get 
            {
                return  AuthenticationClient.GetAuthorizationRequestUrl(Scopes).ExecuteAsync();                
            }
        }

        public async Task<AuthenticationResult> GetAccessToken(string authcode)
        {
            AuthenticationResult res = await AuthenticationClient.AcquireTokenByAuthorizationCode(Scopes, authcode).ExecuteAsync();
            return res;
        }

        public async Task<IEnumerable<IAccount>> GetAllAccounts()
        {
            IEnumerable<IAccount> res = await AuthenticationClient.GetAccountsAsync();
            return res;
        }

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

    //public class CosmosDBManager
    //{

    //    public static string CosmosConnection
    //    {
    //        get { return ConfigurationManager.AppSettings["CosmosConnection"]; }
    //    }
    //    public static string CosmosDB
    //    {
    //        get { return ConfigurationManager.AppSettings["CosmosDB"]; }
    //    }
    //    public static string CosmosCacheContainer
    //    {
    //        get { return ConfigurationManager.AppSettings["CosmosCacheContainer"]; }
    //    }
    //    public static string CosmosAccountsContainer
    //    {
    //        get { return ConfigurationManager.AppSettings["CosmosAccountsContainer"]; }
    //    }
      

    //    private CosmosClient _runninginstance;
    //    // The database we will create
    //    private Database database;

    //    // The container we will create.
    //    private Container container;
    //    public CosmosClient RunningInstance
    //    {
    //        get
    //        {
    //            if (_runninginstance == null) 
    //                //_runninginstance = new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
    //            //_runninginstance = new CosmosClient(("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
    //            _runninginstance = new CosmosClient(CosmosConnection);
    //            return _runninginstance;
    //        }
    //    }



    //    public async Task<Database> CacheDatabase()
    //    {
    //        // Create a new database
    //        this.database = await RunningInstance.CreateDatabaseIfNotExistsAsync(CosmosDB);
    //        return this.database;
    //        //Console.WriteLine("Created Database: {0}\n", this.database.Id);
    //    }

    //    public async Task<Container> GetAccountsContainer()
    //    {
    //        var db = await CacheDatabase();
    //        var acccont = await db.CreateContainerIfNotExistsAsync(CosmosAccountsContainer,"/id");
    //        return acccont;
    //    }

    //    public async void AddAccount(LoginAccount account )
    //    {
    //        Container container =await this.GetAccountsContainer();
    //        await container.CreateItemAsync<LoginAccount>(account, new PartitionKey(account.AccountId));
    //    }

    //    public async Task<List<LoginAccount>> GetAccounts()
    //    {
    //        Container container = await this.GetAccountsContainer();
    //        var sqlQueryText = "SELECT * FROM c";
    //        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
    //        FeedIterator<LoginAccount> allaccounts= container.GetItemQueryIterator<LoginAccount>(queryDefinition);
    //        List<LoginAccount> families = new List<LoginAccount>();

    //        while (allaccounts.HasMoreResults)
    //        {
    //            FeedResponse<LoginAccount> currentResultSet = await allaccounts.ReadNextAsync();
    //            foreach (LoginAccount family in currentResultSet)
    //            {
    //                families.Add(family);
                   
    //            }
    //        }

    //        return families;
    //    }


    //}

    public class LoginAccount
    {
        [JsonProperty("id")]
        public string AccountId{get;set;}
        public string AccountName{get;set; }
    }
}