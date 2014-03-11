<%@ Page language="c#" Codebehind="Eq2ImgAdmin.aspx.cs" AutoEventWireup="false" Inherits="Astrila.Eq2Img.Eq2ImgAdmin" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Eq2ImgAdmin</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<asp:rangevalidator id="PercentageBoxRangeValidator" runat="server" ControlToValidate="percentOfCachedImagesToBeDeletedTextBox"
				Type="Double" MaximumValue="100" MinimumValue="0" ErrorMessage="The value for % of cached images must be between 0 to 100"></asp:rangevalidator><asp:label id="KeyNotSetLabel" runat="server" BackColor="#FFFF80" Width="100%" Height="100%"
				BorderStyle="Dotted">
				<strong>Security Error</strong>
				<hr>
				<p>The Admin key must be set in order to use this page. To set this value please 
					edit your web.config and then add the following line in the appSettings section:</p>
				<p align="center"><code>&lt;add key="Eq2Img_AdminKey" 
						value="SomeThingOtherThanYourPassword!" /&gt;</code></p>
				<p>We <b>highly recommand</b> that you set the value which is not trivial and does 
					not match any of your existing passwords.</p>
			</asp:label><asp:panel id="mainPanel" runat="server">
				<P>Choose the tasks you would like to perform:</P>
				<P>
					<asp:CheckBox id="deleteCachedImagesCheckBox" runat="server" EnableViewState="False" Text="Delete cached images"
						AutoPostBack="True"></asp:CheckBox>
					<asp:Panel id="deleteCachedImagesOptionsPanel" runat="server" Visible="False"><BR>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
<asp:CheckBox id="deleteEqGifsCheckBox" runat="server" Text="Delete cached images for equations"
							Checked="True"></asp:CheckBox><BR>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
<asp:CheckBox id="deleteErrorGifsCheckBox" runat="server" Text="Delete cached images for error messages"
							Checked="True"></asp:CheckBox><BR>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
Delete 
<asp:TextBox id="percentOfCachedImagesToBeDeletedTextBox" runat="server" Width="32px">50</asp:TextBox>
<asp:Label id="Label2" runat="server">% of cached images</asp:Label></asp:Panel></P>
				<P>
					<asp:CheckBox id="unloadMimeTexDLLCheckBox" runat="server" EnableViewState="False" Text="Unload MimeTex DLL "></asp:CheckBox>
					<asp:LinkButton id="unloadHelpLinkButton" runat="server">(Help on how to upgrade MimeTex DLL)</asp:LinkButton>
					<asp:Panel id="MimeTexUnloadHelpPanel" runat="server" BackColor="#FFFFC0" BorderStyle="Outset"
						Visible="False">MimeTex.DLL is Win32 DLL and is loaded 
in-process along with ASP.Net process. For this reason, this DLL would always be 
locked by ASP.Net process making it difficult to replace or delete without 
restarting the IIS. 
<P>To replace or delete this DLL without restarting IIS, please follow these steps:
						</P>
<P>
							<OL>
								<LI>
									Add/Change following settings in web.config:<BR>
									<CODE>&lt;add key="Eq2Img_MimeTexUsage" value="None" /&gt;</CODE>
									<BR>
								This will prevent current users from calling in to MimeTex DLL to generate 
								images for new equations however images for already generated equations will be 
								continued to be served from the cache.
								<LI>
									Check the <CODE>Unload MimeTex DLL</CODE>
								box above and click submit. This will detach the MimeTex DLL from the ASP.Net 
								process.
								<LI>
									Now you can replace the MimeTex DLL or delete it. After you are done, make sure 
									to repeat step 1 above with <CODE>value="InProc"</CODE> isntead of "None".
								</LI>
							</OL>
<P></P></asp:Panel>
				<P></P>
				<P>
					<asp:Label id="Label1" runat="server">Enter the admin key: </asp:Label>
					<asp:TextBox id="adminKeyTextBox" runat="server" TextMode="Password"></asp:TextBox>
					<asp:Button id="adminKeySubmitButton" runat="server" Text="Submit"></asp:Button></P>
			</asp:panel></form>
	</body>
</HTML>
