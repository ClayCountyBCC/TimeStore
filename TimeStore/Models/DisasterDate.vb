Namespace Models


  Public Class Disaster
    Public Property Name As String
    Public Property Disaster_Start As Date
    Public Property Disaster_End As Date
    Public Property Disaster_Period_Type As Integer = 0
    Public Sub New()

    End Sub

    Public Shared Function GetDisasters() As List(Of Disaster)
      Dim query As String = "
      SELECT 
        Name,
        Disaster_Start,
        Disaster_End, 
        Period_Type
      FROM Disaster_Data_OLD
      ORDER BY Disaster_Start"
      Try
        Return Get_Data(Of Disaster)(query, ConnectionStringType.Timestore)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return New List(Of Disaster)
      End Try

    End Function


  End Class

End Namespace