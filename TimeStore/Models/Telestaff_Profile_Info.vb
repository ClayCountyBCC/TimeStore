Imports System.Data

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

  End Class

End Namespace
