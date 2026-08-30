import { Navigate, Outlet } from "react-router-dom";
import { getStoredRoles } from "../../services/authService";

interface ProtectedRouteProps {
  allowedRoles: string[];
}

export default function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const roles = getStoredRoles();
  
  // If user has no roles, or does not have at least one of the allowed roles
  const hasRole = roles.some(r => allowedRoles.includes(r));
  
  if (!hasRole) {
    // Hide behind 404 (Zero Trust Route Hiding)
    return <Navigate to="/404" replace />;
  }

  return <Outlet />;
}
