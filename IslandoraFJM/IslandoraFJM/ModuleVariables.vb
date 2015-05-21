' Module to define all the variables like paths, namespaces, fedora username&pass, drupal sites and applications
Public Module Variables
    'Fedora REST APIs URIs
    Public REST_API_URL As String = "http://localhost:8080/fedora/objects/"

    'Fedora Namespaces
    Public namespace_colection1 As String = "collection1"

    'Solr cores
    Public Collection1_Solr_URL As String = "http://localhost:8080/solr/collection1/select?indent=on&version=2.2&"

    'Solr authorities namespaces
    Public Authorities_Namespace As String = "aut"

    'Drupal sites
    Public Collection1_Drupal_URL As String = "http://mywebsite.com/collection1/fedora/repository/"
    
    'RISearch
    Public RISearch_URL As String = "http://localhost:8080/fedora/risearch"

    'Gsearch
    Public GSearch_URL As String = "http://localhost:8080//fedoragsearch/rest?operation=updateIndex&action=fromPid&value="

    Public fedorausername As String = "myfedorausername"
    Public fedorapassword As String = "myfedorapassword"

    'LogFile 
    Public LogFile As String = "\IngestLog.txt"

    ' Applications 
    Public ImageMagick_Path As String = """C:\Program Files (x86)\ImageMagick-6.8.7-Q16\convert"""
    Public PDF2JSON_Path As String = """C:\Program Files (x86)\PDF2JSON\pdf2json.exe"""
    Public SWFTools_Path As String = """C:\Program Files (x86)\SWFTools\pdf2swf.exe"""
    Public CURLPath As String = """C:\Program Files (x86)\Curl\curl.exe"""
    
End Module