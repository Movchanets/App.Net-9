import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import { useEffect } from 'react'

const tabs: { id: string; label: string }[] = [
  { id: 'profile', label: 'Профіль' },
  { id: 'security', label: 'Логін та пароль' },
  { id: 'notifications', label: 'Сповіщення' },
  { id: 'payments', label: 'Платежі' },
]

export default function SettingsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const navigate = useNavigate()

  // Choose active tab from ?tab=... default to profile
  const active = searchParams.get('tab') || 'profile'

  useEffect(() => {
    // Ensure a tab is present in URL; if not, push default
    if (!searchParams.get('tab')) {
      setSearchParams({ tab: 'profile' }, { replace: true })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onSelect = (id: string) => {
    setSearchParams({ tab: id })
    // keep user in the cabinet settings route
    navigate({ pathname: '/cabinet/user/settings', search: `?tab=${id}` })
  }

  return (
    <div className="p-6 text-text dark:text-white">
      <h1 className="text-2xl font-semibold mb-4 text-text dark:text-white">Налаштування</h1>

      <div className="mb-6 border-b border-surface/60 dark:border-surface/30">
        <nav className="flex gap-2 -mb-px">
          {tabs.map((t) => (
            <button
              key={t.id}
              onClick={() => onSelect(t.id)}
              className={`px-4 py-2 text-sm rounded-t-md border-b-2 transition-colors ${
                active === t.id
                  ? 'border-brand text-brand bg-surface dark:bg-surface/5' 
                  : 'border-transparent text-text-muted dark:text-text-muted/80 hover:text-text dark:hover:text-white'
              }`}
              aria-current={active === t.id ? 'page' : undefined}
            >
              {t.label}
            </button>
          ))}
        </nav>
      </div>

      <div className="bg-white dark:bg-[#071428] p-6 rounded-md shadow-sm text-text dark:text-white">
        {active === 'profile' && (
          <section>
            <h2 className="text-lg font-medium mb-2 text-text dark:text-white">Інформація профілю</h2>
            <p className="text-sm text-text-muted dark:text-text-muted/80">Тут можна змінити ім'я, емейл та фото профілю.</p>
          </section>
        )}

        {active === 'security' && (
          <section>
            <h2 className="text-lg font-medium mb-2">Безпека</h2>
            <p className="text-sm text-text-muted">Тут можна змінити пароль або включити двофакторну аутентифікацію.</p>
          </section>
        )}

        {active === 'notifications' && (
          <section>
            <h2 className="text-lg font-medium mb-2">Сповіщення</h2>
            <p className="text-sm text-text-muted">Налаштування email та push-сповіщень.</p>
          </section>
        )}

        {active === 'payments' && (
          <section>
            <h2 className="text-lg font-medium mb-2">Платежі</h2>
            <p className="text-sm text-text-muted">Керування платіжними методами та історією.</p>
          </section>
        )}

        <div className="mt-6">
          <Link to="/cabinet" className="text-sm text-brand hover:underline">
            Повернутись до кабінету
          </Link>
        </div>
      </div>
    </div>
  )
}
