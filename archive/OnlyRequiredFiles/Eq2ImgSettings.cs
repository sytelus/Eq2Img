using System;
using System.Web;
using System.Configuration;
using System.Globalization;
using System.Web.Caching;

namespace Astrila.Eq2Img
{
	/// <summary>
	/// Summary description for ShowEqSettings.
	/// </summary>
	[Serializable]
	public class Eq2ImgSettings
	{
		//Defines the default values for the settings if they weren't specified in 
		//web.config
		private const int DefaultClientCacheAgeInDays = 180;
		private const string DefaultCacheFolder = @"CachedEqImages";
		private const bool DefaultIsCacheFolderRelative = true;
		private const MimeTexUsageType DefaultMimeTexUsage = MimeTexUsageType.InProc;
		private const string DefaultMimeTexExeRelativeFilePath = @"cgi\MimeTex.exe";
		private const string DefaultMimeTexFullCgiUrl = @"cgi/MimeTex.exe";
		private const string DefaultMimeTexExternalServerUrl = @"http://www.ShitalShah.com/?{0}";
		//This may ease DoS attack but its required for figure type images that MimeTeX supports
		private const int DefaultEquationMaxSize = 10000;

		//Set up the private members for the properties
		private TimeSpan _ClientCacheAge =  TimeSpan.FromDays(DefaultClientCacheAgeInDays);
		private string _CacheFolder = DefaultCacheFolder;
		private bool _IsCacheFolderRelative = DefaultIsCacheFolderRelative;
		private MimeTexUsageType _MimeTexUsage = DefaultMimeTexUsage;
		private string _MimeTexExeRelativeFilePath = DefaultMimeTexExeRelativeFilePath;
		private string _MimeTexFullCgiUrl = DefaultMimeTexFullCgiUrl;
		private string _MimeTexExternalServerUrl = DefaultMimeTexExternalServerUrl;
		private int _EquationMaxSize = DefaultEquationMaxSize;
		private string _AdminKey = string.Empty;

		//By default the cache files created for equations would have this time stamp
		private DateTime _LastModifiedDateHttpHeader = DateTime.Now;

#region "Property Getters/Setters
		public string AdminKey
		{
			get {return _AdminKey;}
			set {_AdminKey = value;}
		}
		public TimeSpan ClientCacheAge
		{
			get {return _ClientCacheAge;}
			set {_ClientCacheAge = value;}
		}
		public int EquationMaxSize
		{
			get {return _EquationMaxSize;}
			set {_EquationMaxSize = value;}
		}
		public DateTime ClientCacheExpiration
		{
			get {return DateTime.Now.Add(_ClientCacheAge);}
		}
		public DateTime LastModifiedDateHttpHeader
		{
			get {return _LastModifiedDateHttpHeader;}
			set {_LastModifiedDateHttpHeader = value;}
		}
		public string CacheFolder
		{
			get {return _CacheFolder;}
			set {_CacheFolder = value;}
		}
		public bool IsCacheFolderRelative
		{
			get {return _IsCacheFolderRelative;}
			set {_IsCacheFolderRelative = value;}
		}
		public MimeTexUsageType MimeTexUsage
		{
			get {return _MimeTexUsage;}
			set {_MimeTexUsage = value;}
		}
		public string MimeTexExeRelativeFilePath
		{
			get {return _MimeTexExeRelativeFilePath;}
			set {_MimeTexExeRelativeFilePath = value;}
		}
		public string MimeTexFullCgiUrl
		{
			get {return _MimeTexFullCgiUrl;}
			set {_MimeTexFullCgiUrl = value;}
		}
		public string MimeTexExternalServerUrl
		{
			get {return _MimeTexExternalServerUrl;}
			set {_MimeTexExternalServerUrl = value;}
		}
		//TODO: late additions. Need to add code for these properties!
		public TimeSpan ClientCacheAgeForErrorMessages
		{
			get {return TimeSpan.FromMinutes(15);}
		}
		public DateTime ClientCacheExpirationForErrorMessages
		{
			get {return DateTime.Now.AddMinutes(15);}
		}
#endregion

		//Each setting name in the config file will have this prefix
		private const string ConfigPrefix = "Eq2Img_";	

		//Load settings from config file
		public void LoadFromConfiguration()
		{
			string adminKeyFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "AdminKey"];
			if (adminKeyFromConfig != null)
				_AdminKey = adminKeyFromConfig;
			else
				_AdminKey = string.Empty;

			string clientCacheAgeInSecondsFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "ClientCacheAgeInSeconds"];
			if (clientCacheAgeInSecondsFromConfig != null)
				_ClientCacheAge = TimeSpan.FromSeconds(int.Parse(clientCacheAgeInSecondsFromConfig, CultureInfo.InvariantCulture));
			else
				_ClientCacheAge = TimeSpan.FromDays(DefaultClientCacheAgeInDays);

			string isCacheFolderRelativeFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "IsCacheFolderRelative"];
			if (isCacheFolderRelativeFromConfig != null)
				_IsCacheFolderRelative = Convert.ToBoolean(isCacheFolderRelativeFromConfig, CultureInfo.InvariantCulture);
			else
				_IsCacheFolderRelative = DefaultIsCacheFolderRelative;

			string equationMaxSizeFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "EquationMaxSize"];
			if (equationMaxSizeFromConfig != null)
				_EquationMaxSize = Convert.ToInt32(equationMaxSizeFromConfig, CultureInfo.InvariantCulture);
			else
				_EquationMaxSize = DefaultEquationMaxSize;

			//Get the path where to store cache images. This coule be either relative to current web app path OR
			//absolute path on server
			string cacheFolderFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "ImageCacheFolder"];
			if (cacheFolderFromConfig != null)
			{
				if (_IsCacheFolderRelative)	//then build absolute path
					_CacheFolder = HttpContext.Current.Server.MapPath(cacheFolderFromConfig);
				else	//get absolute path as is
					_CacheFolder = cacheFolderFromConfig;
			}
			else
			{
				if (!_IsCacheFolderRelative)
				{
					//absolute path must be set if _IsCacheFolderRelative is set to false
					throw new ArgumentException(@"The setting in web.config '" + ConfigPrefix + "IsCacheFolderRelative" + "' is set to false but no absolute path has been provided", ConfigPrefix + "ImageCacheFolder");
				}
				else
					_CacheFolder = HttpContext.Current.Server.MapPath(DefaultCacheFolder);
			}

			string MimeTexUsageFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "MimeTexUsage"];
			if (MimeTexUsageFromConfig != null)
				_MimeTexUsage =(MimeTexUsageType) System.Enum.Parse(typeof(MimeTexUsageType), MimeTexUsageFromConfig, true);
			else
				_MimeTexUsage = DefaultMimeTexUsage;

			string MimeTexExeRelativeFilePathFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "ExeRelativeFilePath"];
			if (MimeTexExeRelativeFilePathFromConfig != null)
				_MimeTexExeRelativeFilePath = MimeTexExeRelativeFilePathFromConfig;
			else
				_MimeTexExeRelativeFilePath = DefaultMimeTexExeRelativeFilePath;

			string MimeTexFullCgiUrlFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "FullCGIUrl"];
			if (MimeTexFullCgiUrlFromConfig != null)
				_MimeTexFullCgiUrl = MimeTexFullCgiUrlFromConfig;
			else
				_MimeTexFullCgiUrl = DefaultMimeTexFullCgiUrl;

			string MimeTexExternalServerUrlFromConfig = ConfigurationSettings.AppSettings[ConfigPrefix + "ExternalServerUrl"];
			if (MimeTexExternalServerUrlFromConfig != null)
				_MimeTexExternalServerUrl = MimeTexExternalServerUrlFromConfig;
			else
				_MimeTexExternalServerUrl = DefaultMimeTexExternalServerUrl;

			_LastModifiedDateHttpHeader = DateTime.Now;
		}

		//Gets the currenly cached settings object OR loads new from config
		private static object _CachedSettingsLocker = new object();
		public static Eq2ImgSettings GetFromContext(HttpContext context)
		{
			if (context == null) throw new ArgumentNullException("context", "The context parameter can not be null");

			Eq2ImgSettings settings = (Eq2ImgSettings) context.Cache["Eq2ImgSettings"];
			if (settings == null)
			{
				lock(_CachedSettingsLocker)
				{
					settings = (Eq2ImgSettings) context.Cache["Eq2ImgSettings"];
					if (settings == null)
					{
						settings = new Eq2ImgSettings();
						settings.LoadFromConfiguration();
						context.Cache.Add("Eq2ImgSettings", settings, new CacheDependency(context.Server.MapPath("web.config")), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.Low, null);
					}
				}
			}
			else {/* return existing settings */}; 

			return settings;
		}
	}

	public enum MimeTexUsageType
	{
		None = 0,
		InProc = 1,
		OutProc = 2,
		Cgi = 3,
		ExternalUrl = 4
	}
}
