using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASPNETMSAL
{
    public partial class DeleteEvent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(this.Request.Params["id"]!=null)
            {
                DeleteEv(this.Request.Params["id"]);
            }
            else Response.Redirect("/");
        }

        string AccessToken
        {
            get { return (Session["AccessToken"] != null) ? Session["AccessToken"].ToString() : ""; }
            set { Session["AccessToken"] = value; }
        }

        public async void DeleteEv(string todel)
        {
            OutlookServicesHelper hlp = new OutlookServicesHelper(AccessToken);
            await hlp.DeleteEvent(todel);
            Response.Redirect("/");
        }
    }
}