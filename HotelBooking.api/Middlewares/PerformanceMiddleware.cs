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
        // 1. Bấm giờ bắt đầu
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 2. Cho code chạy tiếp (Controller, Service, DB...)
        await _next(context);

        // 3. Code chạy xong -> Dừng đồng hồ
        sw.Stop();

        // 4. Ghi log
        // Chỉ log những cái chậm > 500ms (hoặc log hết nếu muốn)
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