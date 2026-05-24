using System.Security.Cryptography;
using System.Text;

namespace LineNoteBot.Middlewares;

public class LineSignatureMiddleware(RequestDelegate next, IConfiguration config)
{
    private const string SignatureHeader = "X-Line-Signature";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/linewebhook"))
        {
            await next(context);
            return;
        }

        context.Request.EnableBuffering();

        if (!context.Request.Headers.TryGetValue(SignatureHeader, out var signature))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-Line-Signature");
            return;
        }

        var body = await ReadBodyAsync(context.Request);

        if (!IsValidSignature(body, signature!))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid signature");
            return;
        }

        context.Request.Body.Position = 0;
        await next(context);
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private bool IsValidSignature(string body, string signature)
    {
        var secret = config["Line:ChannelSecret"]!;
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(body);

        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(data));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(signature)
        );
    }
}