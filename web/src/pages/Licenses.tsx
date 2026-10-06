import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, forgetLicense, recentLicenseIds, type License } from '../api'
import { Icon } from '../components/Icons'
import { Empty, PageHead, Plate } from '../components/ui'
import { useI18n } from '../i18n'

export function Licenses() {
  const { t, date, className } = useI18n()
  const navigate = useNavigate()
  const [licenseId, setLicenseId] = useState('')
  const [driverId, setDriverId] = useState('')
  const [recent, setRecent] = useState<License[]>()

  useEffect(() => {
    // Look up each remembered id; ids from an earlier API run that no longer exist are dropped.
    Promise.all(
      recentLicenseIds().map((id) =>
        api.license(id).catch((err) => {
          if (err.status === 404) forgetLicense(id)
          return undefined
        }),
      ),
    ).then((list) => setRecent(list.filter((x): x is License => Boolean(x))))
  }, [])

  const go = (path: string) => (e: FormEvent) => {
    e.preventDefault()
    navigate(path)
  }

  return (
    <div className="page">
      <PageHead title={t('lic.title')} lede={t('lic.lede')} />

      <div className="lookup">
        <form className="lookup-form" onSubmit={go(`/licenses/${licenseId}`)}>
          <label htmlFor="lic-id">{t('lic.byId')}</label>
          <div className="lookup-row">
            <input
              id="lic-id"
              inputMode="numeric"
              pattern="[0-9]+"
              required
              dir="ltr"
              value={licenseId}
              onChange={(e) => setLicenseId(e.target.value.trim())}
            />
            <button type="submit" className="btn btn-primary">
              <Icon name="search" size={18} />
              {t('lic.open')}
            </button>
          </div>
        </form>
        <form className="lookup-form" onSubmit={go(`/drivers/${driverId}`)}>
          <label htmlFor="drv-id">{t('lic.byDriver')}</label>
          <div className="lookup-row">
            <input
              id="drv-id"
              inputMode="numeric"
              pattern="[0-9]+"
              required
              dir="ltr"
              value={driverId}
              onChange={(e) => setDriverId(e.target.value.trim())}
            />
            <button type="submit" className="btn btn-quiet">
              {t('lic.openDriver')}
            </button>
          </div>
        </form>
      </div>

      <section aria-labelledby="recent">
        <div className="section-head">
          <h2 id="recent">{t('lic.recent')}</h2>
        </div>
        {recent && recent.length > 0 ? (
          <LicensesTable rows={recent} date={date} className={className} />
        ) : (
          recent && <Empty>{t('lic.recentEmpty')}</Empty>
        )}
      </section>
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
