#If DbInit Then
Imports DevZest.Data.DbInit
#End If

Module Program
    Function Main(args As String()) As Integer
#If DbInit Then
        Return args.RunDbInit()
#Else
        Return 0
#End If
    End Function
End Module
