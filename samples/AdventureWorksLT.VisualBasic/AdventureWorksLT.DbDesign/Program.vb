Module Program
    Function Main(args As String()) As Integer
#If DbDesign Then
        Return DevZest.Data.DbDesign.Run(args)
#Else
        Return 0
#End If
    End Function
End Module
