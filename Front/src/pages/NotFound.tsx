import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

export default function NotFound() {
  const navigate = useNavigate()
  const { t } = useTranslation()

  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <div className="text-center space-y-6 px-4">
        <div className="space-y-2">
          <h1 className="text-9xl font-bold text-brand">404</h1>
          <h2 className="text-2xl font-semibold text-text">{t('notfound.title')}</h2>
          <p className="text-text-muted max-w-md mx-auto">{t('notfound.description')}</p>
        </div>
        
        <div className="flex gap-3 justify-center">
          <button
            onClick={() => navigate('/')}
            className="btn-primary"
          >
            {t('notfound.go_home')}
          </button>
          <button
            onClick={() => navigate(-1)}
            className="rounded-md border border-text/20 px-4 py-2 text-text hover:bg-text/5"
          >
            {t('notfound.go_back')}
          </button>
        </div>
      </div>
    </div>
  )
}
