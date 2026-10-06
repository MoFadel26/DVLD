import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { api, useApi, type ApplicationStatus, type LocalApplication } from '../api'
import { Icon } from '../components/Icons'
import { Empty, LaneStrip, Loading, PageHead, Plate, RuleNotice, StatusTag } from '../components/ui'
import { useI18n } from '../i18n'

const filters: (ApplicationStatus | 'all')[] = ['all', 'New', 'Completed', 'Cancelled']

export function Applications() {
  const { t } = useI18n()
  const apps = useApi(api.applications, [])
  const [params, setParams] = useSearchParams()
  const raw = params.get('status')
  const status = filters.includes(raw as ApplicationStatus) ? (raw as ApplicationStatus) : 'all'

  const list = [...(apps.data ?? [])].sort(
    (a, b) => b.localDrivingLicenseApplicationId - a.localDrivingLicenseApplicationId,
  )
  const shown = status === 'all' ? list : list.filter((a) => a.applicationStatus === status)
  const countFor = (f: (typeof filters)[number]) =>
    f === 'all' ? list.length : list.filter((a) => a.applicationStatus === f).length

  return (
    <div className="page">
      <PageHead
        title={t('apps.title')}
        lede={t('apps.lede')}
        actions={
          <Link to="/applications/new" className="btn btn-primary">
            <Icon name="plus" size={18} />
            {t('apps.new')}
          </Link>
        }
      />

      <div className="tabs" role="group" aria-label={t('apps.filter')}>
        {filters.map((f) => (
          <button
            key={f}
            type="button"
            aria-pressed={status === f}
            className="tab"
            onClick={() => setParams(f === 'all' ? {} : { status: f })}
          >
            {f === 'all' ? t('filter.all') : t(`status.${f}`)}
            <span className="tab-count">{apps.data ? countFor(f) : '–'}</span>
          </button>
        ))}
      </div>

      {apps.error ? (
        <RuleNotice error={apps.error} onRetry={apps.reload} />
      ) : apps.loading && !apps.data ? (
        <Loading />
      ) : shown.length === 0 ? (
        <Empty
          action={
            status === 'all' && (
              <Link to="/applications/new" className="btn btn-quiet">
                {t('apps.new')}
              </Link>
            )
          }
        >
          {t('apps.empty')}
        </Empty>
      ) : (
        <ApplicationsTable rows={shown} />
      )}
    </div>
  )
}

export function ApplicationsTable({
  rows,
  hideApplicant = false,
  compact = false,
  flush = false,
}: {
  rows: LocalApplication[]
  hideApplicant?: boolean
  /** Drops the date and fee columns for narrow placements such as the overview. */
  compact?: boolean
  /** Drops the outer border when the table sits inside a card. */
  flush?: boolean
}) {
  const { t, date, money, className } = useI18n()
  const navigate = useNavigate()

  return (
    <div className={flush ? 'table-wrap is-flush' : 'table-wrap'}>
      <table className="table table-apps">
        <thead>
          <tr>
            <th scope="col">{t('col.id')}</th>
            {!hideApplicant && <th scope="col">{t('col.applicant')}</th>}
            <th scope="col">{t('col.class')}</th>
            <th scope="col">{t('col.progress')}</th>
            <th scope="col">{t('col.status')}</th>
            {!compact && (
              <>
                <th scope="col" className="num">
                  {t('col.date')}
                </th>
                <th scope="col" className="num">
                  {t('col.fees')}
                </th>
              </>
            )}
          </tr>
        </thead>
        <tbody>
          {rows.map((a) => {
            const to = `/applications/${a.localDrivingLicenseApplicationId}`
            return (
              <tr key={a.localDrivingLicenseApplicationId} className="row-link" onClick={() => navigate(to)}>
                <td>
                  <Plate kind="app" id={a.localDrivingLicenseApplicationId} to={to} />
                </td>
                {!hideApplicant && (
                  <td className="cell-person">
                    <span className="cell-main">{a.applicantFullName}</span>
                    <span className="cell-sub" dir="ltr">
                      {a.nationalNo}
                    </span>
                  </td>
                )}
                <td className="cell-class">{className(a.licenseClassId, a.className)}</td>
                <td>
                  <LaneStrip passed={a.passedTestCount} status={a.applicationStatus} />
                </td>
                <td className="cell-status">
                  <StatusTag status={a.applicationStatus} />
                </td>
                {!compact && (
                  <>
                    <td className="num">{date(a.applicationDate)}</td>
                    <td className="num">{money(a.paidFees)}</td>
                  </>
                )}
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
