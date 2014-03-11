using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Runtime.InteropServices;

namespace Astrila.Eq2Img
{
	/// <summary>
	/// Summary description for Eq2ImgAdmin.
	/// </summary>
	[ComVisible(false)]
	public class Eq2ImgAdmin : System.Web.UI.Page
	{
		protected System.Web.UI.WebControls.Panel mainPanel;
		protected System.Web.UI.WebControls.Label Label1;
		protected System.Web.UI.WebControls.TextBox adminKeyTextBox;
		protected System.Web.UI.WebControls.Button adminKeySubmitButton;
		protected System.Web.UI.WebControls.Label KeyNotSetLabel;
		protected System.Web.UI.WebControls.CheckBox deleteCachedImagesCheckBox;
		protected System.Web.UI.WebControls.CheckBox unloadMimeTexDLLCheckBox;
		protected System.Web.UI.WebControls.TextBox percentOfCachedImagesToBeDeletedTextBox;
		protected System.Web.UI.WebControls.Label Label2;
		protected System.Web.UI.WebControls.CheckBox deleteEqGifsCheckBox;
		protected System.Web.UI.WebControls.CheckBox deleteErrorGifsCheckBox;
		protected System.Web.UI.WebControls.Panel deleteCachedImagesOptionsPanel;
		protected System.Web.UI.WebControls.RangeValidator PercentageBoxRangeValidator;
		protected System.Web.UI.WebControls.LinkButton unloadHelpLinkButton;
		protected System.Web.UI.WebControls.Panel MimeTexUnloadHelpPanel;
	
		Eq2ImgSettings settings = null;
		private void Page_Load(object sender, System.EventArgs e)
		{
			//retrieve the settings for caching and other stuff
			settings = Eq2ImgSettings.GetFromContext(this.Context);

			if (settings.AdminKey.Length == 0)
			{
				KeyNotSetLabel.Visible=true;
				mainPanel.Visible=false;
			}
			else
			{
				KeyNotSetLabel.Visible = false;
				mainPanel.Visible=true;;
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.deleteCachedImagesCheckBox.CheckedChanged += new System.EventHandler(this.deleteCachedImagesCheckBox_CheckedChanged);
			this.unloadHelpLinkButton.Click += new System.EventHandler(this.unloadHelpLinkButton_Click);
			this.adminKeySubmitButton.Click += new System.EventHandler(this.adminKeySubmitButton_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion


		private void adminKeySubmitButton_Click(object sender, System.EventArgs e)
		{
			if (String.CompareOrdinal(adminKeyTextBox.Text, settings.AdminKey)==0)
			{
				if (deleteCachedImagesCheckBox.Checked)
				{
					double percentFileToDelete = double.Parse(percentOfCachedImagesToBeDeletedTextBox.Text, System.Globalization.CultureInfo.CurrentCulture)/100;
					if (deleteEqGifsCheckBox.Checked)
					{
						DeleteCacheFiles(settings.CacheFolder,"eq_*.gif", percentFileToDelete);
					}
					if (deleteErrorGifsCheckBox.Checked)
					{
						DeleteCacheFiles(settings.CacheFolder,"err_*.gif", percentFileToDelete);
					}
				}
				if (unloadMimeTexDLLCheckBox.Checked)
				{
					IntPtr MimeTexDLLHandle = NativeMethods.GetModuleHandle("MimeTex.dll");
					if (MimeTexDLLHandle != IntPtr.Zero)
						NativeMethods.FreeLibrary(MimeTexDLLHandle);
					else
						Response.Write("MimeTex.DLL is not yet loaded in the process");
				}
			}
			else
				Response.Write("Invalid admin key!");
		}

		private static void DeleteCacheFiles(string cachePath, string fileNamePattern, double percentageToDelete)
		{
			if (Directory.Exists(cachePath))
			{
				DirectoryInfo cacheDirectoryInfo = new DirectoryInfo(cachePath);
				FileSystemInfo[] fileList = cacheDirectoryInfo.GetFileSystemInfos(fileNamePattern);;
				System.Array.Sort(fileList, new FileSystemInfoCustomComparer());
				for (int fileIndex=0;fileIndex < fileList.Length*percentageToDelete;fileIndex++)
					fileList[fileIndex].Delete();
			}
			else {}; //No cached files to delete
		}


		private class FileSystemInfoCustomComparer : IComparer 
		{
			public int  Compare(object x, object y)
			{
				if ((x != null) && (y != null))
					return ((FileSystemInfo) x).LastAccessTime.CompareTo(((FileSystemInfo) y).LastAccessTime);
				else
					return (object.ReferenceEquals(x,y)?0:((x==null)?-1:1));
			}
		}

		private void deleteCachedImagesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			deleteCachedImagesOptionsPanel.Visible = deleteCachedImagesCheckBox.Checked;
		}

		private void unloadHelpLinkButton_Click(object sender, System.EventArgs e)
		{
			MimeTexUnloadHelpPanel.Visible=true;
		}
	}
}
