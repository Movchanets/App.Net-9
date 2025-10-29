import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import { forgotPasswordFormSchema, type ForgotPasswordFormValues } from '../../validation/authSchemas'
import { useState } from 'react'
import { authApi } from '../../api/authApi'

interface ForgotPasswordStepProps {
  onBack: () => void
}
type FormData = ForgotPasswordFormValues

export function ForgotPasswordStep({ onBack }: ForgotPasswordStepProps) {
  const [isLoading, setIsLoading] = useState(false)
  const [success, setSuccess] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: yupResolver(forgotPasswordFormSchema),
  })

  const onSubmit = async (data: FormData) => {
    setIsLoading(true)
    try {
      await authApi.resetPassword({ email: data.email })
      setSuccess(true)
    } catch (error) {
      console.error(error)
    } finally {
      setIsLoading(false)
    }
  }

  if (success) {
    return (
      <div className="space-y-6 text-center">
        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-500/20">
          <svg className="h-8 w-8 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <div>
          <h2 className="text-2xl font-bold text-text">Перевірте пошту</h2>
          <p className="mt-2 text-sm text-text-muted">
            Ми відправили інструкції з відновлення паролю на вашу електронну адресу.
          </p>
        </div>
        <button onClick={onBack} className="btn-primary w-full">
          Повернутися до входу
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <button
          onClick={onBack}
          className="mb-4 flex items-center gap-1 text-sm text-text-muted hover:text-text"
        >
          <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Назад
        </button>
        <h2 className="text-2xl font-bold text-text">Відновлення паролю</h2>
        <p className="mt-2 text-sm text-text-muted">
          Введіть email та ми надішлемо інструкції для відновлення.
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label htmlFor="email" className="mb-1 block text-sm text-text-muted">
            Email
          </label>
          <input
            {...register('email')}
            id="email"
            type="email"
            placeholder="your@email.com"
            className="w-full rounded-lg border border-text/20 bg-transparent px-4 py-3 text-text outline-none transition-colors focus:border-brand"
          />
          {errors.email && <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className="btn-primary w-full disabled:opacity-50"
        >
          {isLoading ? 'Надсилання...' : 'Надіслати'}
        </button>
      </form>
    </div>
  )
}
