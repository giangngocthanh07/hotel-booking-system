using Blazored.LocalStorage;
using HotelBooking.webapp.Pages.User.Owner.Steps;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Forms;
using TagLib.Riff;
using static HotelBooking.webapp.Pages.User.Owner.Steps.StepPolicies;


namespace HotelBooking.webapp.Services
{
    public class HotelFormState
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalStorageService _localStorage;
        private HttpClient _httpClient;
        public event Action OnChange;
        public HotelFormState(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
        {
            _httpClientFactory = httpClientFactory;
            _localStorage = localStorage;
            _httpClient = _httpClientFactory.CreateClient("HotelBookingAPI");
        }

        public StepBasicInfo.HotelVM BasicInfo { get; set; } = new();
        public List<StepAmenities.AmenityVM> Amenities { get; set; } = new();
        public StepImages.HotelImagesVM HotelImages { get; set; } = new();
        public List<PolicyTypeGroupVM> PolicyGroups { get; set; } = new();
        public bool IsLoading { get; private set; }


        private void NotifyStateChanged() => OnChange?.Invoke();


        // Các phương thức xử lý trạng thái form
        // Lấy danh sách toàn bộ tiện nghi bằng cách gọi API
        public async Task LoadCitiesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StepBasicInfo.CityVM>>>("hotel/get-all-cities");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                BasicInfo.Cities = result.Content ?? new List<StepBasicInfo.CityVM>();
            }
        }

        #region LOAD AMENITIES
        public async Task LoadAmenitiesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StepAmenities.AmenityVM>>>("hotel/get-all-amenities");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                Amenities = result.Content ?? new List<StepAmenities.AmenityVM>();
            }
        }

        public List<int> GetSelectedAmenityIds()
        {
            return Amenities
                .Where(a => a.IsSelected)  // Lọc ra những cái được tick
                .Select(a => a.Id)         // Chỉ lấy Id
                .ToList();
        }
        #endregion

        public async Task<List<PolicyTypeVM>> LoadPolicyTypesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PolicyTypeVM>>>("hotel/get-all-policy-types");


            if (response != null && response.StatusCode == "Success" && response.Content != null)
            {
                return response.Content ?? new List<PolicyTypeVM>();
            }
            else
            {
                return new List<PolicyTypeVM>();
            }
        }

        public async Task<List<PolicyVM>> LoadPoliciesByTypeIdAsync(int policyTypeId)
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<PolicyVM>>>($"hotel/get-all-policy-by-type/{policyTypeId}");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                return result.Content ?? new List<PolicyVM>();
            }
            else
            {
                return new List<PolicyVM>();
            }
        }

        #region LOAD POLICIES
        public async Task LoadPolicies()
        {
            IsLoading = true;
            NotifyStateChanged(); // Báo UI biết là đang "Loading..."

            // 1. Lấy tất cả loại Policy
            var allPolicyTypes = await LoadPolicyTypesAsync();
            if (allPolicyTypes == null || !allPolicyTypes.Any())
            {
                IsLoading = false;
                NotifyStateChanged();
                return;
            }

            // 2. Tạo một danh sách "Task" để gọi API song song
            var policyLoadTasks = allPolicyTypes
                .Select(type => LoadPoliciesByTypeIdAsync(type.Id))
                .ToList();

            // 3. Chạy tất cả các Task cùng lúc và chờ hoàn thành
            var policyResults = await Task.WhenAll(policyLoadTasks);

            // Dọn sạch list cũ trước khi thêm
            PolicyGroups.Clear();

            // 4. Kết hợp các danh sách lại
            for (int i = 0; i < allPolicyTypes.Count; i++)
            {
                PolicyGroups.Add(new PolicyTypeGroupVM
                {
                    PolicyType = allPolicyTypes[i],
                    AvailablePolicies = policyResults[i],
                    SelectedPolicyId = 0
                });
            }

            IsLoading = false;
            NotifyStateChanged(); // BÁO UI CẬP NHẬT LẦN CUỐI
        }

        public List<int> GetSelectedPolicyIds()
        {
            return PolicyGroups
                .Where(g => g.SelectedPolicyId > 0) // Lọc những group có chọn policy
                .Select(g => g.SelectedPolicyId)    // Lấy SelectedPolicyId 
                .ToList();
        }
        #endregion

        #region SUBMIT
        public async Task<bool> Submit()
        {
            // Lấy token từ LocalStorage (lưu sau khi login thành công)
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            if (string.IsNullOrEmpty(token))
            {

                return false;
            }

            // Gắn Authorization Header: Bearer <token>


            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // ----- HẾT -----

            // Gửi dữ liệu lên server hoặc xử lý theo yêu cầu
            var _hotelForm = new HotelPostFormVM();
            _hotelForm.Name = BasicInfo.Name;
            _hotelForm.Address = BasicInfo.Address;
            _hotelForm.CityId = BasicInfo.CityId;
            _hotelForm.Description = BasicInfo.Description;
            _hotelForm.CoverFile = BasicInfo.CoverFile;
            _hotelForm.AmenityIds = GetSelectedAmenityIds();
            _hotelForm.PolicyIds = GetSelectedPolicyIds();
            _hotelForm.MainFile = HotelImages.MainFile;
            _hotelForm.SubFiles = HotelImages.SubFiles;

            // Dùng MultipartFormDataContent để gửi files
            using (var content = new MultipartFormDataContent())
            {
                // Thêm các phần khác của form vào content ở đây

                // Thêm các trường dữ liệu đơn giản
                content.Add(new StringContent(_hotelForm.Name), "Name");
                content.Add(new StringContent(_hotelForm.Address), "Address");
                content.Add(new StringContent(_hotelForm.CityId.ToString()), "CityId");
                content.Add(new StringContent(_hotelForm.Description), "Description");

                // Thêm List AmenityIds
                foreach (var amenityId in _hotelForm.AmenityIds)
                {
                    content.Add(new StringContent(amenityId.ToString()), "AmenityIds");
                }

                // Thêm List PolicyIds
                foreach (var policyId in _hotelForm.PolicyIds)
                {
                    content.Add(new StringContent(policyId.ToString()), "PolicyIds");
                }

                // Thêm CoverFile
                if (_hotelForm.CoverFile != null)
                {
                    _hotelForm.CoverFile.Content.Position = 0; // Đảm bảo stream ở vị trí bắt đầu
                    var coverFileContent = new StreamContent(_hotelForm.CoverFile.Content);
                    coverFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_hotelForm.CoverFile.ContentType);
                    content.Add(coverFileContent, "CoverFile", _hotelForm.CoverFile.FileName);
                }

                // Thêm MainFile
                if (_hotelForm.MainFile != null)
                {
                    _hotelForm.MainFile.Content.Position = 0; // Đảm bảo stream ở vị trí bắt đầu
                    var mainFileContent = new StreamContent(_hotelForm.MainFile.Content);
                    mainFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_hotelForm.MainFile.ContentType);
                    content.Add(mainFileContent, "MainFile", _hotelForm.MainFile.FileName);
                }

                // Thêm SubFiles
                foreach (var subFile in _hotelForm.SubFiles)
                {
                    subFile.Content.Position = 0; // Đảm bảo stream ở vị trí bắt đầu
                    var subFileContent = new StreamContent(subFile.Content);
                    subFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(subFile.ContentType);
                    content.Add(subFileContent, "SubFiles", subFile.FileName);
                }

                try

                {
                    var response = await _httpClient.PostAsync("hotel/post-new-hotel", content);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu cần
                    return false;
                }

            }
            ;


        }
        #endregion

        #region RESET
        public void Reset()
        {
            BasicInfo = new();

            // Reset trạng thái chọn của các tiện nghi (Amenities)
            foreach (var amenity in Amenities)
            {
                amenity.IsSelected = false;
            }

            PolicyGroups = new();
            HotelImages = new();

        }

        #endregion

        #region VALIDATE
        public bool ValidateBasicInfo() =>
            !string.IsNullOrWhiteSpace(BasicInfo.Name) &&
            BasicInfo.CityId > 0 &&
            !string.IsNullOrWhiteSpace(BasicInfo.Address) &&
            BasicInfo.CoverFile != null;

        public bool ValidateAmenities() =>
            Amenities.Any(a => a.IsSelected);

        // Trả về 'true' CHỈ KHI tất cả các group
        // đều có SelectedPolicyId > 0
        public bool ValidatePolicy()
        {
            if (PolicyGroups.Count == 0)
            {
                return false; // Chưa tải xong, hoặc không có policy
            }

            // Kiểm tra xem TẤT CẢ các group có SelectedPolicyId > 0 không
            return PolicyGroups.All(group => group.SelectedPolicyId > 0);
        }

        public bool ValidateImages() =>
            HotelImages.MainFile != null && HotelImages.SubFiles.Count == 4;

        // Validate toàn bộ form
        public bool ValidateAll() =>
            ValidateBasicInfo() &&
            ValidateAmenities() &&
            // ValidatePolicy() &&
            ValidateImages();
    }

    #endregion
}