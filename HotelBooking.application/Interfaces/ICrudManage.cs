public interface ICommonManage<TDto, TCreateOrUpdateDTO>
{
    Task<ApiResponse<TDto>> GetByIdAsync(int id);
    Task<ApiResponse<TDto>> CreateAsync(TCreateOrUpdateDTO Dto);
    Task<ApiResponse<TDto>> UpdateAsync(int id, TCreateOrUpdateDTO Dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IStandardManage<TDto, TCreateOrUpdateDTO> : ICommonManage<TDto, TCreateOrUpdateDTO>
{
    Task<ApiResponse<List<TDto>>> GetAllAsync();
}

public interface ITypedManage<TDto, TypeDTO, TCreateOrUpdateDTO> : ICommonManage<TDto, TCreateOrUpdateDTO>
{
    Task<ApiResponse<List<TypeDTO>>> GetTypeDataAsync();
}


