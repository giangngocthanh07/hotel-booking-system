// auth.types.ts
// Dinh nghia tat ca cac kieu du lieu (TypeScript interfaces) cho tinh nang Auth
// File nay KHONG chua logic, chi chua "hinh dang" cua du lieu

// --- REQUEST TYPES (du lieu gui LEN backend) ---

// Kieu du lieu de dang nhap
export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

// Kieu du lieu de dang ky tai khoan khach hang
export interface RegisterRequest {
  username: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
}

// --- RESPONSE TYPES (du lieu nhan VE tu backend) ---

// Du lieu tra ve sau khi dang nhap thanh cong
export interface LoginResponseData {
  accessToken: string;
  fullName: string;
  avatarUrl: string | null;
  roles: string[];
}

// Du lieu tra ve sau khi dang ky thanh cong
export interface RegisterResponseData {
  userId: number;
  username: string;
  email: string;
}

// Wrapper chung cho MOI response tu backend
// T la kieu du lieu ben trong (vi du: LoginResponseData, RegisterResponseData)
export interface ApiResponse<T> {
  data: T;
  isSuccess: boolean;
  message: string;
  statusCode: number;
}
