# Islandora FJM .NET Library
This is a .NET library developed at [Fundación Juan March](http://www.march.es) (Madrid,Spain) to interact with Islandora´s core elements such as Fedora, Solr and RISearch. At this stage only a few modules and classes are included in this library but we plan to release more. The modules and classes included in this release are:

* [**ModuleVariables**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleVariables.vb) - here we need to define all variables such as paths, default namespaces, fedora username&password, etc as well as paths to the applications for image and pdf manipulation such as ImageMagick
* [**ModuleRESTAPI**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleRESTAPI.vb) - module that wraps the [Fedora´s REST API] (https://wiki.duraspace.org/display/FEDORA34/REST+API) calls to .NET functions.
* [**ClassDigitalObject**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ClassDigitalObject.vb) - this class abstracts Fedora´s REST API methods into a class with objects allowing the creation of new objects or loading existing ones with all their properties. And making possible to check and change any information as well as adding or remove datastreams and relationships.
* [**ClassDatastream**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ClassDatastream.vb) - this class allows interacting with Fedora?s datastreams much like with the objects classs. It makes possible to create new datastreams or load existing ones with all their properties.  
* [**ModuleSolr**] (https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleSolr.vb) - this module supports the use of Solr with functions to check if Solr is up, building Solr URL queries and datasets with the results as well as the facets
* [**ModulePowerTools**] (https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModulePowerTools.vb) - this module incoorporates functions to work with images (get height and width, resize) and PDFs (paginate, get number of pages, convert to jpg or swf, etc). ImageMagick (http://www.imagemagick.org/script/index.php), swftools(http://www.swftools.org/), pdf2json (https://code.google.com/p/pdf2json/) need to be installed and their paths added to ModuleVariables.vb 

In further releases we will include other modules and classes to interact with RISearch and classes to create and edit MODS or EAC-CPF records.

So far this library has been used for digital collections such as [All our art catalogues since 1973](http://www.march.es/arte/catalogos/?l=2), [Sim Sala Bim, library of illusionism](http://www.march.es/bibliotecas/ilusionismo/biblioteca-digital-de-ilusionismo.aspx?l=2) and [Fernandez-Shaw and the musical theatre](http://www.march.es/bibliotecas/repositorio-fernandez-shaw/?l=2).


## Some documentation
### Working with Fedora objects and datastreams

Once configured the right variables in the file  [*IslandoraFJM/ModuleVariables.vb*](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleVariables.vb), we can go ahead and create a new digital object

```vbnet 
Dim obj As New DigitalObject("testobject:1", "this is a new object", True)
```

and modify the label of the object with

```vbnet 
obj.ModifyObjectLabel("testobject:1", "this is the new label", "log message to change label")
```

or add an image as a new datastream

```vbnet 
obj.add_datastream("IMG", "M", "IMAGE", "image/jpg", "adding an image datastream", "http://digital.march.es:8080/fedora/objects/cat:1/datastreams/JPG_PORTADA/content", False)
```

and add some metadata as a new datastream

```vbnet 
obj.add_datastream("MODS", "M", "MODS", "text/xml", "Adding metadata", "http://digital.march.es:8080/fedora/objects/cat:1/datastreams/MODS/content", False)
```

We can remove the metadata datastream

```vbnet 
obj.remove_datastream("MODS")
```

Or add relationships to the RELS-EXT

```vbnet 
obj.add_relationship("testobject:1", "info:fedora/fedora-system:def/relations-external#/isMemberofCollection", "info:fedora/collection1:CollectionModel", False)
obj.add_relationship("testobject:1", "info:fedora/fedora-system:def/model#/hasModel", "info:fedora/collection1:ContentModel", True)
```

We can remove the relationship

```vbnet 
obj.remove_relationship("testobject:1", "info:fedora/fedora-system:def/model#/hasModel", "info:fedora/collection1:ContentModel", True)
```

' Or add a relationship to the RELS-IN

```vbnet 
obj.add_relationship("testobject:1" & "/IMG", "info:islandora/islandora-system:def/pageinfo#/width", "500px", True)
```

'Or load an existing object to perform the previous actions
```vbnet 
Dim obj2 As New DigitalObject("islandora:root")
```

### Working with Solr
This module allows to run queries against your Solr index and obtain the results in datasets
Check if Solr is running 
```vbnet 
Dim IsSolrUp as Boolean = Solr_Status_OK
```

Build the URL for a simple Solr Query
```vbnet 
Dim Solr_URL, Query as String 
Solr_URL="http://localhost:8080/solr/collection1/select?"
Query="mods_title:Quijote"

Dim SolrURLQuery as String = Generate_Solr_Search_URL (Solr_URL, Query)
```

More complex queries are supported with filter queries, sorting and specified fields to return, pagination and facets
```vbnet 
Dim Solr_URL, Query, FilterQuery,Sort,FieldsToReturn as String 
Solr_URL="http://localhost:8080/solr/collection1/select?"
Query="mods_title:Quijote"
FilterQuery="mods_genre:book"
Sort="mods_dateCreated asc"
FieldsToReturn="mods_title,mods_dateCreated,mods_genre"

Dim SolrURLQuery as String = Generate_Solr_Search_URL (Solr_URL, Query, FilterQuery, Sort, FieldsToReturn)
```

Once we have constructed the Solr Query we can run the query and get the results in a datatable
```vbnet 
Dim results as New DataSet
Dim drupal_URL as String ="http://myIslandorasite.com" ' to build URLs to access the datastreams
Dim Fields, Datastreams as Dictionary(Of String, String)

Fields.Add("PID_admin", "PID")
Fields.Add("Title", "mods_title")
Fields.Add("Date", "mods_dateCreated")

Datastreams.Add("High Res Image","JPG")
Datastreams.Add("Thumbnail","TN")

results = ReturnDatasetFromSearch(Solr_Query_URL, Drupal_URL, Fields, Datastreams)
```

### Working with PowerTools
This module contains useful functions to work with images and pdfs.

With images it is possible get their height and width or rescale them
```vbnet 
Dim img_path, img_name as String
img_path ="C:/Images/"
img_name ="Image1.jpg"

height = getImgHeight(img_path & img_name)
width = getImgWidth(img_path & img_name)

Dim scale as Integer
scale=250

rescaleJPG(img_name,img_path, scale)
```

With PDFs you can count the number of pages, OCR them or split them into pages
```vbnet
Dim myfile As String = "myPDF.pdf"
Dim pathIn As String = "C:\Temp\"
Dim pathOut As String = "C:\Temp\"

' convert pdf to images
pdf2JPG(myfile, pathIn, pathOut)

'convert pdf to swf file
pdf2swf(myfile, pathIn, pathOut)

'ocr a pdf to json or text files
Dim firstpage As Integer = 1
Dim lastpage As Integer = 10
pdf2json(myfile, pathIn, pathOut, firstpage, lastpage)
pdf2txt(myfile, pathIn, pathOut)

' calculate number of pages
PDFnum_pages(pathIn & myfile)

' split a pdf into one page pdfs
SplitPDF(file, pathIn, pathOut)

' one function to do all the other processes at once
Dim splitFiles, createJPGs, createTNs, createJSONs, createSWFs As Boolean
splitFiles= True 
createJPGs= True 
createTNs= True
createJSONs= True 
createSWFs= True 

PDF2BookFiles(myfile, pathIn, splitFiles, createJPGs, createTNs, createJSONs, createSWFs)

```
