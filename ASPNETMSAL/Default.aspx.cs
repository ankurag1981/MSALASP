using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Graph;
using Azure.Identity;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ASPNETMSAL
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
              InitAccounts();               
            }
            
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
           Response.Redirect("/AddAccount.aspx");
        }
              
        string CurrentUserCachefilepath
        {
            get
            {
                string user = "User1"; // For real multi user application replace this with: HttpContext.Current.User.Identity.Name;
                string cachefilepath = Server.MapPath("/") + user+"_" + ConfigurationManager.AppSettings["CacheFilePath"];
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
        async void InitAccounts()
        {
            var allacc=await authhelper.GetAllAccounts();
            IEnumerable<LoginAccount> accc = allacc.Select(x => new LoginAccount() {AccountName=x.Username,AccountId=x.HomeAccountId.Identifier });
            DropDownList1.Items.Clear();
            DropDownList1.Items.Add(new System.Web.UI.WebControls.ListItem("Select Account", "0"));
            DropDownList1.Items.AddRange(accc.Select(x => new System.Web.UI.WebControls.ListItem(x.AccountName, x.AccountName)).ToArray());
            if (SelectedAccount != "")
            {
                DropDownList1.SelectedValue = SelectedAccount;
                //DropDownList1.SelectedIndexChanged();
                DropDownList1_SelectedIndexChanged(DropDownList1, EventArgs.Empty);
            }

        }              

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnCompose.Enabled = false;
            string strselectedaccount= DropDownList1.SelectedItem.Value;

            if(strselectedaccount!="0")
            {
                SetSelectedAccessToken(strselectedaccount);
            }
        }

        async void SetSelectedAccessToken(string selacc)
        {
            SelectedAccount = selacc;
            var authres = await authhelper.GetAccessTokenfromCache(SelectedAccount);
            AccessToken = authres;
            GetEvents();
        }

        string SelectedAccount
        {
            get { return (Session["SelectedAccount"] != null) ? Session["SelectedAccount"].ToString() : ""; }
            set { Session["SelectedAccount"] = value; }
        }

        
        string AccessToken
        {
            get { return (Session["AccessToken"] != null) ? Session["AccessToken"].ToString() : ""; }
            set { Session["AccessToken"] = value; }
        }
        async void GetEvents()
        {
            OutlookServicesHelper outlookhelper = new OutlookServicesHelper(AccessToken);
            var userev= await outlookhelper.Events;

            var tfomrat= "dd/MM/yyyy hh:mm tt";
            var udata= userev.Select(ev => new { Id=ev.Id, Start = DateTime.Parse(ev.Start.DateTime).ToString(tfomrat), End = DateTime.Parse(ev.End.DateTime).ToString(tfomrat),Subject=ev.Subject,StartTimeZone=ev.OriginalStartTimeZone,EndTimeZone=ev.OriginalEndTimeZone, BodyPreview = ev.BodyPreview });
            
            GridView1.DataSource = udata;//.Select(ev=> new {StartTime=ev.Start.ToString(),EndTime=ev.End.ToString(),Attendees=ev.Attendees,BodyPreview=ev.BodyPreview});
            GridView1.DataBind();
            btnCompose.Enabled = true;
        }

        protected void btnCompose_Click(object sender, EventArgs e)
        {
            Response.Redirect("/Event.aspx");
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if(e.CommandName== "Delete")
            {
                Response.Redirect("/DeleteEvent.aspx?id=" + e.CommandArgument.ToString());
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect("/Event.aspx?id=" + e.CommandArgument.ToString());
            }
        }

    }
}