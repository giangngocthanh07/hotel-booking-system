using MudBlazor;

public static class SnackbarHelper
{
    public static void Show(ISnackbar snackbar, string status, string? message)
    {
        switch (status)
        {
            case StatusCodeResponse.Success:
                snackbar.Add(message ?? "Operation successful", Severity.Success);
                break;

            case StatusCodeResponse.Conflict:
                snackbar.Add(message ?? "Conflict occurred", Severity.Warning);
                break;

            case StatusCodeResponse.NotFound:
                snackbar.Add(message ?? "Not found", Severity.Warning);
                break;

            case StatusCodeResponse.BadRequest:
                snackbar.Add(message ?? "Bad request", Severity.Error);
                break;

            case StatusCodeResponse.Error:
                snackbar.Add(message ?? "Server error", Severity.Error);
                break;

            default:
                snackbar.Add(message ?? "Unknown status", Severity.Info);
                break;
        }
    }
}
