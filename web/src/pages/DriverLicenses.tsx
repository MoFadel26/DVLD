import { Link, useParams } from 'react-router-dom'
import { api, useApi } from '../api'
import { Empty, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'
import { LicensesTable } from './Licenses'

export function DriverLicenses() {
  const { t, date, className } = useI18n()
  const id = Number(useParams().id)
  const driver = useApi(() => api.driver(id), [id])
  const licenses = useApi(() => api.driverLicenses(id), [id])
  const rows = [...(licenses.data ?? [])].sort((a, b) => b.licenseId - a.licenseId)
  const error = driver.error ?? licenses.error

  return (
    <div className="page">
      <PageHead
        plate={<Plate kind="driver" id={id} />}
        title={driver.data?.fullName ?? t('driver.title', { id })}
        lede={
          driver.data ? (
            <>
              <span className="cell-code" dir="ltr">
                {driver.data.nationalNo}
              </span>{' '}
              · <Link to={`/people/${driver.data.personId}`}>{t('driver.person')}</Link>
            </>
          ) : (
            t('driver.lede')
          )
        }
      />
      {error ? (
        <RuleNotice
          error={error}
          onRetry={() => {
            driver.reload()
            licenses.reload()
          }}
        />
      ) : !licenses.data ? (
        <Loading />
      ) : rows.length === 0 ? (
        <Empty>{t('driver.empty')}</Empty>
      ) : (
        <LicensesTable rows={rows} date={date} className={className} />
      )}
    </div>
  )
}
