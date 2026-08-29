// auth.types.ts
// Defines all TypeScript interfaces (data shapes) for the Auth feature.
// This file contains NO logic — only type definitions.

// --- REQUEST TYPES (data sent TO the backend) ---

// Data required to log in
export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

// Data required to register a new customer account
export interface RegisterRequest {
  username: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
}

// --- RESPONSE TYPES (data received FROM the backend) ---

// Data returned after a successful login
export interface LoginResponseData {
  accessToken: string;
  fullName: string;
  avatarUrl: string | null;
  roles: string[];
}

// Data returned after a successful registration
export interface RegisterResponseData {
  userId: number;
  username: string;
  email: string;
}

// Generic wrapper for ALL responses from the backend.
// T is the inner data type (e.g. LoginResponseData, RegisterResponseData)
export interface ApiResponse<T> {
  data: T;
  isSuccess: boolean;
  message: string;
  statusCode: number;
}
