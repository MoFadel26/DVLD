import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { api, ApiError, useApi } from '../api'
import { classIcons } from '../classIcons'
import { Icon } from '../components/Icons'
import { Empty, Loading, PageHead, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'

export function NewApplication() {
  const { t, money, className, classDescription } = useI18n()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const people = useApi(api.people, [])
  const classes = useApi(api.licenseClasses, [])

  const [personId, setPersonId] = useState(params.get('person') ?? '')
  const [classId, setClassId] = useState<number>()
  const [error, setError] = useState<ApiError>()
  const [saving, setSaving] = useState(false)

  const person = people.data?.find((p) => String(p.personId) === personId)

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!personId || !classId) return
    setSaving(true)
    setError(undefined)
    try {
      const app = await api.createApplication(Number(personId), classId)
      navigate(`/applications/${app.localDrivingLicenseApplicationId}`)
    } catch (err) {
      setError(err as ApiError)
      setSaving(false)
    }
  }

  const loadError = people.error ?? classes.error

  return (
    <div className="page page-narrow">
      <PageHead title={t('newApp.title')} lede={t('newApp.lede')} />

      {loadError ? (
        <RuleNotice
          error={loadError}
          onRetry={() => {
            people.reload()
            classes.reload()
          }}
        />
      ) : !people.data || !classes.data ? (
        <Loading />
      ) : people.data.length === 0 ? (
        <Empty
          action={
            <Link to="/people/new" className="btn btn-primary">
              {t('people.new')}
            </Link>
          }
        >
          {t('newApp.noPeople')}
        </Empty>
      ) : (
        <form className="form" onSubmit={submit}>
          <div className="field">
            <label htmlFor="applicant">{t('newApp.applicant')}</label>
            <select id="applicant" required value={personId} onChange={(e) => setPersonId(e.target.value)}>
              <option value="" disabled>
                {t('newApp.choose')}
              </option>
              {people.data.map((p) => (
                <option key={p.personId} value={p.personId}>
                  {p.nationalNo} – {p.fullName} – {t('person.years', { n: p.age })}
                </option>
              ))}
            </select>
          </div>

          <fieldset>
            <legend>{t('newApp.class')}</legend>
            <div className="class-choices">
              {classes.data.map((c) => {
                const short = person && person.age < c.minimumAllowedAge
                return (
                  <label key={c.licenseClassId} className={`class-choice${classId === c.licenseClassId ? ' is-chosen' : ''}`}>
                    <input
                      type="radio"
                      name="licenseClass"
                      required
                      value={c.licenseClassId}
                      checked={classId === c.licenseClassId}
                      onChange={() => setClassId(c.licenseClassId)}
                    />
                    <Icon name={classIcons[c.licenseClassId] ?? 'car'} size={26} className="class-choice-icon" />
                    <span className="class-choice-text">
                      <span className="class-choice-name">{className(c.licenseClassId, c.className)}</span>
                      <span className="class-choice-desc">{classDescription(c.licenseClassId, c.classDescription)}</span>
                      {short && (
                        <span className="class-choice-warn">
                          <Icon name="warning" size={14} />
                          {t('newApp.underAge', { min: c.minimumAllowedAge, age: person.age })}
                        </span>
                      )}
                    </span>
                    <span className="class-choice-meta">
                      <span className="num">{money(c.classFees)}</span>
                      <span className="muted">{c.minimumAllowedAge}+</span>
                    </span>
                  </label>
                )
              })}
            </div>
          </fieldset>

          {error && <RuleNotice error={error} />}

          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={saving || !personId || !classId}>
              {saving ? t('newApp.submitting') : t('newApp.submit')}
            </button>
            <p className="field-hint">{t('newApp.baseFee')}</p>
          </div>
        </form>
      )}
    </div>
  )
}
