' .NET Module to interact with Fedora´s REST API
'  The Fedora REST API exposes a subset of the Fedora Access and Management APIs as a RESTful (Representational State Transfer) Web Service. 
'  See more info at https://wiki.duraspace.org/display/FEDORA34/REST+API
'
Imports System.Net
Imports System.IO
Imports System.Xml
Imports System.Web.HttpServerUtility

Public Module REST_API
    Enum format
        XML
        HTML
    End Enum

#Region "Public Methods"

    Public Function listDatastreams(PID As String, format As format) As XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams?format=" & format)
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim xml = New XmlDocument
        xml.Load(MyRequest.GetResponse().GetResponseStream())
        Return xml
    End Function
    Public Function ingest(PID As String, label As String) As Boolean
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "?label=" & label)
        MyRequest.Method = "POST"
        MyRequest.Credentials = New NetworkCredential(fedoraUsername, fedoraPassword)
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        If src.Contains(PID) Then
            Call LogInformation(Now() & " INFO - Added object  " & PID)
            Return True
        Else
            Call LogInformation(Now() & " ERROR - Adding object  " & PID)
            Return False
        End If
    End Function
    Public Function modifyObject(PID As String, Optional label As String = "", Optional ownerId As String = "", Optional state As String = "", Optional LogMessage As String = "") As Boolean
        Dim Doc As New XmlDocument
        Dim parameters As String
        parameters = ""
        If label <> "" Then
            parameters = "label=" & label
        End If
        If ownerId <> "" Then
            parameters = parameters & "&ownerId=" & ownerId
        End If
        If state <> "" Then
            parameters = parameters & "&state=" & state
        End If
        If LogMessage <> "" Then
            parameters = parameters & "&logMessage=" & LogMessage
        End If
        ' list datastreams
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "?" & parameters)
        MyRequest.Method = "PUT"
        MyRequest.Credentials = New NetworkCredential(fedoraUsername, fedoraPassword)
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        If src.Contains("Error") Then
            Call LogInformation(Now() & " ERROR - Modifying object " & PID)
            Return False
        Else
            Call LogInformation(Now() & " INFO - Modified object  " & PID)
            Return True
        End If
    End Function
    Public Function purgeObject(PID As String) As Boolean
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID)
        MyRequest.Method = "DELETE"
        MyRequest.Credentials = New NetworkCredential(fedoraUsername, fedoraPassword)
        On Error GoTo NoObjectinFedora
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Call LogInformation(Now() & " INFO - Purge Object  " & PID)
        Return True
        Exit Function
NoObjectinFedora:
        Call LogInformation(Now() & " ERROR - Purging Object  " & PID & " -")
        Return False
    End Function
    Public Function modifyDatastream(PID As String, dsID As String, dslabel As String, mimeType As String, LogMessage As String, Optional data As String = "", Optional dsLocation As String = "") As Boolean
        Dim src As String
        If data <> "" Then
            Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?mimeType=" & mimeType & "&logMessage=" & LogMessage)
            MyRequest.Method = "PUT"
            MyRequest.SendChunked = True
            MyRequest.ServicePoint.Expect100Continue = False
            MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
            On Error GoTo NoModifyDS
            Dim databytes As Byte() = System.Text.Encoding.UTF8.GetBytes(data)
            MyRequest.ContentLength = databytes.Length
            MyRequest.ContentType = "text/xml"
            Dim datastream = MyRequest.GetRequestStream()
            datastream.Write(databytes, 0, databytes.Length)
            datastream.Close()
            Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
            Dim reader As New StreamReader(MyResponse.GetResponseStream())
            src = reader.ReadToEnd()
        Else
            Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?mimeType=" & mimeType & "&dsLocation=" & dsLocation & "&logMessage=" & LogMessage)
            MyRequest.Method = "PUT"
            MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
            On Error GoTo NoModifyDS
            Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
            Dim reader As New StreamReader(MyResponse.GetResponseStream())
            src = reader.ReadToEnd()
        End If
        If src.Contains("Error") Then
            Call LogInformation(Now() & " ERROR - Modifying datastream  " & PID & "- dsID: " & dsID & " - " & src)
            Return False
        Else
            Call LogInformation(Now() & " INFO - Modified datastream  " & PID & "- dsID: " & dsID)
            Return True
        End If
        Exit Function
NoModifyDS:
        Call LogInformation(Now() & " ERROR - Modifying datastream  " & PID & "- dsID: " & dsID & " - ")
        Return False
    End Function
    Public Function addDatastream(PID As String, dsID As String, controlGroup As String, dslabel As String, mimeType As String, LogMessage As String, Optional data As String = "", Optional dsLocation As String = "", Optional internal As Boolean = True) As Boolean
        Dim doc As New XmlDocument
        Dim src As String
        If data <> "" Then
            Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?controlGroup=" & controlGroup & "&mimeType=" & mimeType & "&logMessage=" & LogMessage)
            MyRequest.Method = "POST"
            MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
            On Error GoTo NoAddDS
            MyRequest.ContentLength = Len(data)
            Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
            Dim reader As New StreamReader(MyResponse.GetResponseStream())
            src = reader.ReadToEnd()
        ElseIf internal Then
            Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?controlGroup=" & controlGroup & "&mimeType=" & mimeType & "&dsLocation=file:///" & dsLocation & "&logMessage=" & LogMessage)
            MyRequest.Method = "POST"
            MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
            On Error GoTo NoAddDS
            Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
            Dim reader As New StreamReader(MyResponse.GetResponseStream())
            src = reader.ReadToEnd()
        Else
            Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?controlGroup=" & controlGroup & "&mimeType=" & mimeType & "&dsLocation=" & dsLocation & "&logMessage=" & LogMessage)
            MyRequest.Method = "POST"
            MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
            On Error GoTo NoAddDS
            Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
            Dim reader As New StreamReader(MyResponse.GetResponseStream())
            src = reader.ReadToEnd()
        End If

        If src.Contains("Error") Then
            Call LogInformation(Now() & " ERROR - Adding datastream  " & PID & "- dsID: " & dsID)
            Return False
        Else
            Call LogInformation(Now() & " INFO - Added datastream  " & PID & "- dsID: " & dsID)
            Return True
        End If
NoAddDS:
        Call LogInformation(Now() & " ERROR - Adding datastream  " & PID & "- dsID: " & dsID)
        Return False
    End Function
    Public Function addRelationship(PID As String, subject As String, predicate As String, objecto As String, isLiteral As String, Optional datatype As String = "") As Boolean
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/relationships/new?subject=" & System.Web.HttpUtility.UrlEncode(subject) & "&predicate=" & System.Web.HttpUtility.UrlEncode(predicate) & "&object=" & System.Web.HttpUtility.UrlEncode(objecto) & "&isLiteral=" & isLiteral)
        MyRequest.Method = "POST"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoAddRel
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src = reader.ReadToEnd()

        If src.Contains("Error") Then
            Call LogInformation(Now() & " ERROR - Adding relationship  " & PID & "- subject: " & subject & "- predicate: " & predicate)
            Return False
        Else
            Call LogInformation(Now() & " INFO - Added relationship  " & PID & "- subject: " & subject & "- predicate: " & predicate)
            Return True
        End If
NoAddRel:
        Call LogInformation(Now() & " ERROR - Adding relationship  " & PID & "- subject: " & subject & "- predicate: " & predicate)
        Return False
    End Function
    Public Function purgeDatastream(PID As String, dsID As String, LogMessage As String) As Boolean

        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?logMessage=" & LogMessage)
        MyRequest.Method = "DELETE"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoPurgeDs
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()

        If src = "[]" Then
            Call LogInformation(Now() & " ERROR - Purging datastream  " & PID & "- dsID: " & dsID)
            Return False
        Else
            Call LogInformation(Now() & " ERROR - Purged datastream  " & PID & "- dsID: " & dsID)
            Return True
        End If
NoPurgeDs:
        Call LogInformation(Now() & " ERROR - Purging datastream  " & PID & "- dsID: " & dsID)
        Return False
    End Function
    Public Function getDatastream(PID As String, dsID As String, format As format) As XmlDocument
        Dim Doc As New XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "?format=" & format)
        MyRequest.Method = "GET"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoGetDs
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Doc.LoadXml(src)
        Return Doc
NoGetDs:
        Return Nothing
    End Function
    Public Function getDatastreamXML(PID As String, dsID As String) As XmlDocument
        Dim Doc As New XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/datastreams/" & dsID & "/content")
        MyRequest.Method = "GET"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoGetDsXML
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Doc.LoadXml(src)
        Return Doc
NoGetDsXML:
        Return Nothing
    End Function
    Public Function getNextPID(numPIDs As Integer, fedora_namespace As String) As ArrayList
        Dim PIDs As New ArrayList
        Dim Doc As New XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & "nextPID?numPIDs=" & numPIDs & "&namespace=" & fedora_namespace & "&format=XML")
        MyRequest.Method = "POST"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoGetNextPID
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Doc.LoadXml(src)
        Dim nodelist As XmlNodeList
        Dim node As XmlNode
        nodelist = Doc.SelectNodes("//pidList")

        For Each node In nodelist
            PIDs.Add(node.InnerText)
        Next

        Return PIDs
NoGetNextPID:
        Return Nothing
    End Function
    Public Function getObjectXML(PID As String) As XmlDocument
        Dim Doc As New XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/objectXML")
        MyRequest.Method = "GET"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoGetObjXML
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Doc.LoadXml(src)
        Return Doc
NoGetObjXML:
        Return Nothing
    End Function
    Public Function getRelationships(PID As String, subject As String, predicate As String, format As format) As XmlDocument
        Dim Doc As New XmlDocument
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/relationships?subject=" & System.Web.HttpUtility.UrlEncode(subject) & "&predicate=" & System.Web.HttpUtility.UrlEncode(predicate) & "&format=xml")
        MyRequest.Method = "GET"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoRels
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        Doc.LoadXml(src)
        Return Doc
NoRels:
        Return Nothing
    End Function
    Public Function purgeRelationship(PID As String, subject As String, predicate As String, objecto As String, isLiteral As String, Optional datatype As String = "") As Boolean
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(REST_API_URL & PID & "/relationships?subject=" & System.Web.HttpUtility.UrlEncode(subject) & "&predicate=" & System.Web.HttpUtility.UrlEncode(predicate) & "&object=" & objecto & "&isLiteral=" & isLiteral)
        MyRequest.Method = "DELETE"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        On Error GoTo NoPurgeRels
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        If src.Contains("Error") Then
            Return False
        Else
            Return True
        End If
NoPurgeRels:
        Return False
    End Function
    Sub LogInformation(LogMessage As String)
        Dim sw As StreamWriter
        Dim fs As FileStream = Nothing
        If (Not File.Exists(LogFile)) Then
            Try
                fs = File.Create(LogFile)
                sw = File.AppendText(LogFile)
                sw.WriteLine("Start Error Log for today")

            Catch ex As Exception
                MsgBox("Error Creating Log File")
            End Try

        Else
            sw = File.AppendText(LogFile)
            sw.WriteLine(LogMessage)

            sw.Close()
        End If
    End Sub
#End Region
End Module
