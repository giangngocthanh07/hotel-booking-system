public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1. Start timing
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 2. Let the code run (Controller, Service, DB...)
        await _next(context);

        // 3. Stop timing
        sw.Stop();

        // 4. Log
        // Only log slow requests (> 500ms)
        if (sw.ElapsedMilliseconds > 500) 
        {
            _logger.LogWarning($"⚠️ SLOW API: {context.Request.Method} {context.Request.Path} took {sw.ElapsedMilliseconds}ms");
        }
        else
        {
            _logger.LogInformation($"✅ API Speed: {context.Request.Path} took {sw.ElapsedMilliseconds}ms");
        }
    }
}