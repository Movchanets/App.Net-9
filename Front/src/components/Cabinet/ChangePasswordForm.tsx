import React, { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { userApi } from '../../api/userApi'

export default function ChangePasswordForm() {
  const { t } = useTranslation()
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setSuccess(null)

    if (!currentPassword || !newPassword) {
      setError(t('settings.security.errors.fill_all'))
      return
    }
    if (newPassword !== confirm) {
      setError(t('settings.security.errors.password_mismatch'))
      return
    }

    setSaving(true)
    try {
      await userApi.changePassword({ currentPassword, newPassword })
      setSuccess(t('settings.security.change_success'))
      setCurrentPassword('')
      setNewPassword('')
      setConfirm('')
      window.setTimeout(() => setSuccess(null), 3000)
    } catch (err) {
      let msg = t('errors.save_failed')
      if (err instanceof Error) msg = err.message
      else if (typeof err === 'string') msg = err
      setError(msg)
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {error && <div className="text-sm text-red-600">{error}</div>}
      {success && <div className="text-sm text-green-600">{success}</div>}

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.security.current_password')}</label>
        <input
          type="password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.security.new_password')}</label>
        <input
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.security.confirm_password')}</label>
        <input
          type="password"
          value={confirm}
          onChange={(e) => setConfirm(e.target.value)}
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      <div className="flex items-center gap-2">
        <button
          type="submit"
          disabled={saving}
          className="px-4 py-2 bg-brand text-white rounded-md disabled:opacity-60"
        >
          {saving ? t('saving') : t('settings.security.change_password')}
        </button>
      </div>
    </form>
  )
}
