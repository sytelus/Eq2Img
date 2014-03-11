Imports Microsoft.VisualBasic
Imports System
Imports System.Web
Imports System.Net
Imports System.IO
Imports System.Drawing
Imports System.Globalization

Namespace Astrila.Eq2Img
    ''' <summary>
    ''' Summary description for convert.
    ''' </summary>
    Public Class ShowEq : Implements IHttpHandler
        Public Overridable Sub ProcessRequest(ByVal context As System.Web.HttpContext) Implements IHttpHandler.ProcessRequest
            If context Is Nothing Then
                Throw New ArgumentNullException("context", "context can not be null")
            Else
                ' we have a good context
            End If

            'retrieve the settings for caching and other stuff
            Dim settings As Eq2ImgSettings = Eq2ImgSettings.GetFromContext(context)

            Try
                'Build the path where we should be saving the gif for equations
                Dim filePathForCachedEqImage As String = GetFilePathForCachedEqImage(settings.CacheFolder, context.Request.QueryString.ToString())

                'If we have never generated the cache file for this equation
                If (Not File.Exists(filePathForCachedEqImage)) Then
                    'Retrieve the equation from the query string and check it
                    'Request.QueryString.ToString doesn't work for us because chars are all hex escapped
                    Dim equation As String = String.Empty
                    If context.Request.Url.Query.Length > 1 Then
                        equation = context.Request.Url.Query.Substring(1)
                        equation = HexUnEscapper.UnescapeHexCharsInString(equation)
                    Else
                        equation = String.Empty
                    End If

                    If equation.Length = 0 Then
                        Throw New EquationRenderException("Empty Equation")
                    ElseIf equation.Length > settings.EquationMaxSize Then
                        Throw New EquationRenderException("Equation Too Long: " & equation.Substring(0, settings.EquationMaxSize))
                    Else
                        Select Case settings.MimeTexUsage
                            Case MimeTexUsageType.InProc
                                CreateGifUsingInProc(context, equation, filePathForCachedEqImage, settings)
                            Case MimeTexUsageType.Cgi
                                CreateGifUsingCGI(context, equation, filePathForCachedEqImage, settings)
                            Case MimeTexUsageType.ExternalUrl
                                CreateGifUsingExternalUrl(context, equation, filePathForCachedEqImage, settings)
                            Case MimeTexUsageType.OutProc
                                CreateGifUsingOutProc(context, equation, filePathForCachedEqImage, settings)
                            Case MimeTexUsageType.None 'In this mode we server only from cache. This mode is useful if we want to take MimeTex DLL offline for upgrade.
                            Case Else
                                Throw New ArgumentException("Unknown MimeTexUsage: " & settings.MimeTexUsage.ToString())
                        End Select

                        'See if process has really generated the gif file
                        If (Not File.Exists(filePathForCachedEqImage)) Then
                            Throw New EquationRenderException("ImgNotFound: " & equation)
                        Else 'Send teh rendered equation to client
                            SendGifFileToClient(context, filePathForCachedEqImage, settings, False)
                        End If
                    End If
                Else 'Send gif file we already had generated
                    SendGifFileToClient(context, filePathForCachedEqImage, settings, False)
                End If
            Catch ex As EquationRenderException 'Send all rendering errors as message in gif files
                Dim filePathForCachedErrImage As String = GetFilePathForCachedErrImage(settings.CacheFolder, ex.Message)

                'Send already cached error message or create a new gif containing the message
                If (Not File.Exists(filePathForCachedErrImage)) Then
                    WriteStringInGif(ex.Message, filePathForCachedErrImage)
                End If
                SendGifFileToClient(context, filePathForCachedErrImage, settings, True)
            End Try
        End Sub

        Private Sub CreateGifUsingCGI(ByVal context As HttpContext, ByVal equation As String, ByVal filePathForCachedEqImage As String, ByVal settings As Eq2ImgSettings)
            Dim browser As WebClient = New WebClient
            Try
                Dim imageData As Byte() = browser.DownloadData(settings.MimeTexFullCgiUrl & "?" & equation)
                SaveImageInFile(filePathForCachedEqImage, imageData, True)
            Finally
                If TypeOf browser Is IDisposable Then
                    Dim disp As IDisposable = browser
                    disp.Dispose()
                End If
            End Try
        End Sub
        Private Sub CreateGifUsingOutProc(ByVal context As HttpContext, ByVal equation As String, ByVal filePathForCachedEqImage As String, ByVal settings As Eq2ImgSettings)
            Dim MimeTexExeFilePath As String = Path.Combine(context.Request.PhysicalApplicationPath, settings.MimeTexExeRelativeFilePath)
            Dim MimeTexStartupInfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo(MimeTexExeFilePath, "-e """ & filePathForCachedEqImage & """" & " " & """" & equation & """")
            MimeTexStartupInfo.CreateNoWindow = True
            'MimeTexStartupInfo.EnvironmentVariables.Clear();
            MimeTexStartupInfo.ErrorDialog = False
            MimeTexStartupInfo.RedirectStandardOutput = True
            MimeTexStartupInfo.RedirectStandardError = True
            MimeTexStartupInfo.UseShellExecute = False
            Dim MimeTexProcess As System.Diagnostics.Process = System.Diagnostics.Process.Start(MimeTexStartupInfo)
            MimeTexProcess.WaitForExit()
            If (Not File.Exists(filePathForCachedEqImage)) Then
                Dim errorMessage As String = "Failed to render eq '{0}':" & MimeTexProcess.StandardOutput.ReadToEnd()
                errorMessage += Environment.NewLine
                errorMessage += MimeTexProcess.StandardError.ReadToEnd()
                Throw New EquationRenderException(String.Format(CultureInfo.InvariantCulture, errorMessage, equation))
            Else
                'We already have the gif file
            End If
        End Sub

        Private Sub CreateGifUsingExternalUrl(ByVal context As HttpContext, ByVal equation As String, ByVal filePathForCachedEqImage As String, ByVal settings As Eq2ImgSettings)
            Dim browser As WebClient = New WebClient
            Try
                Dim imageData As Byte() = browser.DownloadData(String.Format(CultureInfo.InvariantCulture, settings.MimeTexExternalServerUrl, equation))
                SaveImageInFile(filePathForCachedEqImage, imageData, True)
            Finally
                If TypeOf browser Is IDisposable Then
                    Dim disp As IDisposable = browser
                    disp.Dispose()
                End If
            End Try
        End Sub

        'MimeTeX is unmanaged code and is not thread safe. We must protect the call
        'as there would be lots of simultaneous requests
        Private Shared _MimeTeXCallLocker As Object = New Object
        Private Sub CreateGifUsingInProc(ByVal context As HttpContext, ByVal equation As String, ByVal filePathForCachedEqImage As String, ByVal settings As Eq2ImgSettings)
            SyncLock _MimeTeXCallLocker
                NativeMethods.CreateGifFromEq(equation, filePathForCachedEqImage)
            End SyncLock
        End Sub


        Private Shared Sub WriteStringInGif(ByVal message As String, ByVal filePath As String)
            Dim valueToWrite As String = "[" & message & "]"
            Dim fontToUse As Font = New Font(FontFamily.GenericSansSerif, 8)
            Dim stringSize As SizeF = SizeF.Empty

            ' calculate size of the string.
            Dim size1Bitmap As Bitmap = New Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            Try
                Dim g As Graphics = Graphics.FromImage(size1Bitmap)
                Try
                    stringSize = g.MeasureString(valueToWrite, fontToUse)
                Finally
                    If TypeOf g Is IDisposable Then
                        Dim disp As IDisposable = g
                        disp.Dispose()
                    End If
                End Try
            Finally
                If TypeOf size1Bitmap Is IDisposable Then
                    Dim disp As IDisposable = size1Bitmap
                    disp.Dispose()
                End If
            End Try

            Dim messageBitmap As Bitmap = New Bitmap(CInt(stringSize.Width), CInt(stringSize.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            Try
                Dim g As Graphics = Graphics.FromImage(messageBitmap)
                Try
                    'g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.FillRectangle(New SolidBrush(Color.White), New Rectangle(0, 0, CInt(stringSize.Width), CInt(stringSize.Height)))
                    g.DrawString(valueToWrite, fontToUse, Brushes.Red, 0, 0)
                    'messageBitmap.MakeTransparent(Color.White);
                    g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush)
                    messageBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Gif)
                Finally
                    If TypeOf g Is IDisposable Then
                        Dim disp As IDisposable = g
                        disp.Dispose()
                    End If
                End Try
            Finally
                If TypeOf messageBitmap Is IDisposable Then
                    Dim disp As IDisposable = messageBitmap
                    disp.Dispose()
                End If
            End Try
        End Sub

        Private Shared Function GetEquationHash(ByVal equation As String) As String
            Dim md5Agent As System.Security.Cryptography.MD5 = New System.Security.Cryptography.MD5CryptoServiceProvider
            Try
                Dim byteHash As Byte() = md5Agent.ComputeHash(System.Text.Encoding.UTF8.GetBytes(equation))
                'Not all chars in base64 are valid file name chars: http://www.garykessler.net/library/base64.html
                Return System.Convert.ToBase64String(byteHash).Replace("+", "-").Replace("/", "_")
            Finally
                If TypeOf md5Agent Is IDisposable Then
                    Dim disp As IDisposable = md5Agent
                    disp.Dispose()
                End If
            End Try
        End Function

        Private Shared Function GetFilePathForCachedEqImage(ByVal saveFolder As String, ByVal equation As String) As String
            Return Path.Combine(saveFolder, "eq_" & GetEquationHash(equation) & ".gif")
        End Function

        Private Shared Function GetFilePathForCachedErrImage(ByVal saveFolder As String, ByVal equation As String) As String
            Return Path.Combine(saveFolder, "err_" & GetEquationHash(equation) & ".gif")
        End Function


        Private Shared _GifStandardHeaderBytes As Byte() = New Byte() {&H47, &H49, &H46} 'ASCII Bytes for string "GIF"
        Private Shared Sub ValidateGifData(ByVal imageData As Byte())

            If imageData.Length >= _GifStandardHeaderBytes.Length Then
                For byteIndex As Integer = 0 To _GifStandardHeaderBytes.Length - 1
                    If imageData(byteIndex) <> _GifStandardHeaderBytes(byteIndex) Then
                        Throw New ArgumentException("Invalid GIF header. Byte #" & byteIndex.ToString(CultureInfo.InvariantCulture) & " is not " & _GifStandardHeaderBytes(byteIndex).ToString(CultureInfo.InvariantCulture))
                    End If
                Next byteIndex
            Else
                Throw New ArgumentException("Invalid GIF header. Numbers of bytes is less than expected " & _GifStandardHeaderBytes.Length.ToString(CultureInfo.InvariantCulture))
            End If
        End Sub

        Private Shared Sub SaveImageInFile(ByVal imageFilePath As String, ByVal imageData As Byte(), ByVal validateImageData As Boolean)
            If (Not imageData Is Nothing) AndAlso (imageData.Length > 0) Then
                If validateImageData Then
                    ValidateGifData(imageData)
                End If

                Try
                    Dim gifFileStream As FileStream = File.Create(imageFilePath, imageData.Length)
                    Try
                        gifFileStream.Write(imageData, 0, DirectCast(IIf((imageData.Length < 450000), imageData.Length, 450000), Integer)) 'ToDO: Make this configurable
                    Finally
                        If TypeOf gifFileStream Is IDisposable Then
                            Dim disp As IDisposable = gifFileStream
                            disp.Dispose()
                        End If
                    End Try
                Catch 'multiple threads might try to create the same file
                    If (Not File.Exists(imageFilePath)) Then
                        Throw
                    End If
                End Try
            Else
                'no files should be generated
            End If
        End Sub

        Private Shared Sub SendGifFileToClient(ByVal context As System.Web.HttpContext, ByVal gifFilePath As String, ByVal settings As Eq2ImgSettings, ByVal isErrorMessageImage As Boolean)
            Dim imageData As Byte() = New Byte() {}
            Dim gifFileStream As FileStream = File.Open(gifFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Try
                imageData = New Byte(CType(gifFileStream.Length - 1, Integer)) {}
                gifFileStream.Read(imageData, 0, imageData.Length)
            Finally
                If TypeOf gifFileStream Is IDisposable Then
                    Dim disp As IDisposable = gifFileStream
                    disp.Dispose()
                End If
            End Try

            SendImageToClient(context, imageData, settings, isErrorMessageImage)
        End Sub

        Private Shared Sub SendImageToClient(ByVal context As System.Web.HttpContext, ByVal imageData As Byte(), ByVal settings As Eq2ImgSettings, ByVal isErrorMessageImage As Boolean)
            context.Response.Clear()
            context.Response.ContentType = "image/gif"

            If (Not isErrorMessageImage) Then
                context.Response.Cache.SetExpires(settings.ClientCacheExpiration)
                context.Response.Cache.SetMaxAge(settings.ClientCacheAge)
                context.Response.Cache.SetProxyMaxAge(settings.ClientCacheAge)
            Else
                context.Response.Cache.SetMaxAge(settings.ClientCacheAgeForErrorMessages)
                context.Response.Cache.SetProxyMaxAge(settings.ClientCacheAgeForErrorMessages)
                context.Response.Cache.SetExpires(settings.ClientCacheExpirationForErrorMessages)
            End If
            context.Response.Cache.SetLastModified(settings.LastModifiedDateHttpHeader)
            context.Response.Cache.SetCacheability(HttpCacheability.Public)
            context.Response.CacheControl = "public"
            context.Response.Cache.SetValidUntilExpires(False)
            context.Response.Cache.VaryByParams("*") = True
            context.Response.BinaryWrite(imageData)
            'context.Response.End();	//Ending response will stop cache mechanism
        End Sub

        Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Private Class HexUnEscapper : Inherits System.Uri
            Private Sub New(ByVal dummy As String)
                MyBase.New(dummy)

            End Sub
            Private Shared _OneInstance As HexUnEscapper = New HexUnEscapper("http://stupid.uri.object")
            Public Shared Function UnescapeHexCharsInString(ByVal str As String) As String
                Return _OneInstance.Unescape(str)
            End Function
        End Class


        Public Shared Function HandleEquationQueries() As Boolean
            Dim handlerInstance As IHttpHandler = New ShowEq
            Dim context As HttpContext = HttpContext.Current
            If (context.Request.Url.Query.Length > 1) AndAlso (context.Request.Url.Query.StartsWith("?$") AndAlso (context.Request.Url.Query.EndsWith("$"))) Then
                handlerInstance.ProcessRequest(context)
                Return True
            Else
                Return False
            End If
        End Function

    End Class


    <System.Security.SuppressUnmanagedCodeSecurity()> _
    Friend Class NativeMethods
        Private Sub NativeMethods()
        End Sub
        <System.Runtime.InteropServices.DllImport("MimeTex.dll")> _
        Friend Shared Function CreateGifFromEq(ByVal expr As String, ByVal fileName As String) As Integer
        End Function

        <System.Runtime.InteropServices.DllImport("kernel32.dll")> _
        Friend Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
        End Function

        <System.Runtime.InteropServices.DllImport("kernel32.dll")> _
        Friend Shared Function FreeLibrary(ByVal hLibModule As IntPtr) As <System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function
    End Class

    <Serializable()> _
    Public Class EquationRenderException : Inherits Exception
        Public Sub New()
        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
        Protected Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class
End Namespace

