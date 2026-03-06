using Blazored.LocalStorage;
using HotelBooking.webapp.Pages.User.Owner.Steps;
using static HotelBooking.webapp.Pages.User.Owner.Steps.StepPolicies;
using System.Net.Http.Headers;
using HotelBooking.webapp.ViewModels.Admin;
using HotelBooking.webapp.ViewModels.Hotel;

namespace HotelBooking.webapp.Services
{
    /// <summary>
    /// Manages the state and logic for the multi-step Hotel Registration Form.
    /// </summary>
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

        // --- FORM STATE PROPERTIES ---
        public StepBasicInfo.HotelVM BasicInfo { get; set; } = new();
        public List<StepAmenities.AmenityVM> Amenities { get; set; } = new();
        public List<PolicyTypeGroupVM> PolicyGroups { get; set; } = new();
        public StepImages.HotelImagesVM HotelImages { get; set; } = new();
        public bool IsLoading { get; private set; }

        private void NotifyStateChanged() => OnChange?.Invoke();

        // --- DATA LOADING METHODS ---

        /// <summary>
        /// Fetches all available cities for the basic info step.
        /// </summary>
        public async Task LoadCitiesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StepBasicInfo.CityVM>>>("hotel/get-all-cities");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                BasicInfo.Cities = result.Content ?? new List<StepBasicInfo.CityVM>();
            }
        }

        #region AMENITIES MANAGEMENT
        /// <summary>
        /// Fetches the full list of available hotel amenities.
        /// </summary>
        public async Task LoadAmenitiesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StepAmenities.AmenityVM>>>("hotel/get-all-amenities");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                Amenities = result.Content ?? new List<StepAmenities.AmenityVM>();
            }
        }

        /// <summary>
        /// Extracts the IDs of all amenities currently selected (checked) by the user.
        /// </summary>
        public List<int> GetSelectedAmenityIds()
        {
            return Amenities
                .Where(a => a.IsSelected)
                .Select(a => a.Id)
                .ToList();
        }
        #endregion

        #region POLICIES MANAGEMENT
        public async Task<List<PolicyTypeVM>> LoadPolicyTypesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PolicyTypeVM>>>("hotel/get-all-policy-types");

            if (response != null && response.StatusCode == "Success" && response.Content != null)
            {
                return response.Content ?? new List<PolicyTypeVM>();
            }
            return new List<PolicyTypeVM>();
        }

        public async Task<List<PolicyVM>> LoadPoliciesByTypeIdAsync(int policyTypeId)
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<PolicyVM>>>($"hotel/get-all-policy-by-type/{policyTypeId}");

            if (result != null && result.StatusCode == "Success" && result.Content != null)
            {
                return result.Content ?? new List<PolicyVM>();
            }
            return new List<PolicyVM>();
        }

        /// <summary>
        /// Orchestrates the loading of policy types and their respective policies in parallel.
        /// </summary>
        public async Task LoadPolicies()
        {
            IsLoading = true;
            NotifyStateChanged(); // Alert UI that loading has started

            // 1. Fetch all Policy Types
            var allPolicyTypes = await LoadPolicyTypesAsync();
            if (allPolicyTypes == null || !allPolicyTypes.Any())
            {
                IsLoading = false;
                NotifyStateChanged();
                return;
            }

            // 2. Prepare tasks for parallel API calls (Efficiency)
            var policyLoadTasks = allPolicyTypes
                .Select(type => LoadPoliciesByTypeIdAsync(type.Id))
                .ToList();

            // 3. Execute all tasks concurrently and wait for completion
            var policyResults = await Task.WhenAll(policyLoadTasks);

            PolicyGroups.Clear();

            // 4. Map results back into PolicyGroups
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
            NotifyStateChanged(); // Final UI update trigger
        }

        public List<int> GetSelectedPolicyIds()
        {
            return PolicyGroups
                .Where(g => g.SelectedPolicyId > 0)
                .Select(g => g.SelectedPolicyId)
                .ToList();
        }
        #endregion

        #region SUBMISSION LOGIC
        /// <summary>
        /// Submits the entire hotel form as MultipartFormDataContent.
        /// </summary>
        public async Task<bool> Submit()
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            if (string.IsNullOrEmpty(token)) return false;

            // Set Authentication Header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Assemble the Post ViewModel
            var hotelForm = new HotelPostFormVM
            {
                Name = BasicInfo.Name,
                Address = BasicInfo.Address,
                CityId = BasicInfo.CityId,
                Description = BasicInfo.Description,
                CoverFile = BasicInfo.CoverFile,
                AmenityIds = GetSelectedAmenityIds(),
                PolicyIds = GetSelectedPolicyIds(),
                MainFile = HotelImages.MainFile,
                SubFiles = HotelImages.SubFiles
            };

            using var content = new MultipartFormDataContent();

            // Add simple text fields
            content.Add(new StringContent(hotelForm.Name), "Name");
            content.Add(new StringContent(hotelForm.Address), "Address");
            content.Add(new StringContent(hotelForm.CityId.ToString()), "CityId");
            content.Add(new StringContent(hotelForm.Description), "Description");

            // Add collection IDs
            foreach (var id in hotelForm.AmenityIds) content.Add(new StringContent(id.ToString()), "AmenityIds");
            foreach (var id in hotelForm.PolicyIds) content.Add(new StringContent(id.ToString()), "PolicyIds");

            // Add Cover Image
            if (hotelForm.CoverFile != null)
            {
                hotelForm.CoverFile.Content.Position = 0; // Reset stream position
                var coverFileContent = new StreamContent(hotelForm.CoverFile.Content);
                coverFileContent.Headers.ContentType = new MediaTypeHeaderValue(hotelForm.CoverFile.ContentType);
                content.Add(coverFileContent, "CoverFile", hotelForm.CoverFile.FileName);
            }

            // Add Main Gallery Image
            if (hotelForm.MainFile != null)
            {
                hotelForm.MainFile.Content.Position = 0;
                var mainFileContent = new StreamContent(hotelForm.MainFile.Content);
                mainFileContent.Headers.ContentType = new MediaTypeHeaderValue(hotelForm.MainFile.ContentType);
                content.Add(mainFileContent, "MainFile", hotelForm.MainFile.FileName);
            }

            // Add Sub Gallery Images
            foreach (var subFile in hotelForm.SubFiles)
            {
                subFile.Content.Position = 0;
                var subFileContent = new StreamContent(subFile.Content);
                subFileContent.Headers.ContentType = new MediaTypeHeaderValue(subFile.ContentType);
                content.Add(subFileContent, "SubFiles", subFile.FileName);
            }

            try
            {
                var response = await _httpClient.PostAsync("hotel/post-new-hotel", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log exception if necessary
                return false;
            }
        }
        #endregion

        #region FORM MANAGEMENT (RESET & VALIDATION)
        public void Reset()
        {
            BasicInfo = new();
            foreach (var amenity in Amenities) amenity.IsSelected = false;
            PolicyGroups = new();
            HotelImages = new();
        }

        public bool ValidateBasicInfo() =>
            !string.IsNullOrWhiteSpace(BasicInfo.Name) &&
            BasicInfo.CityId > 0 &&
            !string.IsNullOrWhiteSpace(BasicInfo.Address) &&
            BasicInfo.CoverFile != null;

        public bool ValidateAmenities() => Amenities.Any(a => a.IsSelected);

        /// <summary>
        /// Validates that every policy category has exactly one selected policy.
        /// </summary>
        public bool ValidatePolicy()
        {
            if (PolicyGroups.Count == 0) return false;
            return PolicyGroups.All(group => group.SelectedPolicyId > 0);
        }

        public bool ValidateImages() =>
            HotelImages.MainFile != null && HotelImages.SubFiles.Count == 4;

        /// <summary>
        /// Validates the entire form across all steps.
        /// </summary>
        public bool ValidateAll() =>
            ValidateBasicInfo() &&
            ValidateAmenities() &&
            // ValidatePolicy() && // Optional: depends on business requirements
            ValidateImages();
        #endregion
    }
}