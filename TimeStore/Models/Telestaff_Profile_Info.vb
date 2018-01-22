Imports System.Data.SqlClient

Namespace Models

  Public Class Telestaff_Profile_Info
    Property EmployeeId As Integer
    Property Payrate As Double
    Property FieldPayrate As Double = 0
    Property ProfileType As TelestaffProfileType
    Property WorkDate As Date
    Property ProfileError As Boolean = False

    Public Sub New(EmployeeId As Integer, WorkDate As Date)
      Me.EmployeeId = EmployeeId
      Me.WorkDate = WorkDate
      Dim dr As DataRow = Get_Telestaff_Profile_By_Date(EmployeeId, WorkDate)
      ProfileError = (dr Is Nothing)

      If Not ProfileError AndAlso Not IsDBNull(dr("rsc_hourwage_db")) Then
        ProfileType = dr("PayInfo_No_In")
        Payrate = Math.Round(dr("rsc_hourwage_db"), 5)
        FieldPayrate = Payrate
        If ProfileType = TelestaffProfileType.Office Then
          FieldPayrate = Math.Round(Calculate_Reverse_Telestaff_Office_Payrate(Payrate), 5)
        End If
      End If
    End Sub


    Private Function Get_Telestaff_Profile_By_Date(EmployeeID As Integer,
                                                DateToCheck As Date) As DataRow

      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
      Dim query As String = "
        USE Telestaff;
        SELECT TOP 1
          ISNULL(R.rsc_thru_da, @DateToCheck) rsc_thru_da,
          R.rsc_hourwage_db, 
          R.PayInfo_No_In 
        FROM Resource_Tbl R
        INNER JOIN Resource_Master_Tbl RMT ON R.RscMaster_No_In = RMT.RscMaster_No_In
        WHERE 
          RMT.RscMaster_EmployeeID_Ch = @EmployeeId
        ORDER BY ISNULL(R.Rsc_Thru_Da, @DateToCheck) DESC"
      '--AND R.Rsc_From_Da <= @DateToCheck 
      'AND ISNULL(R.Rsc_Thru_Da, @DateToCheck) >= @DateToCheck

      Dim P(1) As SqlParameter
      P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.VarChar, 30) With {.Value = EmployeeID.ToString}
      P(1) = New SqlParameter("@DateToCheck", Data.SqlDbType.Date) With {.Value = DateToCheck}
      Dim ds As DataSet
      Try
        ds = dbc.Get_Dataset(query, P)
        If ds.Tables(0).Rows.Count > 1 Then
          Log("Too many Telestaff profiles found on date for Employee " & EmployeeID.ToString, EmployeeID.ToString, DateToCheck.ToShortDateString, ds.Tables(0).Rows.Count.ToString)
          Return Nothing
        ElseIf ds.Tables(0).Rows.Count = 1 Then
          Return ds.Tables(0).Rows(0)
        Else
          'Log("No Telestaff profiles found on date for Employee " & EmployeeID.ToString, EmployeeID.ToString, DateToCheck.ToShortDateString, ds.Tables(0).Rows.Count.ToString)
          Return Nothing
        End If

      Catch ex As Exception
        Log(ex)
        Return Nothing
      End Try
    End Function
  End Class

End Namespace
