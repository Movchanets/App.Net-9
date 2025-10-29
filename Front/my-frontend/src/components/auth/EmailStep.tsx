import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import { emailFormSchema, type EmailFormValues } from '../../validation/authSchemas'

interface EmailStepProps {
  onNext: (email: string) => void
  onForgotPassword: () => void
  onGoogleLogin: () => void
  isLoading: boolean
}

type FormData = EmailFormValues

export function EmailStep({ onNext, onForgotPassword, onGoogleLogin, isLoading }: EmailStepProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: yupResolver(emailFormSchema),
  })

  const onSubmit = async (data: FormData) => {
    // Тут має бути виклик authApi.checkEmail(data.email)
    // Для демо - симулюємо перевірку
    onNext(data.email)
  }

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-text">Вхід або реєстрація</h2>
        <p className="mt-2 text-sm text-text-muted">Введіть ваш email для продовження</p>
      </div>

      {/* Google Login */}
      <button
        type="button"
        onClick={onGoogleLogin}
        className="flex w-full items-center justify-center gap-3 rounded-lg border border-text/20 bg-surface-card px-4 py-3 text-text transition-colors hover:bg-text/5"
      >
        <svg className="h-5 w-5" viewBox="0 0 24 24">
          <path
            fill="currentColor"
            d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
          />
          <path
            fill="currentColor"
            d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
          />
          <path
            fill="currentColor"
            d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
          />
          <path
            fill="currentColor"
            d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
          />
        </svg>
        Увійти через Google
      </button>

      <div className="relative">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-text/10"></div>
        </div>
        <div className="relative flex justify-center text-sm">
          <span className="bg-surface px-2 text-text-muted">або через email</span>
        </div>
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
          {isLoading ? 'Завантаження...' : 'Продовжити'}
        </button>
      </form>

      <div className="text-center">
        <button
          type="button"
          onClick={onForgotPassword}
          className="text-sm text-brand hover:text-brand-light"
        >
          Забули пароль?
        </button>
      </div>
    </div>
  )
}
