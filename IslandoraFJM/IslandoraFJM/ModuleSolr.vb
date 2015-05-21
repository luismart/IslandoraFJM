' .NET Class for accesing Solr REST API
'  New functions support the work of the web programmers by helping with the generation of Solr Queries URLs,
'  checking if Solr is up and creating a dataset from a Solr Query.

Imports System.Net
Imports System.IO
Imports System.Xml
Imports System.Web.HttpServerUtility
Imports System.Collections.Specialized

Public Module Solr

#Region "Public Methods"
    ''' <summary>
    ''' Return true if Solr is up and running
    ''' </summary>
    Public Function Solr_Status_OK() As Boolean
        Dim resultadosSolrStatus As New XmlDocument
        Try
            resultadosSolrStatus.Load("http://localhost:8080/solr/admin/cores?action=STATUS")
            Dim noderesultados As XmlNodeList
            noderesultados = resultadosSolrStatus.SelectNodes("//response/lst[@name='status']/lst")
            If noderesultados.Count <> 1 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    '''  Builds the URL for a Solr Search
    ''' </summary>
    ''' <param name="Solr_URL">URL of the Solr core, there are variables with this values assigned already like Ilusionismo_Solr_URL</param>
    ''' <param name="Query">The query parameter</param>
    ''' <param name="Filter_Query">This parameter can be used to specify a query that can be used to restrict the super set of documents from the query</param>
    ''' <param name="Sort">Sorting like "PID asc"</param>
    ''' <param name="FieldsToReturn">This parameter can be used to specify a set of fields to return, limiting the amount of information in the response.</param>
    ''' <param name="Pagination_Rows">This parameter is used to paginate results from a query. It specify the maximum number of documents </param>
    ''' <param name="Pagination_Start">This parameter is used to paginate results from a query.</param>
    ''' <param name="RequestHandler">Request Handler, use "mlt" for recommendation system and leave empty for normal</param>
    ''' <param name="Facets">Boolean to create or not facets</param>
    ''' <param name="Facet_Fields">Array with the fields names to show on the facet</param>
    ''' <param name="wt">Format of the result (csv, json, xml)</param>
    Public Function Generate_Solr_Search_URL(Solr_URL As String, Query As String, Optional Filter_Query As String = "", Optional Sort As String = "", Optional FieldsToReturn As String = "", Optional Pagination_Rows As Integer = 0, Optional Pagination_start As Integer = 0, Optional RequestHandler As String = "standard", Optional Facets As Boolean = False, Optional Facet_Fields() As String = Nothing, Optional Facet_Limit As Integer = 100, Optional wt As String = "standard") As String
        Dim Solr_Search As String
        Solr_Search = Solr_URL & "q=" & System.Web.HttpUtility.UrlEncode(Query)
        If Filter_Query <> "" Then
            Solr_Search = Solr_Search & "&fq=" & System.Web.HttpUtility.UrlEncode(Filter_Query)
        End If
        If Sort <> "" Then
            Solr_Search = Solr_Search & "&sort=" & Sort
        End If
        If FieldsToReturn <> "" Then
            Solr_Search = Solr_Search & "&fl=" & FieldsToReturn
        End If
        If Pagination_Rows >= 0 Then
            Solr_Search = Solr_Search & "&rows=" & Pagination_Rows
        End If
        If Pagination_start > 0 Then
            Solr_Search = Solr_Search & "&start=" & Pagination_start
        End If
        Solr_Search = Solr_Search & "&qt=" & RequestHandler

        Solr_Search = Solr_Search & "&wt=" & wt

        If Facets Then
            Solr_Search = Solr_Search & "&facet=on&facet.mincount=1&facet.limit=" & Facet_Limit
            For i = 0 To Facet_Fields.Count - 1
                Solr_Search = Solr_Search & "&facet.field=" & Facet_Fields(i)
            Next
        End If
        Return Solr_Search
    End Function


    ''' <summary>
    ''' This function returns a dataset with a datatable with information of the value of the solr fields
    ''' and the URLs to the datastreams
    ''' Detects the XML format of the result to select the correct "str" nodes depending on single-value or multiple-value results "arr"
    ''' </summary>
    ''' <param name="Solr_Query_URL">Full URL to query solr</param>
    ''' <param name="Drupal_URL"> URL of the Drupal site to access the content</param>
    ''' <param name="Fields">Dictionary of the type fields.add("name","mods_name").</param>
    ''' <param name="Datastreams">Dictionary of the type datastreams.add("Imagen_grande","JPG_700")</param>
    ''' <returns>A dataset with table tablaResults with the query results with values of the fields specified
    ''' in the Fields and the URL to the datastreams specified in Datastreams</returns>
    Public Function ReturnDatasetFromSearch(Solr_Query_URL As String, Drupal_URL As String, Fields As Dictionary(Of String, String), Datastreams As Dictionary(Of String, String)) As DataSet
        Dim datasetResults As New DataSet
        Dim tablaResults As New DataTable
        Dim totales As New DataTable
        Dim numeroResultados As New Integer

        Fields.Add("PID_admin", "PID") 'we add this field to the dictionary to ensure we always have the PID to generate the URLs of the datastreams

        For i = 0 To Fields.Count - 1
            tablaResults.Columns.Add(Fields.Keys(i), GetType(String))
        Next

        For i = 0 To Datastreams.Count - 1
            tablaResults.Columns.Add(Datastreams.Keys(i), GetType(String))
        Next

        totales.Columns.Add("totalRegistros", GetType(Integer))
        totales.Columns.Add("fecha", GetType(DateTime))

        Dim resultadosBusquedaXML As New XmlDocument
        resultadosBusquedaXML.Load(Solr_Query_URL)
        Dim noderesultados As XmlNodeList
        Dim nodefield As XmlNode

        numeroResultados = resultadosBusquedaXML.SelectSingleNode("//response/result").Attributes("numFound").Value

        totales.Rows.Add({numeroResultados, Now})

        noderesultados = resultadosBusquedaXML.SelectNodes("//response/result/doc")

        Dim results(Fields.Count + Datastreams.Count - 1) As Object  ' an array of results from parsing the solr docs
        For Each node As XmlNode In noderesultados

            For i = 0 To Fields.Count - 1
                On Error Resume Next
                nodefield = node.SelectSingleNode("str[@name='" & Fields.Values(i) & "']")
                If nodefield IsNot Nothing AndAlso nodefield.InnerXml IsNot Nothing Then
                    results(i) = nodefield.InnerText
                Else
                    nodefield = node.SelectSingleNode("arr[@name='" & Fields.Values(i) & "']")
                    If nodefield.InnerXml Is Nothing Then
                        results(i) = ""
                    Else
                        Dim temp As String = ""
                        For Each y As XmlNode In nodefield.ChildNodes
                            temp &= y.InnerText & ", "
                        Next
                        results(i) = Left(temp, temp.Length - 2)
                    End If
                End If
            Next

            For j = Fields.Count To Fields.Count + Datastreams.Count - 1
                results(j) = Drupal_URL & results(Fields.Count - 1) & "/" & Datastreams.Values(j - Fields.Count)
            Next
            tablaResults.Rows.Add(results)
        Next

        datasetResults.Tables.Add(tablaResults)
        datasetResults.Tables.Add(totales)

        Return datasetResults

    End Function

    ''' <summary>
    ''' This function returns a dataset with a datatable with information of the value of the solr fields
    ''' and the URLs to the datastreams with facets datatable
    ''' Detects the XML format of the result to select the correct "str" nodes depending on single-value or multiple-value results "arr"
    ''' </summary>
    ''' <param name="Solr_Query_URL">Full URL to query solr</param>
    ''' <param name="Drupal_URL"> URL of the Drupal site to access the content</param>
    ''' <param name="Fields">Dictionary of the type fields.add("name","mods_name").</param>
    ''' <param name="Datastreams">Dictionary of the type datastreams.add("Imagen_grande","JPG_700")</param>
    ''' <param name="FacetFields">Dictionary of the type fields.add("name","mods_name").</param>
    ''' <returns>A dataset with table tablaResults with the query results with values of the fields specified
    ''' in the Fields and the URL to the datastreams specified in Datastreams</returns>
    Public Function ReturnDatasetFromFacetSearch(Solr_Query_URL As String, Drupal_URL As String, Fields As Dictionary(Of String, String), Datastreams As Dictionary(Of String, String), FacetFields As Dictionary(Of String, String)) As DataSet
        Dim datasetResults As New DataSet
        Dim tablaResults As New DataTable
        Dim tablaFacets As New DataTable
        Dim totales As New DataTable
        Dim numeroResultados As New Integer

        Fields.Add("PID_admin", "PID") 'we add this field to the dictionary to ensure we always have the PID to generate the URLs of the datastreams

        For i = 0 To Fields.Count - 1
            tablaResults.Columns.Add(Fields.Keys(i), GetType(String))
        Next

        For i = 0 To Datastreams.Count - 1
            tablaResults.Columns.Add(Datastreams.Keys(i), GetType(String))
        Next

        totales.Columns.Add("totalRegistros", GetType(Integer))
        totales.Columns.Add("fecha", GetType(DateTime))

        Dim resultadosBusquedaXML As New XmlDocument
        resultadosBusquedaXML.Load(Solr_Query_URL)
        Dim noderesultados As XmlNodeList
        Dim nodeResultadosFacetado As XmlNodeList
        Dim nodefield As XmlNode
        Dim resultsFacets(2) As Object  ' an array of results from parsing the solr docs


        tablaFacets.Columns.Add("facet", GetType(String))
        tablaFacets.Columns.Add("valor", GetType(String))
        tablaFacets.Columns.Add("ocurrencias", GetType(String))

        numeroResultados = resultadosBusquedaXML.SelectSingleNode("//response/result").Attributes("numFound").Value
        totales.Rows.Add({numeroResultados, Now})

        nodeResultadosFacetado = resultadosBusquedaXML.SelectNodes("//response/lst/lst/lst/int")

        For Each node As XmlNode In nodeResultadosFacetado

            resultsFacets(0) = node.ParentNode.Attributes("name").Value
            resultsFacets(1) = node.Attributes(0).Value
            resultsFacets(2) = node.InnerText

            tablaFacets.Rows.Add(resultsFacets)
        Next

        noderesultados = resultadosBusquedaXML.SelectNodes("//response/result/doc")

        Dim results(Fields.Count + Datastreams.Count - 1) As Object  ' an array of results from parsing the solr docs
        For Each node As XmlNode In noderesultados

            For i = 0 To Fields.Count - 1
                On Error Resume Next
                nodefield = node.SelectSingleNode("str[@name='" & Fields.Values(i) & "']")
                If nodefield IsNot Nothing AndAlso nodefield.InnerXml IsNot Nothing Then
                    results(i) = nodefield.InnerText
                Else
                    nodefield = node.SelectSingleNode("arr[@name='" & Fields.Values(i) & "']")
                    If nodefield.InnerXml Is Nothing Then
                        results(i) = ""
                    Else
                        Dim temp As String = ""
                        For Each y As XmlNode In nodefield.ChildNodes
                            temp &= y.InnerText & ", "
                        Next
                        results(i) = Left(temp, temp.Length - 2)
                    End If
                End If
            Next

            For j = Fields.Count To Fields.Count + Datastreams.Count - 1
                results(j) = Drupal_URL & results(Fields.Count - 1) & "/" & Datastreams.Values(j - Fields.Count)
            Next
            tablaResults.Rows.Add(results)
        Next
        datasetResults.Tables.Add(tablaResults)
        datasetResults.Tables.Add(tablaFacets)
        datasetResults.Tables.Add(totales)

        Return datasetResults
    End Function

    ''' <summary>
    ''' Indexes a doc in Solr
    ''' </summary>
    ''' <param name="PID">the id of the object to index in Solr</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SolrIndexDoc(PID As String) As Boolean
        Dim MyRequest As HttpWebRequest = HttpWebRequest.Create(GSearch_URL & PID)
        MyRequest.Method = "GET"
        MyRequest.Credentials = New NetworkCredential(fedorausername, fedorapassword)
        Dim MyResponse As HttpWebResponse = MyRequest.GetResponse()
        Dim reader As New StreamReader(MyResponse.GetResponseStream())
        Dim src As String = reader.ReadToEnd()
        If src.Contains("error") Then
            Call LogInformation(Now() & " INFO - Object  Indexed " & PID)
            Return True
        Else
            Call LogInformation(Now() & " ERROR - Indexing object  " & PID)
            Return False
        End If
    End Function
#End Region
End Module