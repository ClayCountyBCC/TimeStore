Imports System.Net
Imports System.Web.Http

Namespace Controllers
  Public Class PubWorksController
    Inherits ApiController

    ' GET: api/PubWorks
    Public Function GetValues() As IEnumerable(Of String)
      Return New String() {"value1", "value2"}
    End Function


  End Class
End Namespace
