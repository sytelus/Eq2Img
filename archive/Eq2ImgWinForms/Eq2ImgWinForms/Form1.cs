using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace Astrila.Eq2ImgWinForms
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TextBox textBoxEquation;
		private System.Windows.Forms.Button buttonShow;
		private System.Windows.Forms.Panel panelEquationEntry;
		private System.Windows.Forms.Panel panelPicture;
		private System.Windows.Forms.Panel panelTollbar;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButtonRealTimeUpdate;
		private System.Windows.Forms.ToolBarButton toolBarButtonSave;
		private System.Windows.Forms.ToolBarButton toolBarButtonCopy;
		private System.Windows.Forms.ToolBarButton toolBarButtonExamples;
		private System.Windows.Forms.ToolBarButton toolBarButtonTeXHelp;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Button button1;
		private System.ComponentModel.IContainer components;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.textBoxEquation = new System.Windows.Forms.TextBox();
			this.buttonShow = new System.Windows.Forms.Button();
			this.panelEquationEntry = new System.Windows.Forms.Panel();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButtonRealTimeUpdate = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonSave = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonCopy = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonExamples = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonTeXHelp = new System.Windows.Forms.ToolBarButton();
			this.panelPicture = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panelTollbar = new System.Windows.Forms.Panel();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.panelEquationEntry.SuspendLayout();
			this.panelPicture.SuspendLayout();
			this.panelTollbar.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Fuchsia;
			// 
			// textBoxEquation
			// 
			this.textBoxEquation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxEquation.Location = new System.Drawing.Point(112, 8);
			this.textBoxEquation.Multiline = true;
			this.textBoxEquation.Name = "textBoxEquation";
			this.textBoxEquation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxEquation.Size = new System.Drawing.Size(496, 88);
			this.textBoxEquation.TabIndex = 0;
			this.textBoxEquation.Text = "x^2 + y^2 = z^2";
			this.textBoxEquation.TextChanged += new System.EventHandler(this.textBoxEquation_TextChanged);
			// 
			// buttonShow
			// 
			this.buttonShow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonShow.Location = new System.Drawing.Point(536, 104);
			this.buttonShow.Name = "buttonShow";
			this.buttonShow.Size = new System.Drawing.Size(61, 20);
			this.buttonShow.TabIndex = 4;
			this.buttonShow.Text = "Show";
			this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
			// 
			// panelEquationEntry
			// 
			this.panelEquationEntry.Controls.Add(this.toolBar1);
			this.panelEquationEntry.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelEquationEntry.Location = new System.Drawing.Point(0, 0);
			this.panelEquationEntry.Name = "panelEquationEntry";
			this.panelEquationEntry.Size = new System.Drawing.Size(616, 32);
			this.panelEquationEntry.TabIndex = 7;
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButtonRealTimeUpdate,
																						this.toolBarButtonSave,
																						this.toolBarButtonCopy,
																						this.toolBarButtonExamples,
																						this.toolBarButtonTeXHelp});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.ImageList = this.imageList1;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(616, 28);
			this.toolBar1.TabIndex = 10;
			this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButtonRealTimeUpdate
			// 
			this.toolBarButtonRealTimeUpdate.ImageIndex = 4;
			this.toolBarButtonRealTimeUpdate.Pushed = true;
			this.toolBarButtonRealTimeUpdate.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.toolBarButtonRealTimeUpdate.Text = "Update as you type";
			// 
			// toolBarButtonSave
			// 
			this.toolBarButtonSave.ImageIndex = 0;
			this.toolBarButtonSave.Text = "Save Image";
			// 
			// toolBarButtonCopy
			// 
			this.toolBarButtonCopy.ImageIndex = 1;
			this.toolBarButtonCopy.Text = "Copy Image";
			// 
			// toolBarButtonExamples
			// 
			this.toolBarButtonExamples.ImageIndex = 3;
			this.toolBarButtonExamples.Text = "Examples";
			// 
			// toolBarButtonTeXHelp
			// 
			this.toolBarButtonTeXHelp.ImageIndex = 2;
			this.toolBarButtonTeXHelp.Text = "Help on TeX";
			// 
			// panelPicture
			// 
			this.panelPicture.Controls.Add(this.pictureBox1);
			this.panelPicture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPicture.Location = new System.Drawing.Point(0, 160);
			this.panelPicture.Name = "panelPicture";
			this.panelPicture.Size = new System.Drawing.Size(616, 166);
			this.panelPicture.TabIndex = 5;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(616, 166);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// panelTollbar
			// 
			this.panelTollbar.Controls.Add(this.button1);
			this.panelTollbar.Controls.Add(this.label1);
			this.panelTollbar.Controls.Add(this.textBoxEquation);
			this.panelTollbar.Controls.Add(this.buttonShow);
			this.panelTollbar.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTollbar.Location = new System.Drawing.Point(0, 32);
			this.panelTollbar.Name = "panelTollbar";
			this.panelTollbar.Size = new System.Drawing.Size(616, 128);
			this.panelTollbar.TabIndex = 10;
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(416, 104);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(104, 20);
			this.button1.TabIndex = 6;
			this.button1.Text = "Performance Test";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(109, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Type equation here:";
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = "GIF (*.gif)|*.gif";
			// 
			// Form1
			// 
			this.AcceptButton = this.buttonShow;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 326);
			this.Controls.Add(this.panelPicture);
			this.Controls.Add(this.panelTollbar);
			this.Controls.Add(this.panelEquationEntry);
			this.Name = "Form1";
			this.Text = "Equation Writer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panelEquationEntry.ResumeLayout(false);
			this.panelPicture.ResumeLayout(false);
			this.panelTollbar.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}


		private string GetGifFilePath()
		{
			return Path.Combine(Path.GetTempPath(),"Eq2ImgWinForms.gif");
		}

		private void WriteEquation(string equation)
		{
			if (pictureBox1.Image != null)
				pictureBox1.Image.Dispose();

			if (equation.Length > 0)
			{
				string tempGifFilePath = GetGifFilePath();
				try
				{
					NativeMethods.CreateGifFromEq(equation, tempGifFilePath);
					pictureBox1.Image = Image.FromFile(tempGifFilePath);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			else 
			{
				pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "emptyeq.gif"));
			};
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			WriteEquation(textBoxEquation.Text);
		}

		private void buttonShow_Click(object sender, System.EventArgs e)
		{
			WriteEquation(textBoxEquation.Text);
		}

		private void textBoxEquation_TextChanged(object sender, System.EventArgs e)
		{
			if (toolBarButtonRealTimeUpdate.Pushed==true)
				WriteEquation(textBoxEquation.Text);
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == toolBarButtonCopy)
			{
				Clipboard.SetDataObject(pictureBox1.Image);
			}
			else if (e.Button == toolBarButtonSave)
			{
				if (saveFileDialog1.ShowDialog(this)==DialogResult.OK)
					pictureBox1.Image.Save(saveFileDialog1.FileName);
			}
			else if (e.Button == toolBarButtonExamples)
			{
				System.Diagnostics.Process.Start("http://www.forkosh.dreamhost.com/source_mimetex.html?referer=&remhost=#examples");
			}
			else if (e.Button == toolBarButtonTeXHelp)
			{
				System.Diagnostics.Process.Start("http://www.forkosh.com/mimetextutorial.html");
			}
			else if (e.Button == toolBarButtonRealTimeUpdate)
			{}
			else
				MessageBox.Show("This is not implemented yet");
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			DateTime startTime = DateTime.Now;
			for (int i = 0; i < 2000; i++)
			{
				WriteEquation(textBoxEquation.Text);
			}
			MessageBox.Show("Time to render: " + (DateTime.Now.Subtract(startTime).Seconds / 2000.0).ToString() + " seconds");
		}
	
	}

	[System.Security.SuppressUnmanagedCodeSecurity()]
	internal class NativeMethods
	{
		private NativeMethods()
		{ //all methods in this class would be static
		}

		[System.Runtime.InteropServices.DllImport("MimeTex.dll")]
		internal static extern int CreateGifFromEq(string expr, string fileName);

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		internal extern static IntPtr GetModuleHandle(string lpModuleName);

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		internal extern static bool FreeLibrary(IntPtr hLibModule);
	}
}
