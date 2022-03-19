using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASPNETMSAL
{
    public partial class DeleteAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public async void DeleteEv(string todel)
        {
            //OutlookServicesHelper hlp = new AuthenticationServicesHelper(AccessToken);
            //await hlp.DeleteEvent(todel);
            //Response.Redirect("/");
        }
    }
}