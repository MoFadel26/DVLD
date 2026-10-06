import { screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderApp } from '../test/helpers'
import { Steps } from './Steps'

describe('Steps', () => {
  it('marks the current step and announces each state', () => {
    renderApp(
      <Steps
        caption="Route to a driving license"
        steps={[
          { key: 'vision', icon: 'eye', label: 'Vision', state: 'done' },
          { key: 'theory', icon: 'book', label: 'Theory', state: 'current' },
          { key: 'practical', icon: 'wheel', label: 'Practical', state: 'todo' },
        ]}
      />,
    )

    const items = screen.getAllByRole('listitem')
    expect(items).toHaveLength(3)
    expect(items[1]).toHaveAttribute('aria-current', 'step')
    expect(within(items[0]).getByText('Passed')).toBeInTheDocument()
    expect(within(items[1]).getByText('Current stage')).toBeInTheDocument()
    expect(within(items[2]).getByText('Not reached')).toBeInTheDocument()
  })
})
