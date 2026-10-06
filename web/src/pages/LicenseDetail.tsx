import { useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api, ApiError, useApi, type License } from '../api'
import { classIcons } from '../classIcons'
import { Icon } from '../components/Icons'
import { Fact, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n, type Key } from '../i18n'
import { LicenseStatus } from './Licenses'

// Class 3 is the standard car license; the API only issues international licenses from it.
const STANDARD_CLASS_ID = 3

export function LicenseDetail() {
  const { t, date, money, className } = useI18n()
  const id = Number(useParams().id)
  const license = useApi(() => api.license(id), [id])

  if (license.error) {
    return (
      <div className="page">
        <PageHead title={t('lic.title')} />
        <RuleNotice error={license.error} onRetry={license.reload} />
        <p>
          <Link to="/licenses">{t('back')}</Link>
        </p>
      </div>
    )
  }
  if (!license.data) return <Loading />

  const l = license.data

  return (
    <div className="page">
      <PageHead
        plate={<Plate kind="license" id={l.licenseId} />}
        title={l.driverFullName}
        lede={className(l.licenseClassId, l.className)}
        actions={<LicenseStatus license={l} />}
      />

      <div className="license-layout">
        <div>
          <LicenseCard license={l} />
          <dl className="facts facts-stack">
            <Fact label={t('lic.reason')}>{t(`reason.${l.issueReason}`)}</Fact>
            <Fact label={t('lic.fees')}>{money(l.paidFees)}</Fact>
            <Fact label={t('lic.application')}>
              <span className="num">#{l.applicationId}</span>
            </Fact>
            <Fact label={t('lic.driver')}>
              <Plate kind="driver" id={l.driverId} to={`/drivers/${l.driverId}`} />{' '}
              <Link to={`/drivers/${l.driverId}`} className="link-more">
                {t('lic.history')}
              </Link>
            </Fact>
            <Fact label={t('lic.holder')}>
              <Plate kind="person" id={l.personId} to={`/people/${l.personId}`} />
            </Fact>
            {l.notes && <Fact label={t('lic.notes')}>{l.notes}</Fact>}
            <Fact label={t('lic.issued')}>{date(l.issueDate)}</Fact>
          </dl>
        </div>

        <Services license={l} onChanged={license.reload} />
      </div>
    </div>
  )
}

function LicenseCard({ license: l }: { license: License }) {
  const { t, date, className } = useI18n()
  return (
    <figure className={`license-card${l.isActive ? '' : ' is-inactive'}${l.isDetained ? ' is-detained' : ''}`}>
      <div className="license-card-band">
        <span lang="ar" dir="rtl">
          رخصة قيادة
        </span>
        <span lang="en" dir="ltr">
          Driving License
        </span>
      </div>
      <div className="license-card-body">
        <div className="license-card-class">
          <Icon name={classIcons[l.licenseClassId] ?? 'car'} size={34} />
          <span className="license-card-classnum">{l.licenseClassId}</span>
        </div>
        <dl className="license-card-fields">
          <div>
            <dt>{t('lic.holder')}</dt>
            <dd className="license-card-name">{l.driverFullName}</dd>
          </div>
          <div>
            <dt>{t('col.nationalNo')}</dt>
            <dd dir="ltr">{l.nationalNo}</dd>
          </div>
          <div>
            <dt>{t('lic.issued')}</dt>
            <dd>{date(l.issueDate)}</dd>
          </div>
          <div>
            <dt>{t('lic.expires')}</dt>
            <dd>{date(l.expirationDate)}</dd>
          </div>
        </dl>
      </div>
      <figcaption className="license-card-foot">
        <span>{className(l.licenseClassId, l.className)}</span>
        <span dir="ltr" className="num">
          No. {String(l.licenseId).padStart(6, '0')}
        </span>
      </figcaption>
      {l.isDetained && (
        <span className="license-card-stamp" aria-hidden="true">
          {t('lic.detained')}
        </span>
      )}
    </figure>
  )
}

type Notice = { tone: 'done' | 'stop'; text: ReactNode }

function Services({ license: l, onChanged }: { license: License; onChanged: () => void }) {
  const { t, money, date } = useI18n()
  const navigate = useNavigate()
  const [busy, setBusy] = useState<string>()
  const [error, setError] = useState<{ service: string; error: ApiError }>()
  const [notice, setNotice] = useState<Notice>()
  const [renewNotes, setRenewNotes] = useState('')
  const [fine, setFine] = useState('20')

  async function run(service: string, action: () => Promise<void>) {
    setBusy(service)
    setError(undefined)
    setNotice(undefined)
    try {
      await action()
    } catch (err) {
      setError({ service, error: err as ApiError })
    } finally {
      setBusy(undefined)
    }
  }

  const openNew = (next: License) => {
    navigate(`/licenses/${next.licenseId}`)
  }

  const active = l.isActive
  const blocked = (needs: Key | undefined) => needs && t(needs)

  const services: {
    id: string
    title: Key
    sub: Key
    needs: Key | undefined
    fields?: ReactNode
    action: () => Promise<void>
  }[] = [
    {
      id: 'renew',
      title: 'svc.renew',
      sub: 'svc.renew.sub',
      needs: !active ? 'svc.needsActive' : l.isDetained ? 'svc.needsNotDetained' : undefined,
      fields: (
        <div className="field">
          <label htmlFor="renew-notes">{t('svc.notes')}</label>
          <input id="renew-notes" value={renewNotes} onChange={(e) => setRenewNotes(e.target.value)} />
        </div>
      ),
      action: async () => openNew(await api.renew(l.licenseId, renewNotes.trim())),
    },
    {
      id: 'lost',
      title: 'svc.lost',
      sub: 'svc.lost.sub',
      needs: !active ? 'svc.needsActive' : undefined,
      action: async () => openNew(await api.replaceLost(l.licenseId)),
    },
    {
      id: 'damaged',
      title: 'svc.damaged',
      sub: 'svc.damaged.sub',
      needs: !active ? 'svc.needsActive' : undefined,
      action: async () => openNew(await api.replaceDamaged(l.licenseId)),
    },
    l.isDetained
      ? {
          id: 'release',
          title: 'svc.release',
          sub: 'svc.release.sub',
          needs: undefined,
          action: async () => {
            const r = await api.release(l.licenseId)
            setNotice({ tone: 'done', text: t('svc.releasedDone', { fine: money(r.fineFees) }) })
            onChanged()
          },
        }
      : {
          id: 'detain',
          title: 'svc.detain',
          sub: 'svc.detain.sub',
          needs: !active ? 'svc.needsActive' : undefined,
          fields: (
            <div className="field field-money">
              <label htmlFor="fine">{t('svc.fine')}</label>
              <input
                id="fine"
                type="number"
                min="0"
                step="0.01"
                dir="ltr"
                required
                value={fine}
                onChange={(e) => setFine(e.target.value)}
              />
            </div>
          ),
          action: async () => {
            const r = await api.detain(l.licenseId, Number(fine))
            setNotice({ tone: 'stop', text: t('svc.detainedDone', { fine: money(r.fineFees) }) })
            onChanged()
          },
        },
    {
      id: 'intl',
      title: 'svc.intl',
      sub: 'svc.intl.sub',
      needs: !active ? 'svc.needsActive' : l.licenseClassId !== STANDARD_CLASS_ID ? 'svc.needsClass3' : undefined,
      action: async () => {
        const r = await api.international(l.licenseId)
        setNotice({
          tone: 'done',
          text: t('svc.intlDone', { id: r.internationalLicenseId, date: date(r.expirationDate) }),
        })
      },
    },
  ]

  return (
    <section className="services" aria-labelledby="services-title">
      <h2 id="services-title" className="services-title">
        {t('lic.services')}
      </h2>
      {!active && <p className="services-note">{t('lic.inactiveNote')}</p>}
      {notice && (
        <p className={`alert alert-${notice.tone === 'done' ? 'success' : 'warning'}`} role="status">
          <Icon name={notice.tone === 'done' ? 'check' : 'warning'} size={18} className="alert-icon" />
          {notice.text}
        </p>
      )}
      <ul className="service-list">
        {services.map((s) => {
          const reason = blocked(s.needs)
          const submit = (e: FormEvent) => {
            e.preventDefault()
            run(s.id, s.action)
          }
          return (
            <li key={s.id} className={`service${reason ? ' is-blocked' : ''}`}>
              <form onSubmit={submit}>
                <div className="service-text">
                  <h3>{t(s.title)}</h3>
                  <p>{reason ?? t(s.sub)}</p>
                </div>
                {!reason && s.fields}
                <button type="submit" className="btn btn-secondary" disabled={Boolean(reason) || busy !== undefined}>
                  {busy === s.id ? t('svc.working') : t(s.title)}
                </button>
                {error?.service === s.id && <RuleNotice error={error.error} />}
              </form>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
