using Microsoft.AspNetCore.Http;

namespace WebAPI.Exceptions;

public static class ExceptionStatusMapper
{
    public static int GetStatusCode(Exception exception) => exception switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static string GetTitle(Exception exception) => exception switch
    {
        ArgumentException => "Yêu cầu không hợp lệ.",
        KeyNotFoundException => "Không tìm thấy tài nguyên.",
        ConflictException => "Yêu cầu gây xung đột.",
        _ => "Đã xảy ra lỗi máy chủ."
    };
}
