using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using HotelBooking.webapp.Services;
using HotelBooking.webapp.Services.Interface;

namespace HotelBooking.webapp.Pages.Admin.Base
{
    /// <summary>
    /// Generic Base Class cho các trang Admin.
    /// Tự động xử lý: Token, LocalStorage, Navigation, Lifecycle.
    /// 
    /// Usage:
    /// - Management pages: @inherits AdminPageBase (backward compatible)
    /// - Request pages: @inherits AdminPageBase&lt;IRequestService&gt;
    /// - Other pages: @inherits AdminPageBase&lt;IYourService&gt; where IYourService : ITokenService
    /// </summary>
    /// <typeparam name="TService">Service type phải implement ITokenService</typeparam>
    public abstract class AdminPageBase<TService> : ComponentBase, IDisposable 
        where TService : class, ITokenService
    {
        // --- 1. INJECT CÁC SERVICE DÙNG CHUNG ---
        [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] protected TService Service { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        // --- 2. STATE DÙNG CHUNG ---
        protected bool IsJsReady { get; private set; } = false;
        protected bool Disposed { get; private set; } = false;

        // --- 3. ABSTRACT METHODS (Con bắt buộc phải làm) ---
        /// <summary>
        /// Hàm load data - được Cha tự động gọi khi mọi thứ đã sẵn sàng (token đã set).
        /// </summary>
        protected abstract Task LoadDataAsync();

        // --- 4. LIFECYCLE ---
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    // 1. Lấy Token từ LocalStorage (Thủ công nhưng chắc ăn)
                    var token = await LocalStorage.GetItemAsync<string>("accessToken");

                    if (string.IsNullOrEmpty(token))
                    {
                        Navigation.NavigateTo("/login", true);
                        return;
                    }

                    // 2. Bơm vào service (Generic - hoạt động với mọi service có ITokenService)
                    Service.SetToken(token);

                    // 3. Đánh dấu sẵn sàng
                    IsJsReady = true;

                    // 4. Gọi hàm LoadData của con
                    await LoadDataAsync();

                    // 5. Vẽ lại giao diện
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AdminPageBase<{typeof(TService).Name}> Error] {ex}");
                }
            }
        }

        /// <summary>
        /// Hàm hỗ trợ UI: Chỉ vẽ khi đã sẵn sàng
        /// </summary>
        protected bool ShouldRenderUI() => IsJsReady;

        // --- 5. DISPOSE (Dọn dẹp) ---
        public virtual void Dispose()
        {
            Disposed = true;
        }
    }

    /// <summary>
    /// Backward Compatible Base Class cho các trang Management.
    /// Các page cũ dùng @inherits AdminPageBase vẫn hoạt động bình thường.
    /// </summary>
    public abstract class AdminPageBase : AdminPageBase<IManagementService>
    {
        // Không cần thêm gì - kế thừa hoàn toàn từ generic class
    }
}