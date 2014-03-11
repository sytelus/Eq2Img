Imports Microsoft.VisualBasic
Imports System
Imports System.Web
Imports System.Configuration
Imports System.Globalization
Imports System.Web.Caching

Namespace Astrila.Eq2Img
	''' <summary>
	''' Summary description for ShowEqSettings.
	''' </summary>
	<Serializable> _
	Public Class Eq2ImgSettings
		'Defines the default values for the settings if they weren't specified in 
		'web.config
		Private Const DefaultClientCacheAgeInDays As Integer = 180
		Private Const DefaultCacheFolder As String = "CachedEqImages"
		Private Const DefaultIsCacheFolderRelative As Boolean = True
		Private Const DefaultMimeTexUsage As MimeTexUsageType = MimeTexUsageType.InProc
		Private Const DefaultMimeTexExeRelativeFilePath As String = "cgi\MimeTex.exe"
		Private Const DefaultMimeTexFullCgiUrl As String = "cgi/MimeTex.exe"
        Private Const DefaultMimeTexExternalServerUrl As String = "http://www.ShitalShah.com/?{0}"
        'This may ease DoS attack but its required for figure type images that MimeTeX supports
        Private Const DefaultEquationMaxSize As Integer = 10000

		'Set up the private members for the properties
		Private _ClientCacheAge As TimeSpan = TimeSpan.FromDays(DefaultClientCacheAgeInDays)
		Private _CacheFolder As String = DefaultCacheFolder
		Private _IsCacheFolderRelative As Boolean = DefaultIsCacheFolderRelative
		Private _MimeTexUsage As MimeTexUsageType = DefaultMimeTexUsage
		Private _MimeTexExeRelativeFilePath As String = DefaultMimeTexExeRelativeFilePath
		Private _MimeTexFullCgiUrl As String = DefaultMimeTexFullCgiUrl
		Private _MimeTexExternalServerUrl As String = DefaultMimeTexExternalServerUrl
		Private _EquationMaxSize As Integer = DefaultEquationMaxSize
		Private _AdminKey As String = String.Empty

		'By default the cache files created for equations would have this time stamp
		Private _LastModifiedDateHttpHeader As DateTime = DateTime.Now

#region "Property Getters/Setters
		Public Property AdminKey() As String
			Get
				Return _AdminKey
			End Get
			Set
				_AdminKey = Value
			End Set
		End Property
		Public Property ClientCacheAge() As TimeSpan
			Get
				Return _ClientCacheAge
			End Get
			Set
				_ClientCacheAge = Value
			End Set
		End Property
		Public Property EquationMaxSize() As Integer
			Get
				Return _EquationMaxSize
			End Get
			Set
				_EquationMaxSize = Value
			End Set
		End Property
		Public ReadOnly Property ClientCacheExpiration() As DateTime
			Get
				Return DateTime.Now.Add(_ClientCacheAge)
			End Get
		End Property
		Public Property LastModifiedDateHttpHeader() As DateTime
			Get
				Return _LastModifiedDateHttpHeader
			End Get
			Set
				_LastModifiedDateHttpHeader = Value
			End Set
		End Property
		Public Property CacheFolder() As String
			Get
				Return _CacheFolder
			End Get
			Set
				_CacheFolder = Value
			End Set
		End Property
		Public Property IsCacheFolderRelative() As Boolean
			Get
				Return _IsCacheFolderRelative
			End Get
			Set
				_IsCacheFolderRelative = Value
			End Set
		End Property
		Public Property MimeTexUsage() As MimeTexUsageType
			Get
				Return _MimeTexUsage
			End Get
			Set
				_MimeTexUsage = Value
			End Set
		End Property
		Public Property MimeTexExeRelativeFilePath() As String
			Get
				Return _MimeTexExeRelativeFilePath
			End Get
			Set
				_MimeTexExeRelativeFilePath = Value
			End Set
		End Property
		Public Property MimeTexFullCgiUrl() As String
			Get
				Return _MimeTexFullCgiUrl
			End Get
			Set
				_MimeTexFullCgiUrl = Value
			End Set
		End Property
		Public Property MimeTexExternalServerUrl() As String
			Get
				Return _MimeTexExternalServerUrl
			End Get
			Set
				_MimeTexExternalServerUrl = Value
			End Set
		End Property
		'TODO: late additions. Need to add code for these properties!
		Public ReadOnly Property ClientCacheAgeForErrorMessages() As TimeSpan
			Get
				Return TimeSpan.FromMinutes(15)
			End Get
		End Property
		Public ReadOnly Property ClientCacheExpirationForErrorMessages() As DateTime
			Get
				Return DateTime.Now.AddMinutes(15)
			End Get
		End Property
#End Region

		'Each setting name in the config file will have this prefix
		Private Const ConfigPrefix As String = "Eq2Img_"

		'Load settings from config file
		Public Sub LoadFromConfiguration()
			Dim adminKeyFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "AdminKey")
			If Not adminKeyFromConfig Is Nothing Then
				_AdminKey = adminKeyFromConfig
			Else
				_AdminKey = String.Empty
			End If

			Dim clientCacheAgeInSecondsFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "ClientCacheAgeInSeconds")
			If Not clientCacheAgeInSecondsFromConfig Is Nothing Then
				_ClientCacheAge = TimeSpan.FromSeconds(Integer.Parse(clientCacheAgeInSecondsFromConfig, CultureInfo.InvariantCulture))
			Else
				_ClientCacheAge = TimeSpan.FromDays(DefaultClientCacheAgeInDays)
			End If

			Dim isCacheFolderRelativeFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "IsCacheFolderRelative")
			If Not isCacheFolderRelativeFromConfig Is Nothing Then
				_IsCacheFolderRelative = Convert.ToBoolean(isCacheFolderRelativeFromConfig, CultureInfo.InvariantCulture)
			Else
				_IsCacheFolderRelative = DefaultIsCacheFolderRelative
			End If

			Dim equationMaxSizeFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "EquationMaxSize")
			If Not equationMaxSizeFromConfig Is Nothing Then
				_EquationMaxSize = Convert.ToInt32(equationMaxSizeFromConfig, CultureInfo.InvariantCulture)
			Else
				_EquationMaxSize = DefaultEquationMaxSize
			End If

			'Get the path where to store cache images. This coule be either relative to current web app path OR
			'absolute path on server
			Dim cacheFolderFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "ImageCacheFolder")
			If Not cacheFolderFromConfig Is Nothing Then
				If _IsCacheFolderRelative Then 'then build absolute path
					_CacheFolder = HttpContext.Current.Server.MapPath(cacheFolderFromConfig)
				Else 'get absolute path as is
					_CacheFolder = cacheFolderFromConfig
				End If
			Else
				If (Not _IsCacheFolderRelative) Then
					'absolute path must be set if _IsCacheFolderRelative is set to false
					Throw New ArgumentException("The setting in web.config '" & ConfigPrefix & "IsCacheFolderRelative" & "' is set to false but no absolute path has been provided", ConfigPrefix & "ImageCacheFolder")
				Else
					_CacheFolder = HttpContext.Current.Server.MapPath(DefaultCacheFolder)
				End If
			End If

			Dim MimeTexUsageFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "MimeTexUsage")
			If Not MimeTexUsageFromConfig Is Nothing Then
				_MimeTexUsage =CType(System.Enum.Parse(GetType(MimeTexUsageType), MimeTexUsageFromConfig, True), MimeTexUsageType)
			Else
				_MimeTexUsage = DefaultMimeTexUsage
			End If

			Dim MimeTexExeRelativeFilePathFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "ExeRelativeFilePath")
			If Not MimeTexExeRelativeFilePathFromConfig Is Nothing Then
				_MimeTexExeRelativeFilePath = MimeTexExeRelativeFilePathFromConfig
			Else
				_MimeTexExeRelativeFilePath = DefaultMimeTexExeRelativeFilePath
			End If

			Dim MimeTexFullCgiUrlFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "FullCGIUrl")
			If Not MimeTexFullCgiUrlFromConfig Is Nothing Then
				_MimeTexFullCgiUrl = MimeTexFullCgiUrlFromConfig
			Else
				_MimeTexFullCgiUrl = DefaultMimeTexFullCgiUrl
			End If

			Dim MimeTexExternalServerUrlFromConfig As String = ConfigurationSettings.AppSettings(ConfigPrefix & "ExternalServerUrl")
			If Not MimeTexExternalServerUrlFromConfig Is Nothing Then
				_MimeTexExternalServerUrl = MimeTexExternalServerUrlFromConfig
			Else
				_MimeTexExternalServerUrl = DefaultMimeTexExternalServerUrl
			End If

			_LastModifiedDateHttpHeader = DateTime.Now
		End Sub

		'Gets the currenly cached settings object OR loads new from config
		Private Shared _CachedSettingsLocker As Object = New Object()
		Public Shared Function GetFromContext(ByVal context As HttpContext) As Eq2ImgSettings
			If context Is Nothing Then
                Throw New ArgumentNullException("context", "The context parameter can not be null")
			End If

			Dim settings As Eq2ImgSettings = CType(context.Cache("Eq2ImgSettings"), Eq2ImgSettings)
			If settings Is Nothing Then
				SyncLock _CachedSettingsLocker
					settings = CType(context.Cache("Eq2ImgSettings"), Eq2ImgSettings)
					If settings Is Nothing Then
						settings = New Eq2ImgSettings()
						settings.LoadFromConfiguration()
						context.Cache.Add("Eq2ImgSettings", settings, New CacheDependency(context.Server.MapPath("web.config")), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.Low, Nothing)
					End If
				End SyncLock
            Else
                'return existing settings
            End If

            Return settings
		End Function
	End Class

	Public Enum MimeTexUsageType
		None = 0
		InProc = 1
		OutProc = 2
		Cgi = 3
		ExternalUrl = 4
	End Enum
End Namespace
