import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import Home from './pages/Home'
import About from './pages/About'
import Contacts from './pages/Contacts'
import Auth from './pages/Auth'
import ResetPassword from './pages/ResetPassword'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Home />} />
        <Route path="about" element={<About />} />
        <Route path="contacts" element={<Contacts />} />
        <Route path="auth" element={<Auth />} />
        <Route path="reset-password" element={<ResetPassword />} />
      </Route>
    </Routes>
  )
}
