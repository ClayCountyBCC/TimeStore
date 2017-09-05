

Namespace Models

  Public Class Incentive

    Public Enum IncentiveType As Integer
      Public_Safety = 1
      Public_Works = 2
    End Enum

    ReadOnly Property Incentive_Type_Name As String
      Get
        Select Case Incentive_Type
          Case 1
            Return "Public Safety"
          Case 2
            Return "Public Works"
          Case Else
            Return ""
        End Select
      End Get
    End Property
    Property Incentive_Type As Integer
    Property Incentive_Name As String
    Property Incentive_Abrv As String
    Property Incentive_Amount As Double
    Property Start_Date As Date = "1/1/2000"
    Property End_Date As Date = "1/1/2100"

    Public Sub New()

    End Sub

    Public Shared Function Get_All_Incentive_Data() As List(Of Incentive)
      Dim query As String = "
        USE TimeStore;
        SELECT 
          incentive_type Incentive_Type,  
          incentive_abrv Incentive_Abrv, 
          incentive Incentive_Name, 
          amount Incentive_Amount, 
          start_date Start_Date, 
          end_date End_Date 
        FROM Incentives 
        ORDER BY incentive_type ASC, incentive_abrv ASC"
      Return Get_Data(Of Incentive)(query, ConnectionStringType.Telestaff)
      'Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      'Dim sbQ As New StringBuilder
      'With sbQ
      '  .AppendLine("USE TimeStore;")
      '  .AppendLine("SELECT incentive_type, incentive_abrv, incentive, amount, start_date, end_date ")
      '  .AppendLine("FROM Incentives ORDER BY incentive_type ASC, incentive_abrv ASC")
      'End With
      'Try
      '  Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
      '  Dim TTD As List(Of Incentive) = (From d In ds.Tables(0).AsEnumerable Select New Incentive With {
      '                                  .Incentive_Type = d("incentive_type"), .Start_Date = d("start_date"),
      '                                  .Incentive_Name = d("incentive"), .Incentive_Amount = d("amount"),
      '                                  .Incentive_Abrv = d("incentive_abrv"), .End_Date = d("end_date")}).ToList

      '  Dim TS As List(Of Incentive) = GetTelestaffSpecialties()
      '  For Each t In TS
      '    If (From i In TTD Where i.Incentive_Abrv = t.Incentive_Abrv Select i).Count = 0 Then
      '      TTD.Add(t)
      '    End If
      '  Next

      '  Return TTD
      'Catch ex As Exception
      '  Log(ex)
      '  Return New List(Of Incentive)
      'End Try
    End Function


    Private Function Update_Incentives() As Boolean
      Dim dp As New DynamicParameters()
      dp.Add("@Incentive_Amount", Me.Incentive_Amount)
      dp.Add("@Incentive_Abrv", Me.Incentive_Abrv)

      Dim query As String = "
      USE TimeStore;
      UPDATE Incentives
      SET amount=@Incentive_Amount
      WHERE incentive_abrv=@Incentive_Abrv"
      Dim i As Integer = Exec_Query(query, dp, ConnectionStringType.Timestore)
      Return i > -1
    End Function

    Public Shared Function Save_Incentive_Data(Incentives As List(Of Incentive)) As Boolean
      Dim failcount As Integer = 0
      For Each i In Incentives
        If Not i.Update_Incentives() Then failcount += 1
      Next
      Return failcount = 0
    End Function

    'Public Shared Function Save_Incentive_Data(Incentives As List(Of Incentive)) As Boolean
    '  If Incentives.Count = 0 Then Return True
    '  Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    '  Dim incentiveType As Integer = (From i In Incentives Select i.Incentive_Type).First
    '  Dim incentiveAbrv() As String = (From i In Incentives Select i.Incentive_Abrv).ToArray
    '  Dim sbQ As New StringBuilder
    '  With sbQ
    '    .AppendLine("USE TimeStore;")
    '    .AppendLine("DELETE FROM Incentives WHERE incentive_type=")
    '    .Append(incentiveType).Append(" AND incentive_abrv IN (")
    '    For a As Integer = incentiveAbrv.GetLowerBound(0) To incentiveAbrv.GetUpperBound(0)
    '      .Append("'").Append(incentiveAbrv(a)).Append("'")
    '      If a < incentiveAbrv.GetUpperBound(0) Then .Append(",")
    '    Next
    '    .AppendLine(");")
    '  End With
    '  Dim x As Integer = 0
    '  Try
    '    x = dbc.ExecuteNonQuery(sbQ.ToString)
    '  Catch ex As Exception
    '    Log(ex)
    '    Return False
    '  End Try
    '  With sbQ
    '    .Clear()
    '    .AppendLine("INSERT INTO Incentives (incentive_type, incentive_abrv, incentive, amount) ")
    '    .AppendLine("VALUES (@IncentiveType, @IncentiveAbrv, @Incentive, @Amount);")
    '  End With
    '  Try
    '    For Each i In Incentives

    '      Dim p(3) As SqlParameter
    '      p(0) = New SqlParameter("@IncentiveType", Data.SqlDbType.Int) With {.Value = i.Incentive_Type}
    '      p(1) = New SqlParameter("@IncentiveAbrv", Data.SqlDbType.VarChar, 10) With {.Value = i.Incentive_Abrv}
    '      p(2) = New SqlParameter("@Incentive", Data.SqlDbType.VarChar, 50) With {.Value = i.Incentive_Name}
    '      p(3) = New SqlParameter("@Amount", Data.SqlDbType.Decimal) With {.Value = i.Incentive_Amount}

    '      x = dbc.ExecuteNonQuery(sbQ.ToString, p)
    '    Next
    '    Return True
    '  Catch ex As Exception
    '    Log(ex)
    '    Return False
    '  End Try
    'End Function

  End Class

End Namespace