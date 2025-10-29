import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { EmailStep } from '../components/auth/EmailStep'
import { LoginStep } from '../components/auth/LoginStep'
import { RegisterStep } from '../components/auth/RegisterStep'
import { ForgotPasswordStep } from '../components/auth/ForgotPasswordStep'
import { authApi } from '../api/authApi'
import { useAuthStore } from '../store/authStore'

type Step = 'email' | 'login' | 'register' | 'forgot'

export default function Auth() {
  const [step, setStep] = useState<Step>('email')
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const setAuth = useAuthStore((state) => state.setAuth)
  const navigate = useNavigate()

  const handleEmailNext = async (enteredEmail: string) => {
    setEmail(enteredEmail)
    setIsLoading(true)
    setError(null)

    try {
      const result = await authApi.checkEmail(enteredEmail)
      setStep(result.exists ? 'login' : 'register')
    } catch {
      setError("Помилка з'єднання з сервером")
    } finally {
      setIsLoading(false)
    }
  }

  const handleLogin = async (password: string) => {
    setIsLoading(true)
    setError(null)

    try {
      const result = await authApi.login({ email, password })
      setAuth(result.token, result.user)
      navigate('/')
    } catch {
      setError('Невірний пароль')
    } finally {
      setIsLoading(false)
    }
  }

  const handleRegister = async (name: string, password: string) => {
    setIsLoading(true)
    setError(null)

    try {
      const result = await authApi.register({ email, name, password })
      setAuth(result.token, result.user)
      navigate('/')
    } catch {
      setError('Помилка реєстрації')
    } finally {
      setIsLoading(false)
    }
  }

  const handleGoogleLogin = async () => {
    setError(null)
    try {
      alert('Google OAuth буде реалізовано на бекенді')
    } catch {
      setError('Помилка входу через Google')
    }
  }

  const handleBack = () => {
    setStep('email')
    setError(null)
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center p-4">
      <div className="w-full max-w-md rounded-2xl bg-surface-card p-8 shadow-2xl">
        {error && (
          <div className="mb-4 rounded-lg border border-red-500/20 bg-red-500/10 p-3 text-sm text-red-500">
            {error}
          </div>
        )}

        {step === 'email' && (
          <EmailStep
            onNext={handleEmailNext}
            onForgotPassword={() => setStep('forgot')}
            onGoogleLogin={handleGoogleLogin}
            isLoading={isLoading}
          />
        )}

        {step === 'login' && (
          <LoginStep email={email} onBack={handleBack} onSubmit={handleLogin} isLoading={isLoading} />
        )}

        {step === 'register' && (
          <RegisterStep
            email={email}
            onBack={handleBack}
            onSubmit={handleRegister}
            isLoading={isLoading}
          />
        )}

        {step === 'forgot' && <ForgotPasswordStep onBack={handleBack} />}
      </div>
    </div>
  )
}
