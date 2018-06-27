ROBOCOPY %~dp0..\src\Data %~dp0DevZest.Data /MIR /XD .vs obj bin
ROBOCOPY %~dp0..\src\Data.Analyzers %~dp0DevZest.Data.Analyzers /MIR /XD .vs obj bin