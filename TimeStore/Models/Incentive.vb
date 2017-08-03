Namespace Models

    Public Class Incentive
        ReadOnly Property Incentive_Type_Name As String
            Get
                Select Case Incentive_Type
                    Case 1
                        Return "Public Safety"
                    Case 2
                        Return "Public Works"
                    Case Else
                        Return ""
                End Select
            End Get
        End Property
        Property Incentive_Type As Integer
        Property Incentive_Name As String
        Property Incentive_Abrv As String
        Property Incentive_Amount As Double
        Property Start_Date As Date = "1/1/2000"
        Property End_Date As Date = "1/1/2100"
    End Class

  'Public Class Telestaff_Specialty
  '    Property Specialty_Name As String
  '    Property Specialty_Abrv As String
  '    Property Specialty_Amount As Double = 0
  'End Class

End Namespace