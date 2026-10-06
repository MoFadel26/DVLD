import { Route, Routes } from 'react-router-dom'
import { Shell } from './components/Shell'
import { Applications } from './pages/Applications'
import { Overview } from './pages/Overview'

export default function App() {
  return (
    <Routes>
      <Route element={<Shell />}>
        <Route index element={<Overview />} />
        <Route path="applications" element={<Applications />} />
        <Route path="*" element={<Overview />} />
      </Route>
    </Routes>
  )
}
