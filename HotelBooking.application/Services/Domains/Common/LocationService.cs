using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Services.Domains.Common;

public interface ILocationService
{
    public Task<ApiResponse<List<CountryDTO>>> GetCountriesAsync();
    public Task<ApiResponse<List<ProvinceDTO>>> GetProvincesAsync(int countryId);
    public Task<ApiResponse<List<WardDTO>>> GetWardsByProvinceAsync(int provinceId);
}

public class LocationService : ILocationService
{
    private readonly ICountryRepository _countryRepository;
    private readonly IPropertyTypeRepository _propTypeRepository;
    private readonly IProvinceRepository _provinceRepository;
    private readonly IWardRepository _wardRepository;
    private readonly IUnitOfWork _dbu;
    private readonly ILogger _logger;

    public LocationService(ICountryRepository countryRepository, IPropertyTypeRepository propTypeRepository, IProvinceRepository provinceRepository, IWardRepository wardRepository, IUnitOfWork dbu, ILogger logger)
    {
        _countryRepository = countryRepository;
        _propTypeRepository = propTypeRepository;
        _provinceRepository = provinceRepository;
        _wardRepository = wardRepository;
        _dbu = dbu;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CountryDTO>>> GetCountriesAsync()
    {
        try
        {
            var countries = await _countryRepository.GetAllAsync();
            var result = countries.Select(c => new CountryDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocationService.GetCountriesAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<List<CountryDTO>>();
        }
    }

    public async Task<ApiResponse<List<ProvinceDTO>>> GetProvincesAsync(int countryId)
    {
        try
        {
            var provinces = await _provinceRepository.GetByCountryIdAsync(countryId);
            var result = provinces.Select(p => new ProvinceDTO
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocationService.GetProvincesAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<List<ProvinceDTO>>();
        }
    }

    public async Task<ApiResponse<List<WardDTO>>> GetWardsByProvinceAsync(int provinceId)
    {
        try
        {
            var wards = await _wardRepository.GetByProvinceIdAsync(provinceId);
            var result = wards.Select(w => new WardDTO
            {
                Id = w.Id,
                Name = w.Name
            }).ToList();

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocationService.GetWardsByProvinceAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<List<WardDTO>>();
        }
    }

}