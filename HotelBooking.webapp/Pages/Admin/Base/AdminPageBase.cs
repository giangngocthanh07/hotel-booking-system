using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using HotelBooking.webapp.Services.Interface;

namespace HotelBooking.webapp.Pages.Admin.Base
{
    /// <summary>
    /// Generic Base Class for Admin pages.
    /// Automatically handles: Token management, LocalStorage, Navigation, and Lifecycle coordination.
    /// 
    /// Usage:
    /// - Management pages: @inherits AdminPageBase (for backward compatibility)
    /// - Request pages: @inherits AdminPageBase<IRequestService>
    /// - Other pages: @inherits AdminPageBase<IYourService> where IYourService : ITokenService
    /// </summary>
    /// <typeparam name="TService">Service type that must implement ITokenService</typeparam>
    public abstract class AdminPageBase<TService> : ComponentBase, IDisposable
        where TService : class, ITokenService
    {
        // --- 1. SHARED DEPENDENCY INJECTION ---
        [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] protected TService Service { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        // --- 2. SHARED STATE ---
        protected bool IsJsReady { get; private set; } = false;
        protected bool Disposed { get; private set; } = false;

        // --- 3. ABSTRACT METHODS (Must be implemented by derived classes) ---
        /// <summary>
        /// Data loading method - automatically invoked by the Base class 
        /// once the environment is ready (e.g., token is retrieved and set).
        /// </summary>
        protected abstract Task LoadDataAsync();

        // --- 4. LIFECYCLE COORDINATION ---
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    // 1. Retrieve Access Token from LocalStorage
                    var token = await LocalStorage.GetItemAsync<string>("accessToken");

                    if (string.IsNullOrEmpty(token))
                    {
                        // Redirect to login if unauthorized
                        Navigation.NavigateTo("/login", true);
                        return;
                    }

                    // 2. Inject token into the service (Generic - works with any ITokenService)
                    Service.SetToken(token);

                    // 3. Mark state as ready
                    IsJsReady = true;

                    // 4. Trigger child data loading logic
                    await LoadDataAsync();

                    // 5. Re-render UI with loaded data
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AdminPageBase<{typeof(TService).Name}> Error] {ex}");
                }
            }
        }

        /// <summary>
        /// UI Helper: Determines if the component is ready for rendering.
        /// </summary>
        protected bool ShouldRenderUI() => IsJsReady;

        // --- 5. CLEANUP ---
        public virtual void Dispose()
        {
            Disposed = true;
        }
    }

    /// <summary>
    /// Backward Compatible Base Class for Management pages.
    /// Existing pages using @inherits AdminPageBase will continue to function normally.
    /// </summary>
    public abstract class AdminPageBase : AdminPageBase<IManagementService>
    {
        // No additional logic needed - fully inherits from the generic base class
    }
}