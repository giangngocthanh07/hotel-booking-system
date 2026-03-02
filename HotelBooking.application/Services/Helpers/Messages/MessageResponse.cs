/// <summary>
/// Tập hợp tất cả các message response trong hệ thống.
/// Tổ chức theo domain: AdminManagement, UserManagement, Common
/// Mỗi domain có các nested class theo functionality
/// </summary>
public static class MessageResponse
{
    // =====================================================
    // 1. COMMON MESSAGES (Các message chung cho tất cả)
    // =====================================================
    public static class Common
    {
        // Basic CRUD messages
        public const string GET_SUCCESSFULLY = "Lấy thành công!";
        public const string GET_FAILED = "Lấy thất bại!";
        public const string CREATE_SUCCESSFULLY = "Tạo thành công!";
        public const string CREATE_FAILED = "Tạo thất bại!";
        public const string UPDATE_SUCCESSFULLY = "Cập nhật thành công!";
        public const string UPDATE_FAILED = "Cập nhật thất bại!";
        public const string DELETE_SUCCESSFULLY = "Xóa thành công!";
        public const string DELETE_FAILED = "Xóa thất bại!";

        // Status messages
        public const string NOT_FOUND = "Không tìm thấy!";
        public const string BAD_REQUEST = "Yêu cầu không hợp lệ!";
        public const string EMPTY_LIST = "Danh sách rỗng!";
        public const string ERROR_IN_SERVER = "Lỗi ở phía máy chủ!";
    }

    // =====================================================
    // 2. VALIDATION MESSAGES (Các message validation chung)
    // =====================================================
    public static class Validation
    {
        public const string INVALID_MODULE = "Module quản lý không hợp lệ!";
        public const string TYPE_ID_REQUIRED = "Vui lòng chọn loại (TypeId)!";
        public const string INVALID_TYPE_ID = "TypeId phải lớn hơn 0!";
        public const string EMPTY_NAME = "Tên không được để trống!";
        public const string LONG_NAME = "Tên quá dài!";
        public const string EMPTY_TYPE = "Loại không được để trống!";
        public const string NAME_ALREADY_EXISTS = "Tên đã tồn tại!";
        public const string INVALID_AMOUNT = "Số tiền phải tối thiểu bằng 0đ (Miễn phí)";
        public const string PERCENT_INVALID = "Phần trăm không hợp lệ!";
        public const string LONG_DESCRIPTION = "Mô tả quá dài!";

        public static class Pagination
        {
            public const string MISSING_PAGE_INDEX = "Số trang không được để trống!";
            public const string INVALID_PAGE_INDEX = "Số trang phải lớn hơn 0!";
            public const string MISSING_PAGE_SIZE = "Kích thước trang không được để trống!";
            public const string INVALID_PAGE_SIZE = "Kích thước trang phải lớn hơn 0!";
            public const string PAGE_SIZE_TOO_LARGE = "Kích thước trang tối đa là 100!";
        }
    }

    // =====================================================
    // 3. ADMIN MANAGEMENT MESSAGES
    // =====================================================
    public static class AdminManagement
    {
        // Amenity Messages
        public static class Amenity
        {
            public const string EMPTY_NAME = "Tên tiện ích không được để trống!";
            public const string LONG_NAME = "Tên tiện ích quá dài (tối đa 20 ký tự)!";
            public const string EMPTY_TYPE = "Loại tiện ích không được để trống!";
            public const string NAME_ALREADY_EXISTS = "Tên tiện ích đã tồn tại!";
            public const string INVALID_TYPE = "Loại tiện ích không hợp lệ!";
            public const string GREATER_THAN_ZERO = "Giá trị phải lớn hơn 0!";
        }

        // Policy Messages
        public static class Policy
        {
            public const string EMPTY_NAME = "Tên chính sách không được để trống!";
            public const string LONG_NAME = "Tên chính sách quá dài (tối đa 50 ký tự)!";
            public const string EMPTY_TYPE = "Loại chính sách không được để trống!";
            public const string INVALID_AMOUNT = "Số tiền phải lớn hơn 0!";
            public const string NAME_ALREADY_EXISTS = "Tên chính sách đã tồn tại!";
            public const string INVALID_ID_BY_TYPE = "ID không hợp lệ cho loại chính sách này!";
        }

        // Service Messages
        public static class Service
        {
            public const string NAME_ALREADY_EXISTS = "Tên dịch vụ đã tồn tại!";
            public const string LONG_NAME = "Tên dịch vụ quá dài (tối đa 50 ký tự)!";
            public const string EMPTY_NAME = "Tên dịch vụ không được để trống!";
            public const string INVALID_AMOUNT = "Số tiền dịch vụ phải tối thiểu bằng 0đ (Miễn phí) ";
            public const string INVALID_ID_BY_TYPE = "ID không hợp lệ cho loại dịch vụ này!";
            public const string INVALID_TYPE = "Loại dịch vụ không hợp lệ!";
            public const string EMPTY_UNIT = "Đơn vị tính không được để trống!";
            public const string EMPTY_UNIT_NAME = "Tên đơn vị tính không được để trống!";
            public const string LONG_UNIT = "Đơn vị tính quá dài (tối đa 20 ký tự)!";
            public const string STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO = "Dịch vụ tiêu chuẩn phải có giá tối thiểu là 10,000đ!";
            public const string MIN_PASSENGERS = "Số lượng khách tối thiểu phải là 1!";
            public const string MAX_PASSENGERS = "Số lượng khách tối đa không được vượt quá 45!";
            public const string MIN_LUGGAGE = "Số lượng hành lý tối thiểu là 1!";
            public const string MAX_LUGGAGE = "Số lượng hành lý tối đa không được vượt quá 45!";
            public const string INVALID_ROUND_TRIP_PRICE = "Giá khứ hồi phải tối thiểu bằng 0đ (Miễn phí)";
            public const string DEFAULT_ADDITIONAL_FEE = "Phụ phí đêm phải lớn hơn";
            public const string MISSING_ADDITIONAL_FEE_START_TIME = "Vui lòng nhập giờ bắt đầu phụ phí đêm!";
            public const string MISSING_ADDITIONAL_FEE_END_TIME = "Vui lòng nhập giờ kết thúc phụ phí đêm!";
            public const string INVALID_ADDITIONAL_FEE_START_END_TIME = "Giờ bắt đầu và kết thúc không được trùng nhau.";
            public const string ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT = "Khung giờ phụ phí đêm không được quá 12 tiếng.";
        }

        // Room Attributes Messages (BedType, RoomView, RoomQuality, UnitType)
        public static class RoomAttribute
        {
            // BedType Messages
            public static class BedType
            {
                public const string NAME_ALREADY_EXISTS = "Tên loại giường đã tồn tại!";
                public const string EMPTY_NAME = "Tên loại giường không được để trống!";
                public const string LONG_NAME = "Tên loại giường quá dài (tối đa 20 ký tự)!";
                public const string INVALID_DEFAULT_CAPACITY = "Sức chứa không hợp lệ, phải từ 1 đến 10 người!";
                public const string INVALID_MIN_WIDTH = "Chiều rộng tối thiểu phải lớn hơn 0!";
                public const string INVALID_MAX_WIDTH = "Chiều rộng tối đa phải lớn hơn hoặc bằng chiều rộng tối thiểu!";
            }

            // RoomView Messages
            public static class RoomView
            {
                public const string NAME_ALREADY_EXISTS = "Tên hướng phòng đã tồn tại!";
                public const string EMPTY_NAME = "Tên hướng phòng không được để trống!";
                public const string LONG_NAME = "Tên hướng phòng quá dài (tối đa 20 ký tự)!";
            }

            // RoomQuality Messages
            public static class RoomQuality
            {
                public const string NAME_ALREADY_EXISTS = "Tên chất lượng phòng đã tồn tại!";
                public const string EMPTY_NAME = "Tên chất lượng phòng không được để trống!";
                public const string LONG_NAME = "Tên chất lượng phòng quá dài (tối đa 20 ký tự)!";
                public const string INVALID_SORT_ORDER = "Thứ tự ưu tiên không hợp lệ, phải từ 0 đến 10!";
            }

            // UnitType Messages
            public static class UnitType
            {
                public const string NAME_ALREADY_EXISTS = "Tên loại phòng đã tồn tại!";
                public const string EMPTY_NAME = "Tên loại phòng không được để trống!";
                public const string LONG_NAME = "Tên loại phòng quá dài (tối đa 20 ký tự)!";
            }

            // Request Validation Messages
            public static class Request
            {
                public const string INVALID_TYPE = "Loại thuộc tính không hợp lệ!";
                public const string MISSING_ROOM_QUALITY_TYPE = "Vui lòng chọn Loại chất lượng phòng!";
                public const string INVALID_TYPE_ID = "TypeId phải lớn hơn 0!";
                public const string UNSUPPORTED_TYPE_ID_FILTER = "Loại thuộc tính này không hỗ trợ lọc theo TypeId (vui lòng để null)!";
                public const string PAGINATION_REQUIRED = "Dữ liệu phân trang là bắt buộc!";
            }
        }

        // Role Messages
        public static class Role
        {
            public const string NOT_FOUND = "Role không tìm thấy!";
            public const string ALREADY_EXISTS = "Role đã tồn tại!";
            public const string ADD_SUCCESS = "Thêm role thành công!";
            public const string ADD_FAILED = "Thêm role thất bại!";
            public const string UPDATE_SUCCESS = "Cập nhật role thành công!";
            public const string UPDATE_FAILED = "Cập nhật role thất bại!";
            public const string DELETE_SUCCESS = "Xóa role thành công!";
            public const string DELETE_FAILED = "Xóa role thất bại!";
        }
    }

    // =====================================================
    // 4. USER MANAGEMENT MESSAGES
    // =====================================================
    public static class UserManagement
    {
        // Login Messages
        public static class Login
        {
            public const string FAIL = "Đăng nhập thất bại!";
            public const string SUCCESS = "Đăng nhập thành công!";
            public const string INVALID_CREDENTIALS = "Tài khoản hoặc mật khẩu không đúng!";
            public const string USER_BLOCKED = "Tài khoản của bạn đã bị khóa! Vui lòng liên hệ hỗ trợ.";
            public const string USER_DELETED = "Tài khoản của bạn đã bị xóa! Vui lòng liên hệ hỗ trợ.";
            public const string ERROR_IN_SERVER = "Đã xảy ra lỗi trên máy chủ. Vui lòng thử lại sau.";
            
            // Validation messages for login input (basic validation only)
            public const string EMPTY_USERNAME_OR_EMAIL = "Username hoặc Email không được để trống!";
            public const string MAX_LENGTH_USERNAME_OR_EMAIL = "Username hoặc Email không được vượt quá 255 ký tự!";
            public const string MAX_LENGTH_PASSWORD = "Mật khẩu không được vượt quá 100 ký tự!";
        }

        // Register Messages
        public static class Register
        {
            public const string SUCCESS = "Tài khoản đã đăng ký thành công!";
            public const string FAIL = "Đăng ký tài khoản thất bại!";
            public const string USERNAME_EXIST = "Tên tài khoản đã tồn tại!";
            public const string EMAIL_EXIST = "Email đã tồn tại!";
            public const string INVALID_EMAIL = "Định dạng email không hợp lệ!";

            // Phone validation messages
            public const string EMPTY_PHONE = "Số điện thoại không được để trống!";
            public const string INVALID_PHONE = "Số điện thoại phải là 10 chữ số!";

            // Password validation messages
            public const string SHORT_PASSWORD = "Mật khẩu phải có ít nhất 8 ký tự!";
            public const string EMPTY_PASSWORD = "Mật khẩu không được để trống!";
            public const string UPPERCASE_LETTER_PASSWORD = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa!";
            public const string NUMBER_PASSWORD = "Mật khẩu phải chứa ít nhất một chữ số!";
            public const string LOWERCASE_LETTER_PASSWORD = "Mật khẩu phải chứa ít nhất một chữ cái viết thường!";
            public const string SPECIAL_CHARACTER_PASSWORD = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt!";
        }

        // User Management Messages
        public static class User
        {
            public const string NOT_FOUND = "Tài khoản người dùng không tìm thấy!";
            public const string ALREADY_EXISTS = "Người dùng đã tồn tại!";
            public const string ADD_SUCCESS = "Thêm người dùng thành công!";
            public const string ADD_FAILED = "Thêm người dùng thất bại!";
            public const string UPDATE_SUCCESS = "Cập nhật người dùng thành công!";
            public const string UPDATE_FAILED = "Cập nhật người dùng thất bại!";
            public const string DELETE_SUCCESS = "Xóa người dùng thành công!";
            public const string DELETE_FAILED = "Xóa người dùng thất bại!";
        }
    }

    // =====================================================
    // 5. REQUEST MANAGEMENT MESSAGES
    // =====================================================
    public static class RequestManagement
    {
        // Upgrade Request Messages
        public static class UpgradeRequest
        {
            // Get User for Upgrade
            public const string USER_INFO_RETRIEVED = "Thông tin người dùng được lấy thành công!";
            public const string USER_NOT_FOUND = "Không tìm thấy tài khoản người dùng!";

            // Create Request
            public const string REQUEST_CREATED_SUCCESS = "Yêu cầu được tạo thành công!";
            public const string REQUEST_CREATE_FAILED = "Không thể tạo yêu cầu!";
            public const string USER_NOT_CUSTOMER = "Tài khoản của bạn không phải là khách hàng!";
            public const string PENDING_REQUEST_EXISTS = "Yêu cầu đang chờ phê duyệt đã tồn tại cho tài khoản này!";

            // Cancel Request
            public const string REQUEST_CANCELLED_SUCCESS = "Yêu cầu được hủy thành công!";
            public const string REQUEST_CANCEL_FAILED = "Không thể hủy yêu cầu!";

            // Get All Requests
            public const string REQUESTS_RETRIEVED = "Danh sách yêu cầu được lấy thành công!";

            // Get Request By ID
            public const string REQUEST_RETRIEVED = "Yêu cầu được lấy thành công!";
            public const string REQUEST_NOT_FOUND = "Không tìm thấy yêu cầu!";

            // Approve Request
            public const string REQUEST_APPROVED_SUCCESS = "Yêu cầu được phê duyệt thành công!";
            public const string REQUEST_APPROVE_FAILED = "Không thể phê duyệt yêu cầu!";
            public const string REQUEST_STATUS_INVALID = "Yêu cầu không tìm thấy hoặc không ở trạng thái chờ phê duyệt!";

            // Reject Request
            public const string REQUEST_REJECTED_SUCCESS = "Yêu cầu được từ chối thành công!";
            public const string REQUEST_REJECT_FAILED = "Không thể từ chối yêu cầu!";

            // Validation Messages
            public const string ADDRESS_REQUIRED = "Địa chỉ không được để trống!";
            public const string ADDRESS_TOO_LONG = "Địa chỉ không được vượt quá 500 ký tự!";
            public const string TAX_CODE_REQUIRED = "Mã số thuế không được để trống!";
            public const string TAX_CODE_INVALID = "Mã số thuế phải có 10 hoặc 13 chữ số!";
        }

        /// <summary>
        /// Messages cho duyệt khách sạn
        /// </summary>
        public static class HotelApproval
        {
            public const string HOTELS_RETRIEVED = "Danh sách khách sạn được lấy thành công!";
            public const string HOTEL_RETRIEVED = "Thông tin khách sạn được lấy thành công!";
            public const string HOTEL_NOT_FOUND = "Không tìm thấy khách sạn!";
            public const string STATUS_INVALID = "Khách sạn không ở trạng thái chờ duyệt!";
            public const string APPROVED_SUCCESS = "Khách sạn được duyệt thành công!";
            public const string APPROVE_FAILED = "Không thể duyệt khách sạn!";
            public const string REJECTED_SUCCESS = "Khách sạn bị từ chối thành công!";
            public const string REJECT_FAILED = "Không thể từ chối khách sạn!";
        }
    }

    // Alias for easier access
    public static class HotelApproval
    {
        public const string HOTELS_RETRIEVED = RequestManagement.HotelApproval.HOTELS_RETRIEVED;
        public const string HOTEL_RETRIEVED = RequestManagement.HotelApproval.HOTEL_RETRIEVED;
        public const string HOTEL_NOT_FOUND = RequestManagement.HotelApproval.HOTEL_NOT_FOUND;
        public const string STATUS_INVALID = RequestManagement.HotelApproval.STATUS_INVALID;
        public const string APPROVED_SUCCESS = RequestManagement.HotelApproval.APPROVED_SUCCESS;
        public const string APPROVE_FAILED = RequestManagement.HotelApproval.APPROVE_FAILED;
        public const string REJECTED_SUCCESS = RequestManagement.HotelApproval.REJECTED_SUCCESS;
        public const string REJECT_FAILED = RequestManagement.HotelApproval.REJECT_FAILED;
    }

    // =====================================================
    // 6. PAGINATION MESSAGES
    // =====================================================
    public static class Pagination
    {
        public const string MISSING_PAGE_INDEX = "Số trang không được để trống!";
        public const string INVALID_PAGE_INDEX = "Số trang phải lớn hơn 0!";
        public const string MISSING_PAGE_SIZE = "Kích thước trang không được để trống!";
        public const string INVALID_PAGE_SIZE = "Kích thước trang phải lớn hơn 0!";
        public const string PAGE_SIZE_TOO_LARGE = "Kích thước trang tối đa là 100 (Chống DDOS)!";
    }

    // =====================================================
    // 7. MENU REQUEST MESSAGES
    // =====================================================
    public static class ManageMenu
    {
        public const string INVALID_MODULE = "Module không hợp lệ!";
    }

    // =====================================================
    // BACKWARD COMPATIBILITY (Để không break code cũ)
    // =====================================================
    [Obsolete("Sử dụng MessageResponse.Common.GET_SUCCESSFULLY thay vào")]
    public static string GET_SUCCESSFULLY => Common.GET_SUCCESSFULLY;
    [Obsolete("Sử dụng MessageResponse.Common.GET_FAILED thay vào")]
    public static string GET_FAILED => Common.GET_FAILED;
    [Obsolete("Sử dụng MessageResponse.Common.CREATE_SUCCESSFULLY thay vào")]
    public static string CREATE_SUCCESSFULLY => Common.CREATE_SUCCESSFULLY;
    [Obsolete("Sử dụng MessageResponse.Common.CREATE_FAILED thay vào")]
    public static string CREATE_FAILED => Common.CREATE_FAILED;
    [Obsolete("Sử dụng MessageResponse.Common.UPDATE_SUCCESSFULLY thay vào")]
    public static string UPDATE_SUCCESSFULLY => Common.UPDATE_SUCCESSFULLY;
    [Obsolete("Sử dụng MessageResponse.Common.UPDATE_FAILED thay vào")]
    public static string UPDATE_FAILED => Common.UPDATE_FAILED;
    [Obsolete("Sử dụng MessageResponse.Common.DELETE_SUCCESSFULLY thay vào")]
    public static string DELETE_SUCCESSFULLY => Common.DELETE_SUCCESSFULLY;
    [Obsolete("Sử dụng MessageResponse.Common.DELETE_FAILED thay vào")]
    public static string DELETE_FAILED => Common.DELETE_FAILED;
    [Obsolete("Sử dụng MessageResponse.Common.NOT_FOUND thay vào")]
    public static string NOT_FOUND => Common.NOT_FOUND;
    [Obsolete("Sử dụng MessageResponse.Common.BAD_REQUEST thay vào")]
    public static string BAD_REQUEST => Common.BAD_REQUEST;
    [Obsolete("Sử dụng MessageResponse.Validation.NAME_ALREADY_EXISTS thay vào")]
    public static string NAME_ALREADY_EXISTS => Validation.NAME_ALREADY_EXISTS;
    [Obsolete("Sử dụng MessageResponse.Validation.LONG_NAME thay vào")]
    public static string LONG_NAME => Validation.LONG_NAME;
    [Obsolete("Sử dụng MessageResponse.Validation.EMPTY_NAME thay vào")]
    public static string EMPTY_NAME => Validation.EMPTY_NAME;
    [Obsolete("Sử dụng MessageResponse.Common.EMPTY_LIST thay vào")]
    public static string EMPTY_LIST => Common.EMPTY_LIST;
    [Obsolete("Sử dụng MessageResponse.Validation.EMPTY_TYPE thay vào")]
    public static string EMPTY_TYPE => Validation.EMPTY_TYPE;
    [Obsolete("Sử dụng MessageResponse.Common.ERROR_IN_SERVER thay vào")]
    public static string ERROR_IN_SERVER => Common.ERROR_IN_SERVER;

    // Backward compatibility for old message classes
    [Obsolete("Sử dụng MessageResponse.UserManagement.Login thay vào")]
    public static class MessageLogin
    {
        public static string LOGIN_FAIL => UserManagement.Login.FAIL;
        public static string LOGIN_SUCCESS => UserManagement.Login.SUCCESS;
        public static string INVALID_CREDENTIALS => UserManagement.Login.INVALID_CREDENTIALS;
        public static string USER_BLOCKED => UserManagement.Login.USER_BLOCKED;
        public static string USER_DELETED => UserManagement.Login.USER_DELETED;
    }

    [Obsolete("Sử dụng MessageResponse.UserManagement.Register thay vào")]
    public static class MessageRegister
    {
        public static string REGISTER_SUCCESS => UserManagement.Register.SUCCESS;
        public static string REGISTER_FAIL => UserManagement.Register.FAIL;
        public static string USERNAME_EXIST => UserManagement.Register.USERNAME_EXIST;
        public static string EMAIL_EXIST => UserManagement.Register.EMAIL_EXIST;
        public static string INVALID_EMAIL => UserManagement.Register.INVALID_EMAIL;
        public static string SHORT_PASSWORD => UserManagement.Register.SHORT_PASSWORD;
        public static string EMPTY_PASSWORD => UserManagement.Register.EMPTY_PASSWORD;
        public static string UPPERCASE_LETTER_PASSWORD => UserManagement.Register.UPPERCASE_LETTER_PASSWORD;
            public static string NUMBER_PASSWORD => UserManagement.Register.NUMBER_PASSWORD;
        public static string LOWERCASE_LETTER_PASSWORD => UserManagement.Register.LOWERCASE_LETTER_PASSWORD;
        public static string SPECIAL_CHARACTER_PASSWORD => UserManagement.Register.SPECIAL_CHARACTER_PASSWORD;
    }

    [Obsolete("Sử dụng MessageResponse.AdminManagement.Role thay vào")]
    public static class RoleMessage
    {
        public static string ROLE_NOT_FOUND => AdminManagement.Role.NOT_FOUND;
        public static string ROLE_ALREADY_EXISTS => AdminManagement.Role.ALREADY_EXISTS;
        public static string ROLE_ADD_SUCCESS => AdminManagement.Role.ADD_SUCCESS;
        public static string ROLE_ADD_FAILED => AdminManagement.Role.ADD_FAILED;
        public static string ROLE_UPDATE_SUCCESS => AdminManagement.Role.UPDATE_SUCCESS;
        public static string ROLE_UPDATE_FAILED => AdminManagement.Role.UPDATE_FAILED;
        public static string ROLE_DELETE_SUCCESS => AdminManagement.Role.DELETE_SUCCESS;
        public static string ROLE_DELETE_FAILED => AdminManagement.Role.DELETE_FAILED;
    }
}
