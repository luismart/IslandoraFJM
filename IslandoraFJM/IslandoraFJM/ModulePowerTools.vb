' .NET Module with the power tools to process PDFs, Images, SWFs, etc. 
'  ImageMagick (http://www.imagemagick.org/script/index.php), swftools(http://www.swftools.org/), pdf2json (https://code.google.com/p/pdf2json/) need to be installed and their paths added
'  to ModuleVariables.vb 

Imports Acrobat
Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports System.Drawing.Image
Imports System.Web.UI.WebControls

Public Module PowerTools

#Region "Image manipulation"
    ''' <summary>
    ''' Gets image height
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getImgHeight(filePath As String) As String
        Dim image As System.Drawing.Image = System.Drawing.Image.FromFile(filePath)
        Dim img_height As Integer = image.Height
        Return img_height
    End Function
    ''' <summary>
    ''' Gets image width
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getImgWidth(filePath As String) As String
        Dim image As System.Drawing.Image = System.Drawing.Image.FromFile(filePath)
        Dim img_width As Integer = image.Width
        Return img_width
    End Function
    ''' <summary>
    ''' Rescales an image using ImageMagick
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <param name="scale"></param>
    ''' <remarks></remarks>
    Public Sub rescaleJPG(file As String, pathIn As String, scale As Integer)
        Shell(ImageMagick_Path & " -scale " & scale & "x" & scale & " " & pathIn & file & " " & pathIn & file, AppWinStyle.Hide, True)
    End Sub
    ''' <summary>
    ''' Converts image to JPG using ImageMagick
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <remarks></remarks>
    Public Sub JPG2PDF(file As String, pathIn As String, pathOut As String)
        Dim cadena = ImageMagick_Path & " -density 200 -units PixelsPerInch " & pathIn & file & " " & pathOut & Replace(file, "jpg", "pdf")
        Shell(cadena, AppWinStyle.MaximizedFocus, True)
    End Sub
#End Region

#Region "PDF manipulation"
    ''' <summary>
    ''' Converts image to JPG using ImageMagick
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <remarks></remarks>
    Public Sub pdf2JPG(file As String, pathIn As String, pathOut As String)
        Dim cadena = ImageMagick_Path & " -density 200 " & pathIn & file & " " & pathOut & Replace(file, "pdf", "jpg")
        Shell(cadena, AppWinStyle.Hide, True)
    End Sub

    ''' <summary>
    ''' Converts pdf to swf using SWFTools
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <remarks></remarks>
    Public Sub pdf2swf(file As String, pathIn As String, pathOut As String)
        Dim cadena = SWFTools_Path & " -p 1 " & pathIn & file & " " & pathOut & Replace(file, "pdf", "swf")
        Shell(cadena, AppWinStyle.Hide, True)
    End Sub

    ''' <summary>
    ''' Converts pdf to json using PDF2JSON
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <param name="firstpage"></param>
    ''' <param name="lastpage"></param>
    ''' <remarks></remarks>
    Public Sub pdf2json(file As String, pathIn As String, pathOut As String, firstpage As Integer, lastpage As Integer)
        Dim cadena = PDF2JSON_Path & " -f " & firstpage & " -l " & lastpage & " -hidden -enc UTF-8 -compress " & pathIn & file & " " & pathOut & Replace(file, "pdf", "json")
        Shell(cadena, AppWinStyle.Hide, True)
    End Sub

    ''' <summary>
    ''' Converts a pdf to txt using Acrobat
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <remarks></remarks>
    Public Sub pdf2txt(file As String, pathIn As String, pathOut As String)
        Dim AcroXApp As Acrobat.AcroApp
        Dim AcroXAVDoc As Acrobat.AcroAVDoc
        Dim AcroXPDDoc As Acrobat.AcroPDDoc
        Dim Filename As String

        Filename = pathIn & file

        AcroXApp = CreateObject("AcroExch.App")
        'AcroXApp.Show()

        AcroXAVDoc = CreateObject("AcroExch.AVDoc")
        AcroXAVDoc.Open(Filename, "Acrobat")

        AcroXPDDoc = AcroXAVDoc.GetPDDoc

        Dim jsObj As Object
        jsObj = AcroXPDDoc.GetJSObject

        jsObj.SaveAs(pathOut & Replace(file, "pdf", "txt"), "com.adobe.acrobat.plain-text")

        AcroXAVDoc.Close(False)
        AcroXApp.Hide()
        AcroXApp.Exit()
    End Sub

    ''' <summary>
    ''' Returns the number of pages of a PDF using the Acrobat library
    ''' </summary>
    ''' <param name="pdf_location"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function PDFnum_pages(pdf_location As String) As Integer
        Dim AcroXApp As Acrobat.AcroApp
        Dim AcroXAVDoc As Acrobat.AcroAVDoc
        Dim AcroXPDDoc As Acrobat.AcroPDDoc
        AcroXApp = CreateObject("AcroExch.App")
        'AcroXApp.Show()

        AcroXAVDoc = CreateObject("AcroExch.AVDoc")
        AcroXAVDoc.Open(pdf_location, "Acrobat")

        AcroXPDDoc = AcroXAVDoc.GetPDDoc
        Dim num_pages = AcroXPDDoc.GetNumPages
        AcroXAVDoc.Close(False)
        AcroXApp.Hide()
        AcroXApp.Exit()
        Return num_pages
    End Function

    ''' <summary>
    ''' Splits a multipage PDF into one pages PDFs  
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="pathIn"></param>
    ''' <param name="pathOut"></param>
    ''' <remarks></remarks>
    Public Sub SplitPDF(file As String, pathIn As String, pathOut As String)
        Dim AcroXApp As Acrobat.AcroApp
        Dim AcroXAVDoc As Acrobat.AcroAVDoc
        Dim AcroXPDDoc As Acrobat.AcroPDDoc
        AcroXApp = CreateObject("AcroExch.App")
        'AcroXApp.Show()

        AcroXAVDoc = CreateObject("AcroExch.AVDoc")
        AcroXAVDoc.Open(pathIn & file, "Acrobat")

        AcroXPDDoc = AcroXAVDoc.GetPDDoc
        Dim num_pages = AcroXPDDoc.GetNumPages

        'check if folder exists if not create it
        If (Not System.IO.Directory.Exists(pathOut)) Then
            System.IO.Directory.CreateDirectory(pathOut)
        End If

        For i = 0 To num_pages - 1
            Dim newAcroXPDDoc As New Acrobat.AcroPDDoc
            newAcroXPDDoc.Create()
            Dim NewName As String = pathOut & Replace(file, ".pdf", "") & "Page_" & i + 1 & "_of_" & num_pages & ".pdf"
            newAcroXPDDoc.InsertPages(-1, AcroXPDDoc, i, 1, 0)
            newAcroXPDDoc.Save(1, NewName)
            newAcroXPDDoc.Close()
            newAcroXPDDoc = Nothing
        Next

        AcroXAVDoc.Close(False)
        AcroXApp.Hide()
        AcroXApp.Exit()

    End Sub

    ''' <summary>
    ''' From a PDF creates a folder where it puts pdf pages, jpgs, thumbnails, swfs and jsons every 10 pages
    ''' </summary>
    ''' <param name="PDF_Name">PDF File Name</param>
    ''' <param name="PDF_Path">PDF Path</param>
    ''' <param name="splitFiles"></param>
    ''' <param name="createJPGs"></param>
    ''' <param name="createTNs"></param>
    ''' <param name="createJSONs"></param>
    ''' <param name="createSWFs"></param>
    ''' <remarks></remarks>
    Public Sub PDF2BookFiles(PDF_Name As String, PDF_Path As String, splitFiles As Boolean, createJPGs As Boolean, createTNs As Boolean, createJSONs As Boolean, createSWFs As Boolean)
        'split pdfs into pages
        If splitFiles Then
            SplitPDF(PDF_Name, PDF_Path, PDF_Path & Replace(PDF_Name, ".pdf", "\"))
        End If

        'create json
        If createJSONs Then
            Dim num_pages As Integer = PDFnum_pages(PDF_Path & PDF_Name)
            For i = 10 To num_pages Step 10
                pdf2json(PDF_Name, PDF_Path, PDF_Path & Replace(PDF_Name, ".pdf", "\page_") & i - 10 & "_" & i - 1 & "_", i - 10, i - 1) ' generate the json pages
            Next
            Dim lastJSONPage As Integer = (10 - (num_pages Mod 10)) + num_pages
            pdf2json(PDF_Name, PDF_Path, PDF_Path & Replace(PDF_Name, ".pdf", "\page_") & lastJSONPage - 10 & "_" & lastJSONPage - 1 & "_", lastJSONPage - 10, num_pages) ' generate the last json page needed for flexpaper
        End If

        Dim dir As New IO.DirectoryInfo(PDF_Path & Replace(PDF_Name, ".pdf", "\"))
        Dim files As IO.FileInfo() = dir.GetFiles()
        Dim file As IO.FileInfo

        'create JPGs
        If createJPGs Then
            files = dir.GetFiles()
            For Each file In files
                pdf2JPG(file.Name, file.DirectoryName & "\", file.DirectoryName & "\")
            Next
        End If

        'create TNs
        If createTNs Then
            files = dir.GetFiles()
            For Each file In files
                If file.Extension = ".jpg" Then
                    rescaleJPG(file.Name, file.DirectoryName & "\", file.DirectoryName & "\TN_", 250)
                End If
            Next
        End If

        'create SWFs
        If createSWFs Then
            files = dir.GetFiles()
            For Each file In files
                If file.Extension = ".pdf" Then
                    pdf2swf(file.Name, file.DirectoryName & "\", file.DirectoryName & "\")
                End If
            Next
        End If
    End Sub
#End Region

End Module
