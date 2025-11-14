using Blazored.LocalStorage;
using HotelBooking.webapp.Pages.User.Owner.Steps;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Forms;


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

        // Các lựa chọn chính sách// Các ID vẫn giữ lại vì FE vẫn cần bind
        // public int SelectedCheckInId
        // {
        //     get => _selectedCheckInId;
        //     set
        //     {
        //         if (_selectedCheckInId != value)
        //         {
        //             _selectedCheckInId = value;
        //             NotifyStateChanged();
        //         }
        //     }
        // }
        // private int _selectedCheckInId;

        // public int SelectedCheckOutId
        // {
        //     get => _selectedCheckOutId;
        //     set
        //     {
        //         if (_selectedCheckOutId != value)
        //         {
        //             _selectedCheckOutId = value;
        //             NotifyStateChanged();
        //         }
        //     }
        // }
        // private int _selectedCheckOutId;

        // public int SelectedCancellationId
        // {
        //     get => _selectedCancellationId;
        //     set
        //     {
        //         if (_selectedCancellationId != value)
        //         {
        //             _selectedCancellationId = value;
        //             NotifyStateChanged();
        //         }
        //     }
        // }
        // private int _selectedCancellationId;


        private void NotifyStateChanged() => OnChange?.Invoke();

        // public List<int> SelectedPolicyIds
        // {
        //     get
        //     {
        //         var ids = new List<int>();
        //         if (SelectedCheckInId > 0) ids.Add(SelectedCheckInId);
        //         if (SelectedCheckOutId > 0) ids.Add(SelectedCheckOutId);
        //         if (SelectedCancellationId > 0) ids.Add(SelectedCancellationId);
        //         return ids;
        //     }
        // }


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

        // Lấy toàn bộ loại chính sách kèm policies bằng cách gọi API
        // public async Task<List<StepPolicies.PolicyTypeWithPoliciesVM>> LoadAllPolicyTypesWithPoliciesAsync()
        // {
        //     var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StepPolicies.PolicyTypeWithPoliciesVM>>>("hotel/get-all-policytypes-with-policies");

        //     if (result != null && result.StatusCode == "Success" && result.Content != null)
        //     {
        //         return result.Content ?? new List<StepPolicies.PolicyTypeWithPoliciesVM>();
        //     }

        //     return new List<StepPolicies.PolicyTypeWithPoliciesVM>();
        // }

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

        // Reset toàn bộ form
        public void Reset()
        {
            BasicInfo = new();

            // Reset trạng thái chọn của các tiện nghi (Amenities)
            foreach (var amenity in Amenities)
            {
                amenity.IsSelected = false;
            }

            // SelectedCheckInId = 0;
            // SelectedCheckOutId = 0;
            // SelectedCancellationId = 0;


            HotelImages = new();

        }

        // Validate từng phần
        public bool ValidateBasicInfo() =>
            !string.IsNullOrWhiteSpace(BasicInfo.Name) &&
            BasicInfo.CityId > 0 &&
            !string.IsNullOrWhiteSpace(BasicInfo.Address) &&
            BasicInfo.CoverFile != null;

        public bool ValidateAmenities() =>
            Amenities.Any(a => a.IsSelected);

        // public bool ValidatePolicy() =>
        //     SelectedPolicyIds.Count > 0;

        public bool ValidateImages() =>
            HotelImages.MainFile != null && HotelImages.SubFiles.Count == 4;

        // Validate toàn bộ form
        public bool ValidateAll() =>
            ValidateBasicInfo() &&
            ValidateAmenities() &&
            // ValidatePolicy() &&
            ValidateImages();
    }
}