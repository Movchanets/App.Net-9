import { NavLink, useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import LanguageSelector from '../ui/LanguageSelector'
import type { User } from '../../store/authStore'
import { useAuthStore } from '../../store/authStore'

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
  const { t } = useTranslation()
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
      <div
        onClick={onClose}
        className={`absolute inset-0 bg-black/40 transition-opacity duration-200 ${isVisible ? 'opacity-100' : 'opacity-0'}`}
      />

      <aside
        className={`absolute right-0 top-0 h-full w-72 bg-surface-card p-4 shadow-xl transform transition-transform duration-200 flex flex-col ${
          isVisible ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="mb-6 flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-full bg-surface/80 flex items-center justify-center text-lg font-semibold">
              {(() => {
                const img = user?.picture
                return img ? (
                  <img src={img} alt={user?.name ?? 'avatar'} className="h-12 w-12 rounded-full object-cover" />
                ) : (
                  <span className="text-text">{initials}</span>
                )
              })()}
            </div>
          <div>
            <div className="text-sm font-medium text-text">{user?.firstName + ' ' + user?.lastName || 'Користувач'}</div>
            <div className="text-xs text-text-muted">{user?.email || ''}</div>
          </div>
        </div>

        <nav className="flex flex-col gap-1">
          <NavLink to="/cabinet/orders" className={linkClass} onClick={onClose}>
            <span>📦</span>
            {t('menu.orders')}
          </NavLink>

          <NavLink to="/cabinet/tracking" className={linkClass} onClick={onClose}>
            <span>🚚</span>
            {t('menu.tracking')}
          </NavLink>

          <NavLink to="/cabinet/favorites" className={linkClass} onClick={onClose}>
            <span>🤍</span>
            {t('menu.favorites')}
          </NavLink>

          <NavLink to="/cabinet/wallet" className={linkClass} onClick={onClose}>
            <span>👛</span>
            {t('menu.wallet')}
          </NavLink>

          <NavLink to="/cabinet/user/settings?tab=profile" className={linkClass} onClick={onClose}>
            <span>⚙️</span>
            {t('menu.settings')}
          </NavLink>

          <NavLink to="/cabinet/user/settings?tab=security" className={linkClass} onClick={onClose}>
            <span>🔒</span>
            {t('menu.security')}
          </NavLink>

          <NavLink to="/cabinet/support" className={linkClass} onClick={onClose}>
            <span>📞</span>
            {t('menu.support')}
          </NavLink>

          <NavLink to="/cabinet/help" className={linkClass} onClick={onClose}>
            <span>❓</span>
            {t('menu.help')}
          </NavLink>
        </nav>

        <div className="mt-4">
          <LanguageSelector className="mb-3" />
        </div>

        <div className="mt-auto">
          <button
            onClick={() => setConfirmOpen(true)}
            className="w-full rounded-md border border-text/20 px-4 py-2 text-sm text-text hover:bg-text/5 bg-white dark:bg-[#071428]"
          >
            {t('logout')}
          </button>
        </div>
      </aside>

      {confirmOpen && (
        <div className="fixed inset-0 z-60 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/40" onClick={() => setConfirmOpen(false)} />
          <div
            role="dialog"
            aria-modal="true"
            className="relative z-10 w-full max-w-md rounded-md bg-white dark:bg-[#071428] p-6 shadow-lg"
          >
            <h3 className="text-lg font-semibold text-text dark:text-white">{t('confirm_logout_title')}</h3>
            <p className="mt-2 text-sm text-text-muted dark:text-text-muted/80">{t('confirm_logout_text')}</p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                onClick={() => setConfirmOpen(false)}
                className="rounded-md px-3 py-2 text-sm bg-transparent text-text-muted hover:text-text"
              >
                {t('cancel')}
              </button>
              <button
                onClick={() => {
                  logout()
                  onClose()
                  navigate('/')
                }}
                className="rounded-md bg-brand px-3 py-2 text-sm text-white"
              >
                {t('logout')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default SidenavMenu
