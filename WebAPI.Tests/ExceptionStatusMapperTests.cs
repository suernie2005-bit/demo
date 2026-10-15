using WebAPI.Exceptions;

namespace WebAPI.Tests;

public class ExceptionStatusMapperTests
{
    [Fact]
    public void ArgumentException_maps_to_bad_request()
    {
        Assert.Equal(400, ExceptionStatusMapper.GetStatusCode(new ArgumentException()));
    }

    [Fact]
    public void KeyNotFoundException_maps_to_not_found()
    {
        Assert.Equal(404, ExceptionStatusMapper.GetStatusCode(new KeyNotFoundException()));
    }

    [Fact]
    public void ConflictException_maps_to_conflict()
    {
        Assert.Equal(409, ExceptionStatusMapper.GetStatusCode(new ConflictException("duplicate")));
    }

    [Fact]
    public void Unexpected_exception_maps_to_internal_server_error()
    {
        Assert.Equal(500, ExceptionStatusMapper.GetStatusCode(new Exception()));
    }
}
