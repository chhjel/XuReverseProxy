using Microsoft.AspNetCore.Mvc.ModelBinding;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.Common;

public interface IGenericResult
{
    bool Success { get; set; }
    string? Message { get; set; }
}

[GenerateFrontendModel]
public class GenericResult : IGenericResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static GenericResult CreateSuccess(string? message = null) => new() { Success = true, Message = message };
    public static GenericResult CreateError(string error) => new() { Message = error };
    public static GenericResult CreateError(ModelStateDictionary modelState)
    {
        var error = string.Join(". ", modelState
            .Select(x => $"{x.Key}: {string.Join(", ", x.Value?.Errors?.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>())}"));
        return new() { Message = error };
    }

    public static GenericResultData<TData> CreateSuccess<TData>(TData data, string? message = null) => new() { Data = data, Success = true, Message = message };
    public static GenericResultData<TData> CreateError<TData>(string error) => new() { Message = error };
    public static GenericResultData<TData> CreateError<TData>(ModelStateDictionary modelState)
    {
        var error = string.Join(". ", modelState
            .Select(x => $"{x.Key}: {string.Join(", ", x.Value?.Errors?.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>())}"));
        return new() { Message = error };
    }
}

[GenerateFrontendModel]
public class GenericResultData<TData> : GenericResult
{
    public TData? Data { get; set; }
}
