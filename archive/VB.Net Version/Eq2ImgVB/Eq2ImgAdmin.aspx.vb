Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Web
Imports System.Web.SessionState
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Astrila.Eq2Img
    ''' <summary>
    ''' Summary description for Eq2ImgAdmin.
    ''' </summary>
    <ComVisible(False)> _
    Public Class Eq2ImgAdmin : Inherits System.Web.UI.Page
        Protected mainPanel As System.Web.UI.WebControls.Panel
        Protected Label1 As System.Web.UI.WebControls.Label
        Protected adminKeyTextBox As System.Web.UI.WebControls.TextBox
        Protected WithEvents adminKeySubmitButton As System.Web.UI.WebControls.Button
        Protected KeyNotSetLabel As System.Web.UI.WebControls.Label
        Protected WithEvents deleteCachedImagesCheckBox As System.Web.UI.WebControls.CheckBox
        Protected unloadMimeTexDLLCheckBox As System.Web.UI.WebControls.CheckBox
        Protected percentOfCachedImagesToBeDeletedTextBox As System.Web.UI.WebControls.TextBox
        Protected Label2 As System.Web.UI.WebControls.Label
        Protected deleteEqGifsCheckBox As System.Web.UI.WebControls.CheckBox
        Protected deleteErrorGifsCheckBox As System.Web.UI.WebControls.CheckBox
        Protected deleteCachedImagesOptionsPanel As System.Web.UI.WebControls.Panel
        Protected PercentageBoxRangeValidator As System.Web.UI.WebControls.RangeValidator
        Protected WithEvents unloadHelpLinkButton As System.Web.UI.WebControls.LinkButton
        Protected MimeTexUnloadHelpPanel As System.Web.UI.WebControls.Panel

        Private settings As Eq2ImgSettings = Nothing
        Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
            'retrieve the settings for caching and other stuff
            settings = Eq2ImgSettings.GetFromContext(Me.Context)

            If settings.AdminKey.Length = 0 Then
                KeyNotSetLabel.Visible = True
                mainPanel.Visible = False
            Else
                KeyNotSetLabel.Visible = False
                mainPanel.Visible = True

            End If
        End Sub

#Region "Web Form Designer generated code"
        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            '
            ' CODEGEN: This call is required by the ASP.NET Web Form Designer.
            '
            InitializeComponent()
            MyBase.OnInit(e)
        End Sub

        ''' <summary>
        ''' Required method for Designer support - do not modify
        ''' the contents of this method with the code editor.
        ''' </summary>
        Private Sub InitializeComponent()
            '			Me.deleteCachedImagesCheckBox.CheckedChanged += New System.EventHandler(Me.deleteCachedImagesCheckBox_CheckedChanged);
            '			Me.unloadHelpLinkButton.Click += New System.EventHandler(Me.unloadHelpLinkButton_Click);
            '			Me.adminKeySubmitButton.Click += New System.EventHandler(Me.adminKeySubmitButton_Click);
            '			Me.Load += New System.EventHandler(Me.Page_Load);

        End Sub
#End Region


        Private Sub adminKeySubmitButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles adminKeySubmitButton.Click
            If String.CompareOrdinal(adminKeyTextBox.Text, settings.AdminKey) = 0 Then
                If deleteCachedImagesCheckBox.Checked Then
                    Dim percentFileToDelete As Double = Double.Parse(percentOfCachedImagesToBeDeletedTextBox.Text, System.Globalization.CultureInfo.CurrentCulture) / 100
                    If deleteEqGifsCheckBox.Checked Then
                        DeleteCacheFiles(settings.CacheFolder, "eq_*.gif", percentFileToDelete)
                    End If
                    If deleteErrorGifsCheckBox.Checked Then
                        DeleteCacheFiles(settings.CacheFolder, "err_*.gif", percentFileToDelete)
                    End If
                End If
                If unloadMimeTexDLLCheckBox.Checked Then
                    Dim MimeTexDLLHandle As IntPtr = NativeMethods.GetModuleHandle("MimeTex.dll")
                    If Not MimeTexDLLHandle.Equals(IntPtr.Zero) Then
                        NativeMethods.FreeLibrary(MimeTexDLLHandle)
                    Else
                        Response.Write("MimeTex.DLL is not yet loaded in the process")
                    End If
                End If
            Else
                Response.Write("Invalid admin key!")
            End If
        End Sub

        Private Shared Sub DeleteCacheFiles(ByVal cachePath As String, ByVal fileNamePattern As String, ByVal percentageToDelete As Double)
            If Directory.Exists(cachePath) Then
                Dim cacheDirectoryInfo As DirectoryInfo = New DirectoryInfo(cachePath)
                Dim fileList As FileSystemInfo() = cacheDirectoryInfo.GetFileSystemInfos(fileNamePattern)
                System.Array.Sort(fileList, New FileSystemInfoCustomComparer)
                For fileIndex As Integer = 0 To CType(fileList.Length * percentageToDelete, Integer) - 1
                    fileList(fileIndex).Delete()
                Next fileIndex
            Else
                'No cached files to delete
            End If
        End Sub


        Private Class FileSystemInfoCustomComparer : Implements IComparer
            Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
                If (Not x Is Nothing) AndAlso (Not y Is Nothing) Then
                    Return (CType(x, FileSystemInfo)).LastAccessTime.CompareTo((CType(y, FileSystemInfo)).LastAccessTime)
                Else
                    Return DirectCast(IIf(Object.ReferenceEquals(x, y), 0, (IIf((x Is Nothing), -1, 1))), Integer)
                End If
            End Function
        End Class

        Private Sub deleteCachedImagesCheckBox_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles deleteCachedImagesCheckBox.CheckedChanged
            deleteCachedImagesOptionsPanel.Visible = deleteCachedImagesCheckBox.Checked
        End Sub

        Private Sub unloadHelpLinkButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles unloadHelpLinkButton.Click
            MimeTexUnloadHelpPanel.Visible = True
        End Sub
    End Class
End Namespace
