Imports Beckhoff.TwinCAT.HMI.Automation

Module Module1
    ReadOnly Property Name As String
        Get
            Return GetType(Module1).Assembly.GetName().Name
        End Get
    End Property

    Sub Main()
        Dim vsHmi = Nothing
        Dim hmiPrj = Nothing
        Utilities.Init(Name, vsHmi, hmiPrj)
        hmiPrj?.Build()
        hmiPrj?.Build("Debug", True)
    End Sub

End Module
