Public Class TryFilterTarget

    Private _query As OpenCover.Samples.Framework.ITestExceptionQuery

    Public Sub New(ByVal query As OpenCover.Samples.Framework.ITestExceptionQuery)
        _query = query
    End Sub

    Public Sub TryFilter(ByVal val As Integer)
        Try
            _query.ThrowException()
        Catch ex As Exception When val = 1
            _query.InFilter()
            Throw ex
        End Try
    End Sub

End Class
