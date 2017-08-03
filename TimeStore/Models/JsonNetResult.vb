Imports Newtonsoft.Json
Imports System.Web.Mvc
Imports Newtonsoft.Json.Converters
Imports System.Dynamic
Imports System.Globalization


Public Class JsonNetValueProviderFactory
    Inherits ValueProviderFactory
    Public Overrides Function GetValueProvider(controllerContext As ControllerContext) As IValueProvider
        ' first make sure we have a valid context
        If controllerContext Is Nothing Then
            Throw New ArgumentNullException("controllerContext")
        End If

        ' now make sure we are dealing with a json request
        If Not controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) Then
            Return Nothing
        End If

        ' get a generic stream reader (get reader for the http stream)
        Dim streamReader As New StreamReader(controllerContext.HttpContext.Request.InputStream)
        ' convert stream reader to a JSON Text Reader
        Dim JSONReader As New JsonTextReader(streamReader)
        ' tell JSON to read
        If Not JSONReader.Read() Then
            Return Nothing
        End If

        ' make a new Json serializer
        Dim JSONSerializer As New JsonSerializer()
        ' add the dyamic object converter to our serializer
        JSONSerializer.Converters.Add(New ExpandoObjectConverter())

        ' use JSON.NET to deserialize object to a dynamic (expando) object
        Dim JSONObject As [Object]
        ' if we start with a "[", treat this as an array
        If JSONReader.TokenType = JsonToken.StartArray Then
            JSONObject = JSONSerializer.Deserialize(Of List(Of ExpandoObject))(JSONReader)
        Else
            JSONObject = JSONSerializer.Deserialize(Of ExpandoObject)(JSONReader)
        End If

        ' create a backing store to hold all properties for this deserialization
        Dim backingStore As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
        ' add all properties to this backing store
        AddToBackingStore(backingStore, [String].Empty, JSONObject)
        ' return the object in a dictionary value provider so the MVC understands it
        Return New DictionaryValueProvider(Of Object)(backingStore, CultureInfo.CurrentCulture)
    End Function

    Private Shared Sub AddToBackingStore(backingStore As Dictionary(Of String, Object), prefix As String, value As Object)
        Dim d As IDictionary(Of String, Object) = TryCast(value, IDictionary(Of String, Object))
        If d IsNot Nothing Then
            For Each entry As KeyValuePair(Of String, Object) In d
                AddToBackingStore(backingStore, MakePropertyKey(prefix, entry.Key), entry.Value)
            Next
            Return
        End If

        Dim l As IList = TryCast(value, IList)
        If l IsNot Nothing Then
            For i As Integer = 0 To l.Count - 1
                AddToBackingStore(backingStore, MakeArrayKey(prefix, i), l(i))
            Next
            Return
        End If

        ' primitive
        backingStore(prefix) = value
    End Sub

    Private Shared Function MakeArrayKey(prefix As String, index As Integer) As String
        Return prefix & "[" & index.ToString(CultureInfo.InvariantCulture) & "]"
    End Function

    Private Shared Function MakePropertyKey(prefix As String, propertyName As String) As String
        Return If(([String].IsNullOrEmpty(prefix)), propertyName, prefix & "." & propertyName)
    End Function
End Class

Public Class JsonNetResult
    Inherits JsonResult

    Public Overloads Property ContentEncoding() As Encoding
    Public Overloads Property ContentType() As String
    Public Overloads Property Data() As Object
    Public Property SerializerSettings() As JsonSerializerSettings
    Public Property Formatting() As Formatting

    Public Sub New()
        SerializerSettings = New JsonSerializerSettings()
    End Sub

    Public Overrides Sub ExecuteResult(context As ControllerContext)
        If context Is Nothing Then
            Throw New ArgumentNullException("context")
        End If
        Dim response As HttpResponseBase = context.HttpContext.Response
        response.ContentType = If(Not String.IsNullOrEmpty(ContentType), ContentType, "application/json")
        If ContentEncoding IsNot Nothing Then
            response.ContentEncoding = ContentEncoding
        End If
        If Data IsNot Nothing Then
            Dim writer As New JsonTextWriter(response.Output) With { _
                 .Formatting = Formatting _
            }
            Dim serializer As JsonSerializer = JsonSerializer.Create(SerializerSettings)
            serializer.Serialize(writer, Data)
            writer.Flush()
        End If

    End Sub

End Class