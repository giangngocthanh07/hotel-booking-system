namespace HotelBooking.webapp.Helpers;

/// <summary>
/// Centralized repository for all system message responses.
/// Organized by domain: AdminManagement, UserManagement, Common.
/// Each domain contains nested classes categorized by functionality.
/// </summary>
public static class MessageResponse
{
    // =====================================================
    // 1. COMMON MESSAGES (General system-wide messages)
    // =====================================================
    public static class Common
    {
        // Basic CRUD messages
        public const string GET_SUCCESSFULLY = "Data retrieved successfully!";
        public const string GET_FAILED = "Failed to retrieve data!";
        public const string CREATE_SUCCESSFULLY = "Created successfully!";
        public const string CREATE_FAILED = "Failed to create!";
        public const string UPDATE_SUCCESSFULLY = "Updated successfully!";
        public const string UPDATE_FAILED = "Failed to update!";
        public const string DELETE_SUCCESSFULLY = "Deleted successfully!";
        public const string DELETE_FAILED = "Failed to delete!";

        // Status messages
        public const string NOT_FOUND = "Resource not found!";
        public const string BAD_REQUEST = "Invalid request!";
        public const string EMPTY_LIST = "The list is empty!";
        public const string ERROR_IN_SERVER = "Internal server error!";
    }

    // =====================================================
    // 2. VALIDATION MESSAGES (Common validation rules)
    // =====================================================
    public static class Validation
    {
        public const string EMPTY_NAME = "Name cannot be empty!";
        public const string LONG_NAME = "Name is too long!";
        public const string EMPTY_TYPE = "Type cannot be empty!";
        public const string NAME_ALREADY_EXISTS = "Name already exists!";
        public const string INVALID_AMOUNT = "Amount must be greater than 0!";
        public const string LONG_DESCRIPTION = "Description is too long (max 500 characters)!";
        public const string TYPE_ID_REQUIRED = "Type ID is required!";
    }

    // =====================================================
    // 3. ADMIN MANAGEMENT MESSAGES
    // =====================================================
    public static class AdminManagement
    {
        // Amenity Messages
        public static class Amenity
        {
            public const string EMPTY_NAME = "Amenity name is required!";
            public const string LONG_NAME = "Amenity name is too long (max 20 characters)!";
            public const string EMPTY_TYPE = "Amenity type is required!";
        }

        // Policy Messages
        public static class Policy
        {
            public const string EMPTY_NAME = "Policy name is required!";
            public const string LONG_NAME = "Policy name is too long (max 50 characters)!";
            public const string EMPTY_TYPE = "Policy type is required!";
            public const string INVALID_TYPE = "Invalid policy type!";
            public const string INVALID_AMOUNT = "Amount must be greater than 0!";
            public const string INVALID_DIFF_TIME = "Standard time slots must be at least 1 hour apart!";
            public const string MAX_DIFF_TIME = "Time difference cannot exceed 23 hours!";
            public const string INVALID_AGE_RANGE = "Invalid age range!";
            public const string TYPE_MISMATCH = "TypeId mismatch for the selected policy!";

            public const string EMPTY_CHECKIN_TIME = "Check-in time is required!";
            public const string EMPTY_CHECKOUT_TIME = "Check-out time is required!";
            public const string INVALID_FEE = "Fee must be greater than or equal to 0!";
            public const string INVALID_PERCENT = "Refund percent must be between 0 and 100!";
            public const string AGE_RANGE_INVALID = "Max age must be greater than or equal to min age!";
        }

        // Service Messages
        public static class Service
        {
            public const string EMPTY_NAME = "Service name is required!";
            public const string INVALID_TYPE = "Invalid service type!";
            public const string EMPTY_UNIT_NAME = "Unit of measurement is required!";
            public const string LONG_UNIT = "Unit name is too long (max 20 characters)!";
            public const string REQUIRED_PRICE = "Service price must be greater than 1,000!";
            public const string STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO = "Standard service price must be >= 10,000!";
            public const string MIN_PASSENGERS = "Passengers must be at least 1!";
            public const string MAX_PASSENGERS = "Maximum capacity is 45 passengers!";
            public const string MIN_LUGGAGE = "Luggage count cannot be negative!";
            public const string MAX_LUGGAGE = "Maximum capacity is 45 luggage items!";
            public const string INVALID_ROUND_TRIP_PRICE = "Invalid round-trip price!";
            public const string DEFAULT_ADDITIONAL_FEE = "Additional fee must be at least";
            public const string MISSING_ADDITIONAL_FEE_START_TIME = "Night fee start time is required!";
            public const string MISSING_ADDITIONAL_FEE_END_TIME = "Night fee end time is required!";
            public const string ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT = "Night fee duration cannot exceed 12 hours!";
            public const string INVALID_ADDITIONAL_FEE_START_END_TIME = "Start and end times cannot be identical!";
        }

        // Room Attributes Messages
        public static class RoomAttribute
        {
            public static class BedType
            {
                public const string EMPTY_NAME = "Bed type name is required!";
                public const string LONG_NAME = "Bed type name is too long (max 50 characters)!";
                public const string INVALID_DEFAULT_CAPACITY = "Capacity must be between 1 and 10!";
                public const string INVALID_MIN_WIDTH = "Minimum width must be greater than 0!";
                public const string INVALID_MAX_WIDTH = "Maximum width must be greater than or equal to minimum width!";
            }

            public static class RoomView
            {
                public const string EMPTY_NAME = "Room view name is required!";
                public const string LONG_NAME = "Room view name is too long (max 20 characters)!";
                public const string NAME_ALREADY_EXISTS = "Room view name already exists!";
            }

            public static class RoomQuality
            {
                public const string EMPTY_NAME = "Room quality name is required!";
                public const string LONG_NAME = "Room quality name is too long (max 20 characters)!";
            }

            public static class UnitType
            {
                public const string EMPTY_NAME = "Unit type name is required!";
                public const string LONG_NAME = "Unit type name is too long (max 50 characters)!";
            }

            public static class Request
            {
                public const string INVALID_TYPE = "Invalid attribute type!";
                public const string MISSING_ROOM_QUALITY_TYPE = "Please select a Room Quality type!";
                public const string INVALID_TYPE_ID = "TypeId must be greater than 0!";
                public const string UNSUPPORTED_TYPE_ID_FILTER = "This attribute type does not support TypeId filtering (please set to null)!";
            }
        }

        // Role Messages
        public static class Role
        {
            public const string NOT_FOUND = "Role not found!";
            public const string ALREADY_EXISTS = "Role already exists!";
            public const string ADD_SUCCESS = "Role added successfully!";
            public const string ADD_FAILED = "Failed to add role!";
            public const string UPDATE_SUCCESS = "Role updated successfully!";
            public const string UPDATE_FAILED = "Failed to update role!";
            public const string DELETE_SUCCESS = "Role deleted successfully!";
            public const string DELETE_FAILED = "Failed to delete role!";
        }
    }

    // =====================================================
    // 4. USER MANAGEMENT MESSAGES
    // =====================================================
    public static class UserManagement
    {
        public static class Login
        {
            public const string FAIL = "Login failed!";
            public const string SUCCESS = "Login successful!";
            public const string USER_NOT_FOUND = "Account not found!";
            public const string PASSWORD_INCORRECT = "Incorrect password!";
            public const string INVALID_CREDENTIALS = "Invalid username or password!";
            public const string USER_BLOCKED = "Your account has been blocked! Please contact support.";
            public const string USER_DELETED = "Your account has been deleted! Please contact support.";
            public const string ERROR_IN_SERVER = "A server error occurred. Please try again later.";
        }

        public static class Register
        {
            public const string SUCCESS = "Account registered successfully!";
            public const string FAIL = "Account registration failed!";
            public const string USERNAME_EXIST = "Username already exists!";
            public const string EMAIL_EXIST = "Email already exists!";
            public const string INVALID_EMAIL = "Invalid email format!";

            // Password validation
            public const string SHORT_PASSWORD = "Password must be at least 8 characters long!";
            public const string EMPTY_PASSWORD = "Password is required!";
            public const string UPPERCASE_LETTER_PASSWORD = "Password must contain at least one uppercase letter!";
            public const string LOWERCASE_LETTER_PASSWORD = "Password must contain at least one lowercase letter!";
            public const string SPECIAL_CHARACTER_PASSWORD = "Password must contain at least one special character!";
        }

        public static class User
        {
            public const string NOT_FOUND = "User account not found!";
            public const string ALREADY_EXISTS = "User already exists!";
            public const string ADD_SUCCESS = "User added successfully!";
            public const string ADD_FAILED = "Failed to add user!";
            public const string UPDATE_SUCCESS = "User updated successfully!";
            public const string UPDATE_FAILED = "Failed to update user!";
            public const string DELETE_SUCCESS = "User deleted successfully!";
            public const string DELETE_FAILED = "Failed to delete user!";
        }
    }

    // =====================================================
    // 5. PAGINATION MESSAGES
    // =====================================================
    public static class Pagination
    {
        public const string MISSING_PAGE_INDEX = "Page index is required!";
        public const string INVALID_PAGE_INDEX = "Page index must be greater than 0!";
        public const string MISSING_PAGE_SIZE = "Page size is required!";
        public const string INVALID_PAGE_SIZE = "Page size must be greater than 0!";
        public const string PAGE_SIZE_TOO_LARGE = "Maximum page size is 100!";
    }

    // =====================================================
    // 6. REQUEST MANAGEMENT MESSAGES
    // =====================================================
    public static class RequestManagement
    {
        public static class Request
        {
            public const string NOT_FOUND = "Request not found!";
            public const string ERROR_LOADING_DETAILS = "Error loading request details!";
            public const string ERROR_LOADING_STATS = "Error loading statistics!";
            public const string ERROR_LOADING_PAGED = "Error loading paginated requests!";
            public const string NO_REQUESTS_FOUND = "No requests found!";
        }
        public static class UpgradeRequest
        {
            public const string NOT_FOUND = "Request not found!";
            public const string ERROR_LOADING_DETAILS = "Error loading request details!";
            public const string ERROR_LOADING_STATS = "Error loading statistics!";
            public const string ERROR_LOADING_PAGED = "Error loading paginated requests!";
            public const string NO_REQUESTS_FOUND = "No requests found!";

            public const string APPROVE_SUCCESS = "Request approved successfully!";
            public const string APPROVE_FAILED = "Failed to approve request!";
            public const string REJECT_SUCCESS = "Request rejected successfully!";
            public const string REJECT_FAILED = "Failed to reject request!";
        }
    }

    // =====================================================
    // BACKWARD COMPATIBILITY (Obsolete)
    // =====================================================
    [Obsolete("Use MessageResponse.Common.GET_SUCCESSFULLY")]
    public static string GET_SUCCESSFULLY => Common.GET_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.GET_FAILED")]
    public static string GET_FAILED => Common.GET_FAILED;
    [Obsolete("Use MessageResponse.Common.CREATE_SUCCESSFULLY")]
    public static string CREATE_SUCCESSFULLY => Common.CREATE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.CREATE_FAILED")]
    public static string CREATE_FAILED => Common.CREATE_FAILED;
    [Obsolete("Use MessageResponse.Common.UPDATE_SUCCESSFULLY")]
    public static string UPDATE_SUCCESSFULLY => Common.UPDATE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.UPDATE_FAILED")]
    public static string UPDATE_FAILED => Common.UPDATE_FAILED;
    [Obsolete("Use MessageResponse.Common.DELETE_SUCCESSFULLY")]
    public static string DELETE_SUCCESSFULLY => Common.DELETE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.DELETE_FAILED")]
    public static string DELETE_FAILED => Common.DELETE_FAILED;
    [Obsolete("Use MessageResponse.Common.NOT_FOUND")]
    public static string NOT_FOUND => Common.NOT_FOUND;
    [Obsolete("Use MessageResponse.Common.BAD_REQUEST")]
    public static string BAD_REQUEST => Common.BAD_REQUEST;
    [Obsolete("Use MessageResponse.Validation.NAME_ALREADY_EXISTS")]
    public static string NAME_ALREADY_EXISTS => Validation.NAME_ALREADY_EXISTS;
    [Obsolete("Use MessageResponse.Validation.LONG_NAME")]
    public static string LONG_NAME => Validation.LONG_NAME;
    [Obsolete("Use MessageResponse.Validation.EMPTY_NAME")]
    public static string EMPTY_NAME => Validation.EMPTY_NAME;
    [Obsolete("Use MessageResponse.Common.EMPTY_LIST")]
    public static string EMPTY_LIST => Common.EMPTY_LIST;
    [Obsolete("Use MessageResponse.Validation.EMPTY_TYPE")]
    public static string EMPTY_TYPE => Validation.EMPTY_TYPE;
    [Obsolete("Use MessageResponse.Common.ERROR_IN_SERVER")]
    public static string ERROR_IN_SERVER => Common.ERROR_IN_SERVER;

    [Obsolete("Use MessageResponse.UserManagement.Login")]
    public static class MessageLogin
    {
        public static string LOGIN_FAIL => UserManagement.Login.FAIL;
        public static string LOGIN_SUCCESS => UserManagement.Login.SUCCESS;
        public static string USER_NOT_FOUND => UserManagement.Login.USER_NOT_FOUND;
        public static string PASSWORD_INCORRECT => UserManagement.Login.PASSWORD_INCORRECT;
    }

    [Obsolete("Use MessageResponse.UserManagement.Register")]
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
        public static string LOWERCASE_LETTER_PASSWORD => UserManagement.Register.LOWERCASE_LETTER_PASSWORD;
        public static string SPECIAL_CHARACTER_PASSWORD => UserManagement.Register.SPECIAL_CHARACTER_PASSWORD;
    }
}