import { NavLink, useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import LanguageSelector from './LanguageSelector'
import type { User } from '../store/authStore'
import { useAuthStore } from '../store/authStore'

interface SidenavMenuProps {
  isOpen: boolean
  onClose: () => void
  user?: User | null
}

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 px-4 py-3 text-sm rounded-md transition-colors ${
    isActive ? 'bg-brand text-white' : 'text-text-muted hover:text-text hover:bg-surface-card'
  }`

export function SidenavMenu({ isOpen, onClose, user }: SidenavMenuProps) {
  // Keep mounted during close animation
  const [mounted, setMounted] = useState(isOpen)
  const [closing, setClosing] = useState(false)

  useEffect(() => {
    if (isOpen) {
      setMounted(true)
      setClosing(false)
    } else if (mounted) {
      setClosing(true)
      const t = setTimeout(() => {
        setMounted(false)
        setClosing(false)
      }, 220)
      return () => clearTimeout(t)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen])

  useEffect(() => {
    if (!mounted) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [mounted, onClose])

  const { logout } = useAuthStore()
  const navigate = useNavigate()

  const [confirmOpen, setConfirmOpen] = useState(false)

  if (!mounted) return null

  const initials = user?.name
    ? user.name
        .split(' ')
        .map((s) => s[0])
        .slice(0, 2)
        .join('')
        .toUpperCase()
    : 'U'

  const isVisible = isOpen && !closing

  return (
    <div className="fixed inset-0 z-50">
      {/* overlay with fade */}
      <div
        onClick={onClose}
        className={`absolute inset-0 bg-black/40 transition-opacity duration-200 ${isVisible ? 'opacity-100' : 'opacity-0'}`}
      />

      {/* panel sliding in from right */}
      <aside
        className={`absolute right-0 top-0 h-full w-72 bg-surface-card p-4 shadow-xl transform transition-transform duration-200 flex flex-col ${
          isVisible ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="mb-6 flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-full bg-surface/80 flex items-center justify-center text-lg font-semibold">
            {(() => {
              type UserWithImg = User & { img?: string }
              const u = user as UserWithImg | undefined
              return u && u.img ? (
                <img src={u.img} alt="avatar" className="h-12 w-12 rounded-full object-cover" />
              ) : (
                <span className="text-text">{initials}</span>
              )
            })()}
          </div>
          <div>
            <div className="text-sm font-medium text-text">{user?.name || 'Користувач'}</div>
            <div className="text-xs text-text-muted">{user?.email || ''}</div>
          </div>
        </div>

  <nav className="flex flex-col gap-1">
          <NavLink to="/cabinet/orders" className={linkClass} onClick={onClose}>
            <span>📦</span>
            Мої замовлення
          </NavLink>

          <NavLink to="/cabinet/tracking" className={linkClass} onClick={onClose}>
            <span>🚚</span>
            Відстеження
          </NavLink>

          <NavLink to="/cabinet/favorites" className={linkClass} onClick={onClose}>
            <span>🤍</span>
            Обране
          </NavLink>

          <NavLink to="/cabinet/wallet" className={linkClass} onClick={onClose}>
            <span>👛</span>
            Мій гаманець
          </NavLink>

          {/* <NavLink to="/discounts" className={linkClass} onClick={onClose}>
            <span>🏷️</span>
            Знижки та бонуси
          </NavLink> */}

          <NavLink to="/cabinet/user/settings?tab=profile" className={linkClass} onClick={onClose}>
            <span>⚙️</span>
            Налаштування
          </NavLink>

          {/* quick shortcut into specific settings tab */}
          <NavLink to="/cabinet/user/settings?tab=security" className={linkClass} onClick={onClose}>
            <span>🔒</span>
            Логін та пароль
          </NavLink>

          <NavLink to="/cabinet/support" className={linkClass} onClick={onClose}>
            <span>📞</span>
            Підтримка
          </NavLink>

          <NavLink to="/cabinet/help" className={linkClass} onClick={onClose}>
            <span>❓</span>
            Довідка
          </NavLink>
        </nav>

        {/* language selector */}
        <div className="mt-4">
          <LanguageSelector className="mb-3" />
        </div>

        {/* logout button placed inside the aside so it's reachable */}
        <div className="mt-auto">
          <button
            onClick={() => setConfirmOpen(true)}
            className="w-full rounded-md border border-text/20 px-4 py-2 text-sm text-text hover:bg-text/5 bg-white dark:bg-[#071428]"
          >
            Вийти
          </button>
        </div>
      </aside>
  {/* Confirm logout modal */}
      {confirmOpen && (
        <div className="fixed inset-0 z-60 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/40" onClick={() => setConfirmOpen(false)} />
          <div
            role="dialog"
            aria-modal="true"
            className="relative z-10 w-full max-w-md rounded-md bg-white dark:bg-[#071428] p-6 shadow-lg"
          >
            <h3 className="text-lg font-semibold text-text dark:text-white">Підтвердіть вихід</h3>
            <p className="mt-2 text-sm text-text-muted dark:text-text-muted/80">Ви дійсно хочете вийти з облікового запису?</p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                onClick={() => setConfirmOpen(false)}
                className="rounded-md px-3 py-2 text-sm bg-transparent text-text-muted hover:text-text"
              >
                Скасувати
              </button>
              <button
                onClick={() => {
                  logout()
                  onClose()
                  navigate('/')
                }}
                className="rounded-md bg-brand px-3 py-2 text-sm text-white"
              >
                Вийти
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default SidenavMenu
