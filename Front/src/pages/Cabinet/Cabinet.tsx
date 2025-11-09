import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../store/authStore'
import { useState } from 'react'
import LanguageSelector from '../../components/LanguageSelector'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 px-4 py-3 text-sm rounded-md transition-colors ${
    isActive ? 'bg-brand text-white' : 'text-text-muted hover:text-text hover:bg-surface-card'
  }`

export default function Cabinet() {
  const { logout, user } = useAuthStore()
  const navigate = useNavigate()
  const [confirmOpen, setConfirmOpen] = useState(false)
  return (
  <div className="min-h-screen bg-bg dark:bg-[#071428]">
      <div className="max-w-7xl mx-auto p-6">
        <div className="bg-transparent rounded-md overflow-hidden shadow-sm">
          <div className="flex">
            {/* left persistent sidenav */}
            <aside className="w-64 bg-white dark:bg-[#0b1228] dark:border-surface/30 border-r border-surface/40 p-4 text-text dark:text-gray-100 flex flex-col">
              <div className="mb-4 flex items-center gap-3">
                <div className="h-10 w-10 rounded-full bg-violet-200 dark:bg-violet-600 flex items-center justify-center text-sm font-semibold text-white">BM</div>
                <div>
                  <div className="text-sm font-medium">{user?.name || 'Користувач'}</div>
                  <div className="text-xs text-text-muted">{user?.email || ''}</div>
                </div>
              </div>

              <nav className="flex flex-col gap-1 flex-1 overflow-auto">
                <NavLink to="/cabinet/orders" className={linkClass}>
                  <span>📦</span>
                  Мої замовлення
                </NavLink>
                <NavLink to="/cabinet/tracking" className={linkClass}>
                  <span>🚚</span>
                  Відстеження
                </NavLink>
                <NavLink to="/cabinet/favorites" className={linkClass}>
                  <span>🤍</span>
                  Обране
                </NavLink>
                <NavLink to="/cabinet/wallet" className={linkClass}>
                  <span>👛</span>
                  Мій гаманець
                </NavLink>
                <NavLink to="/cabinet/user/settings?tab=profile" className={linkClass}>
                  <span>⚙️</span>
                  Налаштування
                </NavLink>
                <NavLink to="/cabinet/support" className={linkClass}>
                  <span>📞</span>
                  Підтримка
                </NavLink>
                <NavLink to="/cabinet/help" className={linkClass}>
                  <span>❓</span>
                  Довідка
                </NavLink>
              </nav>

              <div className="mt-4">
                <LanguageSelector align="left" />
              </div>

              <div className="mt-4">
                <button
                  onClick={() => setConfirmOpen(true)}
                  className="w-full rounded-md border border-text/20 px-4 py-2 text-sm text-text hover:bg-text/5"
                >
                  Вийти
                </button>
              </div>
            </aside>

            {/* confirm modal for cabinet logout */}
            {confirmOpen && (
              <div className="fixed inset-0 z-60 flex items-center justify-center">
                <div className="absolute inset-0 bg-black/40" onClick={() => setConfirmOpen(false)} />
                <div className="relative z-10 w-full max-w-md rounded-md bg-white dark:bg-[#071428] p-6 shadow-lg">
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
                        setConfirmOpen(false)
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

            {/* main content area with tabs (child routes render here) */}
            <main className="flex-1 p-6 bg-surface">
              <Outlet />
            </main>
          </div>
        </div>
      </div>
    </div>
  )
}
