import { useState } from 'react'
import { EmailStep } from './auth/EmailStep'
import { LoginStep } from './auth/LoginStep'
import { RegisterStep } from './auth/RegisterStep'
import { ForgotPasswordStep } from './auth/ForgotPasswordStep'
import { authApi } from '../api/authApi'
import TurnstileWidget from './TurnstileWidget'
import { useAuthStore } from '../store/authStore'

type Step = 'email' | 'login' | 'register' | 'forgot'

interface AuthModalProps {
  isOpen: boolean
  onClose: () => void
}

export function AuthModal({ isOpen, onClose }: AuthModalProps) {
  const [step, setStep] = useState<Step>('email')
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null)
  const setAuth = useAuthStore((state) => state.setAuth)

  if (!isOpen) return null

  const handleEmailNext = async (enteredEmail: string) => {
    setEmail(enteredEmail)
    setIsLoading(true)
    setError(null)

    try {
      const result = await authApi.checkEmail(enteredEmail, turnstileToken ?? undefined)
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
  const result = await authApi.login({ email, password, turnstileToken: turnstileToken ?? undefined })
      // result is TokenResponse { accessToken, refreshToken }
      setAuth(result.accessToken, result.refreshToken)
      onClose()
    } catch {
      setError('Невірний пароль')
    } finally {
      setIsLoading(false)
    }
  }

  const handleRegister = async (name: string, surname: string, password: string, confirmPassword: string) => {
    setIsLoading(true)
    setError(null)

    try {
  const result = await authApi.register({ email, name, surname, password, confirmPassword, turnstileToken: turnstileToken ?? undefined })
      setAuth(result.accessToken, result.refreshToken)
      onClose()
    } catch {
      setError('Помилка реєстрації')
    } finally {
      setIsLoading(false)
    }
  }

  const handleGoogleLogin = async () => {
    setError(null)
    try {
      // TODO: Integrate real Google OAuth
      alert('Google OAuth буде реалізовано на бекенді')
      // const token = 'google-oauth-token'
      // const result = await authApi.googleLogin(token)
      // setAuth(result.token, result.user)
      // onClose()
    } catch {
      setError('Помилка входу через Google')
    }
  }

  const handleBack = () => {
    setStep('email')
    setError(null)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="relative w-full max-w-md rounded-2xl bg-surface-card p-8 shadow-2xl">
        <button
          onClick={onClose}
          className="absolute right-4 top-4 text-text-muted hover:text-text"
        >
          <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

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
          <LoginStep
            email={email}
            onBack={handleBack}
            onSubmit={handleLogin}
            isLoading={isLoading}
          />
        )}

        {step === 'register' && (
          <RegisterStep
            email={email}
            onBack={handleBack}
            onSubmit={handleRegister}
            isLoading={isLoading}
          />
        )}

        {step === 'forgot' && <ForgotPasswordStep onBack={handleBack} onSubmit={async (email) => {
          setIsLoading(true)
          setError(null)
          try {
            await authApi.requestPasswordReset({ email, turnstileToken: turnstileToken ?? undefined })
            setStep('email')
          } catch {
            setError('Помилка відновлення паролю')
          } finally {
            setIsLoading(false)
          }
        }} />}

        {(step === 'login' || step === 'register' || step === 'forgot') && (
          <div className="mt-4">
            <TurnstileWidget onVerify={(t) => setTurnstileToken(t)} />
          </div>
        )}
      </div>
    </div>
  )
}
