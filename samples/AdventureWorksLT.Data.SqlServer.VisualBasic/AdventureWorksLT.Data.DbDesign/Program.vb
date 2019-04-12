#If DbDesign Then
Imports DevZest.Data.DbDesign
#End If

Module Program
    Function Main(args As String()) As Integer
#If DbDesign Then
        Return args.RunDbDesign()
#Else
        Return 0
#End If
    End Function
End Module
