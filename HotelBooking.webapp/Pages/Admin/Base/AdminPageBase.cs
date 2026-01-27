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
                // A. Lấy Token từ LocalStorage (Thủ công nhưng chắc ăn)
                var token = await LocalStorage.GetItemAsync<string>("accessToken");

                // B. Nếu có token -> Bơm vào Service (Scoped)
                if (!string.IsNullOrEmpty(token))
                {
                    Service.SetToken(token);
                }
                else
                {
                    // Nếu không có token -> Đá về Login (Tùy chọn)
                    // Navigation.NavigateTo("/login");
                    // return;
                }

                // C. Đánh dấu đã sẵn sàng
                IsJsReady = true;

                // D. Gọi hàm LoadData của con
                await LoadDataAsync();

                // E. Vẽ lại giao diện
                StateHasChanged();
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