import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api, ApiError, useApi } from '../api'
import { Icon } from '../components/Icons'
import { Empty, Fact, Loading, PageHead, Plate, RuleNotice } from '../components/ui'
import { useI18n } from '../i18n'
import { ApplicationsTable } from './Applications'

export function PersonDetail() {
  const { t, date, country } = useI18n()
  const navigate = useNavigate()
  const id = Number(useParams().id)
  const person = useApi(() => api.person(id), [id])
  const apps = useApi(api.applications, [])
  const [deleteError, setDeleteError] = useState<ApiError>()
  const [deleting, setDeleting] = useState(false)

  if (person.error) {
    return (
      <div className="page">
        <PageHead title={t('nav.people')} />
        <RuleNotice error={person.error} onRetry={person.reload} />
      </div>
    )
  }
  if (!person.data) return <Loading />

  const p = person.data
  const mine = (apps.data ?? [])
    .filter((a) => a.applicantPersonId === p.personId)
    .sort((a, b) => b.localDrivingLicenseApplicationId - a.localDrivingLicenseApplicationId)

  async function remove() {
    if (!window.confirm(t('person.deleteConfirm', { name: p.fullName }))) return
    setDeleting(true)
    try {
      await api.deletePerson(p.personId)
      navigate('/people')
    } catch (err) {
      setDeleteError(err as ApiError)
      setDeleting(false)
    }
  }

  return (
    <div className="page">
      <PageHead
        plate={<Plate kind="person" id={p.personId} />}
        title={p.fullName}
        lede={
          <span dir="ltr" className="cell-code">
            {p.nationalNo}
          </span>
        }
        actions={
          <>
            <Link to={`/applications/new?person=${p.personId}`} className="btn btn-primary">
              <Icon name="plus" size={18} />
              {t('person.newApp')}
            </Link>
            <Link to={`/people/${p.personId}/edit`} className="btn btn-quiet">
              {t('person.edit')}
            </Link>
          </>
        }
      />

      <dl className="facts">
        <Fact label={t('person.born')}>
          {date(p.dateOfBirth)} <span className="muted">· {t('person.years', { n: p.age })}</span>
        </Fact>
        <Fact label={t('col.gender')}>{t(`gender.${p.gender}`)}</Fact>
        <Fact label={t('col.country')}>{country(p.nationalityCountryId, p.countryName)}</Fact>
        <Fact label={t('col.phone')}>
          <span dir="ltr">{p.phone}</span>
        </Fact>
        <Fact label={t('person.email')}>
          <span dir="ltr">{p.email}</span>
        </Fact>
        <Fact label={t('person.address')}>{p.address}</Fact>
      </dl>

      <section aria-labelledby="person-apps">
        <div className="section-head">
          <h2 id="person-apps">{t('person.applications')}</h2>
        </div>
        {apps.error ? (
          <RuleNotice error={apps.error} onRetry={apps.reload} />
        ) : !apps.data ? (
          <Loading />
        ) : mine.length === 0 ? (
          <Empty>{t('person.noApplications')}</Empty>
        ) : (
          <ApplicationsTable rows={mine} hideApplicant />
        )}
      </section>

      <section className="danger-zone">
        {deleteError && <RuleNotice error={deleteError} />}
        <button type="button" className="btn btn-danger" onClick={remove} disabled={deleting}>
          {t('person.delete')}
        </button>
      </section>
    </div>
  )
}
