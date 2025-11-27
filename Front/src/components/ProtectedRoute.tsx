import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

interface ProtectedRouteProps {
  children: React.ReactNode
  requiredRoles?: string[]
  requireAuth?: boolean
  redirectTo404?: boolean
}

export function ProtectedRoute({ children, requiredRoles, requireAuth = true, redirectTo404 = false }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuthStore()
  const location = useLocation()

  // If authentication is required but user is not authenticated
  if (requireAuth && !isAuthenticated) {
    return redirectTo404 ? <Navigate to="/404" replace /> : <Navigate to="/auth" state={{ from: location }} replace />
  }

  // If specific roles are required
  if (requiredRoles && requiredRoles.length > 0) {
    const userRoles = user?.roles || []
    const hasRequiredRole = requiredRoles.some(role => 
      userRoles.some(userRole => userRole.toLowerCase() === role.toLowerCase())
    )

    if (!hasRequiredRole) {
      return <Navigate to="/404" replace />
    }
  }

  return <>{children}</>
}
