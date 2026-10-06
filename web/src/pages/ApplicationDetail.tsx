import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  api,
  ApiError,
  rememberLicense,
  useApi,
  type LocalApplication,
  type TestAppointment,
  type TestType,
} from '../api'
import { Steps, type Step, type StepState } from '../components/Steps'
import { Icon, type IconName } from '../components/Icons'
import { Fact, Loading, PageHead, Plate, RuleNotice, StatusTag } from '../components/ui'
import { useI18n, type Key } from '../i18n'

const tests: { type: TestType; icon: IconName; lane: Key; title: Key }[] = [
  { type: 1, icon: 'eye', lane: 'route.vision', title: 'test.1' },
  { type: 2, icon: 'book', lane: 'route.theory', title: 'test.2' },
  { type: 3, icon: 'wheel', lane: 'route.practical', title: 'test.3' },
]

type Outcome = 'pass' | 'fail' | 'pending'

function outcomes(appts: TestAppointment[]): Outcome[] {
  return appts.map((a) => (a.testResult === 'Pass' ? 'pass' : a.testResult === 'Fail' ? 'fail' : 'pending'))
}

function testState(index: number, app: LocalApplication): StepState {
  if (index < app.passedTestCount) return 'done'
  if (app.applicationStatus === 'Completed') return 'done'
  if (app.applicationStatus === 'Cancelled') return index === app.passedTestCount ? 'blocked' : 'todo'
  return index === app.passedTestCount ? 'current' : 'todo'
}

export function ApplicationDetail() {
  const { t, date, dateTime, money, className } = useI18n()
  const navigate = useNavigate()
  const id = Number(useParams().id)
  const [tick, setTick] = useState(0)
  const app = useApi(() => api.application(id), [id, tick])
  const appts = useApi(() => Promise.all(tests.map((x) => api.appointments(id, x.type))), [id, tick])
  const [cancelError, setCancelError] = useState<ApiError>()
  const refresh = () => setTick((n) => n + 1)

  if (app.error) {
    return (
      <div className="page">
        <PageHead title={t('app.title')} />
        <RuleNotice error={app.error} onRetry={app.reload} />
      </div>
    )
  }
  if (!app.data) return <Loading />

  const a = app.data
  const isNew = a.applicationStatus === 'New'
  const ready = isNew && a.passedTestCount >= 3
  const lists = appts.data?.map((l) => [...l].sort((x, y) => x.testAppointmentId - y.testAppointmentId))

  const laneSub = (index: number) => {
    const list = lists?.[index]
    if (!list) return undefined
    const results = outcomes(list)
    const fails = results.filter((r) => r === 'fail').length
    const open = list.find((x) => !x.isLocked)
    if (index < a.passedTestCount)
      return fails ? (
        <>
          {t('test.passed')}
          <span className="dot-sep" aria-hidden="true" />
          {t('test.failedTimes', { n: fails })}
        </>
      ) : (
        t('test.passed')
      )
    if (open) return date(open.appointmentDate)
    if (fails) return t('test.failedTimes', { n: fails })
    return undefined
  }

  const lanes: Step[] = [
    { key: 'apply', icon: 'apply', label: t('route.apply'), sub: t('route.apply.sub'), state: 'done' },
    ...tests.map((x, i) => ({
      key: x.lane,
      icon: x.icon,
      label: t(x.lane),
      sub: laneSub(i),
      state: testState(i, a),
    })),
    {
      key: 'license',
      icon: 'card' as const,
      label: t('route.license'),
      sub: t('route.license.sub'),
      state: (a.applicationStatus === 'Completed' ? 'done' : ready ? 'current' : 'todo') as StepState,
    },
  ]

  async function cancel() {
    if (!window.confirm(t('app.cancelConfirm'))) return
    try {
      await api.cancelApplication(a.applicationId)
      setCancelError(undefined)
      refresh()
    } catch (err) {
      setCancelError(err as ApiError)
    }
  }

  return (
    <div className="page">
      <PageHead
        plate={<Plate kind="app" id={a.localDrivingLicenseApplicationId} />}
        title={className(a.licenseClassId, a.className)}
        lede={
          <>
            <Link to={`/people/${a.applicantPersonId}`}>{a.applicantFullName}</Link>{' '}
            <span className="muted" dir="ltr">
              {a.nationalNo}
            </span>
          </>
        }
        actions={
          isNew && (
            <button type="button" className="btn btn-quiet" onClick={cancel}>
              {t('app.cancel')}
            </button>
          )
        }
      />

      <Steps steps={lanes} caption={t('route.label')} action={<StatusTag status={a.applicationStatus} />} />

      {cancelError && <RuleNotice error={cancelError} />}

      {a.applicationStatus === 'Cancelled' && (
        <p className="alert alert-neutral">
          <Icon name="prohibit" size={18} className="alert-icon" />
          {t('app.cancelled')}
        </p>
      )}
      {a.applicationStatus === 'Completed' && (
        <p className="alert alert-success">
          <Icon name="check" size={18} className="alert-icon" />
          <span>
            {t('app.completed')} <Link to="/licenses">{t('app.findLicense')}</Link>
          </span>
        </p>
      )}

      {ready && (
        <IssuePanel
          app={a}
          onIssued={(licenseId) => {
            rememberLicense(licenseId)
            navigate(`/licenses/${licenseId}`)
          }}
        />
      )}

      {appts.error ? (
        <RuleNotice error={appts.error} onRetry={appts.reload} />
      ) : !lists ? (
        <Loading />
      ) : (
        <div className="tests">
          {tests.map((x, i) => (
            <TestPanel
              key={x.type}
              index={i}
              app={a}
              appointments={lists[i]}
              onChange={refresh}
            />
          ))}
        </div>
      )}

      <dl className="facts">
        <Fact label={t('app.applied')}>
          <span className="nowrap">{dateTime(a.applicationDate)}</span>
        </Fact>
        <Fact label={t('app.fees')}>{money(a.paidFees)}</Fact>
        <Fact label={t('app.status')}>
          <StatusTag status={a.applicationStatus} />
        </Fact>
        <Fact label={t('app.applicant')}>
          <Plate kind="person" id={a.applicantPersonId} to={`/people/${a.applicantPersonId}`} />
        </Fact>
      </dl>
    </div>
  )
}

function TestPanel({
  index,
  app,
  appointments,
  onChange,
}: {
  index: number
  app: LocalApplication
  appointments: TestAppointment[]
  onChange: () => void
}) {
  const { t, dateTime, money } = useI18n()
  const test = tests[index]
  const state = testState(index, app)
  const results = outcomes(appointments)
  const open = appointments.find((x) => !x.isLocked)
  const canBook = state === 'current' && !open

  return (
    <section className={`card test is-${state}`} aria-labelledby={`test-${test.type}`}>
      <header className="test-head">
        <span className="test-icon">
          <Icon name={state === 'done' ? 'check' : test.icon} size={20} />
        </span>
        <h2 id={`test-${test.type}`}>{t(test.title)}</h2>
      </header>

      {appointments.length === 0 ? (
        <p className="test-empty">
          {state === 'todo' && index > 0
            ? t('test.locked', { prev: t(tests[index - 1].title) })
            : t('test.none')}
        </p>
      ) : (
        <ol className="appts">
          {appointments.map((ap, i) => (
            <li key={ap.testAppointmentId} className={`appt is-${results[i]}`}>
              <span className="appt-when">
                <span className="appt-n">{t('test.appointment', { n: i + 1 })}</span>
                <span className="nowrap">{dateTime(ap.appointmentDate)}</span>
                {ap.resultNotes && <span className="appt-notes">{ap.resultNotes}</span>}
              </span>
              <span className="appt-fee num">{money(ap.paidFees)}</span>
              <span className={`result result-${results[i]}`}>
                {results[i] === 'pass' ? t('test.passed') : results[i] === 'fail' ? t('test.failed') : t('test.pending')}
              </span>
            </li>
          ))}
        </ol>
      )}

      {state === 'current' && open && <RecordResult testType={test.type} appointment={open} onDone={onChange} />}
      {canBook && (
        <ScheduleTest
          appId={app.localDrivingLicenseApplicationId}
          testType={test.type}
          retake={results.includes('fail')}
          onDone={onChange}
        />
      )}
    </section>
  )
}

function tomorrowAtNine() {
  const d = new Date()
  d.setDate(d.getDate() + 1)
  d.setHours(9, 0, 0, 0)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T09:00`
}

function ScheduleTest({
  appId,
  testType,
  retake,
  onDone,
}: {
  appId: number
  testType: TestType
  retake: boolean
  onDone: () => void
}) {
  const { t } = useI18n()
  const [when, setWhen] = useState(tomorrowAtNine)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<ApiError>()

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(undefined)
    try {
      await api.scheduleTest(appId, testType, new Date(when).toISOString())
      onDone()
    } catch (err) {
      setError(err as ApiError)
      setBusy(false)
    }
  }

  return (
    <form className="test-action" onSubmit={submit}>
      <div className="field">
        <label htmlFor={`when-${testType}`}>{t('test.when')}</label>
        <input id={`when-${testType}`} type="datetime-local" required value={when} onChange={(e) => setWhen(e.target.value)} />
      </div>
      {error && <RuleNotice error={error} />}
      <button type="submit" className="btn btn-primary" disabled={busy}>
        {busy ? t('test.scheduling') : retake ? t('test.reschedule') : t('test.schedule')}
      </button>
    </form>
  )
}

function RecordResult({
  testType,
  appointment,
  onDone,
}: {
  testType: TestType
  appointment: TestAppointment
  onDone: () => void
}) {
  const { t } = useI18n()
  const [notes, setNotes] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<ApiError>()

  async function record(pass: boolean) {
    setBusy(true)
    setError(undefined)
    try {
      await api.takeTest(testType, appointment.testAppointmentId, pass, notes.trim())
      onDone()
    } catch (err) {
      setError(err as ApiError)
      setBusy(false)
    }
  }

  return (
    <div className="test-action">
      <p className="test-action-title">{t('test.record')}</p>
      <div className="field">
        <label htmlFor={`notes-${testType}`}>{t('test.notes')}</label>
        <input id={`notes-${testType}`} value={notes} onChange={(e) => setNotes(e.target.value)} />
      </div>
      {error && <RuleNotice error={error} />}
      <div className="btn-pair">
        <button type="button" className="btn btn-pass" disabled={busy} onClick={() => record(true)}>
          <Icon name="check" size={18} />
          {t('test.pass')}
        </button>
        <button type="button" className="btn btn-fail" disabled={busy} onClick={() => record(false)}>
          <Icon name="close" size={18} />
          {t('test.fail')}
        </button>
      </div>
    </div>
  )
}

function IssuePanel({ app, onIssued }: { app: LocalApplication; onIssued: (licenseId: number) => void }) {
  const { t } = useI18n()
  const [notes, setNotes] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<ApiError>()

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(undefined)
    try {
      const license = await api.issueFirstTime(app.localDrivingLicenseApplicationId, notes.trim())
      onIssued(license.licenseId)
    } catch (err) {
      setError(err as ApiError)
      setBusy(false)
    }
  }

  return (
    <form className="card issue" onSubmit={submit}>
      <div className="issue-text">
        <h2 className="card-title">{t('app.issueTitle')}</h2>
        <p>{t('app.issueLede')}</p>
      </div>
      <div className="field">
        <label htmlFor="issue-notes">{t('app.issueNotes')}</label>
        <input id="issue-notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
      </div>
      {error && <RuleNotice error={error} />}
      <button type="submit" className="btn btn-primary" disabled={busy}>
        <Icon name="card" size={18} />
        {busy ? t('app.issuing') : t('app.issue')}
      </button>
    </form>
  )
}
