using System;
using System.Web;
using System.Net;
using System.IO;
using System.Drawing;
using System.Globalization;

namespace Astrila.Eq2Img
{
	/// <summary>
	/// Summary description for convert.
	/// </summary>
	public class ShowEq : IHttpHandler
	{
		public virtual void ProcessRequest ( System.Web.HttpContext context )
		{
			if (context == null)
				throw new ArgumentNullException("context", "context can not be null");
			else{}; // we have a good context

			//retrieve the settings for caching and other stuff
			Eq2ImgSettings settings = Eq2ImgSettings.GetFromContext(context);

			try
			{
				//Build the path where we should be saving the gif for equations
				string filePathForCachedEqImage = GetFilePathForCachedEqImage(settings.CacheFolder, context.Request.QueryString.ToString());

				//If we have never generated the cache file for this equation
				if (!File.Exists(filePathForCachedEqImage))
				{
					//Retrieve the equation from the query string and check it
					//Request.QueryString.ToString doesn't work for us because chars are all hex escapped
					string equation = string.Empty;
					if (context.Request.Url.Query.Length > 1)
					{
						equation = context.Request.Url.Query.Substring(1);
						equation = HexUnEscapper.UnescapeHexCharsInString(equation);
					}
					else
						equation = string.Empty;

					if (equation.Length == 0)
						throw new EquationRenderException("Empty Equation");
					else if (equation.Length > settings.EquationMaxSize)
						throw new EquationRenderException("Equation Too Long: " + equation.Substring(0, settings.EquationMaxSize));
					else
					{
						switch(settings.MimeTexUsage)
						{
							case MimeTexUsageType.InProc: CreateGifUsingInProc(context, equation, filePathForCachedEqImage, settings); break;
							case MimeTexUsageType.Cgi: CreateGifUsingCGI(context, equation, filePathForCachedEqImage, settings); break;
						    case MimeTexUsageType.ExternalUrl: CreateGifUsingExternalUrl(context, equation, filePathForCachedEqImage, settings); break;
							case MimeTexUsageType.OutProc: CreateGifUsingOutProc(context, equation, filePathForCachedEqImage, settings); break;
							case MimeTexUsageType.None: break;	//In this mode we server only from cache. This mode is useful if we want to take MimeTex DLL offline for upgrade.
							default: throw new ArgumentException("Unknown MimeTexUsage: " + settings.MimeTexUsage.ToString());
						}

						//See if process has really generated the gif file
						if (!File.Exists(filePathForCachedEqImage))
							throw new EquationRenderException("ImgNotFound: " + equation);
						else	//Send teh rendered equation to client
							SendGifFileToClient(context, filePathForCachedEqImage, settings, false);
					}
				}
				else	//Send gif file we already had generated
					SendGifFileToClient(context, filePathForCachedEqImage, settings, false);
			}
			catch(EquationRenderException ex)	//Send all rendering errors as message in gif files
			{
				string filePathForCachedErrImage = GetFilePathForCachedErrImage(settings.CacheFolder, ex.Message);
				
				//Send already cached error message or create a new gif containing the message
				if (!File.Exists(filePathForCachedErrImage))
					WriteStringInGif(ex.Message, filePathForCachedErrImage);
				SendGifFileToClient(context, filePathForCachedErrImage, settings, true);
			}
		}

		private void CreateGifUsingCGI(HttpContext context, string equation, string filePathForCachedEqImage, Eq2ImgSettings settings)
		{
			using (WebClient browser = new WebClient())
			{
				byte[] imageData = browser.DownloadData(settings.MimeTexFullCgiUrl + "?" + equation);
				SaveImageInFile(filePathForCachedEqImage, imageData, true);
			}
		}
		private void CreateGifUsingOutProc(HttpContext context, string equation, string filePathForCachedEqImage, Eq2ImgSettings settings)
		{
			string MimeTexExeFilePath = Path.Combine(context.Request.PhysicalApplicationPath,settings.MimeTexExeRelativeFilePath);
			System.Diagnostics.ProcessStartInfo MimeTexStartupInfo = new System.Diagnostics.ProcessStartInfo(MimeTexExeFilePath, "-e \"" + filePathForCachedEqImage + "\"" + " " + "\"" + equation + "\"");
			MimeTexStartupInfo.CreateNoWindow = true;
			//MimeTexStartupInfo.EnvironmentVariables.Clear();
			MimeTexStartupInfo.ErrorDialog = false;
			MimeTexStartupInfo.RedirectStandardOutput=true;
			MimeTexStartupInfo.RedirectStandardError = true;
			MimeTexStartupInfo.UseShellExecute = false;
			System.Diagnostics.Process MimeTexProcess = System.Diagnostics.Process.Start(MimeTexStartupInfo);
			MimeTexProcess.WaitForExit();
			if (!File.Exists(filePathForCachedEqImage))
			{
				string errorMessage = @"Failed to render eq '{0}':" + MimeTexProcess.StandardOutput.ReadToEnd();
				errorMessage += Environment.NewLine;
				errorMessage += MimeTexProcess.StandardError.ReadToEnd();
				throw new EquationRenderException(string.Format(CultureInfo.InvariantCulture, errorMessage, equation));
			}
			else {}; //We already have the gif file
		}

		private void CreateGifUsingExternalUrl(HttpContext context, string equation, string filePathForCachedEqImage, Eq2ImgSettings settings)
		{
			using (WebClient browser = new WebClient())
			{
				byte[] imageData = browser.DownloadData(string.Format(CultureInfo.InvariantCulture, settings.MimeTexExternalServerUrl, equation));
				SaveImageInFile(filePathForCachedEqImage, imageData, true);
			}
		}

		//MimeTeX is unmanaged code and is not thread safe. We must protect the call
		//as there would be lots of simultaneous requests
		private static object _MimeTeXCallLocker = new object();
		private void CreateGifUsingInProc(HttpContext context, string equation, string filePathForCachedEqImage, Eq2ImgSettings settings)
		{
			lock(_MimeTeXCallLocker)
			{
				NativeMethods.CreateGifFromEq(equation, filePathForCachedEqImage);
			}
		}


		private static void WriteStringInGif(string message, string filePath)
		{
			string valueToWrite = "[" + message + "]";
			Font fontToUse = new Font(FontFamily.GenericSansSerif, 8);
			SizeF stringSize = SizeF.Empty;

			// calculate size of the string.
			using (Bitmap size1Bitmap = new Bitmap(1,1,System.Drawing.Imaging.PixelFormat.Format24bppRgb))
			{
				using (Graphics g = Graphics.FromImage(size1Bitmap))
				{
					stringSize = g.MeasureString(valueToWrite, fontToUse);
				}			
			}

			using (Bitmap messageBitmap = new Bitmap((int)stringSize.Width, (int)stringSize.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
			{
				using (Graphics g = Graphics.FromImage(messageBitmap))
				{
					//g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0,(int)stringSize.Width, (int)stringSize.Height));
					g.DrawString(valueToWrite, fontToUse, Brushes.Red, 0, 0);
					//messageBitmap.MakeTransparent(Color.White);
					g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
					messageBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Gif);
				}
			}
		}

		private static string GetEquationHash(string equation)
		{
			using (System.Security.Cryptography.MD5 md5Agent = new System.Security.Cryptography.MD5CryptoServiceProvider())
			{
				byte[] byteHash = md5Agent.ComputeHash(System.Text.Encoding.UTF8.GetBytes(equation));
				//Not all chars in base64 are valid file name chars: http://www.garykessler.net/library/base64.html
				return System.Convert.ToBase64String(byteHash).Replace("+","-").Replace("/","_");
			}
		}

		private static string GetFilePathForCachedEqImage(string saveFolder, string equation)
		{
			return Path.Combine(saveFolder,"eq_" + GetEquationHash(equation) + ".gif");
		}

		private static string GetFilePathForCachedErrImage(string saveFolder, string equation)
		{
			return Path.Combine(saveFolder,"err_" + GetEquationHash(equation) + ".gif");
		}

		
		private static byte[] _GifStandardHeaderBytes = new byte[] {0x47, 0x49, 0x46};	//ASCII Bytes for string "GIF"
		private static void ValidateGifData(byte[] imageData)
		{
			
			if (imageData.Length >= _GifStandardHeaderBytes.Length)
			{
				for(int byteIndex=0;byteIndex < _GifStandardHeaderBytes.Length;byteIndex++)
					if (imageData[byteIndex]!=_GifStandardHeaderBytes[byteIndex])
						throw new ArgumentException("Invalid GIF header. Byte #" + byteIndex.ToString(CultureInfo.InvariantCulture) + " is not " + _GifStandardHeaderBytes[byteIndex].ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				throw new ArgumentException("Invalid GIF header. Numbers of bytes is less than expected " + _GifStandardHeaderBytes.Length.ToString(CultureInfo.InvariantCulture));
			}
		}

		private static void SaveImageInFile(string imageFilePath, byte[] imageData, bool validateImageData)
		{
			if ((imageData != null) && (imageData.Length > 0))
			{
				if (validateImageData)
					ValidateGifData(imageData);

				try
				{
					using (FileStream gifFileStream = File.Create(imageFilePath, imageData.Length))
					{
						gifFileStream.Write(imageData, 0, (imageData.Length < 450000)?imageData.Length:450000);	//ToDO: Make this configurable
					}
				}
				catch	//multiple threads might try to create the same file
				{
					if (!File.Exists(imageFilePath))
						throw;
				}
			}
			else {}; //no files should be generated
		}

		private static void SendGifFileToClient(System.Web.HttpContext context, string gifFilePath, Eq2ImgSettings settings, bool isErrorMessageImage)
		{
			byte[] imageData = new byte[]{};
			using (FileStream gifFileStream = File.Open(gifFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				imageData = new byte[gifFileStream.Length];
				gifFileStream.Read(imageData, 0, imageData.Length);
			}

			SendImageToClient(context, imageData, settings, isErrorMessageImage);
		}

		private static void SendImageToClient(System.Web.HttpContext context, byte[] imageData, Eq2ImgSettings settings, bool isErrorMessageImage)
		{
			context.Response.Clear();	//Some page template solution might have already added some response
			context.Response.ContentType = "image/gif";

			if (!isErrorMessageImage)
			{
				context.Response.Cache.SetExpires(settings.ClientCacheExpiration);
				context.Response.Cache.SetMaxAge(settings.ClientCacheAge);
				context.Response.Cache.SetProxyMaxAge(settings.ClientCacheAge);
			}
			else
			{
				context.Response.Cache.SetMaxAge(settings.ClientCacheAgeForErrorMessages);
				context.Response.Cache.SetProxyMaxAge(settings.ClientCacheAgeForErrorMessages);
				context.Response.Cache.SetExpires(settings.ClientCacheExpirationForErrorMessages);
			}
			context.Response.Cache.SetLastModified(settings.LastModifiedDateHttpHeader);
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.CacheControl = "public";
			context.Response.Cache.SetValidUntilExpires(false);
			context.Response.Cache.VaryByParams["*"] = true;
			context.Response.BinaryWrite(imageData);
			//context.Response.End();	//Ending response seems to stop cache mechanism
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		private class HexUnEscapper : System.Uri
		{
			private HexUnEscapper(string dummy):base(dummy)
			{
				
			}
			private static HexUnEscapper _OneInstance = new HexUnEscapper("http://stupid.uri.object");
			public static string UnescapeHexCharsInString(string str)
			{
				return _OneInstance.Unescape(str);
			}
		}


		public static bool HandleEquationQueries()
		{
			IHttpHandler handlerInstance = new ShowEq();
			HttpContext context = HttpContext.Current;
			if ((context.Request.Url.Query.Length > 1)&& (context.Request.Url.Query.StartsWith("?$") && (context.Request.Url.Query.EndsWith("$"))))
			{
				handlerInstance.ProcessRequest(context);
				return true;
			}
			else return false;
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

	[Serializable]
	public class EquationRenderException : Exception
	{
		public EquationRenderException ()
		{
		}
		public EquationRenderException(string message) : base(message)
		{
		}
		public EquationRenderException(string message, Exception innerException): base (message, innerException)
		{
		}
		protected EquationRenderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}

