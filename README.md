# Islandora FJM Library
This is a .NET library developed at [Fundación Juan March](http://www.march.es) (Madrid,Spain) to interact with Islandora´s core elements such as Fedora, Solr and RISearch.
Initially only a few modules and classes are included in this library but we planned to release more. The modules and classes included in this release are:

* [**ModuleVariables**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleVariables.vb) - here we need to define all variables such as paths, default namespaces, fedora username&password, etc
* [**ModuleRESTAPI**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ModuleRESTAPI.vb) - module that wraps the [Fedora´s REST API] (https://wiki.duraspace.org/display/FEDORA34/REST+API) calls to .NET functions.
* [**ClassDigitalObject**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ClassDigitalObject.vb) - this class abstracts Fedora´s REST API methods into a class with objects allowing the creation of new objects or loading existing ones with all their properties. And making possible to check and change any information as well as adding or remove datastreams and relationships.
* [**ClassDatastream**](https://github.com/luismart/IslandoraFJM/blob/master/IslandoraFJM/IslandoraFJM/ClassDatastream.vb) - this class allows interacting with Fedora?s datastreams much like with the objects classs. It makes possible to create new datastreams or load existing ones with all their properties.  

In further releases we will include other modules and classes to interact with Solr and RISearch, to use powertools such as ImageMagick,PDF2JSON, SWFTools or features from the AdobePDF library and classes to create and edit MODS or EAC-CPF records.

So far this library has been used for digital collections such as [All our art catalogues since 1973](http://www.march.es/arte/catalogos/?l=2), [Sim Sala Bim, library of illusionism](http://www.march.es/bibliotecas/ilusionismo/biblioteca-digital-de-ilusionismo.aspx?l=2) and [Fernandez-Shaw and the musical theatre](http://www.march.es/bibliotecas/repositorio-fernandez-shaw/?l=2).


## Some documentation

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
obj.add_datastream("IMG", "M", "IMAGE", "image/jpg", "AAAA", "http://digital.march.es:8080/fedora/objects/cat:1/datastreams/JPG_PORTADA/content", False)
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