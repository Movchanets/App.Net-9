import { useEffect, useState } from 'react'
import type { Resolver } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useProfileStore } from '../../store/profileStore'
import type { User } from '../../api/userApi'
import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import * as yup from 'yup'

type FormValues = {
  name: string
  surname: string
  username: string
  phone?: string
}

export default function ProfileForm() {
  const { t } = useTranslation()
  const { profile, fetchProfile, updateProfile } = useProfileStore()
  const [success, setSuccess] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  // build validation schema using localized messages
  const schema = yup.object({
    name: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
    surname: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
    username: yup.string().required(t('validation.required')).min(2, t('validation.min_2')),
    phone: yup
      .string()
      .optional()
      .matches(/^[+]?[0-9]{7,15}$/, t('validation.phone')),
  })

  const defaultValues = {
    name: profile?.name ?? '',
    surname: profile?.surname ?? '',
    username: profile?.username ?? '',
    phone: profile?.phoneNumber ?? '',
  }

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: yupResolver(schema) as unknown as Resolver<FormValues>, defaultValues })

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
    reset(defaultValues)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profile])

  const onSubmit = async (data: FormValues) => {
    setError(null)
    setSuccess(null)
    try {
      const payload: Partial<User> & { username?: string; phone?: string } = {
        name: data.name,
        surname: data.surname,
        username: data.username,
        phone: data.phone,
      }
  const updated = await updateProfile(payload)
  if (updated) reset({ ...data })
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
  const formKey = `profile-${profileIdOrEmail}`

  return (
    <form key={formKey} onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {error && <div className="text-sm text-red-600">{error}</div>}
      {success && <div className="text-sm text-green-600">{success}</div>}

  

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.profile.name')}</label>
        <input {...register('name')} className="w-full px-3 py-2 border rounded-md" />
        {errors.name && <div className="text-sm text-red-600">{errors.name.message}</div>}
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.profile.surname')}</label>
        <input {...register('surname')} className="w-full px-3 py-2 border rounded-md" />
        {errors.surname && <div className="text-sm text-red-600">{errors.surname.message}</div>}
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.profile.username')}</label>
        <input {...register('username')} className="w-full px-3 py-2 border rounded-md" />
        {errors.username && <div className="text-sm text-red-600">{errors.username.message}</div>}
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('settings.profile.phone')}</label>
        <input {...register('phone')} className="w-full px-3 py-2 border rounded-md" />
        {errors.phone && <div className="text-sm text-red-600">{errors.phone.message}</div>}
      </div>

      <div className="flex items-center gap-2">
        <button type="submit" disabled={isSubmitting} className="px-4 py-2 bg-brand text-white rounded-md disabled:opacity-60">
          {isSubmitting ? t('saving') : t('settings.profile.save')}
        </button>
      </div>
    </form>
  )
}
