Imports System.Data
Imports System.Data.SqlClient
Imports Dapper
Imports System.Runtime.Caching

Namespace Models


  Public Class PayrollLock
    Private _default_lock_date As Date?
    Private _lock_dates As New List(Of String)
    Private _lock_times As New List(Of String)
    Public Property pay_period_ending As Date
    Public Property lock_date As Date
    Public Property lock_time As String
    Public Property calculated_lock_datetime As Date
    Public Property created_on As Date
    Public Property created_by As String
    Public ReadOnly Property default_lock_date As Date
      Get
        If Not _default_lock_date.HasValue Then
          _default_lock_date = pay_period_ending.AddDays(1)
        End If
        Return _default_lock_date.Value
      End Get
    End Property
    Public ReadOnly Property default_lock_time As String = "10:00 AM"
    Public ReadOnly Property lock_dates As List(Of String)
      Get
        If _lock_dates.Count = 0 Then
          Dim start = default_lock_date.AddDays(-7)
          For i = 0 To 13
            _lock_dates.Add(start.AddDays(i).ToShortDateString)
          Next
        End If
        Return _lock_dates
      End Get
    End Property

    Public ReadOnly Property lock_times As List(Of String)
      Get
        If _lock_times.Count = 0 Then
          Dim start = Today
          For i = 0 To 23
            _lock_times.Add(start.AddHours(i).ToShortTimeString)
          Next
        End If
        Return _lock_times
      End Get
    End Property

    Public Sub New()
    End Sub

    Public Sub New(ppe As Date)
      pay_period_ending = ppe
      Set_Default()
      Save()
    End Sub

    Public Sub Set_Default()
      lock_date = pay_period_ending.AddDays(1)
      lock_time = "10:00 AM"
      calculated_lock_datetime = pay_period_ending.AddDays(1).AddHours(10)
      created_by = "System"
      created_on = Now
    End Sub

    Public Shared Function Get_PayrollLock(ppe As Date)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", ppe)
      Dim query As String = "
      SELECT
        pay_period_ending
        ,lock_date
        ,lock_time
        ,calculated_lock_datetime
        ,created_on
        ,created_by
      FROM Timestore.dbo.Payroll_Lock
      WHERE
        pay_period_ending = @pay_period_ending"
      Dim data = Get_Data(Of PayrollLock)(query, dp, ConnectionStringType.Timestore)
      If data.Count() = 0 Then
        Return New PayrollLock(ppe)
      Else
        Return data.First
      End If
    End Function

    Public Shared Function Get_Cached_PayrollLock(ppe As Date) As PayrollLock
      Dim key As String = "payroll_lock," & ppe.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(12)
      }
      Return myCache.GetItem(key, CIP)
    End Function

    Public Shared Sub Update_Cached_PayrollLock(lock As PayrollLock)
      Dim key As String = "payroll_lock," & lock.pay_period_ending.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(12)
      }
      myCache.SetObject(key, lock, CIP)
    End Sub

    Public Sub Save()
      Dim query As String = "
        INSERT INTO Payroll_Lock (pay_period_ending, lock_date, lock_time, calculated_lock_datetime, created_by)
        VALUES (@pay_period_ending, @lock_date, @lock_time, @calculated_lock_datetime, @created_by)"
      Try
        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          db.Execute(query, Me)
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
      End Try
    End Sub

    Public Sub Update()
      Dim query As String = "
        UPDATE Payroll_Lock
        SET 
          lock_date = @lock_date
          ,lock_time = @lock_time
          ,calculated_lock_datetime = @calculated_lock_datetime
          ,created_by = @created_by
          ,created_on = GETDATE()
        WHERE
          pay_period_ending = @pay_period_ending;"
      Try
        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          Dim i = db.Execute(query, Me)
          If i > 0 Then
            Update_Cached_PayrollLock(Me)
          End If
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
      End Try
    End Sub


  End Class
End Namespace