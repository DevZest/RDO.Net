Imports DevZest.Data.DbInit

Module Program
    Function Main(args As String()) As Integer
        Return args.RunDbInit()
    End Function
End Module
