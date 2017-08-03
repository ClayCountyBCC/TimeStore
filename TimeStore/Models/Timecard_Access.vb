Imports System.Data.SqlClient
Imports Tools

Namespace Models
  Public Class Timecard_Access
    Public Property EmployeeOutputList As List(Of EmployeeOutput)
    Public Property TimecardTimeExceptionList As List(Of TimecardTimeException)

    Private Access_Types_As_Text() As String = {"None", "User Only", "Departmental 1", "Departmental 2", "Departmental 3", "Departmental 4", "Departmental 5", "All"}
    Private _Access As Access_Types = Access_Types.User_Only

    Public Enum Access_Types As Integer
      No_Access = 0
      User_Only = 1
      Department_1 = 2
      Department_2 = 3
      Department_3 = 4
      Department_4 = 5
      Department_5 = 6
      All_Access = 7
    End Enum
    Public Property ReportsToList As New List(Of Integer)
    Public Property PayPeriodDisplayDate As String = ""
    Public Property ReportsTo As Integer = 0
    Public Property DepartmentsToApprove As New List(Of String)
    Public Property RequiresApproval As Boolean = True
    Public Property EmployeeID As Integer = 0
    Public Property CanChangeAccess As Boolean = False
    ' These are the lists of Departments and / Or Users that people have access to.
    'Public Property Users As New List(Of String)
    ' If they have access to the Exception reports, this will be true.
    Public Property Backend_Reports_Access As Boolean = False
    Public Property Data_Type As String
    Public Property MachineName As String = ""
    Public Property UserName As String = ""
    Public Property IPAddress As String = ""
    Private _UpdatedBy As String = ""

    'Public ReadOnly Property Days_Until_PPE() As Integer
    '    Get
    '        Return Today.Subtract(GetPayPeriodStart(Today).AddDays(13)).TotalDays
    '    End Get
    'End Property


    Public Sub Set_Access_Type(Access_Type As Access_Types)
      _Access = Access_Type
    End Sub

    Public Sub Set_Access_Type(Access_Type As Integer)
      _Access = Access_Type
    End Sub

    Public ReadOnly Property Access_Type As String
      Get
        Return Access_Types_As_Text(_Access)
      End Get
    End Property

    Public ReadOnly Property Raw_Access_Type As Access_Types
      Get
        Return _Access
      End Get
    End Property

    Private Sub Load_TCA_From_Datarow(dr As DataRow)
      EmployeeID = dr("employee_id")
      Data_Type = dr("data_type").ToString
      Set_Access_Type(dr("access_type"))
      RequiresApproval = dr("requires_approval")
      CanChangeAccess = dr("can_change_access")
      Dim rt = Get_All_Cached_ReportsTo()
      If rt.ContainsKey(EmployeeID) Then
        ReportsToList.AddRange(rt(EmployeeID))
      End If
      If Not IsDBNull(dr("reports_to")) AndAlso dr("reports_to") <> 0 Then
        ReportsTo = dr("reports_to")

      End If

      If Not IsDBNull(dr("dept_approval_list")) Then
        If dr("dept_approval_list").ToString.Trim.Length > 0 Then
          DepartmentsToApprove.AddRange(dr("dept_approval_list").ToString.Split(" ").ToList)
        Else
          DepartmentsToApprove.Clear()
        End If
      End If

      'If Not IsDBNull(dr("dept_access_list")) Then
      '    DepartmentsToView.AddRange(dr("dept_access_list").ToString.Split(" ").ToList)
      'End If

      Backend_Reports_Access = dr("backend_reports_access")
    End Sub

    Public Sub New(dr As DataRow)
      Load_TCA_From_Datarow(dr)
    End Sub

    Public Sub New(NewEmployeeID As Integer, Request As HttpRequestBase) ' If the user wasn't found in the access table
      If NewEmployeeID = 0 Then
        _Access = Access_Types.No_Access
      Else
        If Not Request Is Nothing Then
          UserName = Request.LogonUserIdentity.Name
          MachineName = Request.UserHostName
          IPAddress = Request.UserHostAddress
        End If
        Dim ds As DataSet = GetTimeStoreAccess(NewEmployeeID)
        If ds.Tables(0).Rows.Count > 0 Then
          Load_TCA_From_Datarow(ds.Tables(0).Rows(0))
        Else
          EmployeeID = NewEmployeeID
          Dim f As List(Of FinanceData) = GetEmployeeDataFromFinPlus(EmployeeID)
          If f.Count = 1 Then
            Dim fd As FinanceData = f(0)
            Select Case fd.Department
              Case "1703", "2103" '"2102" ' Telestaff
                Data_Type = "telestaff"
              Case Else ' Going to try the timecard database for everything else.
                Data_Type = "timecard"
            End Select
          Else
            _Access = Access_Types.No_Access
            Logging.Log("Found user with no access to Timestore", EmployeeID.ToString, "", "", "", LogType.Database)
          End If
        End If
      End If
    End Sub

    Public Sub New(rawTCA As Raw_Timecard_Access, Request As HttpRequestBase)
      _UpdatedBy = Request.LogonUserIdentity.Name
      IPAddress = Request.UserHostAddress
      MachineName = Request.UserHostName
      Set_Access_Type(rawTCA.Access_Type)
      Data_Type = rawTCA.DataType
      Backend_Reports_Access = rawTCA.BackendReportsAccess
      CanChangeAccess = rawTCA.CanChangeAccess
      If rawTCA.DepartmentsToApprove Is Nothing Then rawTCA.DepartmentsToApprove = New List(Of String)
      DepartmentsToApprove.AddRange(rawTCA.DepartmentsToApprove)
      EmployeeID = rawTCA.EmployeeId
      ReportsTo = rawTCA.ReportsTo
      RequiresApproval = rawTCA.RequiresApproval
    End Sub

    Public Function Save() As Boolean
      ' This function will take a raw timecard access object and then insert it into the 
      ' access table if no row already exists for that user,
      ' otherwise it will update the row.
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      ' first let's see if it exists
      Dim query As String = "SELECT COUNT(*) AS CNT FROM Access WHERE employee_id=" & EmployeeID
      Dim sbQ As New StringBuilder
      Dim i As Integer = dbc.ExecuteScalar(query)
      Dim P() As SqlParameter = New SqlParameter() _
                {
                    New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeID},
                    New SqlParameter("@DataType", Data.SqlDbType.VarChar, 25) With {.Value = Data_Type},
                    New SqlParameter("@AccessType", Data.SqlDbType.Int) With {.Value = _Access},
                    New SqlParameter("@CanChangeAccess", Data.SqlDbType.Bit) With {.Value = CanChangeAccess},
                    New SqlParameter("@RequiresApproval", Data.SqlDbType.Bit) With {.Value = RequiresApproval},
                    New SqlParameter("@ReportsTo", Data.SqlDbType.Int) With {.Value = ReportsTo},
                    New SqlParameter("@ApprovalList", Data.SqlDbType.VarChar, 1000) With {.Value = String.Join(" ", DepartmentsToApprove)},
                    New SqlParameter("@ReportsAccess", Data.SqlDbType.Bit) With {.Value = Backend_Reports_Access},
                    New SqlParameter("@UpdatedBy", Data.SqlDbType.VarChar, 50) With {.Value = _UpdatedBy}
                }
      If i > 0 Then
        ' we update
        sbQ.Append("UPDATE Access SET data_type=@DataType, access_type=@AccessType, can_change_access=@CanChangeAccess, ")
        sbQ.Append("requires_approval=@RequiresApproval, reports_to=@ReportsTo, dept_approval_list=@ApprovalList, ")
        sbQ.Append("backend_reports_access=@ReportsAccess, date_last_updated=GETDATE(), updated_by=@UpdatedBy WHERE employee_id=@EmployeeId;")
      Else
        ' we insert
        sbQ.Append("INSERT INTO Access (employee_id, data_type, access_type, can_change_access, requires_approval, reports_to, ")
        sbQ.Append("dept_approval_list, backend_reports_access, updated_by) ")
        sbQ.Append("VALUES (@EmployeeId, @DataType, @AccessType, @CanChangeAccess, @RequiresApproval, @ReportsTo,")
        sbQ.Append(" @ApprovalList, @ReportsAccess, @UpdatedBy);")
      End If
      Try
        i = dbc.ExecuteNonQuery(sbQ.ToString, P)
        Return i = 1
      Catch ex As Exception
        Log(ex)
        Return False
      End Try

    End Function

  End Class
End Namespace