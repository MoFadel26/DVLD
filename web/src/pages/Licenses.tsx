import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { api, useApi, type License } from '../api'
import { Icon } from '../components/Icons'
import { Empty, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

type StatusFilter = 'all' | 'active' | 'detained' | 'inactive'

const filters: StatusFilter[] = ['all', 'active', 'detained', 'inactive']

function statusOf(l: License): Exclude<StatusFilter, 'all'> {
  return l.isDetained ? 'detained' : l.isActive ? 'active' : 'inactive'
}

export function Licenses() {
  const { t, date, className } = useI18n()
  const licenses = useApi(api.licenses, [])
  const [query, setQuery] = useState('')
  const [params, setParams] = useSearchParams()
  const raw = params.get('status')
  const status = filters.includes(raw as StatusFilter) ? (raw as StatusFilter) : 'all'

  const list = licenses.data ?? []
  const q = query.trim().toLowerCase()
  const rows = list.filter(
    (l) =>
      (status === 'all' || statusOf(l) === status) &&
      (!q ||
        l.driverFullName.toLowerCase().includes(q) ||
        l.nationalNo.toLowerCase().includes(q) ||
        String(l.licenseId) === q.replace(/^lic-?/, '')),
  )
  const countFor = (f: StatusFilter) => (f === 'all' ? list.length : list.filter((l) => statusOf(l) === f).length)
  const label: Record<StatusFilter, string> = {
    all: t('filter.all'),
    active: t('lic.active'),
    detained: t('lic.detained'),
    inactive: t('lic.inactive'),
  }

  return (
    <div className="page">
      <PageHead title={t('lic.title')} lede={t('lic.lede')} />

      <div className="toolbar">
        <label className="search">
          <Icon name="search" size={16} />
          <span className="visually-hidden">{t('lic.search')}</span>
          <input type="search" value={query} onChange={(e) => setQuery(e.target.value)} placeholder={t('lic.search')} />
        </label>
        <div className="tabs" role="group" aria-label={t('lic.filter')}>
          {filters.map((f) => (
            <button
              key={f}
              type="button"
              aria-pressed={status === f}
              className="tab"
              onClick={() => setParams(f === 'all' ? {} : { status: f })}
            >
              {label[f]}
              <span className="tab-count">{licenses.data ? countFor(f) : '–'}</span>
            </button>
          ))}
        </div>
      </div>

      {licenses.error ? (
        <RuleNotice error={licenses.error} onRetry={licenses.reload} />
      ) : !licenses.data ? (
        <Loading />
      ) : list.length === 0 ? (
        <Empty>{t('lic.empty')}</Empty>
      ) : rows.length === 0 ? (
        <Empty>{t('lic.noMatch')}</Empty>
      ) : (
        <LicensesTable rows={rows} date={date} className={className} />
      )}
    </div>
  )
}

export function LicensesTable({
  rows,
  date,
  className,
}: {
  rows: License[]
  date: (iso: string) => string
  className: (id: number, fallback: string) => string
}) {
  const { t } = useI18n()
  const navigate = useNavigate()
  return (
    <div className="table-wrap">
      <table className="table">
        <thead>
          <tr>
            <th scope="col">{t('col.id')}</th>
            <th scope="col">{t('lic.holder')}</th>
            <th scope="col">{t('col.class')}</th>
            <th scope="col">{t('col.reason')}</th>
            <th scope="col">{t('col.status')}</th>
            <th scope="col" className="num">
              {t('col.issued')}
            </th>
            <th scope="col" className="num">
              {t('col.expires')}
            </th>
          </tr>
        </thead>
        <tbody>
          {rows.map((l) => {
            const to = `/licenses/${l.licenseId}`
            return (
              <tr key={l.licenseId} className="row-link" onClick={() => navigate(to)}>
                <td>
                  <Plate kind="license" id={l.licenseId} to={to} />
                </td>
                <td className="cell-person">
                  <span className="cell-main">{l.driverFullName}</span>
                  <span className="cell-sub" dir="ltr">
                    {l.nationalNo}
                  </span>
                </td>
                <td className="cell-class">{className(l.licenseClassId, l.className)}</td>
                <td>{t(`reason.${l.issueReason}`)}</td>
                <td>
                  <LicenseStatus license={l} />
                </td>
                <td className="num">{date(l.issueDate)}</td>
                <td className="num">{date(l.expirationDate)}</td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

export function LicenseStatus({ license }: { license: License }) {
  const { t } = useI18n()
  const [tone, label] = license.isDetained
    ? ['detained', t('lic.detained')]
    : license.isActive
      ? ['active', t('lic.active')]
      : ['inactive', t('lic.inactive')]
  return (
    <span className={`badge badge-${tone}`}>
      <span className="badge-dot" aria-hidden="true" />
      {label}
    </span>
  )
}
