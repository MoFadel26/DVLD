import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import type { ApiError } from '../api'
import { useAuth } from '../auth'
import { Icon } from '../components/Icons'
import { RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function Login() {
  const { t, lang, setLang } = useI18n()
  const { user, signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<ApiError>()
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to={from} replace />

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(undefined)
    try {
      await signIn(username.trim(), password)
      navigate(from, { replace: true })
    } catch (err) {
      setError(err as ApiError)
      setBusy(false)
    }
  }

  return (
    <main className="login">
      <button
        type="button"
        className="toggle login-lang"
        onClick={() => setLang(lang === 'en' ? 'ar' : 'en')}
        aria-label={`${t('lang.label')}: ${t('lang.switch')}`}
      >
        <Icon name="globe" size={16} />
        <span lang={lang === 'en' ? 'ar' : 'en'}>{t('lang.switch')}</span>
      </button>

      <div className="login-card card">
        <div className="login-brand">
          <span className="brand-mark" aria-hidden="true">
            <Icon name="card" size={18} />
          </span>
          <span className="brand-name">DVLD</span>
        </div>
        <h1>{t('login.title')}</h1>
        <p className="lede">{t('login.lede')}</p>

        <form className="form" onSubmit={submit}>
          <div className="field">
            <label htmlFor="username">{t('login.username')}</label>
            <input
              id="username"
              autoComplete="username"
              dir="ltr"
              required
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>
          <div className="field">
            <label htmlFor="password">{t('login.password')}</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              dir="ltr"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          {error && <RuleNotice error={error} />}
          <button type="submit" className="btn btn-primary login-submit" disabled={busy}>
            {busy ? t('login.signingIn') : t('login.submit')}
          </button>
        </form>

        {import.meta.env.DEV && (
          <p className="login-hint">
            {t('login.demo')} <code dir="ltr">admin</code> / <code dir="ltr">Admin@12345</code>
          </p>
        )}
      </div>
    </main>
  )
}
