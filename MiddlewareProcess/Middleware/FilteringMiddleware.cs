using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

public class RequestFilteringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _maskSensitiveData;
    private readonly bool _removeSensitiveFields;

    public RequestFilteringMiddleware(RequestDelegate next, bool maskSensitiveData = true, bool removeSensitiveFields = false)
    {
        _next = next;
        _maskSensitiveData = maskSensitiveData;
        _removeSensitiveFields = removeSensitiveFields;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post)
        {
            context.Request.EnableBuffering();

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    using (var jsonDocument = JsonDocument.Parse(body))
                    {
                        var rootElement = jsonDocument.RootElement.Clone();
                        var filteredJson = ProcessJsonElement(rootElement);

                        var modifiedBody = JsonSerializer.Serialize(filteredJson);
                        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(modifiedBody));
                        context.Request.Body = memoryStream;
                        context.Request.Body.Position = 0;
                    }
                }
            }
        }
        await _next(context);
    }

    private object ProcessJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var processedObject = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    if (SensitiveData.Keywords.Contains(property.Name))
                    {
                        if (_maskSensitiveData)
                        {
                            processedObject[property.Name] = "*****";
                        }
                        else if (!_removeSensitiveFields)
                        {
                            processedObject[property.Name] = null;
                        }
                    }
                    else
                    {
                        processedObject[property.Name] = ProcessJsonElement(property.Value);
                    }
                }
                return processedObject;

            case JsonValueKind.Array:
                var processedArray = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    processedArray.Add(ProcessJsonElement(item));
                }
                return processedArray;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                return element.GetDouble();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();

            case JsonValueKind.Null:
            default:
                return null;
        }
    }
}