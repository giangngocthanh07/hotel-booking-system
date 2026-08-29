# Auth Pages (Login & Register) — Design Spec

**Date:** 2026-08-29
**Feature:** Authentication UI — Login & Register pages
**Frontend:** React + TypeScript (HotelBooking.frontend)
**Backend:** .NET API (HotelBooking.api)

## 1. Goals
- /register — Customer registration
- /login — User login
Both pages connect to existing BE endpoints and use a sunset color theme (blue + pink + white).

## 2. API Contracts
### Register: POST /api/v1/auth/register
Request: { username, fullName, email, phoneNumber, password, confirmPassword }

### Login: POST /api/v1/auth/login
Request: { usernameOrEmail, password }
Response: { data: { accessToken, fullName, avatarUrl, roles[] }, isSuccess }

## 3. Architecture (Container/Presentational Pattern)
- types/auth.types.ts — TypeScript interfaces
- services/authService.ts — fetch() calls to BE API
- pages/LoginPage.tsx — State, event handlers, calls service
- pages/RegisterPage.tsx — State, event handlers, calls service
- components/auth/LoginForm.tsx — Renders form UI, receives props
- components/auth/RegisterForm.tsx — Renders form UI, receives props
- App.tsx — React Router setup

## 4. Color Theme (Sunset)
- Blue: #3B82F6 (buttons, focus)
- Pink: #EC4899 (accents)
- Background gradient: #EFF6FF to #FDF2F8

## 5. Code Principles
- Use for loops, not map()/filter()
- Use plain fetch(), not axios
- Use useState and async/await
