' .NET Class for interacting with Fedora´s datastreams
' Like with the objects it is possible to create new datastreams or load existing ones with all their properties.  
'
Imports System.Xml
Imports System.Xml.XPath

Public Class Datastream
#Region "Variables"
    Private _PID_DO As String ' to do: how to inherit this property from the object and how to use it in the methods
    Private _DSID As String
    Private _Label As String
    Private _ControlGroup As String
    Private _Size As Integer
    Private _CreatedDate As String
    Private _Location As String
    Private _MimeType As String
    Private _State As String
    Private _FormatURI As String
    Private _URL As String
#End Region

#Region "Properties"
    Public Property PID_DO As String
        Get
            Return _PID_DO
        End Get
        Set(value As String)
            _PID_DO = value
        End Set
    End Property
    Public Property DSID As String
        Get
            Return _DSID
        End Get
        Set(value As String)
            _DSID = value
        End Set
    End Property
    Public Property Label As String
        Get
            Return _Label
        End Get
        Set(value As String)
            _Label = value
        End Set
    End Property
    Public Property ControlGroup As String
        Get
            Return _ControlGroup
        End Get
        Set(value As String)
            _ControlGroup = value
        End Set
    End Property
    Public Property MimeType As String
        Get
            Return _MimeType
        End Get
        Set(value As String)
            _MimeType = value
        End Set
    End Property
    Public Property State As String
        Get
            Return _State
        End Get
        Set(value As String)
            _State = value
        End Set
    End Property
    Public Property FormatURI As String
        Get
            Return _FormatURI
        End Get
        Set(value As String)
            _FormatURI = value
        End Set
    End Property
    Public Property URL As String
        Get
            Return _URL
        End Get
        Set(value As String)
            _URL = value
        End Set
    End Property

#End Region

#Region "Public Methods"
    ''' <summary>
    ''' Instanciate a new datastream
    ''' </summary>
    ''' <param name="DSID">Datastream ID</param>
    ''' <param name="Label">Datastream Label</param>
    ''' <param name="ControlGroup">Control Group (M,X,I)</param>
    ''' <param name="MimeType">Mimetype</param>
    ''' <param name="URLLocation">Either a URL or the path of the file</param>
    ''' <param name="Internal">in case the datastream is inside the Islandora machine</param>
    ''' <param name="data">data to be ingested as a string, works well for xml</param>
    ''' <remarks></remarks>
    Friend Sub New(PID_DO As String, DSID As String, Label As String, ControlGroup As String, MimeType As String, LogMessage As String, Optional URLLocation As String = "", Optional Internal As Boolean = False, Optional data As String = "")
        _PID_DO = PID_DO
        _URL = REST_API_URL & _PID_DO & "/datastreams/" & DSID & "/content"
        If URLLocation <> "" Then
            addDatastream(PID_DO, DSID, ControlGroup, Label, MimeType, LogMessage, , URLLocation, Internal)
        Else
            addDatastream(PID_DO, DSID, ControlGroup, Label, MimeType, LogMessage, data, , )
        End If
        LoadDatastream(DSID)
    End Sub
    ''' <summary>
    ''' Instanciate an already existing datastream of an object
    ''' </summary>
    ''' <param name="DSID">Datastream ID</param>
    ''' <remarks></remarks>
    Friend Sub New(PID_DO As String, DSID As String)
        _PID_DO = PID_DO
        _DSID = DSID
        _URL = REST_API_URL & _PID_DO & "/datastreams/" & DSID & "/content"
        LoadDatastream(DSID)
    End Sub

#End Region

#Region "Private Methods"

    Private Sub LoadDatastream(DSID As String)
        'Load datastream properties
        Dim nodelist As XmlNodeList
        Dim node As XmlNode
        Dim datastream As New XmlDocument
        datastream = getDatastream(PID_DO, DSID, REST_API.format.XML)

        nodelist = datastream.GetElementsByTagName("datastreamProfile")
        For Each node In nodelist
            _Label = node.ChildNodes.Item(0).InnerXml
            _CreatedDate = node.ChildNodes.Item(2).InnerXml
            _State = node.ChildNodes.Item(3).InnerXml
            _MimeType = node.ChildNodes.Item(4).InnerXml
            _FormatURI = node.ChildNodes.Item(5).InnerXml
            _ControlGroup = node.ChildNodes.Item(6).InnerXml
            _Size = node.ChildNodes.Item(7).InnerXml
            _Location = node.ChildNodes.Item(10).InnerXml
        Next

    End Sub

#End Region

End Class
