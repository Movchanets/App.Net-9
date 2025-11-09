import { Link, NavLink } from 'react-router-dom'
import { useState } from 'react'
import { AuthModal } from './AuthModal'
import type { User } from '../store/authStore'
import { useAuthStore } from '../store/authStore'
import { SidenavMenu } from './SidenavMenu'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `px-3 py-2 rounded-md text-sm font-medium transition-colors ${
    isActive ? 'bg-brand text-white' : 'text-text-muted hover:text-text hover:bg-surface-card'
  }`

export function Header() {
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false)
  const [isSidenavOpen, setIsSidenavOpen] = useState(false)
  const { isAuthenticated, user, logout } = useAuthStore()

  return (
    <>
      <header className="sticky top-0 z-50 w-full border-b border-text/10 bg-surface-card/95 shadow">
  <div className="mx-auto flex h-20 w-full max-w-7xl items-center justify-between px-4">
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
                {/* Avatar + 'Кабінет' as a single button (opens sidenav) */}
                <button
                  type="button"
                  onClick={() => setIsSidenavOpen(true)}
                  aria-expanded={isSidenavOpen}
                  title="Мій профіль"
                  className="relative group inline-flex flex-col items-center no-underline px-1 py-0.5 rounded-md hover:bg-surface-card"
                >
                  <span className="h-9 w-9 inline-flex items-center justify-center rounded-full bg-surface-card text-sm font-medium ring-1 ring-white/10">
                    {(() => {
                      type UserWithImg = User & { img?: string }
                      const u = user as UserWithImg | null
                      return u && u.img ? (
                        <img src={u.img} alt="avatar" className="h-9 w-9 rounded-full object-cover" />
                      ) : (
                        <span className="text-sm text-text">
                          {user?.name
                            ? user.name
                                .split(' ')
                                .map((s) => s[0])
                                .slice(0, 2)
                                .join('')
                                .toUpperCase()
                            : 'U'}
                        </span>
                      )
                    })()}
                  </span>

                  <span className="mt-1 text-xs text-text-muted hidden sm:block transition-colors duration-150 group-hover:text-text group-hover:font-medium">
                    Кабінет
                  </span>
                </button>
                {/* Cart with label - make whole block clickable */}
                <Link
                  to="/cart"
                  className="relative group inline-flex flex-col items-center no-underline px-1 py-0.5 rounded-md hover:bg-surface-card"
                  aria-label="Кошик"
                >
                  <span className="h-9 w-9 inline-flex items-center justify-center text-lg text-text-muted group-hover:text-text">🛒</span>
                  <span className="mt-1 text-xs text-text-muted hidden sm:block transition-colors duration-150 group-hover:text-text group-hover:font-medium">
                    Кошик
                  </span>
                </Link>

                {/* Favorites with label - whole block clickable */}
                <Link
                  to="/favorites"
                  className="relative group inline-flex flex-col items-center no-underline px-1 py-0.5 rounded-md hover:bg-surface-card"
                  aria-label="Обране"
                >
                  <span className="h-9 w-9 inline-flex items-center justify-center text-lg text-text-muted group-hover:text-text">🤍</span>
                  <span className="mt-1 text-xs text-text-muted hidden sm:block transition-colors duration-150 group-hover:text-text group-hover:font-medium">
                    Обране
                  </span>
                </Link>

                

                <button
                  onClick={logout}
                  className="rounded-md border border-text/20 px-4 py-2 text-sm text-text hover:bg-text/5"
                >
                  Вийти
                </button>

                <SidenavMenu isOpen={isSidenavOpen} onClose={() => setIsSidenavOpen(false)} user={user as User | null} />
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


