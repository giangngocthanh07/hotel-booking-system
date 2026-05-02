using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RoomManagement
{
    public interface IRoomTypeService
    {
        Task<ApiResponse<RoomTypeResponseDTO>> CreateRoomTypeAsync(RoomTypeCreateDTO request);
    }

    public class RoomTypeService : IRoomTypeService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IRoomTypeRepository _roomTypeRepo;
        private readonly IRoomTypeBedConfigRepository _bedConfigRepo;
        private readonly IRoomAttributeFacade _attributeFacade;
        private readonly IUnitOfWork _dbu;
        private readonly ILogger _logger;
        private readonly IValidator<RoomTypeCreateDTO> _validator;

        public RoomTypeService(IHotelRepository hotelRepo, IRoomTypeRepository roomTypeRepo, IRoomTypeBedConfigRepository bedConfigRepo, IValidator<RoomTypeCreateDTO> validator, IRoomAttributeFacade attributeFacade, IUnitOfWork dbu, ILogger logger)
        {
            _hotelRepo = hotelRepo;
            _roomTypeRepo = roomTypeRepo;
            _bedConfigRepo = bedConfigRepo;
            _validator = validator;
            _attributeFacade = attributeFacade;
            _dbu = dbu;
            _logger = logger;
        }
        public async Task<ApiResponse<RoomTypeResponseDTO>> CreateRoomTypeAsync(RoomTypeCreateDTO request)
        {
            // 1. GUARD CLAUSE & VALIDATION
            if (request == null)
            {
                return ResponseFactory.Failure<RoomTypeResponseDTO>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.Common.REQUEST_CANNOT_BE_NULL);
            }

            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return ResponseFactory.Failure<RoomTypeResponseDTO>(StatusCodeResponse.BadRequest, validationResult.Errors[0].ErrorMessage);
            }

            var ghostIdValidation = await ValidateGhostIdsAsync(request);
            if (!ghostIdValidation.IsValid)
            {
                return ResponseFactory.Failure<RoomTypeResponseDTO>(StatusCodeResponse.NotFound, ghostIdValidation.Message);
            }

            try
            {
                // 2. BUSINESS LOGIC
                // a) Lower RoomTypeName
                var normalizedRoomTypeName = request.Name.Trim().ToLower();

                var existingRoomType = await _roomTypeRepo.AnyAsync(x => x.HotelId == request.HotelId && x.Name.ToLower() == normalizedRoomTypeName && x.IsDeleted == false);
                if (existingRoomType)
                {
                    return ResponseFactory.Failure<RoomTypeResponseDTO>(StatusCodeResponse.Conflict, MessageResponse.RoomManagement.ROOM_TYPE_ALREADY_EXISTS);
                }

                await _dbu.BeginTransactionAsync();

                var roomType = new RoomType
                {
                    HotelId = request.HotelId,
                    Name = request.Name.Trim(),
                    Description = request.Description,
                    IsDeleted = false,
                    PricePerNight = request.PricePerNight,
                    AdultCapacity = request.AdultCapacity,
                    ChildCapacity = request.ChildCapacity,
                    Capacity = request.AdultCapacity + request.ChildCapacity,
                    UnitTypeId = request.UnitTypeId,
                    QualityId = request.QualityId,
                    RoomViewId = request.RoomViewId,
                    IsPrivateBathroom = request.IsPrivateBathroom,
                    HasBalcony = request.HasBalcony,
                    HasTerrace = request.HasTerrace,
                    CanAddExtraBed = request.CanAddExtraBed,
                    MaxExtraBeds = request.MaxExtraBeds,
                    AreaSqm = request.AreaSqm,
                    Additional = JsonSerializer.Serialize(new
                    {
                        BedTypes = request.BedTypes,
                        IsSmokingAllowed = request.IsSmokingAllowed,
                        TotalRooms = request.TotalRooms
                    })
                };

                await _roomTypeRepo.AddAsync(roomType);
                await _dbu.SaveChangesAsync();

                foreach (var bedType in request.BedTypes)
                {
                    var bedConfig = new RoomTypeBedConfig
                    {
                        RoomTypeId = roomType.Id,
                        BedTypeId = bedType.BedTypeId,
                        Quantity = bedType.Quantity
                    };
                    await _bedConfigRepo.AddAsync(bedConfig);
                }
                await _dbu.SaveChangesAsync();

                await _dbu.CommitTransactionAsync();

                var additionalInfo = string.IsNullOrEmpty(roomType.Additional) ? new RoomTypeAdditionalData() : JsonSerializer.Deserialize<RoomTypeAdditionalData>(roomType.Additional);

                RoomTypeResponseDTO dto = new RoomTypeResponseDTO
                {
                    Id = roomType.Id,
                    HotelId = roomType.HotelId,
                    Name = roomType.Name,
                    Description = roomType.Description,
                    IsDeleted = false,
                    PricePerNight = roomType.PricePerNight,
                    AdultCapacity = roomType.AdultCapacity,
                    ChildCapacity = roomType.ChildCapacity,
                    UnitTypeId = roomType.UnitTypeId,
                    QualityId = roomType.QualityId,
                    RoomViewId = roomType.RoomViewId,
                    IsPrivateBathroom = roomType.IsPrivateBathroom,
                    HasBalcony = roomType.HasBalcony,
                    HasTerrace = roomType.HasTerrace,
                    CanAddExtraBed = roomType.CanAddExtraBed,
                    MaxExtraBeds = roomType.MaxExtraBeds,
                    AreaSqm = roomType.AreaSqm,

                    IsSmokingAllowed = additionalInfo?.IsSmokingAllowed ?? false,
                    TotalRooms = additionalInfo?.TotalRooms ?? 0,
                    BedTypes = additionalInfo?.BedTypes ?? new List<BedTypeConfigDTO>()

                };

                return ResponseFactory.Success(dto, MessageResponse.Common.CREATE_SUCCESSFULLY);
            }
            catch (Exception ex)
            {
                _logger.LogError("RoomTypeService.CreateRoomTypeAsync: {ErrorMessage}", ex.Message);
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.Failure<RoomTypeResponseDTO>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
            }
        }

        // --- HELPER FUNCTION: Convert child paged result to parent paged result ---
        private async Task<(bool IsValid, string Message)> ValidateGhostIdsAsync(RoomTypeCreateDTO request)
        {
            // Check Hotel
            var isHotelExisted = await _hotelRepo.AnyAsync(x => x.Id == request.HotelId);
            if (!isHotelExisted)
            {
                return (false, MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_NOT_FOUND);
            }
            // Check UnitType
            var isUnitTypeExisted = await _attributeFacade.IsUnitTypeExistedAsync(request.UnitTypeId);
            if (!isUnitTypeExisted)
            {
                return (false, MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_NOT_FOUND);
            }
            // Check Quality (if provided)
            if (request.QualityId.HasValue)
            {
                var isQualityExisted = await _attributeFacade.IsRoomQualityExistedAsync(request.QualityId.Value);
                if (!isQualityExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_NOT_FOUND);
                }
            }
            // Check RoomView (if provided)
            if (request.RoomViewId.HasValue)
            {
                var isRoomViewExisted = await _attributeFacade.IsRoomViewExistedAsync(request.RoomViewId.Value);
                if (!isRoomViewExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_NOT_FOUND);
                }
            }

            // Check BedTypes
            foreach (var bedType in request.BedTypes)
            {
                var isBedTypeExisted = await _attributeFacade.IsBedTypeExistedAsync(bedType.BedTypeId);
                if (!isBedTypeExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_NOT_FOUND);
                }
            }
            return (true, string.Empty);
        }
    }
}
