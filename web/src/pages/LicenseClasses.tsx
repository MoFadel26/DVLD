import { api, useApi } from '../api'
import { classIcons } from '../classIcons'
import { Icon } from '../components/Icons'
import { Loading, PageHead, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function LicenseClasses() {
  const { t, money, className, classDescription } = useI18n()
  const classes = useApi(api.licenseClasses, [])

  return (
    <div className="page">
      <PageHead title={t('classes.title')} lede={t('classes.lede')} />
      {classes.error ? (
        <RuleNotice error={classes.error} onRetry={classes.reload} />
      ) : !classes.data ? (
        <Loading />
      ) : (
        <div className="table-wrap">
          <table className="table table-classes">
            <thead>
              <tr>
                <th scope="col">{t('col.class')}</th>
                <th scope="col" className="num">
                  {t('col.minAge')}
                </th>
                <th scope="col" className="num">
                  {t('col.validity')}
                </th>
                <th scope="col" className="num">
                  {t('col.fee')}
                </th>
              </tr>
            </thead>
            <tbody>
              {classes.data.map((c) => (
                <tr key={c.licenseClassId}>
                  <td>
                    <div className="class-cell">
                      <span className="class-sign" aria-hidden="true">
                        <Icon name={classIcons[c.licenseClassId] ?? 'car'} size={28} />
                        <span className="class-sign-num">{c.licenseClassId}</span>
                      </span>
                      <span>
                        <span className="cell-main">{className(c.licenseClassId, c.className)}</span>
                        <span className="cell-sub">{classDescription(c.licenseClassId, c.classDescription)}</span>
                      </span>
                    </div>
                  </td>
                  <td className="num">{c.minimumAllowedAge}</td>
                  <td className="num">{t('classes.years', { n: c.validityLength })}</td>
                  <td className="num">{money(c.classFees)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
