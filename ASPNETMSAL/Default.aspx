<%@ Page Title="Home Page" async="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASPNETMSAL._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row content-div">
        <div class="col-md-4">         
            <asp:Button ID="Button1" runat="server" Text="Add Account" OnClick="Button1_Click" CssClass="btn btn-primary" />
        </div>       
    </div>
    <div class="row content-div">
        <div class="col-md-8">
           <asp:DropDownList ID="DropDownList1" AutoPostBack="True" runat="server" Height="57px" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" CssClass="form-control"></asp:DropDownList>
        </div>
         <div class="col-md-4">
             <asp:Button ID="btnRemoveAccount" runat="server" Text="Button" OnClick="btnRemoveAccount_Click" />
        </div>
    </div>
    <div class="row content-div">
        <div class="col-md-4">         
            <asp:Button ID="btnCompose" runat="server" Text="Compose Event" CssClass="btn btn-primary" OnClick="btnCompose_Click" Enabled="false" />
        </div>       
    </div>


  <div class="row content-div">
     <div class="col-md-12">
        <asp:GridView ID="GridView1" runat="server" Height="330px" Width="544px" class="table table-stripped" AutoGenerateColumns="False" OnRowCommand="GridView1_RowCommand">
            <Columns>
            <asp:BoundField DataField="Start" HeaderText="Start"  />
            <asp:BoundField DataField="StartTimeZone" HeaderText="Start TimeZone" />
            <asp:BoundField DataField="End" HeaderText="End" />
            <asp:BoundField DataField="EndTimeZone" HeaderText="End TimeZone" />
            <asp:BoundField DataField="Subject" HeaderText="Subject" />
            <asp:BoundField DataField="BodyPreview" HeaderText="Body Preview" />
        
            <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:Button ID="btndelete" runat="server" CausesValidation="false" CommandName="Delete"  Text="Delete" CommandArgument='<%# Eval("Id") %>' />
                </ItemTemplate>
            </asp:TemplateField>
                <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:Button ID="btnEdit" runat="server" CausesValidation="false" CommandName="Edit"  Text="Edit" CommandArgument='<%# Eval("Id") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            </Columns>
        </asp:GridView>
     </div>
   </div>
    

    
</asp:Content>
