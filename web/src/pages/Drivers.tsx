import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, useApi } from '../api'
import { Icon } from '../components/Icons'
import { Empty, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function Drivers() {
  const { t, date } = useI18n()
  const navigate = useNavigate()
  const drivers = useApi(api.drivers, [])
  const [query, setQuery] = useState('')

  const q = query.trim().toLowerCase()
  const rows = (drivers.data ?? []).filter(
    (d) => !q || d.fullName.toLowerCase().includes(q) || d.nationalNo.toLowerCase().includes(q),
  )

  return (
    <div className="page">
      <PageHead title={t('drivers.title')} lede={t('drivers.lede')} />

      <div className="toolbar">
        <label className="search">
          <Icon name="search" size={16} />
          <span className="visually-hidden">{t('drivers.search')}</span>
          <input type="search" value={query} onChange={(e) => setQuery(e.target.value)} placeholder={t('drivers.search')} />
        </label>
        {drivers.data && <span className="toolbar-count">{t('drivers.count', { n: rows.length })}</span>}
      </div>

      {drivers.error ? (
        <RuleNotice error={drivers.error} onRetry={drivers.reload} />
      ) : !drivers.data ? (
        <Loading />
      ) : drivers.data.length === 0 ? (
        <Empty>{t('drivers.empty')}</Empty>
      ) : rows.length === 0 ? (
        <Empty>{t('people.noMatch', { q: query.trim() })}</Empty>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th scope="col">{t('col.id')}</th>
                <th scope="col">{t('col.name')}</th>
                <th scope="col" className="num">
                  {t('col.licenses')}
                </th>
                <th scope="col" className="num">
                  {t('col.activeLicenses')}
                </th>
                <th scope="col" className="num">
                  {t('col.since')}
                </th>
              </tr>
            </thead>
            <tbody>
              {rows.map((d) => {
                const to = `/drivers/${d.driverId}`
                return (
                  <tr key={d.driverId} className="row-link" onClick={() => navigate(to)}>
                    <td>
                      <Plate kind="driver" id={d.driverId} to={to} />
                    </td>
                    <td className="cell-person">
                      <span className="cell-main">{d.fullName}</span>
                      <span className="cell-sub" dir="ltr">
                        {d.nationalNo}
                      </span>
                    </td>
                    <td className="num">{d.licenseCount}</td>
                    <td className="num">{d.activeLicenseCount}</td>
                    <td className="num">{date(d.createdDate)}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
