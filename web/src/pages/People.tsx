import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api, useApi } from '../api'
import { Icon } from '../components/Icons'
import { Empty, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function People() {
  const { t, country } = useI18n()
  const navigate = useNavigate()
  const people = useApi(api.people, [])
  const [query, setQuery] = useState('')

  const q = query.trim().toLowerCase()
  const rows = (people.data ?? []).filter(
    (p) => !q || p.fullName.toLowerCase().includes(q) || p.nationalNo.toLowerCase().includes(q),
  )

  return (
    <div className="page">
      <PageHead
        title={t('people.title')}
        lede={t('people.lede')}
        actions={
          <Link to="/people/new" className="btn btn-primary">
            <Icon name="plus" size={18} />
            {t('people.new')}
          </Link>
        }
      />

      <div className="toolbar">
        <label className="search">
          <Icon name="search" size={18} />
          <span className="visually-hidden">{t('people.search')}</span>
          <input
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={t('people.search')}
          />
        </label>
        {people.data && <span className="toolbar-count">{t('people.count', { n: rows.length })}</span>}
      </div>

      {people.error ? (
        <RuleNotice error={people.error} onRetry={people.reload} />
      ) : people.loading && !people.data ? (
        <Loading />
      ) : rows.length === 0 ? (
        <Empty>{q ? t('people.noMatch', { q: query.trim() }) : t('people.empty')}</Empty>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th scope="col">{t('col.id')}</th>
                <th scope="col">{t('col.name')}</th>
                <th scope="col">{t('col.nationalNo')}</th>
                <th scope="col" className="num">
                  {t('col.age')}
                </th>
                <th scope="col">{t('col.gender')}</th>
                <th scope="col">{t('col.phone')}</th>
                <th scope="col">{t('col.country')}</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((p) => {
                const to = `/people/${p.personId}`
                return (
                  <tr key={p.personId} className="row-link" onClick={() => navigate(to)}>
                    <td>
                      <Plate kind="person" id={p.personId} to={to} />
                    </td>
                    <td className="cell-main">{p.fullName}</td>
                    <td dir="ltr" className="cell-code">
                      {p.nationalNo}
                    </td>
                    <td className="num">{p.age}</td>
                    <td>{t(`gender.${p.gender}`)}</td>
                    <td dir="ltr" className="cell-code">
                      {p.phone}
                    </td>
                    <td>{country(p.nationalityCountryId, p.countryName)}</td>
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
