Imports System.Globalization

Public NotInheritable Class PercentageConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

        If targetType Is Nothing OrElse targetType <> GetType(String) Then
            Return DependencyProperty.UnsetValue
        End If

        If value Is Nothing Then
            Return Nothing
        End If

        If TypeOf value Is System.Nullable(Of Decimal) Then
            Return String.Format("{0:P}", value)
        End If

        Return DependencyProperty.UnsetValue
    End Function

    Private Function TryConvertBack(value As String, culture As CultureInfo, ByRef result As Decimal?) As Boolean
        result = New Decimal?()
        If String.IsNullOrEmpty(value) Then
            Return True
        End If

        Dim decimalResult As Decimal = 0
        If TryConvertBack(value, culture, decimalResult) Then
            result = decimalResult
            Return True
        End If
        Return False
    End Function

    Private Function TryConvertBack(value As String, culture As CultureInfo, ByRef result As Decimal) As Boolean
        result = Nothing
        If String.IsNullOrEmpty(value) Then
            Return False
        End If

        Try
            Dim text = value.Trim()
            If Not culture.IsNeutralCulture AndAlso text.Length > 0 AndAlso culture.NumberFormat IsNot Nothing Then
                Select Case culture.NumberFormat.PercentPositivePattern
                    Case 0, 1
                        If text.Length - 1 = text.LastIndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase) Then
                            text = text.Substring(0, text.Length - 1)
                        End If
                        Exit Select
                    Case 2
                        If text.IndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase) = 0 Then
                            text = text.Substring(1)
                        End If
                        Exit Select
                End Select
            End If
            result = System.Convert.ToDecimal(text, culture)
            Return True
        Catch ex As ArgumentOutOfRangeException
        Catch ex As ArgumentNullException
        Catch ex As FormatException
        Catch ex As OverflowException
        End Try
        Return False
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Dim text As String
        If value Is Nothing Then
            text = Nothing
        ElseIf TypeOf value Is String Then
            text = CType(value, String)
        Else
            Return DependencyProperty.UnsetValue
        End If

        If targetType Is GetType(Decimal?) Then
            Dim result As Decimal? = Nothing
            Return If(TryConvertBack(text, culture, result), result / 100, DependencyProperty.UnsetValue)
        Else
            Return DependencyProperty.UnsetValue
        End If
    End Function
End Class