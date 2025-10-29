import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import { registerFormSchema, type RegisterFormValues } from '../../validation/authSchemas'

interface RegisterStepProps {
  email: string
  onBack: () => void
  onSubmit: (name: string, password: string) => void
  isLoading: boolean
}

type FormData = RegisterFormValues

export function RegisterStep({ email, onBack, onSubmit, isLoading }: RegisterStepProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: yupResolver(registerFormSchema),
  })

  const handleFormSubmit = (data: FormData) => {
    onSubmit(data.name, data.password)
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
        <h2 className="text-2xl font-bold text-text">Реєстрація</h2>
        <p className="mt-2 text-sm text-text-muted">
          Створіть обліковий запис для <span className="font-medium text-text">{email}</span>
        </p>
      </div>

      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <div>
          <label htmlFor="name" className="mb-1 block text-sm text-text-muted">
            Імʼя
          </label>
          <input
            {...register('name')}
            id="name"
            type="text"
            placeholder="Ваше ім'я"
            className="w-full rounded-lg border border-text/20 bg-transparent px-4 py-3 text-text outline-none transition-colors focus:border-brand"
          />
          {errors.name && <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>}
        </div>

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

        <div>
          <label htmlFor="confirmPassword" className="mb-1 block text-sm text-text-muted">
            Підтвердіть пароль
          </label>
          <input
            {...register('confirmPassword')}
            id="confirmPassword"
            type="password"
            placeholder="••••••••"
            className="w-full rounded-lg border border-text/20 bg-transparent px-4 py-3 text-text outline-none transition-colors focus:border-brand"
          />
          {errors.confirmPassword && (
            <p className="mt-1 text-sm text-red-500">{errors.confirmPassword.message}</p>
          )}
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className="btn-primary w-full disabled:opacity-50"
        >
          {isLoading ? 'Реєстрація...' : 'Зареєструватися'}
        </button>
      </form>
    </div>
  )
}
