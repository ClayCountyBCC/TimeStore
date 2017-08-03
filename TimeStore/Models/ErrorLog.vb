Imports System.Data
Imports Dapper
Imports System.Data.SqlClient

Namespace Models

  Public Class ErrorLog
    Public Property AppId As Integer = 20006
    Public Property ApplicationName As String = "TimeStore"
    Public Property ErrorText As String = ""
    Public Property ErrorMessage As String = ""
    Public Property ErrorStacktrace As String = ""
    Public Property ErrorSource As String = ""
    Public Property Query As String = ""

    Public Sub New(text As String,
                   message As String,
                   stacktrace As String,
                   source As String,
                   ErrorQuery As String)
      ErrorText = text
      ErrorMessage = message
      ErrorStacktrace = stacktrace
      ErrorSource = source
      Query = ErrorQuery
      SaveLog()
    End Sub

    Public Sub New(ex As Exception, ErrorQuery As String)
      ErrorText = ex.ToString
      ErrorMessage = ex.Message
      ErrorSource = ex.Source
      ErrorStacktrace = ex.StackTrace
      Query = ErrorQuery
      SaveLog()
    End Sub

    Private Sub SaveLog()
      Dim sql As String = "
          INSERT INTO ErrorData 
          (applicationName, errorText, errorMessage, 
          errorStacktrace, errorSource, query)  
          VALUES (@applicationName, @errorText, @errorMessage,
            @errorStacktrace, @errorSource, @query);"
      Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Log))
        db.Execute(sql, Me)
      End Using
    End Sub

  End Class

End Namespace
