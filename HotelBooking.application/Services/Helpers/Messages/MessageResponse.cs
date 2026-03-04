/// <summary>
/// Centralized collection of all response messages in the system.
/// Organized by domain: AdminManagement, UserManagement, Common.
/// Each domain contains nested classes grouped by functionality.
/// </summary>
public static class MessageResponse
{
    // =====================================================
    // 1. COMMON MESSAGES (shared across all features)
    // =====================================================
    public static class Common
    {
        // Basic CRUD messages
        public const string GET_SUCCESSFULLY = "Retrieved successfully!";
        public const string GET_FAILED = "Failed to retrieve!";
        public const string CREATE_SUCCESSFULLY = "Created successfully!";
        public const string CREATE_FAILED = "Failed to create!";
        public const string UPDATE_SUCCESSFULLY = "Updated successfully!";
        public const string UPDATE_FAILED = "Failed to update!";
        public const string DELETE_SUCCESSFULLY = "Deleted successfully!";
        public const string DELETE_FAILED = "Failed to delete!";

        // Status messages
        public const string NOT_FOUND = "Not found!";
        public const string BAD_REQUEST = "Invalid request!";
        public const string EMPTY_LIST = "The list is empty!";
        public const string ERROR_IN_SERVER = "An error occurred on the server!";
    }

    // =====================================================
    // 2. VALIDATION MESSAGES (shared validation messages)
    // =====================================================
    public static class Validation
    {
        public const string INVALID_MODULE = "Invalid management module!";
        public const string TYPE_ID_REQUIRED = "Please select a type (TypeId)!";
        public const string INVALID_TYPE_ID = "TypeId must be greater than 0!";
        public const string EMPTY_NAME = "Name must not be empty!";
        public const string LONG_NAME = "Name is too long!";
        public const string EMPTY_TYPE = "Type must not be empty!";
        public const string NAME_ALREADY_EXISTS = "Name already exists!";
        public const string INVALID_AMOUNT = "Amount must be at least 0 (Free)";
        public const string PERCENT_INVALID = "Percentage is invalid!";
        public const string LONG_DESCRIPTION = "Description is too long!";

        public static class Pagination
        {
            public const string MISSING_PAGE_INDEX = "Page number must not be empty!";
            public const string INVALID_PAGE_INDEX = "Page number must be greater than 0!";
            public const string MISSING_PAGE_SIZE = "Page size must not be empty!";
            public const string INVALID_PAGE_SIZE = "Page size must be greater than 0!";
            public const string PAGE_SIZE_TOO_LARGE = "Maximum page size is 100!";
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
            public const string EMPTY_NAME = "Amenity name must not be empty!";
            public const string LONG_NAME = "Amenity name is too long (max 20 characters)!";
            public const string EMPTY_TYPE = "Amenity type must not be empty!";
            public const string NAME_ALREADY_EXISTS = "Amenity name already exists!";
            public const string INVALID_TYPE = "Invalid amenity type!";
            public const string GREATER_THAN_ZERO = "Value must be greater than 0!";
        }

        // Policy Messages
        public static class Policy
        {
            public const string EMPTY_NAME = "Policy name must not be empty!";
            public const string LONG_NAME = "Policy name is too long (max 50 characters)!";
            public const string EMPTY_TYPE = "Policy type must not be empty!";
            public const string INVALID_AMOUNT = "Amount must be greater than 0!";
            public const string NAME_ALREADY_EXISTS = "Policy name already exists!";
            public const string INVALID_ID_BY_TYPE = "ID is not valid for this policy type!";
        }

        // Service Messages
        public static class Service
        {
            public const string NAME_ALREADY_EXISTS = "Service name already exists!";
            public const string LONG_NAME = "Service name is too long (max 50 characters)!";
            public const string EMPTY_NAME = "Service name must not be empty!";
            public const string INVALID_AMOUNT = "Service amount must be at least 0 (Free) ";
            public const string INVALID_ID_BY_TYPE = "ID is not valid for this service type!";
            public const string INVALID_TYPE = "Invalid service type!";
            public const string EMPTY_UNIT = "Unit must not be empty!";
            public const string EMPTY_UNIT_NAME = "Unit name must not be empty!";
            public const string LONG_UNIT = "Unit is too long (max 20 characters)!";
            public const string STANDARD_SERVICE_PRICE_GREATER_THAN_ZERO = "Standard service price must be at least 10,000!";
            public const string MIN_PASSENGERS = "Minimum number of passengers must be at least 1!";
            public const string MAX_PASSENGERS = "Maximum number of passengers must not exceed 45!";
            public const string MIN_LUGGAGE = "Minimum luggage count must be at least 1!";
            public const string MAX_LUGGAGE = "Maximum luggage count must not exceed 45!";
            public const string INVALID_ROUND_TRIP_PRICE = "Round-trip price must be at least 0 (Free)";
            public const string DEFAULT_ADDITIONAL_FEE = "Night surcharge must be greater than";
            public const string MISSING_ADDITIONAL_FEE_START_TIME = "Please enter the night surcharge start time!";
            public const string MISSING_ADDITIONAL_FEE_END_TIME = "Please enter the night surcharge end time!";
            public const string INVALID_ADDITIONAL_FEE_START_END_TIME = "Start time and end time must not be the same.";
            public const string ADDITIONAL_FEE_TIME_EXCEEDS_LIMIT = "Night surcharge time window must not exceed 12 hours.";
        }

        // Room Attributes Messages (BedType, RoomView, RoomQuality, UnitType)
        public static class RoomAttribute
        {
            // BedType Messages
            public static class BedType
            {
                public const string NAME_ALREADY_EXISTS = "Bed type name already exists!";
                public const string EMPTY_NAME = "Bed type name must not be empty!";
                public const string LONG_NAME = "Bed type name is too long (max 20 characters)!";
                public const string INVALID_DEFAULT_CAPACITY = "Invalid capacity, must be between 1 and 10 persons!";
                public const string INVALID_MIN_WIDTH = "Minimum width must be greater than 0!";
                public const string INVALID_MAX_WIDTH = "Maximum width must be greater than or equal to minimum width!";
            }

            // RoomView Messages
            public static class RoomView
            {
                public const string NAME_ALREADY_EXISTS = "Room view name already exists!";
                public const string EMPTY_NAME = "Room view name must not be empty!";
                public const string LONG_NAME = "Room view name is too long (max 20 characters)!";
            }

            // RoomQuality Messages
            public static class RoomQuality
            {
                public const string NAME_ALREADY_EXISTS = "Room quality name already exists!";
                public const string EMPTY_NAME = "Room quality name must not be empty!";
                public const string LONG_NAME = "Room quality name is too long (max 20 characters)!";
                public const string INVALID_SORT_ORDER = "Invalid sort order, must be between 0 and 10!";
            }

            // UnitType Messages
            public static class UnitType
            {
                public const string NAME_ALREADY_EXISTS = "Room type name already exists!";
                public const string EMPTY_NAME = "Room type name must not be empty!";
                public const string LONG_NAME = "Room type name is too long (max 20 characters)!";
            }

            // Request Validation Messages
            public static class Request
            {
                public const string INVALID_TYPE = "Invalid attribute type!";
                public const string MISSING_ROOM_QUALITY_TYPE = "Please select a Room Quality Type!";
                public const string INVALID_TYPE_ID = "TypeId must be greater than 0!";
                public const string UNSUPPORTED_TYPE_ID_FILTER = "This attribute type does not support filtering by TypeId (please set it to null)!";
                public const string PAGINATION_REQUIRED = "Pagination data is required!";
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
        // Login Messages
        public static class Login
        {
            public const string FAIL = "Login failed!";
            public const string SUCCESS = "Login successful!";
            public const string INVALID_CREDENTIALS = "Incorrect username or password!";
            public const string USER_BLOCKED = "Your account has been blocked! Please contact support.";
            public const string USER_DELETED = "Your account has been deleted! Please contact support.";
            public const string ERROR_IN_SERVER = "An error occurred on the server. Please try again later.";

            // Validation messages for login input (basic validation only)
            public const string EMPTY_USERNAME_OR_EMAIL = "Username or Email must not be empty!";
            public const string MAX_LENGTH_USERNAME_OR_EMAIL = "Username or Email must not exceed 255 characters!";
            public const string MAX_LENGTH_PASSWORD = "Password must not exceed 100 characters!";

            public static string USER_NOT_FOUND { get; set; }
        }

        // Register Messages
        public static class Register
        {
            public const string SUCCESS = "Account registered successfully!";
            public const string FAIL = "Failed to register account!";
            public const string USERNAME_EXIST = "Username already exists!";
            public const string EMAIL_EXIST = "Email already exists!";
            public const string INVALID_EMAIL = "Invalid email format!";

            // Phone validation messages
            public const string EMPTY_PHONE = "Phone number must not be empty!";
            public const string INVALID_PHONE = "Phone number must be exactly 10 digits!";

            // Password validation messages
            public const string SHORT_PASSWORD = "Password must be at least 8 characters!";
            public const string EMPTY_PASSWORD = "Password must not be empty!";
            public const string UPPERCASE_LETTER_PASSWORD = "Password must contain at least one uppercase letter!";
            public const string NUMBER_PASSWORD = "Password must contain at least one digit!";
            public const string LOWERCASE_LETTER_PASSWORD = "Password must contain at least one lowercase letter!";
            public const string SPECIAL_CHARACTER_PASSWORD = "Password must contain at least one special character!";
        }

        // User Management Messages
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
    // 5. REQUEST MANAGEMENT MESSAGES
    // =====================================================
    public static class RequestManagement
    {
        // Upgrade Request Messages
        public static class UpgradeRequest
        {
            // Get User for Upgrade
            public const string USER_INFO_RETRIEVED = "User information retrieved successfully!";
            public const string USER_NOT_FOUND = "User account not found!";

            // Create Request
            public const string REQUEST_CREATED_SUCCESS = "Request created successfully!";
            public const string REQUEST_CREATE_FAILED = "Failed to create request!";
            public const string USER_NOT_CUSTOMER = "Your account is not a customer account!";
            public const string PENDING_REQUEST_EXISTS = "A pending request already exists for this account!";

            // Cancel Request
            public const string REQUEST_CANCELLED_SUCCESS = "Request cancelled successfully!";
            public const string REQUEST_CANCEL_FAILED = "Failed to cancel request!";

            // Get All Requests
            public const string REQUESTS_RETRIEVED = "Request list retrieved successfully!";

            // Get Request By ID
            public const string REQUEST_RETRIEVED = "Request retrieved successfully!";
            public const string REQUEST_NOT_FOUND = "Request not found!";

            // Approve Request
            public const string REQUEST_APPROVED_SUCCESS = "Request approved successfully!";
            public const string REQUEST_APPROVE_FAILED = "Failed to approve request!";
            public const string REQUEST_STATUS_INVALID = "Request not found or not in pending status!";

            // Reject Request
            public const string REQUEST_REJECTED_SUCCESS = "Request rejected successfully!";
            public const string REQUEST_REJECT_FAILED = "Failed to reject request!";

            // Validation Messages
            public const string ADDRESS_REQUIRED = "Address must not be empty!";
            public const string ADDRESS_TOO_LONG = "Address must not exceed 500 characters!";
            public const string TAX_CODE_REQUIRED = "Tax code must not be empty!";
            public const string TAX_CODE_INVALID = "Tax code must be exactly 10 or 13 digits!";
        }

        /// <summary>
        /// Messages for hotel approval
        /// </summary>
        public static class HotelApproval
        {
            public const string HOTELS_RETRIEVED = "Hotel list retrieved successfully!";
            public const string HOTEL_RETRIEVED = "Hotel information retrieved successfully!";
            public const string HOTEL_NOT_FOUND = "Hotel not found!";
            public const string STATUS_INVALID = "Hotel is not in pending approval status!";
            public const string APPROVED_SUCCESS = "Hotel approved successfully!";
            public const string APPROVE_FAILED = "Failed to approve hotel!";
            public const string REJECTED_SUCCESS = "Hotel rejected successfully!";
            public const string REJECT_FAILED = "Failed to reject hotel!";
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
        public const string MISSING_PAGE_INDEX = "Page number must not be empty!";
        public const string INVALID_PAGE_INDEX = "Page number must be greater than 0!";
        public const string MISSING_PAGE_SIZE = "Page size must not be empty!";
        public const string INVALID_PAGE_SIZE = "Page size must be greater than 0!";
        public const string PAGE_SIZE_TOO_LARGE = "Maximum page size is 100 (DDoS protection)!";
    }

    // =====================================================
    // 7. MENU REQUEST MESSAGES
    // =====================================================
    public static class ManageMenu
    {
        public const string INVALID_MODULE = "Invalid module!";
    }

    // =====================================================
    // BACKWARD COMPATIBILITY (retained to avoid breaking existing code)
    // =====================================================
    [Obsolete("Use MessageResponse.Common.GET_SUCCESSFULLY instead")]
    public static string GET_SUCCESSFULLY => Common.GET_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.GET_FAILED instead")]
    public static string GET_FAILED => Common.GET_FAILED;
    [Obsolete("Use MessageResponse.Common.CREATE_SUCCESSFULLY instead")]
    public static string CREATE_SUCCESSFULLY => Common.CREATE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.CREATE_FAILED instead")]
    public static string CREATE_FAILED => Common.CREATE_FAILED;
    [Obsolete("Use MessageResponse.Common.UPDATE_SUCCESSFULLY instead")]
    public static string UPDATE_SUCCESSFULLY => Common.UPDATE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.UPDATE_FAILED instead")]
    public static string UPDATE_FAILED => Common.UPDATE_FAILED;
    [Obsolete("Use MessageResponse.Common.DELETE_SUCCESSFULLY instead")]
    public static string DELETE_SUCCESSFULLY => Common.DELETE_SUCCESSFULLY;
    [Obsolete("Use MessageResponse.Common.DELETE_FAILED instead")]
    public static string DELETE_FAILED => Common.DELETE_FAILED;
    [Obsolete("Use MessageResponse.Common.NOT_FOUND instead")]
    public static string NOT_FOUND => Common.NOT_FOUND;
    [Obsolete("Use MessageResponse.Common.BAD_REQUEST instead")]
    public static string BAD_REQUEST => Common.BAD_REQUEST;
    [Obsolete("Use MessageResponse.Validation.NAME_ALREADY_EXISTS instead")]
    public static string NAME_ALREADY_EXISTS => Validation.NAME_ALREADY_EXISTS;
    [Obsolete("Use MessageResponse.Validation.LONG_NAME instead")]
    public static string LONG_NAME => Validation.LONG_NAME;
    [Obsolete("Use MessageResponse.Validation.EMPTY_NAME instead")]
    public static string EMPTY_NAME => Validation.EMPTY_NAME;
    [Obsolete("Use MessageResponse.Common.EMPTY_LIST instead")]
    public static string EMPTY_LIST => Common.EMPTY_LIST;
    [Obsolete("Use MessageResponse.Validation.EMPTY_TYPE instead")]
    public static string EMPTY_TYPE => Validation.EMPTY_TYPE;
    [Obsolete("Use MessageResponse.Common.ERROR_IN_SERVER instead")]
    public static string ERROR_IN_SERVER => Common.ERROR_IN_SERVER;

    // Backward compatibility for old message classes
    [Obsolete("Use MessageResponse.UserManagement.Login instead")]
    public static class MessageLogin
    {
        public static string LOGIN_FAIL => UserManagement.Login.FAIL;
        public static string LOGIN_SUCCESS => UserManagement.Login.SUCCESS;
        public static string INVALID_CREDENTIALS => UserManagement.Login.INVALID_CREDENTIALS;
        public static string USER_BLOCKED => UserManagement.Login.USER_BLOCKED;
        public static string USER_DELETED => UserManagement.Login.USER_DELETED;
    }

    [Obsolete("Use MessageResponse.UserManagement.Register instead")]
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

    [Obsolete("Use MessageResponse.AdminManagement.Role instead")]
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
