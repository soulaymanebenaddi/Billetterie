import { Hero } from './components/home/Hero'
import { SiteHeader } from './components/layout/SiteHeader'
import './App.css'

function App() {
  return (
    <div id="accueil" className="app">
      <SiteHeader />
      <main>
        <Hero />
      </main>
    </div>
  )
}

export default App
