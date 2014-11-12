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
    Public RISearch_URL As String = "http://localhost:8080//fedora/risearch"

    Public fedorausername As String = "myfedorausername"
    Public fedorapassword As String = "myfedorapassword"

    'LogFile 
    Public LogFile As String = "\IngestLog.txt"
    
End Module