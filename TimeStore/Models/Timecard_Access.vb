Imports System.Data.SqlClient
Imports Tools
Imports System.Web

Namespace Models
  Public Class Timecard_Access
    Public Property EmployeeOutputList As List(Of EmployeeOutput)
    Public Property TimecardTimeExceptionList As List(Of TimecardTimeException)

    Private ReadOnly Access_Types_As_Text() As String = {"None", "User Only", "Departmental 1", "Departmental 2", "Departmental 3", "Departmental 4", "Departmental 5", "All"}
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
    Public Property PasswordExpirationDate As String = ""
    Public Property PasswordExpiringSoon As Boolean = False
    Public Property ReportsToList As New List(Of Integer)
    Public Property PayPeriodDisplayDate As String = ""
    Public Property ReportsTo As Integer = 0
    Private Property RawDepartmentsToApprove As String = ""
    Public ReadOnly Property DepartmentsToApprove As List(Of String)
      Get
        If RawDepartmentsToApprove.Length > 0 Then
          Return RawDepartmentsToApprove.Split(" ").ToList()
        Else
          Return New List(Of String)
        End If
      End Get
    End Property
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

    Public Sub New()
      UpdatePasswordData()
    End Sub

    Private Sub UpdatePasswordData()
      If EmployeeID < 1000 Then Exit Sub
      Dim aded = AD_EmployeeData.GetCachedEmployeeDataFromAD()
      If aded.ContainsKey(EmployeeID) Then
        Dim e = aded(EmployeeID)
        PasswordExpirationDate = e.PasswordExpirationDate.ToShortDateString()
        PasswordExpiringSoon = e.PasswordExpiring
      End If
    End Sub


    Public Sub New(EID As Integer, dept As String)
      EmployeeID = EID
      UpdatePasswordData()
      Select Case dept
        Case "2103", "1703"
          Data_Type = "telestaff"
        Case Else
          Data_Type = "timecard"
      End Select
      If EID = -1 Then
        Set_Access_Type(Access_Types.No_Access)
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
      RawDepartmentsToApprove = String.Join(" ", rawTCA.DepartmentsToApprove)
      'DepartmentsToApprove.AddRange(rawTCA.DepartmentsToApprove)
      EmployeeID = rawTCA.EmployeeId
      UpdatePasswordData()
      ReportsTo = rawTCA.ReportsTo
      RequiresApproval = rawTCA.RequiresApproval
    End Sub

    Public Function Save() As Boolean
      ' This function will take a raw timecard access object and then insert it into the 
      ' access table if no row already exists for that user,
      ' otherwise it will update the row.
      Dim dbc As New DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
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

    Public Shared Function GetAllAccess_Dict() As Dictionary(Of Integer, Timecard_Access)
      Dim tcal = Get_All_Cached_Access_List()
      Try
        Dim d = tcal.ToDictionary(Function(x) x.EmployeeID, Function(x) x)
        Return d
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return Nothing
      End Try
    End Function

    Public Shared Function GetAllAccess_List() As List(Of Timecard_Access)
      Dim query As String = "
        SELECT
          employee_id EmployeeID,
          data_type,
          access_type _Access,
          can_change_access CanChangeAccess,
          requires_approval RequiresApproval,
          ISNULL(reports_to, 0) ReportsTo,
          dept_approval_list RawDepartmentsToApprove,
          backend_reports_access
        FROM Access"
      Dim al As List(Of Timecard_Access) = Get_Data(Of Timecard_Access)(query, ConnectionStringType.Timestore)
      Dim rt = Get_All_Cached_ReportsTo()
      For Each a In al
        a.UpdatePasswordData()
        If rt.ContainsKey(a.EmployeeID) Then
          a.ReportsToList = rt(a.EmployeeID)
        End If
      Next
      Dim eidlist = (From a In al
                     Select a.EmployeeID).ToList
      Dim fl As List(Of FinanceData) = GetCachedEmployeeDataFromFinplus()
      For Each f In fl
        If Not eidlist.Contains(f.EmployeeId) Then
          al.Add(New Timecard_Access(f.EmployeeId, f.Department))
        End If
      Next
      al.Add(New Timecard_Access(-1, ""))
      Return al
    End Function

    Public Shared Function Get_All_Cached_ReportsTo() As Dictionary(Of Integer, List(Of Integer))
      Dim cip As New Runtime.Caching.CacheItemPolicy
      cip.AbsoluteExpiration = DateTime.Now.AddHours(2)
      Return myCache.GetItem("reportsto", cip)
    End Function

    Public Shared Function Get_All_Cached_Access_Dict() As Dictionary(Of Integer, Timecard_Access)
      Dim cip As New Runtime.Caching.CacheItemPolicy
      cip.AbsoluteExpiration = DateTime.Now.AddHours(2)
      Return myCache.GetItem("allaccessdict", cip)
    End Function

    Public Shared Function Get_All_Cached_Access_List() As List(Of Timecard_Access)
      Dim cip As New Runtime.Caching.CacheItemPolicy
      cip.AbsoluteExpiration = DateTime.Now.AddHours(2)
      Return myCache.GetItem("allaccesslist", cip)
    End Function

    Public Shared Function Get_All_ReportsTo() As Dictionary(Of Integer, List(Of Integer))
      Dim dbc As New DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      Dim query As String = "SELECT employee_id, reports_to FROM Access WHERE reports_to <> 0"
      Dim ds As DataSet = dbc.Get_Dataset(query)
      Dim reportsTo As New Dictionary(Of Integer, List(Of Integer))
      Try
        Dim tmp = (From d In ds.Tables(0).AsEnumerable
                   Select New ReportsTo With {.eId = d("employee_id"),
                  .rTo = d("reports_to")}).ToList
        For Each t In tmp
          Get_ReportsTo_Main(t.rTo, t.rTo, tmp, reportsTo)
        Next
        Return reportsTo
      Catch ex As Exception
        Log(ex)
        Return Nothing
      End Try
    End Function

    Private Shared Sub Get_ReportsTo_Main(base_EmployeeId As Integer,
                    reportsTo_EmployeeId As Integer,
                    ByRef ReportsToList As List(Of ReportsTo),
                    ByRef ReportsTo As Dictionary(Of Integer, List(Of Integer)))

      Dim found = (From r In ReportsToList
                   Where r.rTo = reportsTo_EmployeeId
                   Select r.eId).ToList

      If found.Count > 0 Then
        If Not ReportsTo.ContainsKey(base_EmployeeId) Then
          ReportsTo(base_EmployeeId) = New List(Of Integer)
        End If
        '  ReportsTo(base_EmployeeId).AddRange(found)
        For Each f In found
          If Not ReportsTo(base_EmployeeId).Contains(f) Then ReportsTo(base_EmployeeId).Add(f)
          Get_ReportsTo_Main(base_EmployeeId, f, ReportsToList, ReportsTo)
        Next
      End If

    End Sub

    Public Shared Function Check_Access_To_EmployeeId(AccessBy As Integer, AccessTo As Integer) As Boolean
      Dim tca As Timecard_Access = GetTimeCardAccess(AccessBy)
      Return tca.Check_Access_To_EmployeeId(AccessTo)
    End Function

    Public Function Check_Access_To_EmployeeId(AccessToEID As Integer) As Boolean
      ', Optional ForApproval As Boolean = False ' removed 6/29/2015
      If EmployeeID = AccessToEID Then Return True
      Dim AccessTo As Timecard_Access = GetTimeCardAccess(AccessToEID)

      ' Added this line, if someone is trying to approve someone that is higher than they are, they 
      ' should not be allowed.
      If Raw_Access_Type = Timecard_Access.Access_Types.All_Access Then
        Return True
      ElseIf CType(AccessTo.Raw_Access_Type, Integer) >= CType(Raw_Access_Type, Integer) Then
        ' This will throw out any access requests to people with greater or equal access
        Return False
      End If

      Select Case Raw_Access_Type
        Case Timecard_Access.Access_Types.All_Access
          Return True

        Case Timecard_Access.Access_Types.Department_1, Timecard_Access.Access_Types.Department_2,
            Timecard_Access.Access_Types.Department_3, Timecard_Access.Access_Types.Department_4,
            Timecard_Access.Access_Types.Department_5

          If DepartmentsToApprove.Count > 0 Then
            If DepartmentsToApprove.Contains("ALL") Then
              Return True
            Else
              If Not DepartmentsToApprove.Contains("VIEW") And Not DepartmentsToApprove.Contains("LEAVE") Then
                Dim edl As List(Of Employee_Data) = myCache.GetItem("employeeList")
                Dim dept As String = (From e In edl Where e.EmployeeID = AccessToEID Select e.DepartmentID).First
                If DepartmentsToApprove.Contains(dept) Then Return True
              End If
            End If
          End If

      End Select
      ' If we've gone through the dept check and they still don't have access to this person, we
      ' want to check to see if the user they are trying to load is set to report to them.
      'Dim AccessTo As New Timecard_Access(AccessToEID)
      Return ReportsToList.Contains(AccessTo.EmployeeID)
      '(AccessTo.ReportsToList.Contains(AccessBy.EmployeeID))
    End Function

    Public Shared Function GetTimeCardAccess(Username As String) As Timecard_Access
      Return Get_All_Cached_Access_Dict()(AD_EmployeeData.GetEmployeeIDFromAD(Username))
      'Dim key As String = "tca," & EmployeeId
      'Return myCache.GetItem(key)
    End Function

    Public Shared Function GetTimeCardAccess(EmployeeId As Integer) As Timecard_Access
      Return Get_All_Cached_Access_Dict()(EmployeeId)
      'Dim key As String = "tca," & EmployeeId
      'Return myCache.GetItem(key)
    End Function



  End Class
End Namespace