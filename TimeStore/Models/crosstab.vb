Namespace Models

  Public Class Crosstab
    Property PayPeriodStart As Date = Date.MinValue
    Property orgn As String = ""
    Property EmployeeID As String = ""
    Property LastName As String = ""
    Property FirstName As String = ""
    Property Total As Double = 0
    Property Regular As Double = 0
    Property pc006 As Double = 0
    Property pc007 As Double = 0
    Property pc046 As Double = 0
    Property pc090 As Double = 0
    Property pc095 As Double = 0
    Property pc100 As Double = 0
    Property pc101 As Double = 0
    Property pc110 As Double = 0
    Property pc111 As Double = 0
    Property pc120 As Double = 0
    Property pc121 As Double = 0
    Property pc122 As Double = 0
    Property pc123 As Double = 0
    Property pc124 As Double = 0
    Property pc130 As Double = 0
    Property pc131 As Double = 0
    Property pc134 As Double = 0
    Property pc230 As Double = 0
    Property pc231 As Double = 0
    Property pc232 As Double = 0
    Property pc299 As Double = 0
    Property pc300 As Double = 0
    Property pc301 As Double = 0
    Property pc302 As Double = 0
    Property pc303 As Double = 0
    ReadOnly Property EmployeeID_d As String
      Get
        If EmployeeID = "" Then Return "Total" Else Return EmployeeID
      End Get
    End Property

    ReadOnly Property Orgn_d As String
      Get
        If orgn = "" Then Return "Total" Else Return orgn
      End Get
    End Property

    ReadOnly Property Total_d As String
      Get
        If Total = 0 Then Return "" Else Return String.Format("{0:N2}", Total)
      End Get
    End Property
    ReadOnly Property Regular_d As String
      Get
        If Regular = 0 Then Return "" Else Return String.Format("{0:N2}", Regular)
      End Get
    End Property
    ReadOnly Property pc006_d As String
      Get
        If pc006 = 0 Then Return "" Else Return String.Format("{0:N2}", pc006)
      End Get
    End Property
    ReadOnly Property pc007_d As String
      Get
        If pc007 = 0 Then Return "" Else Return String.Format("{0:N2}", pc007)
      End Get
    End Property
    ReadOnly Property pc046_d As String
      Get
        If pc046 = 0 Then Return "" Else Return String.Format("{0:N2}", pc046)
      End Get
    End Property
    ReadOnly Property pc090_d As String
      Get
        If pc090 = 0 Then Return "" Else Return String.Format("{0:N2}", pc090)
      End Get
    End Property
    ReadOnly Property pc095_d As String
      Get
        If pc095 = 0 Then Return "" Else Return String.Format("{0:N2}", pc095)
      End Get
    End Property
    ReadOnly Property pc100_d As String
      Get
        If pc100 = 0 Then Return "" Else Return String.Format("{0:N2}", pc100)
      End Get
    End Property
    ReadOnly Property pc101_d As String
      Get
        If pc101 = 0 Then Return "" Else Return String.Format("{0:N2}", pc101)
      End Get
    End Property
    ReadOnly Property pc110_d As String
      Get
        If pc110 = 0 Then Return "" Else Return String.Format("{0:N2}", pc110)
      End Get
    End Property
    ReadOnly Property pc111_d As String
      Get
        If pc111 = 0 Then Return "" Else Return String.Format("{0:N2}", pc111)
      End Get
    End Property
    ReadOnly Property pc120_d As String
      Get
        If pc120 = 0 Then Return "" Else Return String.Format("{0:N2}", pc120)
      End Get
    End Property
    ReadOnly Property pc121_d As String
      Get
        If pc121 = 0 Then Return "" Else Return String.Format("{0:N2}", pc121)
      End Get
    End Property
    ReadOnly Property pc122_d As String
      Get
        If pc122 = 0 Then Return "" Else Return String.Format("{0:N2}", pc122)
      End Get
    End Property
    ReadOnly Property pc123_d As String
      Get
        If pc123 = 0 Then Return "" Else Return String.Format("{0:N2}", pc123)
      End Get
    End Property
    ReadOnly Property pc124_d As String
      Get
        If pc124 = 0 Then Return "" Else Return String.Format("{0:N2}", pc124)
      End Get
    End Property
    ReadOnly Property pc130_d As String
      Get
        If pc130 = 0 Then Return "" Else Return String.Format("{0:N2}", pc130)
      End Get
    End Property
    ReadOnly Property pc131_d As String
      Get
        If pc131 = 0 Then Return "" Else Return String.Format("{0:N2}", pc131)
      End Get
    End Property
    ReadOnly Property pc134_d As String
      Get
        If pc134 = 0 Then Return "" Else Return String.Format("{0:N2}", pc134)
      End Get
    End Property
    ReadOnly Property pc230_d As String
      Get
        If pc230 = 0 Then Return "" Else Return String.Format("{0:N2}", pc230)
      End Get
    End Property
    ReadOnly Property pc231_d As String
      Get
        If pc231 = 0 Then Return "" Else Return String.Format("{0:N2}", pc231)
      End Get
    End Property
    ReadOnly Property pc232_d As String
      Get
        If pc232 = 0 Then Return "" Else Return String.Format("{0:N2}", pc232)
      End Get
    End Property

    ReadOnly Property pc299_d As String
      Get
        If pc299 = 0 Then Return "" Else Return String.Format("{0:N2}", pc299)
      End Get
    End Property

    ReadOnly Property pc300_d As String
      Get
        If pc300 = 0 Then Return "" Else Return String.Format("{0:N2}", pc300)
      End Get
    End Property

    ReadOnly Property pc301_d As String
      Get
        If pc301 = 0 Then Return "" Else Return String.Format("{0:N2}", pc301)
      End Get
    End Property

    ReadOnly Property pc302_d As String
      Get
        If pc302 = 0 Then Return "" Else Return String.Format("{0:N2}", pc302)
      End Get
    End Property

    ReadOnly Property pc303_d As String
      Get
        If pc303 = 0 Then Return "" Else Return String.Format("{0:N2}", pc303)
      End Get
    End Property

    Public Sub New(d As DataRow, f As FinanceData, PPS As Date)
      PayPeriodStart = PPS
      EmployeeID = f.EmployeeId
      orgn = f.Department
      FirstName = f.EmployeeFirstName
      LastName = f.EmployeeLastName
      'EmployeeID = IsNull(d("employee_id"), "")
      'orgn = IsNull(d("orgn"), "")
      'FirstName = GetFirstName(f, EmployeeID.ToString)
      'LastName = GetLastName(f, EmployeeID.ToString)
      Regular = IsNull(d("sumReg"), 0)
      Total = IsNull(d("TotalHours"), 0)
      pc006 = IsNull(d("006"), 0)
      pc007 = IsNull(d("007"), 0)
      pc046 = IsNull(d("046"), 0)
      pc095 = IsNull(d("095"), 0)
      pc090 = IsNull(d("090"), 0)
      pc100 = IsNull(d("100"), 0)
      pc101 = IsNull(d("101"), 0)
      pc110 = IsNull(d("110"), 0)
      pc111 = IsNull(d("111"), 0)
      pc120 = IsNull(d("120"), 0)
      pc121 = IsNull(d("121"), 0)
      pc122 = IsNull(d("122"), 0)
      pc123 = IsNull(d("123"), 0)
      pc124 = IsNull(d("124"), 0)
      pc130 = IsNull(d("130"), 0)
      pc131 = IsNull(d("131"), 0)
      pc134 = IsNull(d("134"), 0)
      pc230 = IsNull(d("230"), 0)
      pc231 = IsNull(d("231"), 0)
      pc232 = IsNull(d("232"), 0)
      pc299 = IsNull(d("299"), 0)
      pc300 = IsNull(d("300"), 0)
      pc301 = IsNull(d("301"), 0)
      pc302 = IsNull(d("302"), 0)
      pc303 = IsNull(d("303"), 0)


    End Sub

    Public Sub New(f As FinanceData, PPS As Date)
      PayPeriodStart = PPS
      EmployeeID = f.EmployeeId
      orgn = f.Department
      FirstName = f.EmployeeFirstName
      LastName = f.EmployeeLastName
    End Sub

    Public Sub New(d As DataRow, PPS As Date)
      PayPeriodStart = PPS
      EmployeeID = ""
      orgn = d("orgn")
      FirstName = ""
      LastName = ""
      Regular = IsNull(d("sumReg"), 0)
      Total = IsNull(d("TotalHours"), 0)
      pc006 = IsNull(d("006"), 0)
      pc007 = IsNull(d("007"), 0)
      pc046 = IsNull(d("046"), 0)
      pc095 = IsNull(d("095"), 0)
      pc090 = IsNull(d("090"), 0)
      pc100 = IsNull(d("100"), 0)
      pc101 = IsNull(d("101"), 0)
      pc110 = IsNull(d("110"), 0)
      pc111 = IsNull(d("111"), 0)
      pc120 = IsNull(d("120"), 0)
      pc121 = IsNull(d("121"), 0)
      pc122 = IsNull(d("122"), 0)
      pc123 = IsNull(d("123"), 0)
      pc124 = IsNull(d("124"), 0)
      pc130 = IsNull(d("130"), 0)
      pc131 = IsNull(d("131"), 0)
      pc134 = IsNull(d("134"), 0)
      pc230 = IsNull(d("230"), 0)
      pc231 = IsNull(d("231"), 0)
      pc232 = IsNull(d("232"), 0)
      pc299 = IsNull(d("299"), 0)
      pc300 = IsNull(d("300"), 0)
      pc301 = IsNull(d("301"), 0)
      pc302 = IsNull(d("302"), 0)
      pc303 = IsNull(d("303"), 0)
    End Sub

  End Class

End Namespace
