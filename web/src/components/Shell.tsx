import { useEffect, useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { Icon, type IconName } from './Icons'

const nav: { to: string; key: 'nav.overview' | 'nav.people' | 'nav.applications' | 'nav.licenses' | 'nav.drivers' | 'nav.classes'; icon: IconName }[] = [
  { to: '/', key: 'nav.overview', icon: 'home' },
  { to: '/people', key: 'nav.people', icon: 'person' },
  { to: '/applications', key: 'nav.applications', icon: 'form' },
  { to: '/licenses', key: 'nav.licenses', icon: 'card' },
  { to: '/drivers', key: 'nav.drivers', icon: 'wheel' },
  { to: '/classes', key: 'nav.classes', icon: 'classes' },
]

type Theme = 'light' | 'dark'

const THEME_KEY = 'dvld.theme'

function useTheme() {
  // index.html sets data-theme before first paint from the saved or system preference.
  const [theme, setTheme] = useState<Theme>(() =>
    document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light',
  )

  useEffect(() => {
    document.documentElement.dataset.theme = theme
  }, [theme])

  const toggle = () => {
    const next = theme === 'dark' ? 'light' : 'dark'
    localStorage.setItem(THEME_KEY, next)
    setTheme(next)
  }

  return { theme, toggle }
}

export function Shell() {
  const { t, lang, setLang } = useI18n()
  const { theme, toggle } = useTheme()
  const { user, signOut } = useAuth()

  return (
    <div className="shell">
      <aside className="sidebar">
        <NavLink to="/" className="brand">
          <span className="brand-mark" aria-hidden="true">
            <Icon name="card" size={18} />
          </span>
          <span className="brand-text">
            <span className="brand-name">DVLD</span>
            <span className="brand-sub">{t('brand.sub')}</span>
          </span>
        </NavLink>

        <nav className="nav" aria-label={t('nav.main')}>
          {nav.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.to === '/'} className="nav-link">
              <Icon name={item.icon} size={18} />
              <span>{t(item.key)}</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-foot">
          <div className="toggles">
            <button
              type="button"
              className="toggle"
              onClick={() => setLang(lang === 'en' ? 'ar' : 'en')}
              aria-label={`${t('lang.label')}: ${t('lang.switch')}`}
            >
              <Icon name="globe" size={16} />
              <span lang={lang === 'en' ? 'ar' : 'en'}>{t('lang.switch')}</span>
            </button>
            <button
              type="button"
              className="toggle toggle-icon"
              onClick={toggle}
              aria-label={theme === 'dark' ? t('theme.toLight') : t('theme.toDark')}
              title={theme === 'dark' ? t('theme.toLight') : t('theme.toDark')}
            >
              <Icon name={theme === 'dark' ? 'sun' : 'moon'} size={16} />
            </button>
          </div>
          {user && (
            <div className="user-row">
              <span className="user-avatar" aria-hidden="true">
                {user.username.slice(0, 1).toUpperCase()}
              </span>
              <span className="user-name" dir="ltr">
                {user.username}
              </span>
              <button type="button" className="toggle toggle-icon" onClick={signOut} aria-label={t('auth.signOut')} title={t('auth.signOut')}>
                <Icon name="logout" size={16} className="flip-rtl" />
              </button>
            </div>
          )}
        </div>
      </aside>

      <main className="main" id="main">
        <Outlet />
      </main>
    </div>
  )
}
