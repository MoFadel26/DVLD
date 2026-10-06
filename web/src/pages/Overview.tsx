import { Link } from 'react-router-dom'
import { api, useApi, type LocalApplication } from '../api'
import { Icon, type IconName } from '../components/Icons'
import { ApplicationsTable } from './Applications'
import { Empty, Loading, PageHead, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function Overview() {
  const { t } = useI18n()
  const apps = useApi(api.applications, [])
  const people = useApi(api.people, [])
  const classes = useApi(api.licenseClasses, [])

  const list = apps.data ?? []
  const open = list.filter((a) => a.applicationStatus === 'New')
  const atStage = (n: number) => open.filter((a) => a.passedTestCount === n).length
  const completed = list.filter((a) => a.applicationStatus === 'Completed').length
  const value = (n: number) => (apps.data ? n : '–')

  const stats: { key: string; icon: IconName; label: string; value: number | string; note: string; highlight?: boolean }[] = [
    { key: 'all', icon: 'form', label: t('route.apply'), value: value(list.length), note: t('stat.submitted') },
    { key: 'vision', icon: 'eye', label: t('route.vision'), value: value(atStage(0)), note: t('stat.waiting') },
    { key: 'theory', icon: 'book', label: t('route.theory'), value: value(atStage(1)), note: t('stat.waiting') },
    { key: 'practical', icon: 'wheel', label: t('route.practical'), value: value(atStage(2)), note: t('stat.waiting') },
    {
      key: 'ready',
      icon: 'card',
      label: t('stat.ready'),
      value: value(atStage(3)),
      note: t('stat.issued', { n: value(completed) }),
      highlight: atStage(3) > 0,
    },
  ]

  return (
    <div className="page">
      <PageHead
        title={t('overview.title')}
        lede={t('overview.lede')}
        actions={
          <Link to="/applications/new" className="btn btn-primary">
            <Icon name="plus" size={16} />
            {t('overview.newApp')}
          </Link>
        }
      />

      <section className="stats" aria-label={t('route.label')}>
        {stats.map((s) => (
          <div key={s.key} className={`card stat${s.highlight ? ' is-highlight' : ''}`}>
            <div className="stat-top">
              <span className="stat-label">{s.label}</span>
              <span className="stat-icon" aria-hidden="true">
                <Icon name={s.icon} size={16} />
              </span>
            </div>
            <span className="stat-value">{s.value}</span>
            <span className="stat-note">{s.note}</span>
          </div>
        ))}
      </section>

      <div className="split">
        <section className="card" aria-labelledby="open-apps">
          <div className="card-head">
            <h2 id="open-apps" className="card-title">
              {t('overview.open')}
            </h2>
            <Link to="/applications" className="link-more">
              {t('overview.seeAll')}
              <Icon name="arrowNext" size={14} className="flip-rtl" />
            </Link>
          </div>
          {apps.error ? (
            <div className="card-body">
              <RuleNotice error={apps.error} onRetry={apps.reload} />
            </div>
          ) : apps.loading && !apps.data ? (
            <Loading />
          ) : open.length === 0 ? (
            <div className="card-body">
              <Empty>{t('overview.openEmpty')}</Empty>
            </div>
          ) : (
            <ApplicationsTable rows={sortNewest(open)} compact flush />
          )}
        </section>

        <aside className="split-side">
          <section className="card" aria-labelledby="checks">
            <div className="card-head">
              <h2 id="checks" className="card-title">
                {t('overview.checks')}
              </h2>
            </div>
            <ol className="check-list">
              {(['check.1', 'check.2', 'check.3', 'check.4'] as const).map((k, i) => (
                <li key={k}>
                  <span className="check-num">{i + 1}</span>
                  {t(k)}
                </li>
              ))}
            </ol>
          </section>

          <section className="card" aria-labelledby="on-file">
            <div className="card-head">
              <h2 id="on-file" className="card-title">
                {t('overview.counts')}
              </h2>
            </div>
            <dl className="tally">
              <div>
                <dt>
                  <Link to="/people">{t('overview.people')}</Link>
                </dt>
                <dd>{people.data?.length ?? '–'}</dd>
              </div>
              <div>
                <dt>
                  <Link to="/applications">{t('overview.apps')}</Link>
                </dt>
                <dd>{apps.data?.length ?? '–'}</dd>
              </div>
              <div>
                <dt>
                  <Link to="/classes">{t('overview.classes')}</Link>
                </dt>
                <dd>{classes.data?.length ?? '–'}</dd>
              </div>
            </dl>
          </section>
        </aside>
      </div>
    </div>
  )
}

function sortNewest(rows: LocalApplication[]) {
  return [...rows].sort((a, b) => b.localDrivingLicenseApplicationId - a.localDrivingLicenseApplicationId)
}
