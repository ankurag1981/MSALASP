using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Graph;
using Azure.Identity;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Configuration;


namespace ASPNETMSAL
{
    public partial class AddAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                // if the url params contain code then it is rediected from authorization end point after user has logged in 
                //given permissions to for scopes
                if (this.Page.Request.Params["code"] != null)
                {
                    // Get access token using auth code
                    FinalizeAccount(this.Page.Request.Params["code"]);
                }
                else
                {
                    // Add new account
                    AddNewAccount();
                }
            }
        }
        
        //Get Current User Cache file

        string CurrentUserCachefilepath
        {
            get
            {
                string user = "User1"; // For real multi user application replace this with: HttpContext.Current.User.Identity.Name;
                string cachefilepath = Server.MapPath("/") + user + "_" + ConfigurationManager.AppSettings["CacheFilePath"];
                return cachefilepath;
            }
        }

        AuthenticationServicesHelper _authhelper;
        AuthenticationServicesHelper authhelper
        {
            get
            {
                
                if (_authhelper == null) _authhelper = new AuthenticationServicesHelper(CurrentUserCachefilepath);
                return _authhelper;
            }
        }

        // Add account by redurecting the user to auth url which prompts user to sign into the the new account and
        // confirm permissions to scopes. Once done it redirects to the Redircet url (Which in our case is this page itself) with the 'code' param in url
        async void AddNewAccount()
        {
            Uri uri = await authhelper.AuthorizationUri;
            Response.Write("<script>location.href='" + uri.AbsoluteUri + "'</script>");
        }

        // Add account to token caceh file by gettign access token
        async void FinalizeAccount(string code)
        {
            AuthenticationResult token = await authhelper.GetAccessToken(code);

            // Access token is retreived , the page gets redirected to default page
            Response.Redirect("/");
        }
    }
}