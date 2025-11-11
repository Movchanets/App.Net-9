import { Routes, Route } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Layout } from './components/layout/Layout'
import Home from './pages/home/Home'
import About from './pages/info/About'
import Contacts from './pages/info/Contacts'
import Auth from './pages/auth/Auth'
import ResetPassword from './pages/reset/ResetPassword'
import SettingsPage from './pages/Cabinet/SettingsPage'
import Cabinet from './pages/Cabinet/Cabinet'
// no top-level Fragment needed here

export default function App() {
  const { t } = useTranslation()
  return (
    <Routes>
      <Route element={<Layout />}>
          <Route index element={<Home />} />
        <Route path="about" element={<About />} />
        <Route path="contacts" element={<Contacts />} />
        <Route path="auth" element={<Auth />} />
        <Route path="reset-password" element={<ResetPassword />} />

          <Route path="cabinet/*" element={<Cabinet />}>
          <Route index element={<div className="p-6">{t('greeting', { name: '' })}</div>} />
          <Route path="user/settings" element={<SettingsPage />} />
          <Route path="orders" element={<div className="p-6">{t('menu.orders')} ({t('common.empty')})</div>} />
          <Route path="tracking" element={<div className="p-6">{t('menu.tracking')} ({t('common.empty')})</div>} />
          <Route path="favorites" element={<div className="p-6">{t('menu.favorites')} ({t('common.empty')})</div>} />
          <Route path="wallet" element={<div className="p-6">{t('menu.wallet')} ({t('common.empty')})</div>} />
          <Route path="support" element={<div className="p-6">{t('menu.support')} ({t('common.empty')})</div>} />
          <Route path="help" element={<div className="p-6">{t('menu.help')} ({t('common.empty')})</div>} />
        </Route>
      </Route>
    </Routes>
  )
}
