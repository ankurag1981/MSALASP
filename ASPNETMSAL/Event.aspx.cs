using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASPNETMSAL
{
    public partial class Event : System.Web.UI.Page
    {
        string AccessToken
        {
            get { return (Session["AccessToken"] != null) ? Session["AccessToken"].ToString() : ""; }
            set { Session["AccessToken"] = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (this.Request.Params["id"] != null)
                {
                    btnCreate.Visible = false;
                    InitEvent(this.Request.Params["id"]);
                }
                else btnUpdate.Visible = false;
            }
            

        }

        async void InitEvent(string evid)
        {
            OutlookServicesHelper ohelper = new OutlookServicesHelper(AccessToken);
            Microsoft.Graph.Event ev = await ohelper.GetEventById(evid);
            //txtStart.TextMode = TextBoxMode.DateTime;
            txtStart.Text = DateTime.Parse(ev.Start.DateTime).ToString("yyyy-MM-ddThh:mm"); 
            //txtStart.

            txtEnd.Text = DateTime.Parse(ev.End.DateTime).ToString("yyyy-MM-ddThh:mm"); 
            txtSubject.Text = ev.Subject;
            txtAttendees.Text = String.Join(";",ev.Attendees.Select(x => x.EmailAddress.Address));
            txtBody.Text = ev.Body.Content;
        }


        protected void btnCreate_Click(object sender, EventArgs e)
        {
            createev();

        }


        async void createev()
        {
            OutlookServicesHelper ohelper = new OutlookServicesHelper(AccessToken);
            Microsoft.Graph.Event ev = new Microsoft.Graph.Event();
            ev.Start = new Microsoft.Graph.DateTimeTimeZone();
            ev.Start.DateTime = txtStart.Text;
            ev.Start.TimeZone = "India Standard Time";
            ev.End = new Microsoft.Graph.DateTimeTimeZone();
            ev.End.DateTime = txtEnd.Text;
            ev.End.TimeZone = "India Standard Time";
            ev.Subject = txtSubject.Text;
            
            ev.Body = new Microsoft.Graph.ItemBody();
           
            ev.Body.ContentType = Microsoft.Graph.BodyType.Html;
            ev.Body.Content = txtBody.Text;
            if(txtAttendees.Text!="")
            {
                string[] emails = txtAttendees.Text.Split(';');
                ev.Attendees = emails.Select(x => new Microsoft.Graph.Attendee() { EmailAddress = new Microsoft.Graph.EmailAddress() { Address = x }, Type = Microsoft.Graph.AttendeeType.Required });

            }
            var evc = await ohelper.AddEvent(ev);
            var e = evc;
            Response.Redirect("/");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("/");
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateEvent();
        }

        async void UpdateEvent()
        {

            OutlookServicesHelper ohelper = new OutlookServicesHelper(AccessToken);
            Microsoft.Graph.Event ev = new Microsoft.Graph.Event();
            ev.Start = new Microsoft.Graph.DateTimeTimeZone();
            ev.Start.DateTime = txtStart.Text;
            ev.Start.TimeZone = "India Standard Time";
            ev.End = new Microsoft.Graph.DateTimeTimeZone();
            ev.End.DateTime = txtEnd.Text;
            ev.End.TimeZone = "India Standard Time";
            ev.Subject = txtSubject.Text;
            ev.Body = new Microsoft.Graph.ItemBody();
            ev.Body.ContentType = Microsoft.Graph.BodyType.Html;
            ev.Body.Content = txtBody.Text;
            if (txtAttendees.Text != "")
            {
                string[] emails = txtAttendees.Text.Split(';');
                //Microsoft.Graph.Attendee att = new Microsoft.Graph.Attendee();
                //att.EmailAddress =
                ev.Attendees = emails.Select(x => new Microsoft.Graph.Attendee() { EmailAddress = new Microsoft.Graph.EmailAddress() { Address = x },Type=Microsoft.Graph.AttendeeType.Required });

            }
            var evc = await ohelper.UpdateEvent(this.Request.Params["id"],ev);
            var e = evc;
            Response.Redirect("/");
        }
    }


}