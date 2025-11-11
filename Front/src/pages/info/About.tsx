import { useTranslation } from 'react-i18next'

export default function About() {
  const { t } = useTranslation()

  return (
    <div className="prose prose-invert max-w-none">
      <h1>{t('site.about.title')}</h1>
      <p>{t('site.about.description')}</p>
      <h2>{t('site.principles_title')}</h2>
      <ul>
        <li>{t('site.principle1')}</li>
        <li>{t('site.principle2')}</li>
        <li>{t('support.tagline')}</li>
      </ul>
    </div>
  )
}
