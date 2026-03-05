namespace HotelBooking.application.Interfaces;
public interface ICommonManage<TDto, TCreateDTO, TUpdateDTO>
{
    Task<ApiResponse<TDto>> GetByIdAsync(int id);
    Task<ApiResponse<TDto>> CreateAsync(TCreateDTO Dto);
    Task<ApiResponse<TDto>> UpdateAsync(int id, TUpdateDTO Dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IStandardManage<TDto, TCreateDTO, TUpdateDTO> : ICommonManage<TDto, TCreateDTO, TUpdateDTO>
{
    Task<ApiResponse<List<TDto>>> GetAllAsync();
}

public interface ITypedManage<TDto, TypeDTO, TCreateDTO, TUpdateDTO> : ICommonManage<TDto, TCreateDTO, TUpdateDTO>
{
    Task<ApiResponse<List<TypeDTO>>> GetTypeDataAsync();
}


