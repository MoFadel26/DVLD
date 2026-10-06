import type { CSSProperties, ReactNode } from 'react'
import { useI18n } from '../i18n'
import { Icon, type IconName } from './Icons'

export type StepState = 'done' | 'current' | 'todo' | 'blocked'

export type Step = {
  key: string
  icon: IconName
  label: string
  sub?: ReactNode
  state: StepState
}

/** Horizontal progress through the licensing steps; stacks vertically on narrow cards. */
export function Steps({
  steps,
  caption,
  action,
}: {
  steps: Step[]
  caption: ReactNode
  action?: ReactNode
}) {
  const { t } = useI18n()
  const stateLabel: Record<StepState, string> = {
    done: t('route.state.done'),
    current: t('route.state.current'),
    todo: t('route.state.todo'),
    blocked: t('route.state.blocked'),
  }

  return (
    <section className="card steps-card" aria-label={t('route.label')}>
      <div className="card-head">
        <h2 className="card-title">{caption}</h2>
        {action}
      </div>
      <ol className="steps">
        {steps.map((step, i) => (
          <li
            key={`${step.key}-${step.state}`}
            className={`step is-${step.state}`}
            style={{ '--i': i } as CSSProperties}
            aria-current={step.state === 'current' ? 'step' : undefined}
          >
            <span className="step-dot" aria-hidden="true">
              {step.state === 'done' ? (
                <Icon name="check" size={16} />
              ) : step.state === 'blocked' ? (
                <Icon name="close" size={16} />
              ) : (
                <Icon name={step.icon} size={16} />
              )}
            </span>
            <span className="step-text">
              <span className="step-label">{step.label}</span>
              <span className="step-sub">{step.sub}</span>
              <span className="visually-hidden">{stateLabel[step.state]}</span>
            </span>
          </li>
        ))}
      </ol>
    </section>
  )
}
