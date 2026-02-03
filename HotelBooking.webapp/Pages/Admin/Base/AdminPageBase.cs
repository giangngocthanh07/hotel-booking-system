using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using HotelBooking.webapp.Services; // Nhớ using namespace Service của bạn

namespace HotelBooking.webapp.Pages.Admin.Base
{
    public abstract class AdminPageBase : ComponentBase, IDisposable
    {
        // --- 1. INJECT CÁC SERVICE DÙNG CHUNG ---
        [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] protected IManagementService Service { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        // --- 2. STATE DÙNG CHUNG ---
        protected bool IsJsReady { get; private set; } = false;
        protected bool Disposed { get; private set; } = false;

        // --- 3. ABSTRACT METHODS (Con bắt buộc phải làm) ---
        // Hàm này sẽ được Cha tự động gọi khi mọi thứ đã sẵn sàng
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

                    // 2. Bơm vào service
                    Service.SetToken(token);

                    // 3. Đánh dấu sẵn sàng
                    IsJsReady = true;

                    // D. Gọi hàm LoadData của con
                    await LoadDataAsync();

                    // E. Vẽ lại giao diện
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AdminPageBase Error] {ex}");
                    // Có thể hiện thông báo lỗi lên UI nếu muốn
                }
            }
        }

        // Hàm hỗ trợ UI: Chỉ vẽ khi đã sẵn sàng
        protected bool ShouldRenderUI() => IsJsReady;

        // --- 5. DISPOSE (Dọn dẹp) ---
        public virtual void Dispose()
        {
            Disposed = true;
            // Console.WriteLine($"[DISPOSE] {GetType().Name} destroyed.");
        }
    }
}