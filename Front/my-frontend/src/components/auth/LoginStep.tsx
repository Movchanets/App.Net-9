import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import { loginFormSchema, type LoginFormValues } from '../../validation/authSchemas'

interface LoginStepProps {
  email: string
  onBack: () => void
  onSubmit: (password: string) => void
  isLoading: boolean
}

type FormData = LoginFormValues

export function LoginStep({ email, onBack, onSubmit, isLoading }: LoginStepProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: yupResolver(loginFormSchema),
  })

  const handleFormSubmit = (data: FormData) => {
    onSubmit(data.password)
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
        <h2 className="text-2xl font-bold text-text">Вхід</h2>
        <p className="mt-2 text-sm text-text-muted">
          Увійдіть в обліковий запис <span className="font-medium text-text">{email}</span>
        </p>
      </div>

      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <div>
          <label htmlFor="password" className="mb-1 block text-sm text-text-muted">
            Пароль
          </label>
          <input
            {...register('password')}
            id="password"
            type="password"
            placeholder="••••••••"
            className="w-full rounded-lg border border-text/20 bg-transparent px-4 py-3 text-text outline-none transition-colors focus:border-brand"
          />
          {errors.password && <p className="mt-1 text-sm text-red-500">{errors.password.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className="btn-primary w-full disabled:opacity-50"
        >
          {isLoading ? 'Вхід...' : 'Увійти'}
        </button>
      </form>
    </div>
  )
}
