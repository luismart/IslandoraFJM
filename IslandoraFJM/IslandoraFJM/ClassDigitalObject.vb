' .NET Class for interacting with Fedora´s objects abstracting the REST API.
'  It is possible to create new objects or load existing ones with all their properties. And then you can check and change those as well as
'  adding or remove datastreams and relationships.
'
Imports System.Xml
Imports System.Xml.XPath

Public Class DigitalObject

#Region "Variables"
    Private _PID As String
    Private _Owner As String
    Private _Label As String
    Private _State As String
    Private _CreatedDate As String
    Private _LastModifiedDate As String
    Private _ContentModel As String
    Private _ds_list As New List(Of Datastream)
    Private _foXML As XmlDocument
    Private _DC As XmlDocument
#End Region

#Region "Properties"
    Public Property PID As String
        Get
            Return _PID
        End Get
        Set(value As String)
            _PID = value
        End Set
    End Property
    Public Property Owner As String
        Get
            Return _Owner
        End Get
        Set(value As String)
            _Owner = value
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
    Public Property State As String
        Get
            Return _State
        End Get
        Set(value As String)
            _State = value
        End Set
    End Property
    Public ReadOnly Property ContentModel As String
        Get
            Return _ContentModel
        End Get
    End Property
    Public ReadOnly Property LastModifiedDate As String
        Get
            Return _LastModifiedDate
        End Get
    End Property
    Public ReadOnly Property CreatedDate As String
        Get
            Return _CreatedDate
        End Get
    End Property
    Public Property ds_list As List(Of Datastream)
        Get
            Return _ds_list
        End Get
        Set(value As List(Of Datastream))
            _ds_list = value
        End Set
    End Property
    Public ReadOnly Property foXML As XmlDocument
        Get
            Return _foXML
        End Get
    End Property
    Public ReadOnly Property DC As XmlDocument
        Get
            Return _DC
        End Get
    End Property
#End Region

#Region "Public Methods"
    ''' <summary>
    ''' Instanciate a new object forcing a PID or requesting the next PID in a Namespace
    ''' </summary>
    ''' <param name="PIDorNameSpace">PID. Compulsory</param>
    ''' <param name="Label">Label. Compulsory</param>
    ''' <param name="ForcePID">Label. Compulsory</param>
    ''' <remarks></remarks>
    Public Sub New(PIDorNameSpace As String, Label As String, ForcePID As Boolean)
        If ForcePID Then
            Me._PID = PIDorNameSpace
        Else
            Dim PIDList As ArrayList
            PIDList = getNextPID(1, PIDorNameSpace)
            Me._PID = PIDList(0)
        End If
        If ingest(_PID, Label) Then
            LoadObject(_PID)
        End If
    End Sub
    ''' <summary>
    ''' Instanciate an already existing object
    ''' </summary>
    ''' <param name="PID">PID, with its NameSpace (ex: cat:123)</param>
    ''' <remarks></remarks>
    Public Sub New(PID As String)
        Me._PID = PID
        LoadObject(PID)
    End Sub
    ''' <summary>
    ''' Sub to add a new datastream
    ''' </summary>
    ''' <remarks>TO DO: fix boolean returned</remarks>
    Public Function add_datastream(dsid As String, controlGroup As String, dslabel As String, mimeType As String, LogMessage As String, Optional dsLocation As String = "", Optional internal As Boolean = True) As Boolean
        Dim ds As New Datastream(PID, dsid, dslabel, controlGroup, mimeType, LogMessage, dsLocation, internal)
        Me.ds_list(ds_list.Count - 1).DSID = dsid
        Me.ds_list.Add(ds)
        Return True
    End Function
    ''' <summary>
    ''' Sub to remove datastream
    ''' </summary>
    ''' <param name="dsid"></param>
    ''' <returns>boolean</returns>
    ''' <remarks>TO DO: fix boolean returned</remarks>
    Public Function remove_datastream(dsid As String) As Boolean
        Me.ds_list.Remove(ds_list.Find(Function(c) c.DSID = dsid))
        Return purgeDatastream(PID, dsid, "deleting datastream")
    End Function
    ''' <summary>
    ''' Sub to modify datastream
    ''' </summary>
    ''' <param name="dsID">dsid</param>
    ''' <param name="dslabel">label</param>
    ''' <param name="mimeType">mime type</param>
    ''' <param name="LogMessage">log message</param>
    ''' <param name="dsLocation">location as url</param>
    ''' <remarks>TO DO: fix boolean returned</remarks>
    Public Function modify_datastream(dsID As String, dslabel As String, mimeType As String, LogMessage As String, Optional data As String = "", Optional dsLocation As String = "") As Boolean
        Return modifyDatastream(PID, dsID, dslabel, mimeType, LogMessage, data, dsLocation)
    End Function
    ''' <summary>
    ''' Function to add a relationship
    ''' </summary>
    ''' <param name="subjectPID">digital object that is subject</param>
    ''' <param name="predicate">predicate of the relationship</param>
    ''' <param name="objecto">the value of the relationship</param>
    ''' <param name="isLiteral">True if element value, False if attribute value</param>
    ''' <param name="datatype"></param>
    ''' <returns>Boolean</returns>
    ''' <remarks>TO DO: fix boolean returned</remarks>
    Public Function add_relationship(subjectPID As String, predicate As String, objecto As String, isLiteral As String, Optional datatype As String = "") As Boolean
        addRelationship(PID, "info:fedora/" & subjectPID, predicate, objecto, isLiteral, )
        Return True
    End Function

    Public Function remove_relationship(subjectPID As String, predicate As String, objecto As String, isLiteral As String, Optional datatype As String = "") As Boolean
        purgeRelationship(PID, "info:fedora/" & subjectPID, predicate, objecto, isLiteral, )
        Return True
    End Function
    ''' <summary>
    ''' Function to get a particular relationship in an object
    ''' </summary>
    ''' <param name="subject">digital object that is subject</param>
    ''' <param name="predicate">predicate of the relationship</param>
    ''' <returns>XMLDocument</returns>
    ''' <remarks></remarks>
    Public Function get_relationship(subject As String, predicate As String) As XmlDocument
        Dim resultXML = getRelationships(PID, subject, predicate, format.XML)
        Return resultXML
    End Function
    ''' <summary>
    ''' Loads the DC property of the object as an XMLDocument
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Load_DC()
        Me._DC = getDatastreamXML(PID, "DC")
    End Sub
    ''' <summary>
    ''' Gets the datastream of an object
    ''' </summary>
    ''' <param name="dsid">dsid</param>
    ''' <returns>datastream object</returns>
    ''' <remarks></remarks>
    Public Function getDatastreamObject(dsid As String) As Datastream
        Dim ds As New Datastream(_PID, dsid)
        Return ds
    End Function
    ''' <summary>
    ''' Create a datatable from datastream info
    ''' </summary>
    Public Function get_dataTable_Datastreams_Info() As DataTable
        Dim ds_datatable As New DataTable
        ds_datatable.Columns.Add("DSID", GetType(String))
        ds_datatable.Columns.Add("Label", GetType(String))
        ds_datatable.Columns.Add("MimeType", GetType(String))

        For Each ds In Me.ds_list
            Dim label As String
            If ds.Label Is Nothing Then
                label = ""
            Else
                label = ds.Label
            End If
            ds_datatable.Rows.Add(ds.DSID, label, ds.MimeType)
        Next
        Return ds_datatable
    End Function
    Public Function ModifyObjectLabel(PID As String, label As String, logMessage As String) As Boolean
        Return modifyObject(PID, label, , , logMessage)
    End Function
    Public Function RemoveObject() As Boolean
        Return purgeObject(PID)
    End Function
    Public Function Exists(pid As String) As Boolean
        Dim xml = getObjectXML(pid)
        If Not xml.HasChildNodes Then
            Return False
        Else
            Return True
        End If
    End Function
#End Region

#Region "Private Methods"
    ''' <summary>
    ''' This private sub fills in all of the object´s profile information and its list of datastreams
    ''' </summary>
    ''' <param name="PID">PID</param>
    ''' <remarks></remarks>
    Private Sub LoadObject(PID As String)
        LoadObjectProperties(PID) 'Load objects properties
        LoadDatastreams(PID) 'Loads object´s datastreams properties
        LoadContentModel(PID) 'Load content model property only when there is a RELS-EXT
    End Sub

    Private Sub LoadObjectProperties(PID As String)
        Me._foXML = getObjectXML(PID)
        Dim nodelist As XmlNodeList
        Dim node As XmlNode
        Dim manager = New XmlNamespaceManager(Me._foXML.NameTable)
        manager.AddNamespace("foxml", "info:fedora/fedora-system:def/foxml#")

        nodelist = foXML.SelectNodes("//foxml:digitalObject/foxml:objectProperties", manager)

        For Each node In nodelist
            Me._State = node.ChildNodes.Item(0).Attributes.GetNamedItem("VALUE").Value
            Me._Label = node.ChildNodes.Item(1).Attributes.GetNamedItem("VALUE").Value
            Me._Owner = node.ChildNodes.Item(2).Attributes.GetNamedItem("VALUE").Value
            Me._CreatedDate = node.ChildNodes.Item(3).Attributes.GetNamedItem("VALUE").Value
            Me._LastModifiedDate = node.ChildNodes.Item(4).Attributes.GetNamedItem("VALUE").Value
        Next
    End Sub
    Private Sub LoadDatastreams(PID As String)
        Dim datastreams As New XmlDocument
        datastreams = listDatastreams(Me._PID, REST_API.format.XML)

        Dim element As XmlElement
        Dim node As XmlNode
        Dim DSID As New ArrayList
        node = datastreams.FirstChild.NextSibling
        For Each element In node
            DSID.Add(element.Attributes.GetNamedItem("dsid").Value)
        Next

        '...instanciate the datastreams and add them to me._dslist
        For i = 0 To DSID.Count - 1
            Dim ds As New Datastream(PID, DSID(i))
            Me._ds_list.Add(ds)
        Next
    End Sub
    Private Sub LoadContentModel(PID As String)
        Dim manager = New XmlNamespaceManager(Me._foXML.NameTable)
        Dim nodelist As XmlNodeList
        For Each ds In ds_list
            If ds.DSID = "RELS-EXT" Then
                Dim CModel As New XmlDocument
                CModel = getDatastreamXML(PID, "RELS-EXT")
                manager = New XmlNamespaceManager(CModel.NameTable)
                manager.AddNamespace("fedora-model", "info:fedora/fedora-system:def/model#")
                manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")

                nodelist = CModel.SelectNodes("//rdf:RDF/rdf:Description/fedora-model:hasModel", manager)
                For Each node In nodelist
                    On Error Resume Next
                    Me._ContentModel = node.Attributes.GetNamedItem("rdf:resource").Value
                Next
            End If
        Next
    End Sub

#End Region

End Class