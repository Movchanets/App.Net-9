import { useEffect, useState } from 'react'
import type { Resolver } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useProfileStore } from '../../store/profileStore'

import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import * as yup from 'yup'

type InfoFormValues = { name: string; surname: string; username: string }
type PhoneFormValues = { phone: string }
type EmailFormValues = { email: string }

export default function ProfileForm() {
  const { t } = useTranslation()
  const { profile, fetchProfile, updateInfo, updatePhone, updateEmail } = useProfileStore()
  const [success, setSuccess] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  // build validation schema using localized messages
  const infoSchema = yup.object({
    name: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
    surname: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
    username: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
  })

  const phoneSchema = yup.object({
    phone: yup
      .string()
      .required(t('validation.required'))
      .matches(/^[+]?[0-9]{7,15}$/, t('validation.phone')),
  })

  const emailSchema = yup.object({
    email: yup.string().required(t('validation.required')).email(t('validation.email')),
  })

  const infoDefaults: InfoFormValues = {
    name: typeof profile?.name === 'string' ? profile!.name : '',
    surname: typeof profile?.surname === 'string' ? profile!.surname : '',
    username: typeof (profile as any)?.username === 'string' ? (profile as any).username : '',
  }
  const phoneDefaults: PhoneFormValues = { phone: typeof profile?.phoneNumber === 'string' ? profile!.phoneNumber : '' }
  const emailDefaults: EmailFormValues = { email: typeof profile?.email === 'string' ? (profile as any).email : '' }

  const {
    register: registerInfo,
    handleSubmit: handleSubmitInfo,
    reset: resetInfo,
    formState: { errors: infoErrors, isSubmitting: infoSubmitting },
  } = useForm<InfoFormValues>({ resolver: yupResolver(infoSchema) as unknown as Resolver<InfoFormValues>, defaultValues: infoDefaults })

  const {
    register: registerPhone,
    handleSubmit: handleSubmitPhone,
    reset: resetPhone,
    formState: { errors: phoneErrors, isSubmitting: phoneSubmitting },
  } = useForm<PhoneFormValues>({ resolver: yupResolver(phoneSchema) as unknown as Resolver<PhoneFormValues>, defaultValues: phoneDefaults })

  const {
    register: registerEmail,
    handleSubmit: handleSubmitEmail,
    reset: resetEmail,
    formState: { errors: emailErrors, isSubmitting: emailSubmitting },
  } = useForm<EmailFormValues>({ resolver: yupResolver(emailSchema) as unknown as Resolver<EmailFormValues>, defaultValues: emailDefaults })

  // Fetch profile if we don't have it yet
  useEffect(() => {
    if (!profile) {
      fetchProfile().catch(() => setError(t('errors.fetch_failed')))
    }
  }, [profile, fetchProfile, t])

  // When profile becomes available (or changes), ensure form values are in sync.
  // We'll call reset as a fallback for already-mounted forms, but also
  // force a remount via formKey so defaultValues are applied on mount.
  useEffect(() => {
    if (!profile) return
    resetInfo(infoDefaults)
    resetPhone(phoneDefaults)
    resetEmail(emailDefaults)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profile])

  const onSubmitInfo = async (data: InfoFormValues) => {
    setError(null)
    setSuccess(null)
    try {
      const updated = await updateInfo({ name: data.name, surname: data.surname, username: data.username })
      if (updated) resetInfo({ ...data })
      setSuccess(t('settings.profile.save_success'))
      setTimeout(() => setSuccess(null), 3000)
    } catch (err) {
      let msg = t('errors.save_failed')
      if (err instanceof Error) msg = err.message
      else if (typeof err === 'string') msg = err
      setError(msg)
    }
  }

  const onSubmitPhone = async (data: PhoneFormValues) => {
    setError(null)
    setSuccess(null)
    try {
      const updated = await updatePhone(data.phone)
      if (updated) resetPhone({ ...data })
      setSuccess(t('settings.profile.save_success'))
      setTimeout(() => setSuccess(null), 3000)
    } catch (err) {
      let msg = t('errors.save_failed')
      if (err instanceof Error) msg = err.message
      else if (typeof err === 'string') msg = err
      setError(msg)
    }
  }

  const onSubmitEmail = async (data: EmailFormValues) => {
    setError(null)
    setSuccess(null)
    try {
      const updated = await updateEmail(data.email)
      if (updated) resetEmail({ ...data })
      setSuccess(t('settings.profile.save_success'))
      setTimeout(() => setSuccess(null), 3000)
    } catch (err) {
      let msg = t('errors.save_failed')
      if (err instanceof Error) msg = err.message
      else if (typeof err === 'string') msg = err
      setError(msg)
    }
  }

  if (!profile && !error) return <div className="text-sm text-text-muted">{t('auth.loading')}</div>

  const profileIdOrEmail = profile ? ((profile as { id?: string; email?: string }).id ?? (profile as { id?: string; email?: string }).email ?? 'me') : 'empty'
  const infoFormKey = `profile-info-${profileIdOrEmail}`
  const phoneFormKey = `profile-phone-${profileIdOrEmail}`
  const emailFormKey = `profile-email-${profileIdOrEmail}`

  return (
    <div className="space-y-8">
      <form key={infoFormKey} onSubmit={handleSubmitInfo(onSubmitInfo)} className="space-y-4">
        {error && <div className="text-sm text-red-600">{error}</div>}
        {success && <div className="text-sm text-green-600">{success}</div>}
        <div>
          <label className="block text-sm font-medium mb-1">{t('settings.profile.name')}</label>
          <input {...registerInfo('name')} className="w-full px-3 py-2 border rounded-md" />
          {infoErrors.name && <div className="text-sm text-red-600">{infoErrors.name.message}</div>}
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">{t('settings.profile.surname')}</label>
          <input {...registerInfo('surname')} className="w-full px-3 py-2 border rounded-md" />
          {infoErrors.surname && <div className="text-sm text-red-600">{infoErrors.surname.message}</div>}
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">{t('settings.profile.username')}</label>
          <input {...registerInfo('username')} className="w-full px-3 py-2 border rounded-md" />
          {infoErrors.username && <div className="text-sm text-red-600">{infoErrors.username.message}</div>}
        </div>
        <div className="flex items-center gap-2">
          <button type="submit" disabled={infoSubmitting} className="px-4 py-2 bg-brand text-white rounded-md disabled:opacity-60">
            {infoSubmitting ? t('saving') : t('settings.profile.save')}
          </button>
        </div>
      </form>

      <form key={phoneFormKey} onSubmit={handleSubmitPhone(onSubmitPhone)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('settings.profile.phone')}</label>
          <input {...registerPhone('phone')} className="w-full px-3 py-2 border rounded-md" />
          {phoneErrors.phone && <div className="text-sm text-red-600">{phoneErrors.phone.message}</div>}
        </div>
        <div className="flex items-center gap-2">
          <button type="submit" disabled={phoneSubmitting} className="px-4 py-2 bg-brand text-white rounded-md disabled:opacity-60">
            {phoneSubmitting ? t('saving') : t('settings.profile.save')}
          </button>
        </div>
      </form>

      <form key={emailFormKey} onSubmit={handleSubmitEmail(onSubmitEmail)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('settings.profile.email')}</label>
          <input {...registerEmail('email')} className="w-full px-3 py-2 border rounded-md" />
          {emailErrors.email && <div className="text-sm text-red-600">{emailErrors.email.message}</div>}
        </div>
        <div className="flex items-center gap-2">
          <button type="submit" disabled={emailSubmitting} className="px-4 py-2 bg-brand text-white rounded-md disabled:opacity-60">
            {emailSubmitting ? t('saving') : t('settings.profile.save')}
          </button>
        </div>
      </form>
    </div>
    
  )
}
