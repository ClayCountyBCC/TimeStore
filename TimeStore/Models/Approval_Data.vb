Namespace Models
  Public Class Approval_Data
    Public Property EmployeeName As String = ""
    Public Property EmployeeID As Integer = 0
    Public Property Initial_Approval_By_EmployeeID As Integer = 0
    Public Property Data_Type As String = "telestaff"
    Public Property Access_Type As Integer = 1
    Public Property DepartmentID As String = ""
    Public Property DepartmentName As String = ""
    Public Property GroupName As String = ""
    Public Property Classify As String
    Public Property ReportsTo As Integer = 0
    Public Property PayPeriodStart As Date
    Public Property PayPeriodEnding As Date
    Public Property Approved As Boolean = False

    ReadOnly Property ViewPayPeriodStart As Date
      Get
        Return PayPeriodStart.AddDays(-13)
      End Get
    End Property

    ReadOnly Property ViewPayPeriodEnding As Date
      Get
        Return PayPeriodStart.AddDays(13)
      End Get
    End Property

    Public Property WorkTypeList As New List(Of GenericTimecard.WorkType)

    Public Sub New()
    End Sub

    Public Sub New(dr As DataRow, ByRef emp As List(Of Employee_Data), ByRef dl As List(Of FinplusDepartment))
      ' Assumes a datarow from the Saved_Time table

      EmployeeID = dr("employee_id")
      Dim e As Employee_Data = (From ed In emp Where ed.EmployeeID = EmployeeID Select ed).First
      EmployeeName = e.EmployeeDisplay
      GroupName = e.GroupName
      ReportsTo = dr("reports_to")
      Data_Type = dr("data_type").ToString.Trim
      Access_Type = dr("access_type").ToString.Trim
      Classify = dr("classify").ToString.Trim
      DepartmentID = dr("orgn").ToString.Trim
      DepartmentName = (From d In dl Where d.DepartmentNumber = DepartmentID Select d.DepartmentDisplay).First.trim
      PayPeriodEnding = dr("pay_period_ending")
      PayPeriodStart = PayPeriodEnding.AddDays(-13)
    End Sub

    Public ReadOnly Property Payrates() As List(Of Double)
      Get
        If WorkTypeList.Count = 0 Then Return New List(Of Double)
        Return (From w In WorkTypeList Select w.payRate Distinct).ToList
      End Get
    End Property

    Public Sub Save_Note(EmployeeId As Integer, PayPeriodEnding As Date, ApprovalType As Integer, ApprovedBy As String)
      Dim sbNote As New StringBuilder()
      For Each p In Payrates
        With sbNote
          Select Case ApprovalType
            Case 1
              .Append("Initial Approval for Hours at payrate ")
            Case 2, 3
              .Append("Final Approval for Hours at payrate ")
          End Select

          .Append(p.ToString("C2")).Append(": ")

          Dim tmp As String = ""
          For Each w In (From wtl In WorkTypeList Where wtl.payRate = p And wtl.hours > 0 Select wtl).ToList
            .Append(tmp).Append(w.name).Append(" ").Append(w.hours).Append(" hours")
            tmp = ","
          Next
        End With
        Add_Timestore_Note(EmployeeId, PayPeriodEnding, sbNote.ToString, ApprovedBy)
        sbNote.Clear()
      Next
    End Sub

  End Class
End Namespace