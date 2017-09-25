Namespace Models

  Public Class TelestaffTimeData
    Property EmployeeId As Integer ' The employeeID from Finplus, seems to be standardized across all databases.
    Property WorkDate As Date ' The date this work was performed
    Property StartTime As Date ' the time this particular chunk of time started
    Property EndTime As Date ' the time this chunk ended.
    Property DisasterRule As Integer = 0
    Property WorkHours As Double ' The number of hours worked on this date
    Property PayRate As Double ' The hourly rate paid for these hours
    Property WorkCode As String ' The workcode that defines the kind of work this is, ie: straight pay, OT, vacation, holiday, etc.
    Property Comment As String = ""
    Property Job As String = ""
    Property FLSAHoursRequirement As Double ' This field will contain the number of hours needed by the Job above to qualify for OT.
    Property WorkType As String = ""
    Property WorkTypeAbrv As String = ""
    Property ConstantShift As Boolean = False
    Property ShiftType As String = ""
    Property ShiftDuration As Double = 0 ' The number of hours for this type of employee.  either 80 for dispatch/office or 106 (field)
    Property ProfileType As TelestaffProfileType ' dispatch / field / office
    Property StratName As String = ""
    Property ProfileID As Integer ' the specific profile ID in telestaff.
    'Property ProfileDesc As String ' Their profile's title string.
    Property RequiresApproval As Boolean = False ' Indicates if the hours have been approved or not.
    Property IsPaidTime As Boolean = False
    Property IsWorkingTime As Boolean = False
    Property CountsTowardsOvertime As Boolean = False ' This is whether or not the hours count towards your total hours for overtime.
    Property Specialties As String = ""
    Property ProfileStartDate As Date
    Property ProfileEndDate As Date
    Public Function Clone() As TelestaffTimeData
      Dim x As New TelestaffTimeData
      x.Comment = Comment
      x.ProfileID = ProfileID
      'x.ProfileDesc = ProfileDesc
      x.RequiresApproval = RequiresApproval
      'x.ShiftDuration = ShiftDuration
      x.ConstantShift = ConstantShift
      x.ShiftType = ShiftType
      x.ProfileType = ProfileType
      x.WorkTypeAbrv = WorkTypeAbrv
      x.WorkType = WorkType
      x.WorkHours = WorkHours
      x.WorkDate = WorkDate
      x.WorkCode = WorkCode
      x.StartTime = StartTime
      x.StratName = StratName
      x.PayRate = PayRate
      x.Specialties = Specialties
      x.Job = Job
      x.FLSAHoursRequirement = FLSAHoursRequirement
      x.EndTime = EndTime
      x.EmployeeId = EmployeeId
      x.IsPaidTime = IsPaidTime
      x.IsWorkingTime = IsWorkingTime
      x.CountsTowardsOvertime = CountsTowardsOvertime
      x.ProfileStartDate = ProfileStartDate
      x.ProfileEndDate = ProfileEndDate
      Return x
    End Function
  End Class




End Namespace
