import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api, ApiError, type Person, type PersonInput } from '../api'
import { Loading, PageHead, RuleNotice } from '../components/ui'
import { countries, useI18n } from '../i18n'

type Draft = PersonInput & { nationalNo: string }

const blank: Draft = {
  nationalNo: '',
  firstName: '',
  secondName: '',
  thirdName: '',
  lastName: '',
  dateOfBirth: '',
  gender: 0,
  address: '',
  phone: '',
  email: '',
  nationalityCountryId: 1,
  imagePath: null,
}

function fromPerson(p: Person): Draft {
  return {
    nationalNo: p.nationalNo,
    firstName: p.firstName,
    secondName: p.secondName,
    thirdName: p.thirdName ?? '',
    lastName: p.lastName,
    dateOfBirth: p.dateOfBirth.slice(0, 10),
    gender: p.gender === 'Female' ? 1 : 0,
    address: p.address,
    phone: p.phone,
    email: p.email,
    nationalityCountryId: p.nationalityCountryId,
    imagePath: p.imagePath,
  }
}

export function PersonForm() {
  const { t, lang } = useI18n()
  const navigate = useNavigate()
  const params = useParams()
  const editId = params.id ? Number(params.id) : undefined

  const [draft, setDraft] = useState<Draft>(blank)
  const [original, setOriginal] = useState<Person>()
  const [loadError, setLoadError] = useState<ApiError>()
  const [error, setError] = useState<ApiError>()
  const [saving, setSaving] = useState(false)
  const [today] = useState(() => new Date().toISOString().slice(0, 10))

  useEffect(() => {
    if (editId === undefined) return
    api
      .person(editId)
      .then((p) => {
        setOriginal(p)
        setDraft(fromPerson(p))
      })
      .catch((err: ApiError) => setLoadError(err))
  }, [editId])

  if (loadError) return <RuleNotice error={loadError} />
  if (editId !== undefined && !original) return <Loading />

  const set = <K extends keyof Draft>(key: K, value: Draft[K]) => setDraft((d) => ({ ...d, [key]: value }))

  async function submit(e: FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError(undefined)
    const { nationalNo, ...input } = draft
    const body: PersonInput = {
      ...input,
      thirdName: input.thirdName?.trim() || null,
      dateOfBirth: new Date(`${input.dateOfBirth}T00:00:00Z`).toISOString(),
    }
    try {
      const saved =
        editId === undefined
          ? await api.createPerson({ ...body, nationalNo: nationalNo.trim() })
          : await api.updatePerson(editId, body)
      navigate(`/people/${saved.personId}`)
    } catch (err) {
      setError(err as ApiError)
      setSaving(false)
    }
  }

  return (
    <div className="page page-narrow">
      <PageHead title={original ? t('form.editPerson', { name: original.fullName }) : t('form.newPerson')} />

      <form className="form" onSubmit={submit}>
        <fieldset>
          <legend>{t('form.identity')}</legend>
          <div className="field">
            <label htmlFor="nationalNo">{t('form.nationalNo')}</label>
            <input
              id="nationalNo"
              dir="ltr"
              required
              maxLength={20}
              value={draft.nationalNo}
              disabled={editId !== undefined}
              onChange={(e) => set('nationalNo', e.target.value)}
              aria-describedby="nationalNo-hint"
            />
            <p className="field-hint" id="nationalNo-hint">
              {t('form.nationalNoHint')}
            </p>
          </div>
          <div className="field-row">
            <div className="field">
              <label htmlFor="firstName">{t('form.firstName')}</label>
              <input id="firstName" required value={draft.firstName} onChange={(e) => set('firstName', e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="secondName">{t('form.secondName')}</label>
              <input id="secondName" required value={draft.secondName} onChange={(e) => set('secondName', e.target.value)} />
            </div>
          </div>
          <div className="field-row">
            <div className="field">
              <label htmlFor="thirdName">{t('form.thirdName')}</label>
              <input id="thirdName" value={draft.thirdName ?? ''} onChange={(e) => set('thirdName', e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="lastName">{t('form.lastName')}</label>
              <input id="lastName" required value={draft.lastName} onChange={(e) => set('lastName', e.target.value)} />
            </div>
          </div>
          <div className="field-row field-row-3">
            <div className="field">
              <label htmlFor="dob">{t('form.dob')}</label>
              <input
                id="dob"
                type="date"
                required
                max={today}
                value={draft.dateOfBirth}
                onChange={(e) => set('dateOfBirth', e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="gender">{t('form.gender')}</label>
              <select id="gender" value={draft.gender} onChange={(e) => set('gender', Number(e.target.value) as 0 | 1)}>
                <option value={0}>{t('gender.Male')}</option>
                <option value={1}>{t('gender.Female')}</option>
              </select>
            </div>
            <div className="field">
              <label htmlFor="country">{t('form.country')}</label>
              <select
                id="country"
                value={draft.nationalityCountryId}
                onChange={(e) => set('nationalityCountryId', Number(e.target.value))}
              >
                {countries.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c[lang]}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </fieldset>

        <fieldset>
          <legend>{t('form.contact')}</legend>
          <div className="field-row">
            <div className="field">
              <label htmlFor="phone">{t('form.phone')}</label>
              <input id="phone" type="tel" dir="ltr" required value={draft.phone} onChange={(e) => set('phone', e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="email">{t('form.email')}</label>
              <input id="email" type="email" dir="ltr" required value={draft.email} onChange={(e) => set('email', e.target.value)} />
            </div>
          </div>
          <div className="field">
            <label htmlFor="address">{t('form.address')}</label>
            <input id="address" required value={draft.address} onChange={(e) => set('address', e.target.value)} />
          </div>
        </fieldset>

        {error && <RuleNotice error={error} />}

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? t('form.saving') : t('form.save')}
          </button>
          <Link to={editId === undefined ? '/people' : `/people/${editId}`} className="btn btn-quiet">
            {t('form.cancel')}
          </Link>
        </div>
      </form>
    </div>
  )
}
