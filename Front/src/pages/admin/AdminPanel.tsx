import { useAuthStore } from '../../store/authStore'

export default function AdminPanel() {
  const { user } = useAuthStore()

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-text">Admin Panel</h1>
        <p className="text-text-muted mt-2">Welcome, {user?.firstName || user?.name}!</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-text mb-2">Users</h3>
          <p className="text-text-muted text-sm">Manage user accounts</p>
        </div>
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-text mb-2">Orders</h3>
          <p className="text-text-muted text-sm">View all orders</p>
        </div>
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-text mb-2">Settings</h3>
          <p className="text-text-muted text-sm">System configuration</p>
        </div>
      </div>

      <div className="card p-6">
        <h3 className="text-lg font-semibold text-text mb-4">Your Roles</h3>
        <div className="flex flex-wrap gap-2">
          {user?.roles && user.roles.length > 0 ? (
            user.roles.map((role, idx) => (
              <span
                key={idx}
                className="px-3 py-1 rounded-full bg-brand/10 text-brand text-sm font-medium"
              >
                {role}
              </span>
            ))
          ) : (
            <span className="text-text-muted text-sm">No roles assigned</span>
          )}
        </div>
      </div>

      <div className="card p-6">
        <h3 className="text-lg font-semibold text-text mb-4">Your Permissions</h3>
        <div className="flex flex-wrap gap-2">
          {user?.permissions && user.permissions.length > 0 ? (
            user.permissions.map((permission, idx) => (
              <span
                key={idx}
                className="px-3 py-1 rounded-full bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300 text-sm font-medium"
              >
                {permission}
              </span>
            ))
          ) : (
            <span className="text-text-muted text-sm">No permissions assigned</span>
          )}
        </div>
      </div>
    </div>
  )
}
