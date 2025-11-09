import { Routes, Route } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Layout } from './components/Layout'
import Home from './pages/Home'
import About from './pages/About'
import Contacts from './pages/Contacts'
import Auth from './pages/Auth'
import ResetPassword from './pages/ResetPassword'
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
          <Route path="orders" element={<div className="p-6">{t('menu.orders')} (порожньо)</div>} />
          <Route path="tracking" element={<div className="p-6">{t('menu.tracking')} (порожньо)</div>} />
          <Route path="favorites" element={<div className="p-6">{t('menu.favorites')} (порожньо)</div>} />
          <Route path="wallet" element={<div className="p-6">{t('menu.wallet')} (порожньо)</div>} />
          <Route path="support" element={<div className="p-6">{t('menu.support')} (порожньо)</div>} />
          <Route path="help" element={<div className="p-6">{t('menu.help')} (порожньо)</div>} />
        </Route>
      </Route>
    </Routes>
  )
}
