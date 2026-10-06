import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import type { ApiError, ApplicationStatus } from '../api'
import { hasKey, useI18n } from '../i18n'
import { Icon } from './Icons'

type PlateKind = 'person' | 'app' | 'license' | 'driver'

const platePrefix: Record<PlateKind, string> = {
  person: 'P',
  app: 'APP',
  license: 'LIC',
  driver: 'DRV',
}

/** A record id, e.g. APP-12, as a compact monospace tag. */
export function Plate({ kind, id, to }: { kind: PlateKind; id: number; to?: string }) {
  const text = `${platePrefix[kind]}-${id}`
  return to ? (
    <Link to={to} className="id-tag id-link" onClick={(e) => e.stopPropagation()}>
      {text}
    </Link>
  ) : (
    <span className="id-tag">{text}</span>
  )
}

export function StatusTag({ status }: { status: ApplicationStatus }) {
  const { t } = useI18n()
  return (
    <span className={`badge badge-${status.toLowerCase()}`}>
      <span className="badge-dot" aria-hidden="true" />
      {t(`status.${status}`)}
    </span>
  )
}

export function PageHead({
  title,
  lede,
  actions,
  plate,
}: {
  title: ReactNode
  lede?: ReactNode
  actions?: ReactNode
  plate?: ReactNode
}) {
  return (
    <header className="page-head">
      <div className="page-head-text">
        {plate}
        <h1>{title}</h1>
        {lede && <p className="lede">{lede}</p>}
      </div>
      {actions && <div className="page-head-actions">{actions}</div>}
    </header>
  )
}

/** A refusal from the API: the title names the rule, the detail is the API's own message. */
export function RuleNotice({ error, onRetry }: { error: ApiError; onRetry?: () => void }) {
  const { t } = useI18n()
  const network = error.status === 0 || error.title === 'Network'
  const key = `err.${error.title}`
  const title = network ? t('err.network') : hasKey(key) ? t(key) : error.title || t('err.title')

  return (
    <div className="alert alert-danger" role="alert">
      <Icon name="alert" size={18} className="alert-icon" />
      <div>
        <p className="alert-title">{title}</p>
        {network ? (
          <p className="alert-detail">
            {t('err.networkHint')}{' '}
            <code dir="ltr">ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api</code>
          </p>
        ) : (
          error.detail && (
            <p className="alert-detail" dir="ltr">
              {error.detail}
            </p>
          )
        )}
        {onRetry && (
          <button type="button" className="btn btn-quiet btn-small" onClick={onRetry}>
            {t('err.retry')}
          </button>
        )}
      </div>
    </div>
  )
}

export function Loading() {
  const { t } = useI18n()
  return (
    <div className="loading" role="status">
      <span className="loading-bar" aria-hidden="true" />
      <span>{t('loading')}</span>
    </div>
  )
}

export function Empty({ children, action }: { children: ReactNode; action?: ReactNode }) {
  return (
    <div className="empty">
      <p>{children}</p>
      {action}
    </div>
  )
}

/** Three segments for Vision, Theory, Practical inside a table row. */
export function LaneStrip({ passed, status }: { passed: number; status: ApplicationStatus }) {
  const { t } = useI18n()
  const names = [t('route.vision'), t('route.theory'), t('route.practical')]
  const label = names
    .map((n, i) => `${n}: ${i < passed ? t('route.state.done') : i === passed && status === 'New' ? t('route.state.current') : t('route.state.todo')}`)
    .join(', ')

  return (
    <span className={`lanes lanes-${status.toLowerCase()}`} role="img" aria-label={label}>
      {names.map((n, i) => (
        <span
          key={n}
          className={`lane-mark ${i < passed ? 'is-done' : i === passed && status === 'New' ? 'is-current' : ''}`}
        />
      ))}
      <span className="lanes-count" aria-hidden="true">
        {passed}/3
      </span>
    </span>
  )
}

export function Fact({ label, children }: { label: ReactNode; children: ReactNode }) {
  return (
    <div className="fact">
      <dt>{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}
