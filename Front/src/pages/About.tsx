import { useTranslation } from 'react-i18next'

export default function About() {
  const { t } = useTranslation()

  return (
    <div className="prose prose-invert max-w-none">
      <h1>{t('site.about.title')}</h1>
      <p>{t('site.about.description')}</p>
      <h2>Наші принципи</h2>
      <ul>
        <li>Якість і прозорість цін.</li>
        <li>Швидка доставка і зручний сервіс.</li>
        <li>{t('support.tagline')}</li>
      </ul>
    </div>
  )
}


