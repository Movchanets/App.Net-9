import { Link, NavLink } from 'react-router-dom'
import { useState } from 'react'
import { AuthModal } from './AuthModal'
import { useAuthStore } from '../store/authStore'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `px-3 py-2 rounded-md text-sm font-medium transition-colors ${
    isActive ? 'bg-brand text-white' : 'text-text-muted hover:text-text hover:bg-surface-card'
  }`

export function Header() {
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false)
  const { isAuthenticated, user, logout } = useAuthStore()

  return (
    <>
      <header className="sticky top-0 z-50 w-full border-b border-text/10 bg-surface-card/95 shadow">
        <div className="mx-auto flex h-16 w-full max-w-7xl items-center justify-between px-4">
          <Link to="/" className="flex items-center gap-2">
            <div className="h-8 w-8 rounded bg-brand" />
            <span className="text-lg font-semibold text-text">ShopNine</span>
          </Link>

          <nav className="hidden gap-1 md:flex">
            <NavLink to="/" className={navLinkClass} end>
              Головна
            </NavLink>
            <NavLink to="/about" className={navLinkClass}>
              Про нас
            </NavLink>
            <NavLink to="/contacts" className={navLinkClass}>
              Контакти
            </NavLink>
          </nav>

          <div className="flex items-center gap-2">
            {isAuthenticated ? (
              <div className="flex items-center gap-3">
                <span className="text-sm text-text">Привіт, {user?.name}</span>
                <button
                  onClick={logout}
                  className="rounded-md border border-text/20 px-4 py-2 text-sm text-text hover:bg-text/5"
                >
                  Вийти
                </button>
              </div>
            ) : (
              <button onClick={() => setIsAuthModalOpen(true)} className="btn-primary">
                Увійти
              </button>
            )}
          </div>
        </div>
      </header>

      <AuthModal isOpen={isAuthModalOpen} onClose={() => setIsAuthModalOpen(false)} />
    </>
  )
}


