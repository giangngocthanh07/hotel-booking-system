
using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RequestManagement.Customer
{
    public interface IHotelRegistrationService
    {
        // Hotel Registration
        Task<ApiResponse<bool>> HotelRegistrationAsync(HotelRegistrationDTO request);
    }

    public class HotelRegistrationService : IHotelRegistrationService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IValidator<HotelRegistrationDTO> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public HotelRegistrationService(IHotelRepository hotelRepo, IValidator<HotelRegistrationDTO> validator, IUnitOfWork unitOfWork)
        {
            _hotelRepo = hotelRepo;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> HotelRegistrationAsync(HotelRegistrationDTO request)
        {
            try
            {
                var validation = await _validator.ValidateAsync(request);
                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.BadRequest,
                        validation.Errors.First().ErrorMessage);
                }

                // Check duplicate name
                var hasDuplicateName = await _hotelRepo.AnyAsync(h => h.Name == request.Name);
                if (hasDuplicateName)
                {
                    return ResponseFactory.Failure<bool>(
                        StatusCodeResponse.Conflict,
                        MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NAME_ALREADY_EXISTS);
                }

                // Add Hotel Request to Hotel with Pending Status
                var additionalData = new HotelAdditionalInfo
                {
                    StarRating = request.StarRating,
                    PublicPhone = request.PublicPhone,
                    PublicEmail = request.PublicEmail,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TaxCode = request.TaxCode,
                    BusinessLicenseUrl = request.BusinessLicenseUrl
                };

                var hotel = new Hotel
                {
                    Name = request.Name,
                    Address = request.Address,
                    Description = request.Description,
                    PropertyTypeId = request.PropertyTypeId,
                    CountryId = request.CountryId,
                    ProvinceId = request.ProvinceId,
                    WardId = request.WardId,
                    Status = RequestStatusConst.Pending,
                    Additional = JsonSerializer.Serialize(additionalData)
                };

                await _hotelRepo.AddAsync(hotel);
                var result = await _unitOfWork.SaveChangesAsync();

                return ResponseFactory.Success<bool>(true, MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<bool>();
            }
        }
    }
}