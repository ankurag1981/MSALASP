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

                if (this.Page.Request.Params["code"] != null)
                {
                    FinalizeAccount(this.Page.Request.Params["code"]);
                }
                else
                {
                    AddNewAccount();
                }
            }
        }
        

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
        async void AddNewAccount()
        {
            Uri uri = await authhelper.AuthorizationUri;
            Response.Write("<script>location.href='" + uri.AbsoluteUri + "'</script>");
        }


        async void FinalizeAccount(string code)
        {
            AuthenticationResult token = await authhelper.GetAccessToken(code);
            Response.Redirect("/");
        }
    }
}