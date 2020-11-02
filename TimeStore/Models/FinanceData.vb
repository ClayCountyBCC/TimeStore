Namespace Models

  Public Class FinanceData
    Property TimeStoreAccess As Timecard_Access = Nothing
    Property EmployeeId As Integer = Integer.MinValue
    Property EmployeeName As String = ""
    Property EmployeeLastName As String = ""
    Property EmployeeFirstName As String = ""
    Property EmployeeType As String = "" 'E for Exempt, N for NonExempt
    Property isFulltime As Boolean = True
    Property HireDate As Date = Date.MinValue
    Property BirthDate As Date = Date.MinValue
    Property JobTitle As String = ""
    Property Department As String = ""
    Property DepartmentName As String = ""
    Property Classify As String = ""
    Property Bargain As String = ""
    'Property PayPeriodStart As Date = Date.MinValue
    Property HoursNeededForOvertime As Double = 0
    Property Base_Payrate As Double = 0
    Property Comp_Time_Code As String = ""
    Property Banked_Vacation_Hours As Double = Double.MinValue
    Property Banked_Sick_Hours As Double = Double.MinValue
    Property Banked_Comp_Hours As Double = Double.MinValue
    Property Banked_Holiday_Hours As Double = Double.MinValue
    Public Property TerminationDate As Date = Date.MaxValue
    Public ReadOnly Property IsTerminated As Boolean
      Get
        Return TerminationDate <> Date.MaxValue
      End Get
    End Property
    Public ReadOnly Property IsExempt As Boolean
      Get
        Return EmployeeType = "E"
      End Get
    End Property

    Public Sub New(dr As DataRow, Optional RefreshDisplayName As Boolean = False)
      Try
        EmployeeId = dr("empl_no")
        EmployeeLastName = dr("l_name").ToString.Trim
        EmployeeFirstName = dr("f_name").ToString.Trim
        Bargain = dr("bargain").trim
        EmployeeName = EmployeeFirstName & " " & EmployeeLastName
        EmployeeType = IsNull(dr("empl_type"), "")
        BirthDate = IsNull(dr("birthdate"), Date.MinValue)
        HireDate = dr("hire_date")
        JobTitle = dr("title").ToString.Trim
        Department = dr("department").ToString.Trim
        If Not IsDBNull(dr("pay_hours")) Then
          HoursNeededForOvertime = dr("pay_hours")
        End If
        Base_Payrate = dr("rate")
        Banked_Holiday_Hours = 0
        Banked_Comp_Hours = 0
        Classify = dr("classify").ToString.Trim
        DepartmentName = dr("department_name").ToString.Trim
        isFulltime = (dr("part_time").ToString.Trim = "F")
        If Not IsDBNull(dr("term_date")) Then
          TerminationDate = dr("term_date")
        End If
        Select Case IsNull(dr("lv5_cd"), "").ToString.Trim
          Case "500" ' Comp time bank
            Banked_Comp_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
            Comp_Time_Code = "500"
          Case "510" ' Holiday Bank
            Banked_Holiday_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
          Case Else
        End Select
        If IsNull(dr("lv6_cd"), "").ToString.Trim = "600" Then
          Banked_Comp_Hours = IsNull(dr("lv6_bal"), Double.MinValue)
          Comp_Time_Code = "600"
        End If

        Banked_Vacation_Hours = IsNull(dr("lv2_bal"), Double.MinValue)
        Banked_Sick_Hours = IsNull(dr("lv1_bal"), Double.MinValue)
      Catch ex As Exception
        Log(ex)
        'Tools.Log(ex, toolsAppId, Tools.Logging.LogType.Database)
      End Try
    End Sub
  End Class

End Namespace
