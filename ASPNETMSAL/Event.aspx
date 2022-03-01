<%@ Page Title="" Language="C#" async="true" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Event.aspx.cs" Inherits="ASPNETMSAL.Event" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
   <div>
        
          <div class="form-group">
            <label for="exampleFormControlInput1">Start Date</label>
            <asp:TextBox ID="txtStart" textmode="DateTimeLocal" runat="server" CssClass="form-control" ></asp:TextBox>
          </div>
          <div class="form-group">
            <label for="exampleFormControlInput1">End Date</label>
            <asp:TextBox ID="txtEnd" textmode="DateTimeLocal" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
          <div class="form-group">
            <label for="exampleFormControlInput1">Attendees</label>
            <asp:TextBox ID="txtAttendees" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
          <div class="form-group">
            <label for="exampleFormControlInput1">Subject</label>
            <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" ></asp:TextBox>
          </div>      
          <div class="form-group">
            <label for="exampleFormControlInput1">Body</label>
            <asp:TextBox ID="txtBody" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="10" ></asp:TextBox>
          </div>
          <div class="form-group">
            <div class="col-md-2">
              <asp:Button ID="btnCreate" runat="server" Text="Create" OnClick="btnCreate_Click" CssClass="btn btn-primary" />
            </div>
            <div class="col-md-2">
              <asp:Button ID="btnUpdate" runat="server" Text="Update" OnClick="btnUpdate_Click" CssClass="btn btn-primary" />
            </div>
            <div class="col-md-1">
             <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-danger"/>
            </div>
          </div>
          

    </div>
</asp:Content>
