Imports System.Data
Imports System.Data.SqlClient
Imports Dapper

Namespace Models

  Public Class PayrollStatus
    Private Enum CurrentStatus As Integer
      Start = 0
      Reset = 1
      EditsCompleted = 2
      Approved = 5
      Posted = 6
    End Enum
    Public Enum DatabaseTarget As Integer
      Finplus_Production = 0
      Finplus_Training = 1
      Not_Selected = 2
    End Enum

    Public Property pay_period_ending As Date
    Public Property started_on As Date?
    Public Property started_by As String = ""
    Public Property edits_completed_on As Date?
    Public Property edits_completed_by As String = ""
    Public Property edits_approved_on As Date?
    Public Property edits_approved_by As String = ""
    Public Property finplus_updated_on As Date?
    Public Property finplus_updated_by As String = ""
    Public ReadOnly Property has_edit_access As Boolean
      Get
        Return my_access.PayrollAccess > 0
      End Get
    End Property
    Public ReadOnly Property has_approval_access As Boolean
      Get
        Return my_access.PayrollAccess > 1
      End Get
    End Property

    Public ReadOnly Property can_start As Boolean
      Get
        'If my_access Is Nothing Then Return False
        Return has_edit_access AndAlso started_by.Length = 0
      End Get
    End Property
    Public ReadOnly Property can_reset As Boolean
      Get
        'If my_access Is Nothing Then Return False
        Return has_edit_access AndAlso can_edit AndAlso edits_completed_by.Length = 0 And Not can_start
      End Get
    End Property
    Public ReadOnly Property can_edit As Boolean
      Get
        'If my_access Is Nothing Then Return False
        ' They can't edit anymore if the edits have been approved
        Return has_edit_access AndAlso edits_approved_by.Length = 0 AndAlso can_start = False
      End Get
    End Property
    Public ReadOnly Property can_approve_edits As Boolean
      Get
        'If my_access Is Nothing Then Return False
        Return has_approval_access AndAlso can_start = False AndAlso edits_approved_by.Length = 0 AndAlso finplus_updated_by.Length = 0
      End Get
    End Property
    Public ReadOnly Property can_update_finplus As Boolean
      Get
        'If my_access Is Nothing Then Return False
        Return has_approval_access AndAlso edits_approved_by.Length > 0 AndAlso finplus_updated_by.Length = 0
      End Get
    End Property
    Public Property my_access As Timecard_Access = Nothing
    Public Property target_db As DatabaseTarget = 2
    Public Property include_benefits As Boolean = True

    Public Sub New()
    End Sub

    Public Sub New(ppe As Date, tca As Timecard_Access)
      pay_period_ending = ppe
      my_access = tca
    End Sub

    Public Shared Function GetPayrollStatus(PayPeriodEnding As Date, tca As Timecard_Access) As PayrollStatus
      Dim dp As New DynamicParameters
      dp.Add("@ppe", PayPeriodEnding)
      Dim query As String = "
      SELECT
        pay_period_ending
        ,started_on
        ,started_by
        ,edits_completed_on
        ,edits_completed_by
        ,edits_approved_on
        ,edits_approved_by
        ,finplus_updated_on
        ,finplus_updated_by
        ,target_db
        ,include_benefits
      FROM TimeStore.dbo.Payroll_Status
      WHERE
        pay_period_ending = @ppe"
      Try
        Dim pslist As List(Of PayrollStatus) = Get_Data(Of PayrollStatus)(query, dp, ConnectionStringType.Timestore)

        If pslist Is Nothing Then Return Nothing

        If pslist.Count = 0 Then
          Return New PayrollStatus(PayPeriodEnding, tca)
        Else
          Dim ps = pslist.First
          ps.my_access = tca
          Return ps
        End If
      Catch ex As Exception
        Dim e As New Models.ErrorLog(ex, query)
        Return Nothing
      End Try

    End Function

    Public Shared Function StartPayroll(PayPeriodEnding As Date,
                                        IncludeBenefits As Boolean,
                                        Target As DatabaseTarget,
                                        tca As Timecard_Access) As PayrollStatus
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      dp.Add("@username", tca.UserName)
      dp.Add("@target_db", Target)
      dp.Add("@include_benefits", IIf(IncludeBenefits, 1, 0))
      Dim sbQuery As New StringBuilder
      sbQuery.AppendLine("BEGIN TRANSACTION;")
      sbQuery.AppendLine("BEGIN TRY")
      sbQuery.AppendLine(CapturePayratesQuery(Target, IncludeBenefits))
      sbQuery.AppendLine(CaptureLeaveBanksQuery(Target))
      sbQuery.AppendLine(SetupPayrollChanges())
      sbQuery.AppendLine(CreateNewPayrollStatus())
      sbQuery.AppendLine("END TRY")
      sbQuery.AppendLine("BEGIN CATCH")
      sbQuery.AppendLine("IF @@TRANCOUNT > 0")
      sbQuery.AppendLine("ROLLBACK TRANSACTION;")
      sbQuery.AppendLine("END CATCH;")
      sbQuery.AppendLine("IF @@TRANCOUNT > 0")
      sbQuery.AppendLine("COMMIT TRANSACTION;")
      Try

        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          db.Execute(sbQuery.ToString, dp)
          Return GetPayrollStatus(PayPeriodEnding, tca)
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return Nothing
      End Try

    End Function

    'Private Shared Function UpdateStatus(Status As CurrentStatus, PayPeriodEnding As Date, tca As Timecard_Access, cs As ConnectionStringType, Optional IncludeBenefits As Boolean = True) As PayrollStatus
    '  Try
    '    Dim dp As New DynamicParameters
    '    dp.Add("@pay_period_ending", PayPeriodEnding)
    '    dp.Add("@username", tca.UserName)
    '    Dim SP As String = ""
    '    Select Case Status
    '      Case CurrentStatus.Start
    '        dp.Add("@include_benefits", IIf(IncludeBenefits, 1, 0))
    '        SP = "StartPayroll"
    '      Case CurrentStatus.Reset
    '        dp.Add("@include_benefits", IIf(IncludeBenefits, 1, 0))
    '        SP = "ResetPayroll"
    '      Case CurrentStatus.EditsCompleted
    '        SP = "EditsCompleted"
    '      Case CurrentStatus.Approved
    '        SP = "EditsApproved"
    '      Case CurrentStatus.Posted
    '        SP = "Posted"
    '    End Select
    '    Using db As IDbConnection = New SqlConnection(GetCS(cs))
    '      db.Execute(SP, dp, commandType:=CommandType.StoredProcedure)
    '      Return GetPayrollStatus(PayPeriodEnding, tca)
    '    End Using
    '  Catch ex As Exception
    '    Dim e As New ErrorLog(ex, "")
    '    Return Nothing
    '  End Try
    'End Function

    Public Shared Function ResetPayroll(PayPeriodEnding As Date,
                                        tca As Timecard_Access) As PayrollStatus
      Dim query As String = "
      BEGIN TRANSACTION;
        BEGIN TRY
          DELETE FROM Finplus_Payrates WHERE pay_period_ending=@pay_period_ending;
          DELETE FROM Finplus_Payroll WHERE pay_period_ending=@pay_period_ending;
          DELETE FROM Payroll_Status WHERE pay_period_ending=@pay_period_ending;
          DELETE FROM Payroll_Changes WHERE pay_period_ending=@pay_period_ending;
          INSERT INTO TimeStore.dbo.notes (employee_id, pay_period_ending, note, note_added_by)
          VALUES (@employee_id, @pay_period_ending, 'Payroll Process has been reset.', @username);
        END TRY
        BEGIN CATCH
          IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;          
        END CATCH;

        IF @@TRANCOUNT > 0 
          COMMIT TRANSACTION;"
      Try
        Dim dp As New DynamicParameters
        dp.Add("@pay_period_ending", PayPeriodEnding)
        dp.Add("@employee_id", tca.EmployeeID)
        dp.Add("@username", tca.UserName)
        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          db.Execute(query, dp)
          Return GetPayrollStatus(PayPeriodEnding, tca)
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return Nothing
      End Try
    End Function

    Public Shared Function MarkEditsComplete(PayPeriodEnding As Date, tca As Timecard_Access, cs As ConnectionStringType) As PayrollStatus

    End Function

    Public Shared Function ApproveEdits(PayPeriodEnding As Date, tca As Timecard_Access, cs As ConnectionStringType) As PayrollStatus

    End Function

    Public Shared Function CancelApproval(PayPeriodEnding As Date, tca As Timecard_Access, cs As ConnectionStringType) As PayrollStatus

    End Function

    Public Shared Function PostToFinplus(PayPeriodEnding As Date, tca As Timecard_Access, cs As ConnectionStringType) As PayrollStatus

    End Function

    Public Shared Function CapturePayratesQuery(Target As DatabaseTarget, IncludeBenefits As Boolean) As String
      Dim db As String = ""
      Select Case Target
        Case DatabaseTarget.Finplus_Training
          db = "trnfinplus51"
        Case DatabaseTarget.Finplus_Production
          db = "finplus51"
      End Select
      Dim query As String = $"
      INSERT INTO Finplus_Payrates
                 (pay_period_ending
                 ,employee_id
                 ,rate_number
                 ,paycode
                 ,payrate
                 ,home_orgn
                 ,classify
                 ,pay_group
                 ,pay_hours
                 ,added_by)

      SELECT
        @pay_period_ending
        ,CAST(PR.empl_no AS INT) employee_id
        ,PR.rate_no
        ,PR.pay_cd
        ,PR.rate
        ,E.home_orgn
        ,PR.classify
        ,PR.group_x
        ,PR.pay_hours
        ,UPPER(@username)
      FROM CLAYBCCFINDB.finplus51.dbo.payrate PR
      INNER JOIN CLAYBCCFINDB.{db}.dbo.person P ON PR.empl_no = P.empl_no AND P.term_date IS NULL
      INNER JOIN CLAYBCCFINDB.{db}.dbo.employee E ON P.empl_no = E.empl_no
      WHERE
        status_x='A' "
      If Not IncludeBenefits Then
        query += "AND PR.group_x NOT IN ('S'); "
      Else
        query += "; "
      End If
      Return query
    End Function

    Public Shared Function CaptureLeaveBanksQuery(Target As DatabaseTarget) As String
      Dim db As String = ""
      Select Case Target
        Case DatabaseTarget.Finplus_Training
          db = "trnfinplus51"
        Case DatabaseTarget.Finplus_Production
          db = "finplus51"
      End Select
      Dim query As String = $"
	INSERT INTO [TimeStore].dbo.[Finplus_Payroll]
           ([pay_period_ending]
           ,[empl_no]
           ,[pay_freq]
           ,[card_requ]
           ,[sp1_amt]
           ,[sp1_cd]
           ,[sp2_amt]
           ,[sp2_cd]
           ,[sp3_amt]
           ,[sp3_cd]
           ,[chk_locn]
           ,[last_paid]
           ,[fed_exempt]
           ,[fed_marital]
           ,[fed_dep]
           ,[add_fed]
           ,[sta_exempt]
           ,[state_id]
           ,[pr_state]
           ,[sta_marital]
           ,[sta_dep]
           ,[add_state]
           ,[loc_exempt]
           ,[locl]
           ,[pr_local]
           ,[loc_marital]
           ,[loc_dep]
           ,[add_local]
           ,[fic_exempt]
           ,[earn_inc]
           ,[lv_date]
           ,[lv1_cd]
           ,[lv1_bal]
           ,[lv1_tak]
           ,[lv1_ear]
           ,[lv2_cd]
           ,[lv2_bal]
           ,[lv2_tak]
           ,[lv2_ear]
           ,[lv3_cd]
           ,[lv3_bal]
           ,[lv3_tak]
           ,[lv3_ear]
           ,[lv4_cd]
           ,[lv4_bal]
           ,[lv4_tak]
           ,[lv4_ear]
           ,[lv5_cd]
           ,[lv5_bal]
           ,[lv5_tak]
           ,[lv5_ear]
           ,[lv6_cd]
           ,[lv6_bal]
           ,[lv6_tak]
           ,[lv6_ear]
           ,[lv7_cd]
           ,[lv7_bal]
           ,[lv7_tak]
           ,[lv7_ear]
           ,[lv8_cd]
           ,[lv8_bal]
           ,[lv8_tak]
           ,[lv8_ear]
           ,[lv9_cd]
           ,[lv9_bal]
           ,[lv9_tak]
           ,[lv9_ear]
           ,[lv10_cd]
           ,[lv10_bal]
           ,[lv10_tak]
           ,[lv10_ear]
           ,[tearn_c]
           ,[tearn_m]
           ,[tearn_q]
           ,[tearn_y]
           ,[tearn_ft]
           ,[ftearn_c]
           ,[ftearn_m]
           ,[ftearn_q]
           ,[ftearn_y]
           ,[ftearn_ft]
           ,[fiearn_c]
           ,[fiearn_m]
           ,[fiearn_q]
           ,[fiearn_y]
           ,[fiearn_ft]
           ,[mdearn_c]
           ,[mdearn_m]
           ,[mdearn_q]
           ,[mdearn_y]
           ,[mdearn_ft]
           ,[stearn_c]
           ,[stearn_m]
           ,[stearn_q]
           ,[stearn_y]
           ,[stearn_ft]
           ,[s2earn_c]
           ,[s2earn_m]
           ,[l2earn_y]
           ,[s2earn_y]
           ,[s2earn_ft]
           ,[loearn_c]
           ,[loearn_m]
           ,[loearn_q]
           ,[loearn_y]
           ,[loearn_ft]
           ,[allow_c]
           ,[allow_m]
           ,[allow_q]
           ,[allow_y]
           ,[allow_ft]
           ,[nocash_c]
           ,[nocash_m]
           ,[nocash_q]
           ,[nocash_y]
           ,[nocash_ft]
           ,[fedtax_c]
           ,[fedtax_m]
           ,[fedtax_q]
           ,[fedtax_y]
           ,[fedtax_ft]
           ,[fictax_c]
           ,[fictax_m]
           ,[fictax_q]
           ,[fictax_y]
           ,[fictax_ft]
           ,[medtax_c]
           ,[medtax_m]
           ,[medtax_q]
           ,[medtax_y]
           ,[medtax_ft]
           ,[statax_c]
           ,[statax_m]
           ,[statax_q]
           ,[statax_y]
           ,[statax_ft]
           ,[st2tax_c]
           ,[st2tax_m]
           ,[lt2tax_y]
           ,[st2tax_y]
           ,[st2tax_ft]
           ,[loctax_c]
           ,[loctax_m]
           ,[loctax_q]
           ,[loctax_y]
           ,[loctax_ft]
           ,[eic_c]
           ,[eic_m]
           ,[eic_q]
           ,[eic_y]
           ,[eic_ft]
           ,[rfiearn_y]
           ,[rfictax_y]
           ,[rmdearn_y]
           ,[rmedtax_y]
           ,[flsa_cycle_y]
           ,[flsa_cycle_hrs]
           ,[flsa_hours]
           ,[flsa_amount]
           ,[rfiearn_c]
           ,[rfiearn_m]
           ,[rfiearn_q]
           ,[rfiearn_ft]
           ,[rfictax_c]
           ,[rfictax_m]
           ,[rfictax_q]
           ,[rfictax_ft]
           ,[rmdearn_c]
           ,[rmdearn_m]
           ,[rmdearn_q]
           ,[rmdearn_ft]
           ,[rmedtax_c]
           ,[rmedtax_m]
           ,[rmedtax_q]
           ,[rmedtax_ft]
           ,added_by)

SELECT 
  @pay_period_ending
  ,[empl_no]
      ,[pay_freq]
      ,[card_requ]
      ,[sp1_amt]
      ,[sp1_cd]
      ,[sp2_amt]
      ,[sp2_cd]
      ,[sp3_amt]
      ,[sp3_cd]
      ,[chk_locn]
      ,[last_paid]
      ,[fed_exempt]
      ,[fed_marital]
      ,[fed_dep]
      ,[add_fed]
      ,[sta_exempt]
      ,[state_id]
      ,[pr_state]
      ,[sta_marital]
      ,[sta_dep]
      ,[add_state]
      ,[loc_exempt]
      ,[locl]
      ,[pr_local]
      ,[loc_marital]
      ,[loc_dep]
      ,[add_local]
      ,[fic_exempt]
      ,[earn_inc]
      ,[lv_date]
      ,[lv1_cd]
      ,[lv1_bal]
      ,[lv1_tak]
      ,[lv1_ear]
      ,[lv2_cd]
      ,[lv2_bal]
      ,[lv2_tak]
      ,[lv2_ear]
      ,[lv3_cd]
      ,[lv3_bal]
      ,[lv3_tak]
      ,[lv3_ear]
      ,[lv4_cd]
      ,[lv4_bal]
      ,[lv4_tak]
      ,[lv4_ear]
      ,[lv5_cd]
      ,[lv5_bal]
      ,[lv5_tak]
      ,[lv5_ear]
      ,[lv6_cd]
      ,[lv6_bal]
      ,[lv6_tak]
      ,[lv6_ear]
      ,[lv7_cd]
      ,[lv7_bal]
      ,[lv7_tak]
      ,[lv7_ear]
      ,[lv8_cd]
      ,[lv8_bal]
      ,[lv8_tak]
      ,[lv8_ear]
      ,[lv9_cd]
      ,[lv9_bal]
      ,[lv9_tak]
      ,[lv9_ear]
      ,[lv10_cd]
      ,[lv10_bal]
      ,[lv10_tak]
      ,[lv10_ear]
      ,[tearn_c]
      ,[tearn_m]
      ,[tearn_q]
      ,[tearn_y]
      ,[tearn_ft]
      ,[ftearn_c]
      ,[ftearn_m]
      ,[ftearn_q]
      ,[ftearn_y]
      ,[ftearn_ft]
      ,[fiearn_c]
      ,[fiearn_m]
      ,[fiearn_q]
      ,[fiearn_y]
      ,[fiearn_ft]
      ,[mdearn_c]
      ,[mdearn_m]
      ,[mdearn_q]
      ,[mdearn_y]
      ,[mdearn_ft]
      ,[stearn_c]
      ,[stearn_m]
      ,[stearn_q]
      ,[stearn_y]
      ,[stearn_ft]
      ,[s2earn_c]
      ,[s2earn_m]
      ,[l2earn_y]
      ,[s2earn_y]
      ,[s2earn_ft]
      ,[loearn_c]
      ,[loearn_m]
      ,[loearn_q]
      ,[loearn_y]
      ,[loearn_ft]
      ,[allow_c]
      ,[allow_m]
      ,[allow_q]
      ,[allow_y]
      ,[allow_ft]
      ,[nocash_c]
      ,[nocash_m]
      ,[nocash_q]
      ,[nocash_y]
      ,[nocash_ft]
      ,[fedtax_c]
      ,[fedtax_m]
      ,[fedtax_q]
      ,[fedtax_y]
      ,[fedtax_ft]
      ,[fictax_c]
      ,[fictax_m]
      ,[fictax_q]
      ,[fictax_y]
      ,[fictax_ft]
      ,[medtax_c]
      ,[medtax_m]
      ,[medtax_q]
      ,[medtax_y]
      ,[medtax_ft]
      ,[statax_c]
      ,[statax_m]
      ,[statax_q]
      ,[statax_y]
      ,[statax_ft]
      ,[st2tax_c]
      ,[st2tax_m]
      ,[lt2tax_y]
      ,[st2tax_y]
      ,[st2tax_ft]
      ,[loctax_c]
      ,[loctax_m]
      ,[loctax_q]
      ,[loctax_y]
      ,[loctax_ft]
      ,[eic_c]
      ,[eic_m]
      ,[eic_q]
      ,[eic_y]
      ,[eic_ft]
      ,[rfiearn_y]
      ,[rfictax_y]
      ,[rmdearn_y]
      ,[rmedtax_y]
      ,[flsa_cycle_y]
      ,[flsa_cycle_hrs]
      ,[flsa_hours]
      ,[flsa_amount]
      ,[rfiearn_c]
      ,[rfiearn_m]
      ,[rfiearn_q]
      ,[rfiearn_ft]
      ,[rfictax_c]
      ,[rfictax_m]
      ,[rfictax_q]
      ,[rfictax_ft]
      ,[rmdearn_c]
      ,[rmdearn_m]
      ,[rmdearn_q]
      ,[rmdearn_ft]
      ,[rmedtax_c]
      ,[rmedtax_m]
      ,[rmedtax_q]
      ,[rmedtax_ft]
      ,UPPER(@username)
FROM CLAYBCCFINDB.{db}.[dbo].[payroll]
"
      Return query
    End Function

    Public Shared Function SetupPayrollChanges() As String

      Dim query As String = $"
        INSERT INTO TimeStore.[dbo].[Payroll_Changes]
        (
          [employee_id]
          ,[pay_period_ending]
          ,[paycode]
          ,[payrate]
          ,[project_code]
          ,[hours]
          ,[amount]
          ,[orgn]
          ,[classify]
        )
          SELECT
            [employee_id]
            ,[pay_period_ending]
            ,[paycode]
            ,[payrate]
            ,[project_code]
            ,[hours]
            ,[amount]
            ,[orgn]
            ,[classify]
          FROM
            Saved_Time
          WHERE
            pay_period_ending = @pay_period_ending;


        INSERT INTO TimeStore.[dbo].[Payroll_Changes]
        (
          [employee_id]
          ,[pay_period_ending]
          ,[paycode]
          ,[payrate]
          ,[project_code]
          ,[hours]
          ,[amount]
          ,[orgn]
          ,[classify]
        )
        SELECT
          FP.employee_id
          ,FP.pay_period_ending
          ,FP.paycode
          ,FP.payrate
          ,'' project_code
          ,FP.pay_hours hours
          ,FP.pay_hours * FP.payrate amount
          ,FP.home_orgn orgn
          ,FP.classify
        FROM Finplus_Payrates FP
        LEFT OUTER JOIN Payroll_Changes PC ON PC.employee_id = FP.employee_id
          AND PC.paycode = FP.paycode
          AND PC.pay_period_ending = @pay_period_ending
        WHERE
          FP.pay_period_ending=@pay_period_ending
          AND PC.employee_id IS NULL
          AND FP.paycode NOT IN ('002', '001'); "
      Return query
    End Function

    Public Shared Function CreateNewPayrollStatus() As String
      Dim query As String = "
        INSERT INTO Payroll_Status(
          pay_period_ending
          ,started_on
          ,started_by
          ,target_db
          ,include_benefits)
        VALUES (
          @pay_period_ending
          ,GETDATE()
          ,UPPER(@username)
          ,@target_db
          ,@include_benefits);"
      Return query
    End Function


  End Class
End Namespace