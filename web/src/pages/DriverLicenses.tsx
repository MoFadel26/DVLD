import { useParams } from 'react-router-dom'
import { api, useApi } from '../api'
import { Empty, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'
import { LicensesTable } from './Licenses'

export function DriverLicenses() {
  const { t, date, className } = useI18n()
  const id = Number(useParams().id)
  const licenses = useApi(() => api.driverLicenses(id), [id])
  const rows = [...(licenses.data ?? [])].sort((a, b) => b.licenseId - a.licenseId)

  return (
    <div className="page">
      <PageHead
        plate={<Plate kind="driver" id={id} />}
        title={rows[0]?.driverFullName ?? t('driver.title', { id })}
        lede={t('driver.lede')}
      />
      {licenses.error ? (
        <RuleNotice error={licenses.error} onRetry={licenses.reload} />
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
